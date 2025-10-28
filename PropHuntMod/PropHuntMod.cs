using BepInEx;
using BepInEx.Logging;
using GlobalEnums;
using HarmonyLib;
using PropHuntMod.Keybinds;
using UnityEngine;
using static PropHuntMod.Logging.Logging;

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
        private static ModConfiguration config = new ModConfiguration();

        private void Awake()
        {
            
            TempLog("Prop Hunt Loaded.");
            config.LoadConfig(Config);
            Harmony.CreateAndPatchAll(typeof(PropHuntMod), null);
        }

        private void Update()
        {
            if (hornet.hornet != null)
            {
                hornet.EnsureHornetHidden();
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
		}

        // Instakill for hiders
        [HarmonyPrefix]
		[HarmonyPatch(typeof(HeroController), "TakeDamage")]
		public static void TakeDamage(HeroController __instance, GameObject go, CollisionSide damageSide, ref int damageAmount, HazardType hazardType, DamagePropertyFlags damagePropertyFlags = DamagePropertyFlags.None)
        {
            if (!config.disableDamage.Value) return;

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
            if (config.disableDamage.Value)
            {
                __instance.enabled = false;
                return true;
            }
            return false;
        }
		[HarmonyPrefix]
		[HarmonyPatch(typeof(AlertRange), "Awake")]
		public static bool Awake(AlertRange __instance)
        {
            if (config.disableDamage.Value)
            {
			    __instance.enabled = false;
			    return true;
            }
            return false;
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