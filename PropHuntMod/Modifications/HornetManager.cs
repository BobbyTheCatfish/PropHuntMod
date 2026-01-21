using PropHuntMod.Utils;
using PropHuntMod.Utils.Networking;
using UnityEngine;

namespace PropHuntMod.Modifications
{
    public class HornetManager
    {
        public bool shouldBeShown;
        public GameObject hornet;
        public ushort playerID;
        MeshRenderer render;
        public bool isRemote;
        public void ToggleHornet()
        {
            Log.LogInfo("Toggling Hornet");
            if (hornet == null) SetHornet();

            var render = hornet.GetComponent<MeshRenderer>();
            Network.SendHideStatus(render.enabled);

            render.enabled = !render.enabled;
            shouldBeShown = render.enabled;

        }

        public void ToggleHornet(bool show)
        {
            if (hornet == null) SetHornet();

            var render = hornet.GetComponent<MeshRenderer>();
            render.enabled = show;
            shouldBeShown = show;

            if (!isRemote) Network.SendHideStatus(!show);
        }

        public void EnsureHornetHidden()
        {
            if (hornet == null) SetHornet();
            if (shouldBeShown) return;

            if (render.enabled == true) render.enabled = false;
        }

        public void SetHornet()
        {
            if (!isRemote)
            {
                hornet = GameObject.FindGameObjectWithTag("Player");
            }
            else
            {
                Log.LogInfo($"Setting hornet for {playerID}");
                var player = PlayerManager.GetPlayerManager(playerID).playerAvatar;
                if (player.PlayerObject == null)
                {
                    Log.LogError($"{playerID} PlayerObject is null");
                    return;
                }    
                Log.LogInfo(player.PlayerObject.name);
                hornet = player.PlayerObject;
            }

            if (hornet == null) Log.LogError("Hornet not found! OH NO!");
            render = hornet.GetComponent<MeshRenderer>();
        }
    }
}
