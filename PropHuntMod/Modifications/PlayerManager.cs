using PropHuntMod.Utils;
using PropHuntMod.Utils.Networking;
using SilksongMultiplayer;
using Steamworks;
using UnityEngine;

namespace PropHuntMod.Modifications
{
    internal class PlayerManager
    {
        public HornetManager hornetManager;
        public BaseCoverManager coverManager;
        public PlayerAvatar playerAvatar;

        public string currentCoverObjName;
        public Vector3 currentCoverObjLocation;
        public bool currentHideState;
        public CSteamID steamID;

        public PlayerManager(CSteamID steamID)
        {
            hornetManager = new HornetManager();
            coverManager = new BaseCoverManager();

            this.steamID = steamID;
            hornetManager.steamID = steamID;
            coverManager.steamID = steamID;

            SilksongMultiplayerAPI.remotePlayers.TryGetValue(steamID, out var remotePlayer);
            if (!IsRemotePlayer(steamID))
            {
                PropHuntMod.Log.LogError($"{steamID} is local.");
                return;
            }

            PropHuntMod.playerManager.Add(steamID, this);

            if (remotePlayer == null)
            {
                PropHuntMod.Log.LogError($"No remotePlayer for {steamID}");
                return;
            }

            playerAvatar = remotePlayer;
        }
        public static PlayerManager GetPlayerManager(CSteamID steamID)
        {
            PropHuntMod.playerManager.TryGetValue(steamID, out var player);
            if (player == null) player = new PlayerManager(steamID);

            return player;
        }
        public static bool IsRemotePlayer(CSteamID steamID)
        {
            bool isRemote = steamID.ToString() != "0" && steamID != null && steamID != SteamUser.GetSteamID();
            //PropHuntMod.Log.LogInfo($"isRemote: {isRemote}");
            return isRemote;
        }
        internal static bool IsHostInSameRoom(CSteamID steamID)
        {
            var player = GetPlayerManager(steamID);
            if (player == null || player.playerAvatar == null) return false;

            bool result = player.playerAvatar.mapName == PropHuntMod.cover.currentScene;

            if (result) PropHuntMod.Log.LogInfo($"{steamID} is in the same room");
            else PropHuntMod.Log.LogInfo($"{steamID} is in room {player.playerAvatar.mapName}, you are in {PropHuntMod.cover.currentScene}");

            return result;
        }
        public void EnsurePropCover()
        {
            if (currentCoverObjName == null)
            {
                coverManager.DisableProp(hornetManager);
                hornetManager.ToggleHornet(true);
                return;
            }

            if (IsHostInSameRoom(steamID))
            {
                var toClone = PropValidation.currentSceneObjects.GetSpecific(o => o.name == currentCoverObjName);
                if (toClone == null)
                {
                    PropHuntMod.Log.LogError($"Unable to find GameObject {currentCoverObjName} for {steamID}");
                    return;
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
