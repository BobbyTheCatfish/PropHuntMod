using PropHuntMod.Utils.Networking;
using Steamworks;
using UnityEngine;

namespace PropHuntMod.Modifications
{
    public class HornetManager
    {
        bool shouldBeShown;
        public GameObject hornet;
        public CSteamID steamID;
        MeshRenderer render;
        public void ToggleHornet()
        {
            PropHuntMod.Log.LogInfo("Toggling Hornet");
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

            if (!PlayerManager.IsRemotePlayer(steamID)) PacketSend.SendHideStatus(!show);
        }

        public void EnsureHornetHidden()
        {
            if (hornet == null) SetHornet();
            if (shouldBeShown) return;

            if (render.enabled == true) render.enabled = false;
        }

        public void SetHornet()
        {
            if (!PlayerManager.IsRemotePlayer(steamID))
            {
                hornet = GameObject.FindGameObjectWithTag("Player");
            }
            else
            {
                PropHuntMod.Log.LogInfo($"Setting hornet for {steamID}");
                var player = PlayerManager.GetPlayerManager(steamID).playerAvatar;
                PropHuntMod.Log.LogInfo(player.gameObject.name);
                hornet = player.gameObject;
            }

            if (hornet == null) PropHuntMod.Log.LogError("Hornet not found! OH NO!");
            render = hornet.GetComponent<MeshRenderer>();
        }
    }
}
