using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using PropHuntMod.Modifications;
using UnityEngine;
using Steamworks;
using System.Collections.Generic;
using PropHuntMod.Utils.Networking;

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
    [BepInPlugin("com.bobbythecatfish.prophunt", "Prop Hunt", "0.1.0")]
    [BepInProcess("Hollow Knight Silksong.exe")]
    public class PropHuntMod : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        internal static HornetManager hornet = new HornetManager();
        internal static CoverManager cover = new CoverManager();
        //private static AttackCooldownPatches attackPatches = new AttackCooldownPatches(config);
        private static NoDamage noDamage = new NoDamage(cover);
        internal static Dictionary<CSteamID, PlayerManager> playerManager = new Dictionary<CSteamID, PlayerManager>();
        static HeroController heroController;

        private void Awake()
        {
            Log = base.Logger;
            Log.LogInfo("Prop Hunt Loaded.");
            Utils.Config.LoadConfig(this.Config);
            Harmony.CreateAndPatchAll(typeof(PropHuntMod), null);
            Harmony.CreateAndPatchAll(typeof(NoDamage), null);
            Harmony.CreateAndPatchAll(typeof(CoverManager), null);
            CustomPacketHandlers.Init();
            //Harmony.CreateAndPatchAll(typeof(AttackCooldownPatches), null);
        }

        private void Update()
        {
            if (hornet.hornet != null)
            {
                hornet.EnsureHornetHidden();
            }

            if (heroController == null)
            {
                heroController = FindFirstObjectByType<HeroController>();
            }

            // No keybinds if inputs are blocked
            if (heroController != null)
            {
                if (heroController.IsInputBlocked()) return;
            }

            // TOGGLE VISIBILITY
            if (Input.GetKeyDown(Utils.Config.hideHornetKey))
            {
				hornet.SetHornet();
				hornet.ToggleHornet();
            }
            // SET PROP
            if (Input.GetKeyDown(Utils.Config.swapPropKey))
            {
                hornet.SetHornet();
                cover.EnableProp(hornet);
            }
            if (Input.GetKeyDown(Utils.Config.resetKey))
            {
                cover.DisableProp(hornet);
            }

            cover.MoveOwnProp(Direction.Down, KeyCode.Keypad2);
            cover.MoveOwnProp(Direction.Left, KeyCode.Keypad4);
            cover.MoveOwnProp(Direction.Right, KeyCode.Keypad6);
            cover.MoveOwnProp(Direction.Up, KeyCode.Keypad8);
            cover.MoveOwnProp(Direction.Front, KeyCode.Keypad7);
            cover.MoveOwnProp(Direction.Back, KeyCode.Keypad9);
            cover.MoveOwnProp(Direction.Reset, KeyCode.Keypad5);

            if (
                !Input.GetKey(KeyCode.Keypad2) && !Input.GetKey(KeyCode.Keypad4) &&
                !Input.GetKey(KeyCode.Keypad6) && !Input.GetKey(KeyCode.Keypad8) &&
                !Input.GetKey(KeyCode.Keypad7) && !Input.GetKey(KeyCode.Keypad9)
                )
            {
                cover.SendPropPosition();
            }

            if (Input.GetKeyDown(KeyCode.Slash)) cover.ViewCoverComponents();
		}

        // Disable prop on scene change
        [HarmonyPrefix]
        [HarmonyPatch(typeof (SceneLoad), "Begin")]
        public static void OnSceneChange(SceneLoad __instance)
        {
            cover.DisableProp(hornet);
            Log.LogInfo($"Changing scene to {__instance.TargetSceneName}");
            cover.currentScene = __instance.TargetSceneName;
            cover.applicableCovers = null;

            //foreach (var player in playerManager.Values)
            //{
            //    player.EnsurePropCover();
            //}
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof (GameManager), "OnNextLevelReady")]
        static void OnNextLevelReady()
        {
            Debug.Log($"Ensuring cover for {playerManager.Count} players");
            foreach (var player in playerManager.Values)
            {
                player.EnsurePropCover();
            }
        }

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof (Breakable), "Break")]
        //public static void OnBreak(Breakable __instance)
        //{
        //    if (hornet == null) return;
        //    if (__instance.transform.parent.gameObject.name != hornet.hornet.name && !cover.IsCovered())
        //    {
        //        hornet.hornet.GetComponent<HeroController>().DamageSelf(1);
        //    }
        //}
    }
}