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

        public IClientPlayer PlayerAvatar => PropHuntMod.client.ClientManager.GetPlayer(playerID);
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
            //if (remotePlayer?.PlayerContainer?.tag == "Player")
            //{
            //    throw new Exception($"Player {remotePlayer.Id} ({remotePlayer.Username}) is not remote.");
            //}


            if (remotePlayer == null)
            {
                Log.LogError($"No remotePlayer for {playerID}. Are they remote?");
                return;
            }

            PropHuntMod.playerManager.Add(playerID, this);
        }

        public static PlayerManager GetPlayerManager(PlayerID playerID)
        {
            PropHuntMod.playerManager.TryGetValue(playerID, out var player);
            if (player == null) player = new PlayerManager(playerID);

            return player;
        }
        public bool IsHostInSameRoom()
        {
            if (PlayerAvatar == null) return false;

            //Log.LogInfo($"{playerID} IsInLocalScene: {PlayerAvatar.IsInLocalScene}");

            //bool result = scene == SceneManager.GetActiveScene().name;
            bool result = PlayerAvatar.IsInLocalScene;

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

            if (IsHostInSameRoom())
            {
                if (PropValidation.currentSceneObjects == null) PropValidation.GetAllProps();
                var toClone = PropValidation.currentSceneObjects.GetSpecific(o => o.name == currentCoverObjName);
                if (toClone == null)
                {
                    var errorMsg = $"Unable to find GameObject {currentCoverObjName} for player #{playerID} ({PlayerAvatar.Username})";
                    PropHuntClient.LocalMessage($"ERROR: {errorMsg}");
                    Log.LogError(errorMsg);
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

        public void SetProp(string name)
        {
            if (string.IsNullOrEmpty(name)) name = "";

            currentCoverObjName = name;
            ResetCoverPosition();
            EnsurePropCover();
        }

        public void SetPropLocation(Vector3 location, float rotation)
        {
            currentCoverObjLocation = location;
            currentCoverObjRotation = rotation;

            if (IsHostInSameRoom())
            {
                coverManager.SetPropLocation(location, rotation);
            }
        }

        public void SetHideStatus(bool hiding)
        {
            hornetManager.shouldBeShown = !hiding;
            if (IsHostInSameRoom())
            {
                hornetManager.ToggleHornet(!hiding);
            }
        }

        public void ResetCoverPosition()
        {
            SetPropLocation(Vector3.zero, 0);
        }

        public static void EnsureAllPropCovers()
        {
            Debug.Log($"Ensuring cover for {PropHuntMod.playerManager.Count} players");
            foreach (var player in PropHuntMod.playerManager.Values)
            {
                player.EnsurePropCover();
            }
        }
    }
}
