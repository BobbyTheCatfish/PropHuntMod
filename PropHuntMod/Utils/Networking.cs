using HarmonyLib;
using SilksongMultiplayer;
using Steamworks;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;
using static PropHuntMod.Logging.Logging;

namespace PropHuntMod.Utils.Networking
{
    struct CustomPackets
    {
        public const string PropSwap = "ph.swp";
        public const string PropLocation = "ph.loc";
        public const string HideStatus = "ph.st";
    }

    [HarmonyPatch]
    class PacketReciept
    {
        public static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("SilksongMultiplayer.NetworkDataReceiver");
            TempLog("Patching SilksongMultiplayer.NetworkDataReceiver");
            return AccessTools.Method(type, "ProcessPacket");
        }

        public static void Prefix(byte[] data, CSteamID senderID)
        {
            int offset = 0;
            var packet = PacketDeserializer.ReadString(data, ref offset);

            TempLog(senderID.GetAccountID().m_AccountID.ToString());

            switch(packet)
            {
                case CustomPackets.PropSwap:
                    TempLog("PROP SWAP PACKET RECEIVED");
                    break;
                case CustomPackets.PropLocation:
                    TempLog("PROP LOCATION PACKET RECEIVED");
                    break;
                case CustomPackets.HideStatus:
                    TempLog("HIDE STATUS PACKET RECEIVED");
                    break;

                default: break;
            }
        }
    }

    public static class PacketSend
    {

        private static void SendData(byte[] data)
        {
            object[] parameters = { data, (EP2PSend)2 };
            System.Type type = AccessTools.TypeByName("SilksongMultiplayer.NetworkDataSender");
            MethodInfo Broadcast = AccessTools.Method(type, "Broadcast");
            Broadcast.Invoke(null, parameters);
        }

        public static void SendPropSwap(string cloneOriginalName)
        {
            byte[] data = PacketSerializer.Combine(
                PacketSerializer.SerializeString(CustomPackets.PropSwap),
                PacketSerializer.SerializeString(cloneOriginalName)
            );
            SendData(data);
            TempLog("Sending prop swap packet");
        }

        public static void SendHideStatus(bool hiding)
        {
            byte[] data = PacketSerializer.Combine(
                PacketSerializer.SerializeString(CustomPackets.HideStatus),
                PacketSerializer.SerializeBool(hiding)
            );

            SendData(data);
            TempLog("Sending hide status packet");
        }

        public static void SendPropLocation(Vector3 location)
        {
            byte[] data = PacketSerializer.Combine(
                PacketSerializer.SerializeString(CustomPackets.PropLocation),
                PacketSerializer.SerializeFloat(location.x),
                PacketSerializer.SerializeFloat(location.y),
                PacketSerializer.SerializeFloat(location.z)
            );

            SendData(data);
            TempLog("Sending prop location packet");
        }

    }

}
