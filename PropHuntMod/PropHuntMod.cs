using BepInEx;
using HarmonyLib;
using PropHuntMod.Patches;
using PropHuntMod.Utils;
using PropHuntMod.Networking.Client;
using PropHuntMod.Networking.Server;
using SSMP.Api.Client;
using SSMP.Api.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using PropHuntMod.Props;
using PropHuntMod.Players;

#if DEBUG
using PropHuntMod.Tests;
#endif

/**
 * FEATURE LIST
 * Hide/Show Hornet
 * Spawn and attach a game object (prop) to hornet
 * Move the prop in x/y/z, 2d rotate as well
 * Dynamically get game objects in current scene
 * Slow down attacks (Currently disabled)
 * 
 * 
 * TODO:
 * Add more props other than breakable ones (corpses, enemies?, etc)
 * 
 */

namespace PropHuntMod
{
    using PlayerID = UInt16;

    [BepInPlugin("com.bobbythecatfish.prophunt", Utils.Config.ModName, Utils.Config.ModVersion)]
    [BepInDependency("ssmp")]
    [BepInDependency("io.github.flibber-hk.filteredlogs", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("Hollow Knight Silksong.exe")]
    public class PropHuntMod : BaseUnityPlugin
    {
        internal static PropHuntMod Instance;
        internal SelfHornetManager hornet = new SelfHornetManager();
        internal SelfCoverManager cover = new SelfCoverManager();
        //private static AttackCooldownPatches attackPatches = new AttackCooldownPatches(config);
        //private static readonly NoDamage noDamage = new NoDamage();
        private static PropMovementControls movement;
        internal static Dictionary<PlayerID, PlayerManager> playerManager = new Dictionary<PlayerID, PlayerManager>();
        internal static IClientApi client;
        internal static bool modEnabled = false;
        internal static bool showHitboxes = false;

        internal static List<Action> nextFrameActions = new List<Action>();
        static List<Action> _nextFrames = new List<Action>();

        void Awake()
        {
            Instance = this;
            Utils.Config.LoadConfig(Config);

            ClientAddon.RegisterAddon(new Client());
            ServerAddon.RegisterAddon(new Server());

            HeroController.OnHeroInstanceSet += (heroController) => AttackCooldownPatches.ModifySprintFSM();
        }
        public static void Initialize(IClientApi clientApi)
        {
            Log.LogInfo("Prop Hunt mod Loaded.");

            client = clientApi;

            Harmony.CreateAndPatchAll(typeof(PropHuntMod), "prophunt");
            Harmony.CreateAndPatchAll(typeof(NoDamage), "prophunt");
            Harmony.CreateAndPatchAll(typeof(BaseCoverManager), "prophunt");
            Harmony.CreateAndPatchAll(typeof(AttackCooldownPatches), "prophunt");
            Harmony.CreateAndPatchAll(typeof(ScenePatches), "prophunt");
            modEnabled = true;
            movement = new PropMovementControls();

#if DEBUG
            showHitboxes = true;
            TestManager.GetAllTests();
#endif
        }

        public static void Unload()
        {
            Harmony.UnpatchID("prophunt");
            modEnabled = false;
        }

        bool IsInputDisabled()
        {
            // pause menu, inventory, etc
            if ((HeroController.instance?.IsInputBlocked() ?? false) && PropMovementControls.MovementState == MovementState.Normal) return true;
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
            }
            else if (logged == 1)
            {
                Log.LogInfo("Input restored");
                logged = 0;
            }

#if DEBUG
            TestManager.Update();
            // Effects testing
            //if (Input.GetKeyDown(KeyCode.O))
            //{
                //GameManager.instance.cameraCtrl.FadeOut(GlobalEnums.CameraFadeType.JUST_FADE);
                //GameManager.instance.screenFader_fsm.SendEvent("SCENE FADE OUT");
                //AttackCooldownPatches.ModifySprintFSM();
                //PropTesting.PropNext();
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
            //}

            //if (Input.GetKeyDown(KeyCode.U))
            //{
                //GameManager.instance.cameraCtrl.FadeSceneIn();
                //GameManager.instance.screenFader_fsm.SendEvent("SCENE FADE IN");
                //PropTesting.PropPrevious();
            //}
#endif

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
                cover.EnableRandomProp();
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
    }
}