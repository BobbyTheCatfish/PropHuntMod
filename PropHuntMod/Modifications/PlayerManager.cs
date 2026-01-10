using PropHuntMod.Utils.Networking;
using SilksongMultiplayer;
using Steamworks;
using UnityEngine;

namespace PropHuntMod.Modifications
{
    internal class PlayerManager
    {
        public HornetManager hornetManager;
        public CoverManager coverManager;
        public PlayerAvatar playerAvatar;

        public string currentCoverObjName;
        public Vector3 currentCoverObjLocation;
        public bool currentHideState;
        public CSteamID steamID;

        public PlayerManager(CSteamID steamID)
        {
            hornetManager = new HornetManager();
            coverManager = new CoverManager();

            this.steamID = steamID;
            hornetManager.steamID = steamID;
            coverManager.steamID = steamID;

            SilksongMultiplayerAPI.remotePlayers.TryGetValue(steamID, out var remotePlayer);
            if (!IsRemotePlayer(steamID))
            {
                PropHuntMod.Log.LogError($"No remotePlayer for {steamID}");
                return;
            }

            playerAvatar = remotePlayer;
        }

        public static PlayerManager GetPlayerManager(CSteamID steamID)
        {
            PropHuntMod.playerManager.TryGetValue(steamID, out var player);

            if (player == null)
            {
                player = new Modifications.PlayerManager(steamID);
                PropHuntMod.playerManager.Add(steamID, player);
            }

            return player;
        }

        public static bool IsRemotePlayer(CSteamID steamID)
        {
            PropHuntMod.Log.LogInfo(steamID);
            bool isRemote = steamID.ToString() != "0" && steamID != null;
            PropHuntMod.Log.LogInfo($"isRemote: {isRemote}");
            return isRemote;
        }

        public void EnsurePropCover()
        {
            if (currentCoverObjName == null)
            {
                coverManager.DisableProp(hornetManager);
                return;
            }

            if (CustomPacketHandlers.IsHostInSameRoom(steamID))
            {
                var toClone = GameObject.Find(currentCoverObjName);
                if (toClone == null)
                {
                    //if (cloneOriginalName.EndsWith("(Clone)"))
                    //{
                    //    toClone = GameObject.Find(cloneOriginalName.Substring(0, cloneOriginalName.Length - 7));
                    //}
                    if (toClone == null)
                    {
                        PropHuntMod.Log.LogError($"Unable to find GameObject {currentCoverObjName} for {steamID}");
                        return;
                    }
                }

                coverManager.EnableProp(hornetManager, toClone);
            }
            else
            {
                coverManager.DisableProp(hornetManager, false);
            }
        }
    }
}
