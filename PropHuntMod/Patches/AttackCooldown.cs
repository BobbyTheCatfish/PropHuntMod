using GlobalEnums;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using PropHuntMod.Utils;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace PropHuntMod.Patches
{
    class CheckAttackCooldown : FsmStateAction
    {
        public FsmOwnerDefault gameObject;
        public FsmEventTarget eventTarget;
        public FsmEvent positiveEvent;
        public FsmEvent negativeEvent;
        public override void OnEnter()
        {
            GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(gameObject);
            if (!(ownerDefaultTarget == null))
            {
                if (AttackCooldownPatches.CanDoAttack())
                {
                    base.Fsm.Event(eventTarget, negativeEvent);
                }
                else
                {
                    Utils.Log.LogDebug($"Can't do attack, doing {positiveEvent.Name}");
                    base.Fsm.Event(eventTarget, positiveEvent);
                }

                Finish();
            }
        }
    }
    internal class AttackCooldownPatches
    {
        static bool CanAttack = true;
        static IEnumerator ResetCooldown()
        {
            CanAttack = false;
            
            if (Config.AttackCooldown > 0)
            {
                yield return new WaitForSeconds(Config.AttackCooldown);
            }

            CanAttack = true;
        }
        internal static bool CanDoAttack()
        {
            if (!CanAttack) return false;

            PropHuntMod.Instance.StartCoroutine(ResetCooldown());
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HeroController), "Attack")]
        public static bool Attack(HeroController __instance, AttackDirection attackDir)
        {
            // This is used for platforming, can't really take it away
            if (attackDir == AttackDirection.downward)
            {
                return true;
            }

            return CanDoAttack();
        }

        public static void ModifySprintFSM()
        {
            Log.LogDebug("Modifying Sprint FSM");

            var fsm = HeroController.instance.gameObject.GetComponents<PlayMakerFSM>().First(f => f.Fsm.Name == "Sprint").Fsm;

            // Check for prior modifications
            if (fsm.Events.Any(e => e.Name == "SEEKER DELAY")) return;

            // Create new event
            var delayEvent = new FsmEvent("SEEKER DELAY");
            delayEvent.IsGlobal = false;
            delayEvent.IsSystemEvent = false;

            fsm.Events = fsm.Events.Append(delayEvent).ToArray();            

            // Grab the Dash Stab Dir state
            // This one happens right before doing the attacking
            var dashStabDir = fsm.States.FirstOrDefault(s => s.Name == "Dash Stab Dir");

            // Grab the Continue Sprint? state
            // This one happens after the attack finishes
            var continueSprint = fsm.States.FirstOrDefault(s => s.Name == "Continue Sprint?");

            // Create a transition from Dash Stab Dir to Continue Sprint?
            var transition = new FsmTransition();
            transition.FsmEvent = delayEvent;
            transition.ToFsmState = continueSprint;
            transition.LinkStyle = FsmTransition.CustomLinkStyle.Default;
            transition.LinkConstraint = FsmTransition.CustomLinkConstraint.None;
            transition.LinkTarget = FsmTransition.CustomLinkTarget.None;

            dashStabDir.Transitions = dashStabDir.Transitions.Append(transition).ToArray();

            // Grab the SendEventByScale action as a reference for the target and owner properties
            SendEventByScale reference = dashStabDir.Actions.First(a => a is SendEventByScale) as SendEventByScale;

            // Create and add the new action for the new transition
            var action = new CheckAttackCooldown
            {
                gameObject = reference.gameObject,
                eventTarget = reference.eventTarget,
                positiveEvent = delayEvent,
                Enabled = true,
                IsOpen = true
            };

            dashStabDir.Actions = dashStabDir.Actions.Prepend(action).ToArray();
            dashStabDir.SaveActions();
        }
    }
}
