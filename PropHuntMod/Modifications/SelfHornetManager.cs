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
        public SelfHornetManager()
        {
            isRemote = false;
            instance = this;
        }
        public void ToggleHornet()
        {
            ToggleHornet(!shouldBeShown);
        }

        public override bool ToggleHornet(bool show)
        {
            if (PropHuntClient.isSeeker && !show)
            {
                PropHuntClient.LocalMessage("You're a seeker, you can't hide yourself!");
                return false;
            }
            var success = base.ToggleHornet(show);
            if (success)
            {
                ClientNetwork.SendHideStatus(!show);
                hornet.transform.Find("Charm Effects/White Circlet Light").gameObject.SetActive(show);
                hornet.transform.Find("Charm Effects/Wisp Lantern Light").gameObject.SetActive(show);
                hornet.transform.Find("HeroLight").gameObject.SetActive(show);
            }

            return success;
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
        }
    }
}
