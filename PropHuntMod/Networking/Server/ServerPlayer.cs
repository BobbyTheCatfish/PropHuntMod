using SSMP.Api.Server;
using UnityEngine;

namespace PropHuntMod.Networking.Server
{
    internal class ServerPlayer
    {
        public ushort id;
        public bool hidden = false;
        public bool seeker = false;
        public string propName;
        public string propPath;
        public Vector3 propLocation = Vector3.zero;
        public float propRotation = 0;
        public float propScale = 1;
        public IServerPlayer PlayerAvatar => Server.api.ServerManager.GetPlayer(id);
        public int swapCount = 0;
        public ServerPlayer(ushort id)
        {
            this.id = id;
        }
    }
}
