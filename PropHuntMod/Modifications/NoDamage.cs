using GlobalEnums;
using HarmonyLib;
using UnityEngine;
using PropHuntMod.Utils;

namespace PropHuntMod.Modifications
{
    internal class NoDamage
    {
        static readonly bool INSTA_KILL = false;

        // Instakill for hiders
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HeroController), "TakeDamage")]
        public static void TakeDamage(HeroController __instance, GameObject go, CollisionSide damageSide, ref int damageAmount, HazardType hazardType, DamagePropertyFlags damagePropertyFlags = DamagePropertyFlags.None)
        {
            if (!Config.disableDamage) return;

            //if (go.name == "Bone Goomba") // Used for testing
            if (go.tag == "Player" && SelfCoverManager.instance.IsHiding && INSTA_KILL)
            {
                damageAmount = 9000;
            }
            else if (hazardType == HazardType.ENEMY)
            {
                damageAmount = 0;
            }
            //Log.LogInfo(go.name);
        }

        static bool DisableAlertRange(AlertRange alertRange)
        {
            if (Config.disableDamage == true)
            {
                alertRange.enabled = false;
                return false;
            }
            return true;
        }

        // Prevent detection by enemies
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AlertRange), "OnEnable")]
        public static bool OnEnable(AlertRange __instance)
        {
            return DisableAlertRange(__instance);
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AlertRange), "Awake")]
        public static bool Awake(AlertRange __instance)
        {
            return DisableAlertRange(__instance);
        }
    }
}
