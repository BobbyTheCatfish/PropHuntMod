using BepInEx;
using BepInEx.Logging;
using GlobalEnums;
using HarmonyLib;
using PropHuntMod.Modifications;
using UnityEngine;
using Steamworks;
using System.Collections.Generic;

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

        bool packetsPatched = false;
        internal static HornetManager hornet = new HornetManager();
        internal static CoverManager cover = new CoverManager();
        private static ModConfiguration config = new ModConfiguration();
        //private static AttackCooldownPatches attackPatches = new AttackCooldownPatches(config);
        private static NoDamage noDamage = new NoDamage(config, cover);
        internal static Dictionary<CSteamID, PlayerManager> playerManager = new Dictionary<CSteamID, PlayerManager>();
        static HeroController heroController;

        private void Awake()
        {
            Log = base.Logger;
            Log.LogInfo("Prop Hunt Loaded.");
            config.LoadConfig(Config);
            Harmony.CreateAndPatchAll(typeof(PropHuntMod), null);
            Harmony.CreateAndPatchAll(typeof(NoDamage), null);
            Harmony.CreateAndPatchAll(typeof(CoverManager), null);
            //Harmony.CreateAndPatchAll(typeof(AttackCooldownPatches), null);
        }

        private void Update()
        {
            //if (!packetsPatched)
            //{
            //    //Log.LogInfo("IS THIS WORKING???");
            //    //Harmony.CreateAndPatchAll(typeof(Utils.Networking.PacketReciept), null);
            //    packetsPatched = true;
            //}
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
            if (Input.GetKeyDown(config.hideHornetKey.Value))
            {
				hornet.SetHornet();
				hornet.ToggleHornet();
            }
            // SET PROP
            if (Input.GetKeyDown(config.swapPropKey.Value))
            {
                hornet.SetHornet();
                cover.EnableProp(hornet);
            }
            if (Input.GetKeyDown(config.resetKey.Value))
            {
                cover.DisableProp(hornet);
            }

            cover.MoveProp(Direction.Down, KeyCode.Keypad2);
            cover.MoveProp(Direction.Left, KeyCode.Keypad4);
            cover.MoveProp(Direction.Right, KeyCode.Keypad6);
            cover.MoveProp(Direction.Up, KeyCode.Keypad8);
            cover.MoveProp(Direction.Front, KeyCode.Keypad7);
            cover.MoveProp(Direction.Back, KeyCode.Keypad9);

            if (
                !Input.GetKey(KeyCode.Keypad2) && !Input.GetKey(KeyCode.Keypad4) &&
                !Input.GetKey(KeyCode.Keypad6) && !Input.GetKey(KeyCode.Keypad8) &&
                !Input.GetKey(KeyCode.Keypad7) && !Input.GetKey(KeyCode.Keypad9)
                )
            {
                cover.SendPropPosition();
            }
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