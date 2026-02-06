using PropHuntMod.Modifications;
using PropHuntMod.Utils.Networking.FromServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PropHuntMod.Utils.Networking
{
    internal static class ClientErrorCorrection
    {
        public static void DiagnoseError(FailedAction data)
        {
            switch (data.FixMethod)
            {
                case CorrectionActions.None:
                    break;
                case CorrectionActions.DisableClientProp: // requires ticket
                    SelfCoverManager.instance.DisableProp(false, false, data.BypassTicketID);
                    break;
                case CorrectionActions.DisablePlayerProp:
                    DisablePlayerProp(data);
                    break;
                case CorrectionActions.PreviousScene: // requires ticket
                    PropHuntMod.Instance.StartCoroutine(PreviousScene(data));
                    break;
                case CorrectionActions.ToggleHornetTrue:
                    SelfHornetManager.instance.ToggleHornet(true);
                    break;
                case CorrectionActions.ToggleHornetFalse: // requires ticket
                    SelfHornetManager.instance.ToggleHornet(false, data.BypassTicketID);
                    break;
                case CorrectionActions.RestoreLastProp:
                    RestoreLastProp(data);
                    break;
                case CorrectionActions.RestoreTriggerHandler:
                    RestoreTriggerHandler(data);
                    break;
                case CorrectionActions.BecomeSeeker:
                    BecomeSeeker(data);
                    break;
                default:
                    throw new NotImplementedException(data.FixMethod.ToString());
            }
        }

        static void DisablePlayerProp(FailedAction data)
        {
            var player = PlayerManager.GetPlayerManager(data.AffectedID);
            player.SetProp("");
        }

        static IEnumerator PreviousScene(FailedAction data)
        {
            for (int i = 0; i < 5; i++)
            {
                if (!string.IsNullOrEmpty(PropHuntMod.PreviousScene)) break;

                Log.LogInfo($"Attempt {i + 1} to find previous scene failed");
                yield return new WaitForSeconds(1);
            }

            if (string.IsNullOrEmpty(PropHuntMod.PreviousScene))
            {
                PropHuntMod.client.ClientManager.Disconnect();
                PropHuntClient.LocalMessage("Fatal Error: Unable to find previous scene.");
                yield break;
            }

            var loadInfo = new GameManager.SceneLoadInfo
            {
                SceneName = PropHuntMod.PreviousScene,
                //EntryGateName = PropHuntMod.PreviousGate,
                PreventCameraFadeOut = true,
                WaitForSceneTransitionCameraFade = false,
                Visualization = GameManager.SceneLoadVisualizations.Default,
                AlwaysUnloadUnusedAssets = true,
                IsFirstLevelForPlayer = false
            };

            PropHuntMod.SceneTransitionTicket = data.BypassTicketID;
            GameManager.instance.BeginSceneTransition(loadInfo);
        }

        public static void RestoreLastProp(FailedAction data)
        {
            var cm = SelfCoverManager.instance;
            var prop = PropValidation.currentSceneObjects.GetSpecific(go => go.name == cm.prevCover);
            
            if (prop == null)
            {
                Log.LogFatal($"Couldn't restore previous prop {cm.prevCover}");
                return;
            }

            SelfCoverManager.instance.EnableProp(prop, data.BypassTicketID);
        }

        static void RestoreTriggerHandler(FailedAction data)
        {
            var player = PlayerManager.GetPlayerManager(data.AffectedID);
            var cover = player.coverManager.cover;
            
            if (cover != null)
            {
                var trigger = cover.GetComponent<TriggerHandler>();
                if (trigger == null)
                {
                    trigger = cover.AddComponent<TriggerHandler>();
                    trigger.isRemote = true;
                    trigger.playerID = data.AffectedID;
                }
                else
                {
                    Log.LogInfo($"Player {data.AffectedID} still had their TriggerHandler. Not restoring.");
                }
            }
            else
            {
                Log.LogError($"Player {data.AffectedID}'s prop was already destroyed. Can't restore trigger handler");
            }
        }
        static void BecomeSeeker(FailedAction data)
        {
            PropHuntClient.isSeeker = true;
            if (PropHuntClient.GameState == GameState.SeekerWait)
            {
                SelfHornetManager.instance.SetSeekerObscure(true);
            }
        }
    }
}
