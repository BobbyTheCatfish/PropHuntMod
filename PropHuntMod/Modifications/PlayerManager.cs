using PropHuntMod.Utils;
using SSMP.Api.Client;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PropHuntMod.Modifications
{
    using PlayerID = UInt16;
    internal class PlayerManager
    {
        public BaseHornetManager hornetManager;
        public BaseCoverManager coverManager;

        public IClientPlayer playerAvatar => PropHuntMod.client.ClientManager.GetPlayer(playerID);
        public string currentCoverObjName = "";
        public Vector3 currentCoverObjLocation = Vector3.zero;
        public float currentCoverObjRotation = 0;
        public bool currentHideState = false;
        public PlayerID playerID;

        public PlayerManager(PlayerID playerID)
        {
            hornetManager = new BaseHornetManager();
            coverManager = new BaseCoverManager();

            this.playerID = playerID;
            hornetManager.playerID = playerID;
            coverManager.playerID = playerID;

            PropHuntMod.client.ClientManager.TryGetPlayer(playerID, out var remotePlayer);
            //if (!IsRemotePlayer(playerID))
            //{
            //    Log.LogError($"{playerID} is local.");
            //    return;
            //}

            PropHuntMod.playerManager.Add(playerID, this);

            if (remotePlayer == null)
            {
                Log.LogError($"No remotePlayer for {playerID}");
                return;
            }
        }
        public static PlayerManager GetPlayerManager(PlayerID playerID)
        {
            PropHuntMod.playerManager.TryGetValue(playerID, out var player);
            if (player == null) player = new PlayerManager(playerID);

            return player;
        }
        //public static bool IsRemotePlayer(ushort playerID)
        //{
        //    bool isRemote = playerID != 0 && playerID != null;
        //    Log.LogInfo($"isRemote: {isRemote}");
        //    return isRemote;
        //}
        internal static bool IsHostInSameRoom(ushort playerID)
        {
            var player = GetPlayerManager(playerID);
            if (player == null || player.playerAvatar == null) return false;

            bool result = player.playerAvatar.IsInLocalScene; // == PropHuntMod.cover.currentScene;

            if (result) Log.LogInfo($"{playerID} is in the same room");
            else Log.LogInfo($"{playerID} is in another room, you are in {SceneManager.GetActiveScene().name}");

            return result;
        }
        public void EnsurePropCover()
        {
            if (string.IsNullOrEmpty(currentCoverObjName))
            {
                coverManager.DisableProp(hornetManager);
                hornetManager.ToggleHornet(true);
                return;
            }

            if (IsHostInSameRoom(playerID))
            {
                if (PropValidation.currentSceneObjects == null) PropValidation.GetAllProps();
                var toClone = PropValidation.currentSceneObjects.GetSpecific(o => o.name == currentCoverObjName);
                if (toClone == null)
                {
                    Log.LogError($"Unable to find GameObject {currentCoverObjName} for {playerID}");
                    return;
                }

                coverManager.EnableProp(hornetManager, toClone);
                coverManager.SetPropLocation(currentCoverObjLocation, currentCoverObjRotation);
            }
            else
            {
                coverManager.DisableProp(hornetManager, false);
            }
        }

        public void ResetCoverPosition()
        {
            currentCoverObjLocation = Vector3.zero;
            currentCoverObjRotation = 0;
        }
    }
}
