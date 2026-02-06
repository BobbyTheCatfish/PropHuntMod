using BepInEx;
using HarmonyLib;
using PropHuntMod.Modifications;
//using PropHuntMod.Utils.Networking;
using PropHuntMod.Utils;
using PropHuntMod.Utils.Networking;
using SSMP.Api.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/**
 * FEATURE LIST
 * Hide/Show Hornet
 * Spawn and attach a game object (prop) to hornet
 * Move the prop in x/y/z
 * Dynamically get game objects in current scene
 * Add more props other than breakable ones (corpses, enemies?, etc)
 * Slow down attacks (Currently disabled)
 *
 * 
 * TODO:
 * Integrate with multiplayer mod
 *  - Find out which player is whichz
 *  - Send prop information packets
 * 
 */

namespace PropHuntMod
{
    using PlayerID = UInt16;

    public enum MovementState
    {
        Normal,
        Move2D,
        MoveZ,
        Rotate
    }

    [BepInPlugin("com.bobbythecatfish.prophunt", Utils.Config.ModName, Utils.Config.ModVersion)]
    [BepInDependency("ssmp")]
    [BepInProcess("Hollow Knight Silksong.exe")]
    public class PropHuntMod : BaseUnityPlugin
    {
        internal static PropHuntMod Instance;
        internal SelfHornetManager hornet = new SelfHornetManager();
        internal SelfCoverManager cover = new SelfCoverManager();
        //private static AttackCooldownPatches attackPatches = new AttackCooldownPatches(config);
        private static readonly NoDamage noDamage = new NoDamage();
        private static PropMovementControls movement;
        internal static Dictionary<PlayerID, PlayerManager> playerManager = new Dictionary<PlayerID, PlayerManager>();
        internal static IClientApi client;
        internal static bool modEnabled = false;
        internal static bool showHitboxes = false;

        internal static string CurrentScene;
        internal static string PreviousScene;
        internal static int SceneTransitionTicket = -1;
        //internal static string PreviousGate;

        internal static List<Action> nextFrameActions = new List<Action>();
        static List<Action> _nextFrames = new List<Action>();

        void Awake()
        {
            Instance = this;
            Utils.Config.LoadConfig(Config);
            Log.SetLogger(base.Logger);
            SSMP.Api.Client.ClientAddon.RegisterAddon(new PropHuntClient());
            SSMP.Api.Server.ServerAddon.RegisterAddon(new PropHuntServer());
        }
        public static void Initialize(IClientApi clientApi)
        {
            Log.LogInfo("Prop Hunt mod Loaded.");

            client = clientApi;

            Harmony.CreateAndPatchAll(typeof(PropHuntMod), "prophunt");
            Harmony.CreateAndPatchAll(typeof(NoDamage), "prophunt");
            Harmony.CreateAndPatchAll(typeof(BaseCoverManager), "prophunt");
            modEnabled = true;
            //Harmony.CreateAndPatchAll(typeof(AttackCooldownPatches), "prophunt");
        }

        public static void Unload()
        {
            Harmony.UnpatchID("prophunt");
            modEnabled = false;
        }

        bool IsInputDisabled()
        {
            // pause menu, inventory, etc
            if ((HeroController.instance?.IsInputBlocked() ?? false) && MovementState == MovementState.Normal) return true;
            //if (client.UiManager.ChatBox.IsOpen) return true;
            if (GameManager.SilentInstance?.GameState == GlobalEnums.GameState.MAIN_MENU) return true;

            // Prevent keybinds if chat window or other text input is up
            GameObject selection = EventSystem.current.currentSelectedGameObject;
            if (selection != null)
            {
                if (selection.GetComponent<InputField>()?.gameObject.activeInHierarchy ?? false) return true;
            }

            return false;
        }
        int logged = 0;
        private void Update()
        {
            if (!modEnabled) return;

            if (_nextFrames.Count > 0)
            {
                Log.LogInfo($"Executing {_nextFrames.Count} late actions");
                foreach (var action in _nextFrames)
                {
                    action.Invoke();
                }
                _nextFrames.Clear();
            }

            if (hornet.hornet != null)
            {
                hornet.EnsureHornetHidden();
            }

            // Prop movement
            if (hornet.hornet == null) return;
            movement?.Update();

            // No keybinds if inputs are blocked
            if (IsInputDisabled())
            {
                if (logged == 0) Log.LogInfo("Input blocked");
                logged = 1;
                return;
            } else if (logged == 1)
            {
                Log.LogInfo("Input restored");
                logged = 0;
            }

            // Effects testing
            if (Input.GetKeyDown(KeyCode.O))
            {
                //Utils.Networking.ClientNetwork.OnPropFound(new Utils.Networking.FromServer.PropFound
                //{
                //    IsClientFound = true,
                //    PropOwnerID = 0
                //});
                //EffectsManager.PlayConfetti();
                //if (Input.GetKey(KeyCode.LeftShift)) EffectsManager.PlayFoundSound(true);
                //else if (Input.GetKey(KeyCode.RightShift)) EffectsManager.PlayFoundSound(false);
                //else if (Input.GetKey(KeyCode.LeftControl))
                //{
                //    var data = new Utils.Networking.FromServer.GameOver { IsWinner = true, WasCanceled = false, WinnerUsername = "BobbyTC" };
                //    Utils.Networking.ClientNetwork.OnGameOver(data);
                //}
                //else if (Input.GetKey(KeyCode.RightControl))
                //{
                //    var data = new Utils.Networking.FromServer.GameOver { IsWinner = false, WasCanceled = false, WinnerUsername = "BobbyTC" };
                //    Utils.Networking.ClientNetwork.OnGameOver(data);
                //}
                //else
                //{
                //    var data = new Utils.Networking.FromServer.GameOver { IsWinner = false, WasCanceled = true, WinnerUsername = "Nobody" };
                //    Utils.Networking.ClientNetwork.OnGameOver(data);
                //}
            }

            /**************
             *  KEYBINDS  *
             **************/

            // TOGGLE VISIBILITY
            if (Input.GetKeyDown(Utils.Config.HideHornetKey))
            {
                hornet.ToggleHornet();
            }
            // SET PROP
            if (Input.GetKeyDown(Utils.Config.SwapPropKey))
            {
                cover.EnableProp();
            }
            if (Input.GetKeyDown(Utils.Config.ResetKey))
            {
                cover.DisableProp(hornet);
            }
        }
        void LateUpdate()
        {
            if (nextFrameActions.Count > 0)
            {
                Log.LogInfo($"Adding {nextFrameActions.Count} actions");
                _nextFrames = nextFrameActions.ToList();
                nextFrameActions.Clear();
            }
        }

        // Store previous scene while in transition
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof (TransitionPoint), "TryDoTransition")]
        //internal static void OnDoSceneTransition(TransitionPoint __instance)
        //{
        //    var gm = GameManager.instance;
        //    if (!gm || TransitionPoint.IsTransitionBlocked)
        //    {
        //        return;
        //    }

        //    var hc = HeroController.instance;
        //    if (gm.GameState == GlobalEnums.GameState.ENTERING_LEVEL)
        //    {
        //        if (__instance.GetGatePosition() != GlobalEnums.GatePosition.bottom || !hc.isHeroInPosition || hc.Body.linearVelocity.y >= 0f)
        //        {
        //            PreviousScene = __instance.targetScene;
        //            //PreviousGate = __instance.entryPoint;
        //        }
        //    }
        //}

        // Disable prop on scene change
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SceneLoad), "Begin")]
        internal static void OnSceneChange(SceneLoad __instance)
        {
            if (!modEnabled) return;
            if (GameManager.SilentInstance.GameState == GlobalEnums.GameState.MAIN_MENU)
            {
                //Log.LogInfo("Begin", GameManager.SilentInstance.GameState);
                return;
            }
            movement = new PropMovementControls();
            SelfCoverManager.instance.DisableProp(false, true);
            //Log.LogInfo($"Changing scene to {__instance.TargetSceneName}");
            PreviousScene = CurrentScene;
            CurrentScene = __instance.TargetSceneName;
            PropValidation.ResetProps();

            //foreach (var player in playerManager.Values)
            //{
            //    player.EnsurePropCover();
            //}
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameManager), "OnNextLevelReady")]
        internal static void OnNextLevelReady()
        {
            if (!modEnabled) return;
            if (GameManager.SilentInstance.GameState == GlobalEnums.GameState.MAIN_MENU)
            {
                //Log.LogInfo(GameManager.SilentInstance.GameState);
                return;
            }

            PropValidation.GetAllProps();

            if (PropHuntClient.GameState != GameState.NotStarted && !PropHuntClient.isSeeker)
            {
                if (SceneTransitionTicket != -1)
                {
                    ClientErrorCorrection.RestoreLastProp(new Utils.Networking.FromServer.FailedAction
                    {
                        AffectedID = 0,
                        BypassTicketID = SceneTransitionTicket,
                        FailedPacket = CustomPackets.PropSwap,
                        FixMethod = CorrectionActions.PreviousScene
                    });
                    SceneTransitionTicket = -1;
                }
                else
                {
                    SelfCoverManager.instance.EnableProp();
                }
            }

            //PlayerManager.EnsureAllPropCovers();
        }

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof (Breakable), "Break")]
        //public static void OnBreak(Breakable __instance)
        //{
        //    if (!modEnabled) return;
        //
        //    if (hornet == null) return;
        //    if (__instance.transform.parent.gameObject.name != hornet.hornet.name && !cover.IsCovered())
        //    {
        //        hornet.hornet.GetComponent<HeroController>().DamageSelf(1);
        //    }
        //}
    }
}