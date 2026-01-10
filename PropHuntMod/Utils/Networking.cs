using HarmonyLib;
using SilksongMultiplayer;
using SilksongMultiplayer.NetworkData;
using Steamworks;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;
using PropHuntMod.Modifications;
using System.Text.RegularExpressions;

namespace PropHuntMod.Utils.Networking
{
    struct CustomPackets
    {
        public const int PropSwap = 99;
        public const int PropLocation = 98;
        public const int HideStatus = 97;
        public const int PropFound = 96;
    }

    static class CustomPacketHandlers
    {
        public readonly static NetworkCustomPacket propSwap = new NetworkCustomPacket(CustomPackets.PropSwap, HandlePropSwap);
        public readonly static NetworkCustomPacket propLocation = new NetworkCustomPacket(CustomPackets.PropLocation, HandlePropLocation);
        public readonly static NetworkCustomPacket hideStatus = new NetworkCustomPacket(CustomPackets.HideStatus, HandleHideStatus);
        public readonly static NetworkCustomPacket propFound = new NetworkCustomPacket(CustomPackets.PropFound, HandlePropFound);

        public static void Init()
        {
            SilksongMultiplayerAPI.AddCustomPacket(propSwap);
            SilksongMultiplayerAPI.AddCustomPacket(propLocation);
            SilksongMultiplayerAPI.AddCustomPacket(hideStatus);
            SilksongMultiplayerAPI.AddCustomPacket(propFound);
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
        internal static bool IsHostInSameRoom(CSteamID steamID)
        {
            SilksongMultiplayerAPI.remotePlayers.TryGetValue(steamID, out var room);
            bool result = room.mapName == PropHuntMod.cover.currentScene;

            if (result) PropHuntMod.Log.LogInfo($"{steamID} is in the same room");
            else PropHuntMod.Log.LogInfo($"{steamID} is in room {room}, you are in {PropHuntMod.cover.currentScene}");

            return result;
        }
        private static void HandlePropSwap(byte[] data, CSteamID senderID, int offset)
        {
            string cloneOriginalName = PacketDeserializer.ReadString(data, ref offset);
            PropHuntMod.Log.LogInfo($"{senderID} hiding as {cloneOriginalName}");

            PlayerManager player = GetPlayerManager(senderID);

            if (cloneOriginalName == "")
            {
                player.currentCoverObjName = null;
            }
            else
            {
                player.currentCoverObjName = cloneOriginalName;
            }
            player.EnsurePropCover();
        }
        private static void HandlePropLocation(byte[] data, CSteamID senderID, int offset)
        {
            Vector3 propPosition = PacketDeserializer.ReadVector3(data, ref offset);
            PlayerManager player = GetPlayerManager(senderID);

            player.currentCoverObjLocation = propPosition;

            if (IsHostInSameRoom(senderID))
            {
                player.coverManager.SetPropLocation(propPosition);
            }

            PropHuntMod.Log.LogInfo($"{senderID} prop moved to {propPosition}");
        }

        private static void HandleHideStatus(byte[] data, CSteamID senderID, int offset)
        {
            bool isHiding = PacketDeserializer.ReadBool(data, ref offset);
            PropHuntMod.Log.LogWarning($"HIDING: {isHiding}");
            PlayerManager player = GetPlayerManager(senderID);

            player.currentHideState = isHiding;

            if (IsHostInSameRoom(senderID))
            {
                player.hornetManager.ToggleHornet(!isHiding);
            }

            PropHuntMod.Log.LogInfo($"{senderID} hiding status set to {isHiding}");
        }
    }

    public static class PacketSend
    {

        public static void SendPropSwap(string cloneOriginalName)
        {
            CustomPacketHandlers.propSwap.SendPacket(
                PacketSerializer.SerializeString(cloneOriginalName)
            );
        }

        public static void SendHideStatus(bool hiding)
        {
            CustomPacketHandlers.hideStatus.SendPacket(
                PacketSerializer.SerializeBool(hiding)
            );
        }

        public static void SendPropLocation(Vector3 location)
        {
            CustomPacketHandlers.propLocation.SendPacket(
                PacketSerializer.SerializeVector3(location)
            );
        }

    }

}
