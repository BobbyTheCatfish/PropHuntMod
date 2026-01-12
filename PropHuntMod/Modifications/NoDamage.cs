using GlobalEnums;
using HarmonyLib;
using UnityEngine;
using PropHuntMod.Utils;

namespace PropHuntMod.Modifications
{
    internal class NoDamage
    {
        static BaseCoverManager cover;
        static readonly bool INSTA_KILL = false;
        public NoDamage(BaseCoverManager coverManager)
        {
            cover = coverManager;
        }

        // Instakill for hiders
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HeroController), "TakeDamage")]
        public static void TakeDamage(HeroController __instance, GameObject go, CollisionSide damageSide, ref int damageAmount, HazardType hazardType, DamagePropertyFlags damagePropertyFlags = DamagePropertyFlags.None)
        {
            if (!Config.disableDamage) return;

            //if (go.name == "Bone Goomba") // Used for testing
            if (go.tag == "Player" && cover.IsCovered() && INSTA_KILL)
            {
                damageAmount = 9000;
            }
            else if (hazardType == HazardType.ENEMY)
            {
                damageAmount = 0;
            }
            //Log.LogInfo(go.name);
        }

        // Prevent detection by enemies
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AlertRange), "OnEnable")]
        public static bool OnEnable(AlertRange __instance)
        {
            if (Config.disableDamage == true)
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
            if (Config.disableDamage == true)
            {
                __instance.enabled = false;
                return true;
            }
            return false;
        }
    }
}
