using PropHuntMod.Utils;
using System;
using UnityEngine;

namespace PropHuntMod.Players
{
    using PlayerID = UInt16;
    public class BaseHornetManager
    {
        public bool shouldBeShown = true;
        public GameObject hornet;
        public PlayerID playerID;
        internal MeshRenderer render;
        public bool isRemote;

        public virtual bool ToggleHornet(bool show)
        {
            if (!HornetExists()) return false;

            Log.LogInfo($"Toggling Hornet: {show}");

            //var render = hornet.GetComponent<MeshRenderer>();
            render.enabled = show;
            shouldBeShown = show;

            ToggleNametag(show);

            return true;
        }

        public void EnsureHornetHidden()
        {
            if (!HornetExists()) return;
            if (shouldBeShown) return;

            if (render.enabled == true) render.enabled = false;
        }

        public bool HornetExists()
        {
            if (hornet == null) SetHornet();
            return hornet != null;
        }

        public virtual void SetHornet()
        {
            Log.LogInfo($"Setting hornet for {playerID}");
            var player = PlayerManager.GetPlayerManager(playerID)?.PlayerAvatar;

            if (player?.PlayerObject == null)
            {
                Log.LogError($"{playerID} PlayerObject is null");
                return;
            }

            //Log.LogInfo(player.PlayerObject.name);
            hornet = player.PlayerObject;

            if (hornet == null)
            {
                Log.LogError("Hornet not found! OH NO!");
                return;
            }

            render = hornet.GetComponent<MeshRenderer>();
        }

        public void ToggleNametag(bool show)
        {
            if (!HornetExists()) return;

            Transform nametag;
            if (hornet.transform.parent) nametag = hornet.transform.parent.Find("Username");
            else nametag = hornet.transform.Find("Username");

            Log.LogInfo($"Nametag: {nametag}, setting to {show}");
            nametag?.gameObject.SetActive(show);
        }
    }
}
