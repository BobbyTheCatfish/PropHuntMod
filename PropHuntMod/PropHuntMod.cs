using BepInEx;
using HarmonyLib;
using PropHuntMod.Modifications;
//using PropHuntMod.Utils.Networking;
using PropHuntMod.Utils;
using SSMP.Api.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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
 *  - Find out which player is which
 *  - Send prop information packets
 * 
 */

namespace PropHuntMod
{
    using PlayerID = UInt16;
    [BepInPlugin("com.bobbythecatfish.prophunt", Utils.Config.ModName, Utils.Config.ModVersion)]
    [BepInDependency("ssmp")]
    [BepInProcess("Hollow Knight Silksong.exe")]
    public class PropHuntMod : BaseUnityPlugin
    {
        internal SelfHornetManager hornet = new SelfHornetManager();
        internal SelfCoverManager cover = new SelfCoverManager();
        //private static AttackCooldownPatches attackPatches = new AttackCooldownPatches(config);
        private static NoDamage noDamage = new NoDamage();
        internal static Dictionary<PlayerID, PlayerManager> playerManager = new Dictionary<PlayerID, PlayerManager>();
        HeroController heroController => HeroController.instance;
        internal static IClientApi client;
        internal static bool modEnabled = false;

        internal static bool showHitboxes = false;

        void Awake()
        {
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

        private void Update()
        {
            if (!modEnabled) return;

            if (hornet.hornet != null)
            {
                hornet.EnsureHornetHidden();
            }

            // No keybinds if inputs are blocked
            if (heroController != null)
            {
                if (heroController.IsInputBlocked()) return;
            }


            /**************
             *  KEYBINDS  *
             **************/

            // Prevent keybinds if chat window or other text input is up
            GameObject selection = EventSystem.current.currentSelectedGameObject;
            if (selection != null)
            {
                if (selection.GetComponent<InputField>()?.gameObject.activeInHierarchy ?? false) return;
            }

            // TOGGLE VISIBILITY
            if (Input.GetKeyDown(Utils.Config.hideHornetKey))
            {
                hornet.ToggleHornet();
            }
            // SET PROP
            if (Input.GetKeyDown(Utils.Config.swapPropKey))
            {
                cover.EnableProp();
            }
            if (Input.GetKeyDown(Utils.Config.resetKey))
            {
                cover.DisableProp(hornet);
            }

            // Prop movement
            {
                if (hornet.hornet == null) return;

                cover.MoveProp(Direction.Down, KeyCode.Keypad2);
                cover.MoveProp(Direction.Left, KeyCode.Keypad4);
                cover.MoveProp(Direction.Right, KeyCode.Keypad6);
                cover.MoveProp(Direction.Up, KeyCode.Keypad8);
                cover.MoveProp(Direction.Front, KeyCode.Keypad7);
                cover.MoveProp(Direction.Back, KeyCode.Keypad9);
                cover.MoveProp(Direction.Reset, KeyCode.Keypad5, true);

                cover.MoveProp(Direction.RotateLeft, KeyCode.Keypad1);
                cover.MoveProp(Direction.RotateRight, KeyCode.Keypad3);
            }

            if (
                !Input.GetKey(KeyCode.Keypad2) && !Input.GetKey(KeyCode.Keypad4) &&
                !Input.GetKey(KeyCode.Keypad6) && !Input.GetKey(KeyCode.Keypad8) &&
                !Input.GetKey(KeyCode.Keypad7) && !Input.GetKey(KeyCode.Keypad9) &&
                !Input.GetKey(KeyCode.Keypad1) && !Input.GetKey(KeyCode.Keypad3)
                )
            {
                cover.SendPropPosition();
            }
        }

        // Disable prop on scene change
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SceneLoad), "Begin")]
        internal static void OnSceneChange(SceneLoad __instance)
        {
            if (!modEnabled) return;
            if (GameManager.instance.GameState == GlobalEnums.GameState.MAIN_MENU)
            {
                Log.LogInfo("Begin", GameManager.instance.GameState);
                return;
            }
            SelfCoverManager.instance.DisableProp(false, true);
            Log.LogInfo($"Changing scene to {__instance.TargetSceneName}");
            //cover.currentScene = __instance.TargetSceneName;
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
            if (GameManager.instance.GameState == GlobalEnums.GameState.MAIN_MENU)
            {
                Log.LogInfo(GameManager.instance.GameState);
                return;
            }

            PropValidation.GetAllProps();

            if (PropHuntClient.roundStarted && !PropHuntClient.isSeeker)
            {
                SelfCoverManager.instance.EnableProp();
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