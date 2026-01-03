using System;
using UnityEngine;
using PropHuntMod.Utils.Networking;
using static PropHuntMod.Logging.Logging;

namespace PropHuntMod.Modifications
{
    public class HornetManager
    {
        bool shouldBeShown;
        public GameObject hornet;
        MeshRenderer render;
        public void ToggleHornet()
        {
            TempLog("Toggling Hornet");
            if (hornet == null) SetHornet();

            var render = hornet.GetComponent<MeshRenderer>();
            PacketSend.SendHideStatus(render.enabled);

            render.enabled = !render.enabled;
            shouldBeShown = render.enabled;

        }

        public void ToggleHornet(bool show)
        {
            if (hornet == null) SetHornet();

            var render = hornet.GetComponent<MeshRenderer>();
            render.enabled = show;
            shouldBeShown = show;

            PacketSend.SendHideStatus(!show);
        }

        public void EnsureHornetHidden()
        {
            if (hornet == null) SetHornet();
            if (shouldBeShown) return;

            if (render.enabled == true) render.enabled = false;
        }

        public void SetHornet()
        {
            hornet = GameObject.FindGameObjectWithTag("Player");
            render = hornet.GetComponent<MeshRenderer>();

            if (hornet == null) TempLog("Hornet not found! OH NO!");
        }
    }
}
