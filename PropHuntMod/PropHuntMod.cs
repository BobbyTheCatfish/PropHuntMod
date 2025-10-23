using BepInEx;
using BepInEx.Logging;
using GenericVariableExtension;
using GlobalEnums;
using HarmonyLib;
using HarmonyLib.Tools;
using PropHuntMod.Keybinds;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static PropHuntMod.Logging.Logging;
using HutongGames.PlayMaker.Actions;

/**
 * FEATURE LIST
 * Hide/Show Hornet
 * Spawn and attach a game object (prop) to hornet
 * Move the prop in x/y/z
 * Dynamically get game objects in current scene
 *
 * 
 * TODO:
 * Add more props other than breakable ones (corpses, enemies?, etc)
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
    
        static HornetManager hornet = new HornetManager();
        static CoverManager cover = new CoverManager();

        private void Awake()
        {
            TempLog("Prop Hunt Loaded.");
            Harmony.CreateAndPatchAll(typeof(PropHuntMod), null);
        }

        private void Update()
        {
            if (hornet.hornet != null)
            {
                hornet.EnsureHornetHidden();
            }
            // TOGGLE VISIBILITY
            if (Input.GetKeyDown(KeyCode.H))
            {
				hornet.SetHornet();
				hornet.ToggleHornet();
            }
            // SET PROP
            if (Input.GetKeyDown(KeyCode.P))
            {
                hornet.SetHornet();
                cover.EnableProp(hornet);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                cover.DisableProp(hornet);
            }
            if (Input.GetKey(KeyCode.Keypad2)) cover.MoveProp(Direction.Down);
            if (Input.GetKey(KeyCode.Keypad4)) cover.MoveProp(Direction.Left);
            if (Input.GetKey(KeyCode.Keypad6)) cover.MoveProp(Direction.Right);
            if (Input.GetKey(KeyCode.Keypad8)) cover.MoveProp(Direction.Up);
			if (Input.GetKey(KeyCode.Keypad7)) cover.MoveProp(Direction.Front);
			if (Input.GetKey(KeyCode.Keypad9)) cover.MoveProp(Direction.Back);
		}

        // Instakill for hiders
        [HarmonyPrefix]
		[HarmonyPatch(typeof(HeroController), "TakeDamage")]
		public static void TakeDamage(HeroController __instance, GameObject go, CollisionSide damageSide, ref int damageAmount, HazardType hazardType, DamagePropertyFlags damagePropertyFlags = DamagePropertyFlags.None)
        {
            //if (go.name == "Bone Goomba") // Used for testing
            if (go.tag == "Player" && cover != null)
            {
                damageAmount = 9000;
            }
            else if (hazardType == HazardType.ENEMY)
            {
                damageAmount = 0;
            }
            //TempLog(go.name);
        }

        // Prevent detection by enemies
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AlertRange), "OnEnable")]
        public static bool OnEnable(AlertRange __instance)
        {
            __instance.enabled = false;
            return true;
        }
		[HarmonyPrefix]
		[HarmonyPatch(typeof(AlertRange), "Awake")]
		public static bool Awake(AlertRange __instance)
        {
			__instance.enabled = false;
			return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof (SceneLoad), "Begin")]
        public static void OnSceneChange(SceneLoad __instance)
        {
            cover.DisableProp(hornet);
            TempLog($"Changing scene to {__instance.TargetSceneName}");
        }
	}
}