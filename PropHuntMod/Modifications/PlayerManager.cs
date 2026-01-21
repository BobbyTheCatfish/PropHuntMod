using PropHuntMod.Utils;
using PropHuntMod.Utils.Networking;
using SilksongMultiplayer;
using Steamworks;
using UnityEngine;

namespace PropHuntMod.Modifications
{
    using PlayerID = CSteamID;
    internal class PlayerManager
    {
        public HornetManager hornetManager;
        public BaseCoverManager coverManager;
        public PlayerAvatar playerAvatar;

        public string currentCoverObjName;
        public Vector3? currentCoverObjLocation;
        public bool currentHideState;
        public PlayerID playerID;

        public PlayerManager(PlayerID playerID)
        {
            hornetManager = new HornetManager();
            coverManager = new BaseCoverManager();

            this.playerID = playerID;
            hornetManager.playerID = playerID;
            coverManager.playerID = playerID;

            SilksongMultiplayerAPI.remotePlayers.TryGetValue(playerID, out var remotePlayer);
            if (!IsRemotePlayer(playerID))
            {
                Log.LogError($"{playerID} is local.");
                return;
            }

            PropHuntMod.playerManager.Add(playerID, this);

            if (remotePlayer == null)
            {
                Log.LogError($"No remotePlayer for {playerID}");
                return;
            }

            playerAvatar = remotePlayer;
        }
        public static PlayerManager GetPlayerManager(PlayerID playerID)
        {
            PropHuntMod.playerManager.TryGetValue(playerID, out var player);
            if (player == null) player = new PlayerManager(playerID);

            return player;
        }
        public static bool IsRemotePlayer(PlayerID playerID)
        {
            bool isRemote = playerID.ToString() != "0" && playerID != null && playerID != SteamUser.GetSteamID();
            //Log.LogInfo($"isRemote: {isRemote}");
            return isRemote;
        }
        internal static bool IsHostInSameRoom(PlayerID playerID)
        {
            var player = GetPlayerManager(playerID);
            if (player == null || player.playerAvatar == null) return false;

            bool result = player.playerAvatar.mapName == PropHuntMod.cover.currentScene;

            if (result) Log.LogInfo($"{playerID} is in the same room");
            else Log.LogInfo($"{playerID} is in room {player.playerAvatar.mapName}, you are in {PropHuntMod.cover.currentScene}");

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

            if (IsHostInSameRoom(playerID))
            {
                var toClone = PropValidation.currentSceneObjects.GetSpecific(o => o.name == currentCoverObjName);
                if (toClone == null)
                {
                    Log.LogError($"Unable to find GameObject {currentCoverObjName} for {playerID}");
                    return;
                }

                coverManager.EnableProp(hornetManager, toClone);
                if (currentCoverObjLocation != null) coverManager.SetPropLocation(currentCoverObjLocation ?? Vector3.zero);
            }
            else
            {
                coverManager.DisableProp(hornetManager, false);
            }
        }
    }
}
