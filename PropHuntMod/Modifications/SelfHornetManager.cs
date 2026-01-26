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

        public new bool ToggleHornet(bool show)
        {
            var success = base.ToggleHornet(show);
            if (success) ClientNetwork.SendHideStatus(!show);

            return success;
        }

        public override void SetHornet()
        {
            hornet = GameObject.FindGameObjectWithTag("Player");
            if (hornet == null)
            {
                Debug.LogError("SELF HORNET NOT FOUND");
                return;
            }
            render = hornet.GetComponent<MeshRenderer>();
        }
    }
}
