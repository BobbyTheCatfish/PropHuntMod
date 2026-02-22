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
        #pragma warning disable IDE0060 // Remove unused parameter
        public static void TakeDamage(HeroController __instance, GameObject go, CollisionSide damageSide, ref int damageAmount, HazardType hazardType, DamagePropertyFlags damagePropertyFlags = DamagePropertyFlags.None)
        #pragma warning restore IDE0060 // Remove unused parameter
        {
            if (!Config.DisableDamage) return;

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
            if (Config.DisableDamage == true)
            {
                var isEnemy = alertRange.transform.parent.gameObject.layer == (int)PhysLayers.ENEMIES;
                if (isEnemy)
                {
                    alertRange.enabled = false;
                    return false;
                }
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

        // Prevent damage from props hiting cogs
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CogMultiHitter), "OnTriggerEnter2D")]
        public static bool CogDamage(CogMultiHitter __instance, Collider2D other)
        {
            Log.LogInfo("Cog doing damage");
            if (other.GetComponent<TriggerHandler>()) return false;

            return true;
        }
    }
}
