using PropHuntMod.Utils;
using PropHuntMod.Utils.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PropHuntMod.Modifications
{
    internal class SelfHornetManager : BaseHornetManager
    {
        public static SelfHornetManager instance;
        GameObject obscurance;
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
            if (PropHuntClient.isSeeker && !show)
            {
                PropHuntClient.LocalMessage("You're a seeker, you can't hide yourself!");
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
                var cam = GameCameras.instance.tk2dCam;
                obscurance = new GameObject("SEEKER OBSCURANCE");
                obscurance.transform.SetParentReset(cam.transform);
                obscurance.transform.SetScale2D(new Vector2(100, 100));
                obscurance.transform.SetLocalPositionZ(10);

                var sprite = obscurance.AddComponent<SpriteRenderer>();
                var copySprite = cam.transform.Find("Masker Blackout").GetComponent<SpriteRenderer>();
                sprite.sprite = copySprite.sprite;
                sprite.material = copySprite.material;

                obscurance.SetActive(false);
            }
            if (enabled)
            {
                HeroController.instance.AddInputBlocker(obscurance);
                obscurance.SetActive(true);
            }
            else
            {
                HeroController.instance.RemoveInputBlocker(obscurance);
                obscurance.SetActive(false);
            }
                //if (!HornetExists()) return;
            //    Vector3 rotation = GameCameras.instance.transform.rotation.eulerAngles;
            //rotation.y = enabled ? 180 : 0;
            //GameCameras.instance.transform.rotation = Quaternion.Euler(rotation);
        }

        public void OnDeath()
        {
            SelfCoverManager.instance.DisableProp(false, false);
            Log.LogDebug("I ded");
        }
    }
}
