using SilksongMultiplayer;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public PlayerManager(CSteamID steamID)
        {
            hornetManager = new HornetManager();
            coverManager = new CoverManager();

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
    }
}
