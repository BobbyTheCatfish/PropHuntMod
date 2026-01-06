using HarmonyLib;
using SilksongMultiplayer;
using SilksongMultiplayer.NetworkData;
using Steamworks;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;
using PropHuntMod.Modifications;

namespace PropHuntMod.Utils.Networking
{
    struct CustomPackets
    {
        public const int PropSwap = 99;
        public const int PropLocation = 98;
        public const int HideStatus = 97;
    }

    [HarmonyPatch(typeof (NetworkDataReceiver), "ProcessPacket")]
    class PacketReciept
    {
        //public static MethodBase TargetMethod()
        //{
        //    NetworkDataReceiver
        //    var type = AccessTools.TypeByName("SilksongMultiplayer.NetworkDataReceiver");
        //    PropHuntMod.Log.LogInfo("Patching SilksongMultiplayer.NetworkDataReceiver");
        //    return AccessTools.Method(type, "ProcessPacket");
        //}

        public static void Prefix(byte[] data, CSteamID senderID)
        {
            int offset = 0;
            
            int packetType = PacketDeserializer.ReadByte(data, ref offset);

            //PropHuntMod.Log.LogInfo(senderID);

            switch(packetType)
            {
                case CustomPackets.PropSwap:
                    PropHuntMod.Log.LogInfo("PROP SWAP PACKET RECEIVED");
                    HandlePropSwap(data, senderID, ref offset);
                    break;
                case CustomPackets.PropLocation:
                    PropHuntMod.Log.LogInfo("PROP LOCATION PACKET RECEIVED");
                    HandlePropLocation(data, senderID, ref offset);
                    break;
                case CustomPackets.HideStatus:
                    PropHuntMod.Log.LogInfo("HIDE STATUS PACKET RECEIVED");
                    HandleHideStatus(data, senderID, ref offset);
                    break;

                default: break;
            }
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
        private static bool IsHostInSameRoom(CSteamID steamID)
        {
            SilksongMultiplayerAPI.playerSceneMap.TryGetValue(steamID, out var room);
            bool result = room == PropHuntMod.cover.currentScene;

            if (result) PropHuntMod.Log.LogInfo($"{steamID} is in the same room");
            else PropHuntMod.Log.LogInfo($"{steamID} is in room {room}, you are in {PropHuntMod.cover.currentScene}");

                return result;
        }
        private static void HandlePropSwap(byte[] data, CSteamID senderID, ref int offset)
        {
            string cloneOriginalName = PacketDeserializer.ReadString(data, ref offset);
            PropHuntMod.Log.LogInfo($"{senderID} hiding as {cloneOriginalName}");

            PlayerManager player = GetPlayerManager(senderID);

            if (cloneOriginalName == "")
            {
                player.currentCoverObjName = null;
                if (IsHostInSameRoom(senderID)) player.coverManager.DisableProp(player.hornetManager);
            }
            else
            {
                player.currentCoverObjName = cloneOriginalName;
                if (IsHostInSameRoom(senderID))
                {
                    var toClone = GameObject.Find(cloneOriginalName);
                    if (toClone == null)
                    {
                        PropHuntMod.Log.LogError($"Unable to find GameObject {cloneOriginalName} for {senderID}");
                        return;
                    }
                    player.coverManager.EnableProp(player.hornetManager, toClone);
                }
            }



        }

        private static void HandlePropLocation(byte[] data, CSteamID senderID, ref int offset)
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

        private static void HandleHideStatus(byte[] data, CSteamID senderID, ref int offset)
        {
            bool isHiding = PacketDeserializer.ReadBool(data, ref offset);
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

        private static void SendData(byte[] data, string packetName)
        {
            PropHuntMod.Log.LogInfo($"Sending {packetName} packet");
            NetworkDataSender.Broadcast(data, EP2PSend.k_EP2PSendReliable);
        }

        public static void SendPropSwap(string cloneOriginalName)
        {
            byte[] data = PacketSerializer.Combine(
                PacketSerializer.SerializeByte(CustomPackets.PropSwap),
                PacketSerializer.SerializeString(cloneOriginalName)
            );
            SendData(data, "prop swap");
        }

        public static void SendHideStatus(bool hiding)
        {
            byte[] data = PacketSerializer.Combine(
                PacketSerializer.SerializeByte(CustomPackets.HideStatus),
                PacketSerializer.SerializeBool(hiding)
            );

            SendData(data, "hide status");
        }

        public static void SendPropLocation(Vector3 location)
        {
            byte[] data = PacketSerializer.Combine(
                PacketSerializer.SerializeByte(CustomPackets.PropLocation),
                PacketSerializer.SerializeVector3(location)
            );

            SendData(data, "prop location");
        }

    }

}
