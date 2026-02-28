
using HarmonyLib;
using PropHuntMod.Networking.Client;
using PropHuntMod.Networking.Server;
using PropHuntMod.Props;
using PropHuntMod.Utils;

namespace PropHuntMod.Patches
{
    internal class ScenePatches
    {
        internal static string CurrentScene;
        internal static string PreviousScene;
        internal static int SceneTransitionTicket = -1;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SceneLoad), "Begin")]
        internal static void OnSceneChange(SceneLoad __instance)
        {
            if (!PropHuntMod.modEnabled) return;
            if (GameManager.SilentInstance.GameState == GlobalEnums.GameState.MAIN_MENU)
            {
                //Log.LogInfo("Begin", GameManager.SilentInstance.GameState);
                return;
            }
            SelfCoverManager.instance.DisableProp(false, true);

            //Log.LogInfo($"Changing scene to {__instance.TargetSceneName}");
            PreviousScene = CurrentScene;
            CurrentScene = __instance.TargetSceneName;
            PropValidation.ResetProps();

            //foreach (var player in playerManager.Values)
            //{
            //    player.EnsurePropCover();
            //}
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameManager), "OnNextLevelReady")]
        internal static void OnNextLevelReady()
        {
            if (!PropHuntMod.modEnabled) return;
            if (GameManager.SilentInstance.GameState == GlobalEnums.GameState.MAIN_MENU)
            {
                //Log.LogInfo(GameManager.SilentInstance.GameState);
                return;
            }

            PropValidation.GetAllProps();
#if DEBUG
            PropTesting.OnSceneChange();
#endif

            if (Client.GameState != GameState.NotStarted && !Client.isSeeker)
            {
                if (SceneTransitionTicket != -1)
                {
                    ClientErrorCorrection.RestoreLastProp(new FailedAction
                    {
                        AffectedID = 0,
                        BypassTicketID = SceneTransitionTicket,
                        FailedPacket = CustomPackets.PropSwap,
                        FixMethod = CorrectiveActions.PreviousScene
                    });
                    SceneTransitionTicket = -1;
                }
                else
                {
                    SelfCoverManager.instance.EnableRandomProp();
                }
            }

            //PlayerManager.EnsureAllPropCovers();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Breakable), "Break")]
        public static void OnBreak(Breakable __instance)
        {
            //if (!modEnabled) return;

            //var newObj = GameObject.Instantiate(__instance.gameObject, __instance.transform.parent);
            //newObj.name = __instance.name;

            //newObj.SetActive(false);

            //if (hornet == null) return;
            //if (__instance.transform.parent.gameObject.name != hornet.hornet.name && !cover.IsCovered())
            //{
            //    hornet.hornet.GetComponent<HeroController>().DamageSelf(1);
            //}
        }
    }
}
