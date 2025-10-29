using GlobalEnums;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PropHuntMod.Modifications
{
    internal class NoDamage
    {
        static ModConfiguration config;
        static CoverManager cover;
        public NoDamage(ModConfiguration configuration, CoverManager coverManager)
        {
            config = configuration;
            cover = coverManager;
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
    }
}
