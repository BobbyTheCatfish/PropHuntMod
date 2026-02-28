using PropHuntMod.Utils;
using PropHuntMod.Networking.Client;
using UnityEngine;
using PropHuntMod.Props;
using HutongGames.PlayMaker.Actions;

namespace PropHuntMod.Players
{
    internal class InputBlocker { }
    internal class SelfHornetManager : BaseHornetManager
    {
        static InputBlocker blocker = new InputBlocker();
        public static SelfHornetManager instance;
        SpriteRenderer obscurance;
        public SelfHornetManager()
        {
            isRemote = false;
            instance = this;
        }
        public void ToggleHornet()
        {
            if (!Config.AllowDebugFeatures && !SelfCoverManager.instance.IsHiding) return;
            
            ToggleHornet(!shouldBeShown);
        }

        public bool ToggleHornet(bool show, int ticket = -1)
        {
            if (Client.isSeeker && !show)
            {
                Client.LocalMessage("You're a seeker, you can't hide yourself!");
                return false;
            }
            var success = base.ToggleHornet(show);
            if (success)
            {
                if (Config.AllowDebugFeatures) ClientNetwork.SendHideStatus(!show, ticket);

                hornet.transform.Find("Charm Effects/White Circlet Light").gameObject.SetActive(show);
                hornet.transform.Find("Charm Effects/Wisp Lantern Light").gameObject.SetActive(show);
                hornet.transform.Find("HeroLight").gameObject.SetActive(show);
            }

            return success;

        }

        public override bool ToggleHornet(bool show)
        {
            return ToggleHornet(show, -1);
        }

        public override void SetHornet()
        {
            hornet = GameObject.Find("Hero_Hornet(Clone)");
            if (hornet == null)
            {
                Debug.LogError("SELF HORNET NOT FOUND");
                return;
            }
            render = hornet.GetComponent<MeshRenderer>();

            HeroController.instance.OnDeath -= OnDeath;
            HeroController.instance.OnDeath += OnDeath;
        }

        public void SetSeekerObscure(bool enabled)
        {
            if (obscurance == null)
            {
                var ogFader = GameCameras.instance.hudCamera.transform.Find("In-game/Screen Fader").gameObject;
                var fader = GameObject.Instantiate(ogFader, ogFader.transform.parent);

                Component.Destroy(fader.GetComponent<ScreenFaderState>());
                obscurance = fader.GetComponent<SpriteRenderer>();

                obscurance.sortingLayerName = "Vignette";
                obscurance.sortingOrder = 0;
            }
            if (enabled)
            {
                HeroController.instance.AddInputBlocker(blocker);
                obscurance.color = new Color(0, 0, 0, 1);
                //GameManager.instance.screenFader_fsm.SendEvent("SCENE FADE OUT INSTANT");
            }
            else
            {
                HeroController.instance.RemoveInputBlocker(blocker);
                obscurance.color = new Color(0, 0, 0, 0);
                //GameManager.instance.screenFader_fsm.SendEvent("SCENE FADE IN");
            }
        }

        public void OnDeath()
        {
            SelfCoverManager.instance.DisableProp(false, false);
            Log.LogDebug("I ded");
        }
    }
}
