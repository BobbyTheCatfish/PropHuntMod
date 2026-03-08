using PropHuntMod.Utils;
using SSMP.Api.Server;
using Vector3 = SSMP.Math.Vector3;

namespace PropHuntMod.Networking.Server
{
    internal class ServerPlayer
    {
        public ushort id;
        public bool hidden = false;
        public bool seeker = false;
        public string propName;
        public string propPath;
        public Vector3 propLocation = Consts.DEFAULT_LOCATION;
        public float propRotation = Consts.DEFAULT_ROTATION;
        public float propScale = Consts.DEFAULT_SCALE;
        public IServerPlayer PlayerAvatar => Server.api.ServerManager.GetPlayer(id);
        public int swapCount = 0;
        public ServerPlayer(ushort id)
        {
            this.id = id;
        }

        public void ResetProp()
        {
            propName = "";
            propPath = "";
            propLocation = Consts.DEFAULT_LOCATION;
            propRotation = Consts.DEFAULT_ROTATION;
            propScale = Consts.DEFAULT_SCALE;
        }
    }
}
