using PropHuntMod.Networking.Server;
using PropHuntMod.Patches;
using PropHuntMod.Players;
using PropHuntMod.Props;
using PropHuntMod.Utils;
using System;
using System.Collections;
using UnityEngine;

namespace PropHuntMod.Networking.Client
{
    internal static class ClientErrorCorrection
    {
        public static void DiagnoseError(FailedAction data)
        {
            switch (data.FixMethod)
            {
                case CorrectiveActions.None:
                    break;
                case CorrectiveActions.DisableClientProp: // requires ticket
                    SelfCoverManager.instance.DisableProp(false, false, data.BypassTicketID);
                    break;
                case CorrectiveActions.DisablePlayerProp:
                    DisablePlayerProp(data);
                    break;
                case CorrectiveActions.PreviousScene: // requires ticket
                    PropHuntMod.Instance.StartCoroutine(PreviousScene(data));
                    break;
                case CorrectiveActions.ToggleHornetTrue:
                    SelfHornetManager.instance.ToggleHornet(true);
                    break;
                case CorrectiveActions.ToggleHornetFalse: // requires ticket
                    SelfHornetManager.instance.ToggleHornet(false, data.BypassTicketID);
                    break;
                case CorrectiveActions.RestoreLastProp:
                    RestoreLastProp(data);
                    break;
                case CorrectiveActions.RestoreTriggerHandler:
                    RestoreTriggerHandler(data);
                    break;
                case CorrectiveActions.BecomeSeeker:
                    BecomeSeeker();
                    break;
                default:
                    throw new NotImplementedException(data.FixMethod.ToString());
            }
        }

        static void DisablePlayerProp(FailedAction data)
        {
            var player = PlayerManager.GetPlayerManager(data.AffectedID);
            player.SetProp("", "");
        }

        static IEnumerator PreviousScene(FailedAction data)
        {
            for (int i = 0; i < 5; i++)
            {
                if (!string.IsNullOrEmpty(ScenePatches.PreviousScene)) break;

                Log.LogWarning($"Attempt {i + 1} to find previous scene failed");
                yield return new WaitForSeconds(1);
            }

            if (string.IsNullOrEmpty(ScenePatches.PreviousScene))
            {
                PropHuntMod.client.ClientManager.Disconnect();
                Client.LocalMessage("Fatal Error: Unable to find previous scene.");
                yield break;
            }

            var loadInfo = new GameManager.SceneLoadInfo
            {
                SceneName = ScenePatches.PreviousScene,
                //EntryGateName = PropHuntMod.PreviousGate,
                PreventCameraFadeOut = true,
                WaitForSceneTransitionCameraFade = false,
                Visualization = GameManager.SceneLoadVisualizations.Default,
                AlwaysUnloadUnusedAssets = true,
                IsFirstLevelForPlayer = false
            };

            ScenePatches.SceneTransitionTicket = data.BypassTicketID;
            GameManager.instance.BeginSceneTransition(loadInfo);
        }

        public static void RestoreLastProp(FailedAction data)
        {
            var cm = SelfCoverManager.instance;
            var propObj = PropValidation.FindGameObject(cm.PrevCoverPath);
            
            var prop = PropValidation.PrepareProp(propObj);
            
            if (propObj == null || prop == null)
            {
                Log.LogFatal($"Couldn't restore previous prop {cm.PrevCoverPath}");
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
                    Log.LogDebug($"Player {data.AffectedID} still had their TriggerHandler. Not restoring.");
                }
            }
            else
            {
                Log.LogError($"Player {data.AffectedID}'s prop was already destroyed. Can't restore trigger handler");
            }
        }
        static void BecomeSeeker()
        {
            Client.isSeeker = true;
            if (Client.GameState == GameState.SeekerWait)
            {
                SelfHornetManager.instance.SetSeekerObscure(true);
            }
        }
    }
}
