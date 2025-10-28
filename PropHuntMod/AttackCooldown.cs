using GlobalEnums;
using HarmonyLib;
using System;
using System.Diagnostics;
using static PropHuntMod.Logging.Logging;

namespace PropHuntMod
{
    internal class AttackCooldownPatches
    {
        static ModConfiguration config;
        TraceListener listener = new ConsoleTraceListener();
        public AttackCooldownPatches(ModConfiguration configuration)
        {
            config = configuration;
            Trace.Listeners.Add(listener);
        }

        public static void SetAttackCooldown(ref float cooldown)
        {
            float time = config.attackCooldown.Value;
            cooldown = time;
            TempLog("cooled down");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HeroController), "CanAttack")]
        public static void Attack(HeroController __instance)
        {
            var stack = new StackTrace(true);
            Console.WriteLine(stack.ToString());
            //TempLog("ATTACK!");
        }

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(HeroController), "DidAttack")]
        //public static void DidAttack(HeroController __instance, ref float ___attack_cooldown) => SetAttackCooldown(__instance, ref ___attack_cooldown);

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(HeroController), "NeedleArtRecovery")]
        //public static void NeedleArtRecovery(HeroController __instance, ref float ___attack_cooldown) => SetAttackCooldown(__instance, ref ___attack_cooldown);

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(HeroController), "CrestAttackRecovery")]
        //public static void CrestAttackRecovery(HeroController __instance, ref float ___attack_cooldown) => SetAttackCooldown(__instance, ref ___attack_cooldown);

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(HeroController), "SetStartWithDashStabBounce")]
        //public static void SetStartWithDashStabBounce(HeroController __instance, ref float ___attack_cooldown) => SetAttackCooldown(__instance, ref ___attack_cooldown);

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(HeroController), "SetStartWithDownSpikeBounce")]
        //public static void SetStartWithDownSpikeBounce(HeroController __instance, ref float ___attack_cooldown) => SetAttackCooldown(__instance, ref ___attack_cooldown);

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(HeroController), "SetStartWithDownSpikeBounceSlightlyShort")]
        //public static void SetStartWithDownSpikeBounceSlightlyShort(HeroController __instance, ref float ___attack_cooldown) => SetAttackCooldown(__instance, ref ___attack_cooldown);

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(HeroController), "SetStartWithHarpoonBounce")]
        //public static void SetStartWithHarpoonBounce(HeroController __instance, ref float ___attack_cooldown) => SetAttackCooldown(__instance, ref ___attack_cooldown);

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(HeroController), "SetStartWithWitchSprintBounce")]
        //public static void SetStartWithWitchSprintBounce(HeroController __instance, ref float ___attack_cooldown) => SetAttackCooldown(__instance, ref ___attack_cooldown);

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(HeroController), "CanAttackAction")]
        //public static bool CanAttackAction(HeroController __instance, ref float ___attack_cooldown, HeroControllerStates ___cState, ref bool __result)
        //{
        //    TempLog(___attack_cooldown.ToString());
        //    TempLog(___cState.dashing.ToString());
        //    __result = ___attack_cooldown < 0;
        //    return true;
        //}
    }
}
