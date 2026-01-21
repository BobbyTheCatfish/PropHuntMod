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
    using PlayerID = CSteamID;
    struct CustomPackets
    {
        public const int PropSwap = 99;
        public const int PropLocation = 98;
        public const int HideStatus = 97;
        public const int PropFound = 96;
        public const int Heartbeat = 95;
    }

    static class CustomPacketHandlers
    {
        public readonly static NetworkCustomPacket propSwap = new NetworkCustomPacket(CustomPackets.PropSwap, HandlePropSwap);
        public readonly static NetworkCustomPacket propLocation = new NetworkCustomPacket(CustomPackets.PropLocation, HandlePropLocation);
        public readonly static NetworkCustomPacket hideStatus = new NetworkCustomPacket(CustomPackets.HideStatus, HandleHideStatus);
        public readonly static NetworkCustomPacket propFound = new NetworkCustomPacket(CustomPackets.PropFound, HandlePropFound);
        public readonly static NetworkCustomPacket heartbeat = new NetworkCustomPacket(CustomPackets.Heartbeat, HandleHeartbeat);
        public static void Init()
        {
            SilksongMultiplayerAPI.AddCustomPacket(propSwap);
            SilksongMultiplayerAPI.AddCustomPacket(propLocation);
            SilksongMultiplayerAPI.AddCustomPacket(hideStatus);
            SilksongMultiplayerAPI.AddCustomPacket(propFound);
            SilksongMultiplayerAPI.AddCustomPacket(heartbeat);
        }

        public static Modifications.PlayerManager GetPlayerManager(PlayerID playerID)
        {
            PropHuntMod.playerManager.TryGetValue(playerID, out var player);

            if (player == null)
            {
                player = new Modifications.PlayerManager(playerID);
            }

            return player;
        }
        private static void HandlePropSwap(byte[] data, PlayerID senderID, int offset)
        {
            string cloneOriginalName = PacketDeserializer.ReadString(data, ref offset);
            Log.LogInfo($"{senderID} hiding as {cloneOriginalName}");

            Modifications.PlayerManager player = GetPlayerManager(senderID);
            player.currentCoverObjLocation = null;

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
        private static void HandlePropLocation(byte[] data, PlayerID senderID, int offset)
        {
            Vector3 propPosition = PacketDeserializer.ReadVector3(data, ref offset);
            Modifications.PlayerManager player = GetPlayerManager(senderID);

            player.currentCoverObjLocation = propPosition;

            if (Modifications.PlayerManager.IsHostInSameRoom(senderID))
            {
                player.coverManager.SetPropLocation(propPosition);
            }
            else
            {
                player.currentCoverObjLocation = propPosition;
            }
            Log.LogInfo($"{senderID} prop moved to {propPosition}");
        }

        private static void HandleHideStatus(byte[] data, PlayerID senderID, int offset)
        {
            bool isHiding = PacketDeserializer.ReadBool(data, ref offset);
            Log.LogWarning($"HIDING: {isHiding}");
            Modifications.PlayerManager player = GetPlayerManager(senderID);

            player.currentHideState = isHiding;

            if (Modifications.PlayerManager.IsHostInSameRoom(senderID))
            {
                player.hornetManager.ToggleHornet(!isHiding);
            }

            Log.LogInfo($"{senderID} hiding status set to {isHiding}");
        }
        
        private static void HandlePropFound(byte[] data, PlayerID senderID, int offset)
        {
            ulong rawTargetID = PacketDeserializer.ReadULong(data, ref offset);
            var targetID = new PlayerID(rawTargetID);

            Log.LogInfo(SteamUser.GetSteamID());

            if (targetID == SteamUser.GetSteamID())
            {
                Log.LogInfo("I've been found!");
                PropHuntMod.cover.DisableProp(PropHuntMod.hornet);
            }
            else
            {
                var player = GetPlayerManager(senderID);
                player.coverManager.DisableProp(player.hornetManager);
                Log.LogInfo($"{SteamFriends.GetFriendPersonaName(senderID)} has been found");
            }
        }

        private static void HandleHeartbeat(byte[] data, PlayerID senderID, int offset)
        {
            bool hidden = PacketDeserializer.ReadBool(data, ref offset);
            string coverName = PacketDeserializer.ReadString(data, ref offset);
            Vector3 coverPosition = PacketDeserializer.ReadVector3(data, ref offset);

            Modifications.PlayerManager player = GetPlayerManager(senderID);
            if (coverName == "") player.currentCoverObjName = null;
            else player.currentCoverObjName = coverName;

            player.currentCoverObjLocation = coverPosition;

            player.EnsurePropCover();
            player.hornetManager.ToggleHornet(!hidden);
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

        public static void SendPropFound(PlayerID propOwner)
        {
            CustomPacketHandlers.propFound.SendPacket(
                PacketSerializer.SerializeULong(propOwner.m_SteamID)
            );
        }

        public static void SendHeartbeat()
        {
            bool hidden = !PropHuntMod.hornet.shouldBeShown;
            string coverName = PropHuntMod.cover.coverOGName;
            Vector3 coverPosition = PropHuntMod.cover.cover != null ? PropHuntMod.cover.cover.transform.position : Vector3.zero;

            CustomPacketHandlers.heartbeat.SendPacket(
                PacketSerializer.Combine(
                    PacketSerializer.SerializeBool(hidden),
                    PacketSerializer.SerializeString(coverName),
                    PacketSerializer.SerializeVector3(coverPosition)
                )
                
            );
        }
    }

}
