using PropHuntMod.Modifications;
using SSMP.Api.Client;
using SSMP.Api.Client.Networking;
using SSMP.Networking.Packet;
using UnityEngine;

namespace PropHuntMod.Utils.Networking
{
    public enum CustomPackets
    {
        PropSwap,
        ForcePropSwap,
        PropLocation,
        HideStatus,
        PropFound,
        Heartbeat,
        GameOver,
    }

    public class NetworkData : IPacketData
    {
        public bool IsReliable => true;
        public bool DropReliableDataIfNewerExists => true;
        public ushort Id { get; set; }
        public virtual void WriteData(IPacket packet)
        {
            //packet.Write(Id);
        }
        public virtual void ReadData(IPacket packet)
        {
            Id = packet.ReadUShort();
        }
    }

    public class PropSwapData : NetworkData
    {
        public string propName { get; set; }

        public override void WriteData(IPacket packet)
        {
            packet.Write(propName);
        }

        public override void ReadData(IPacket packet)
        {
            Id = packet.ReadUShort();
            propName = packet.ReadString();
        }
    }

    public class ForcePropSwapData : NetworkData
    {
        public override void WriteData(IPacket packet) { }
        public override void ReadData(IPacket packet) { }
    }

    public class PropLocationData : NetworkData
    {
        public Vector3 propPosition { get; set; }
        public float propRotation { get; set; }
        public override void WriteData(IPacket packet)
        {
            packet.Write(propPosition.x);
            packet.Write(propPosition.y);
            packet.Write(propPosition.z);

            packet.Write(propRotation);
        }

        public override void ReadData(IPacket packet)
        {
            Id = packet.ReadUShort();
            propPosition = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());
            propRotation = packet.ReadFloat();
        }
    }

    public class HideStatusData : NetworkData
    {
        public bool isHiding { get; set; }

        public override void WriteData(IPacket packet)
        {
            packet.Write(isHiding);
        }

        public override void ReadData(IPacket packet)
        {
            Id = packet.ReadUShort();
            isHiding = packet.ReadBool();
        }
    }

    public class PropFoundData : NetworkData
    {
        public bool isClientFound { get; set; }
        public ushort propOwnerID { get; set; }
        public override void WriteData(IPacket packet)
        {
            packet.Write(propOwnerID);
        }
        public override void ReadData(IPacket packet)
        {
            Id = packet.ReadUShort();
            isClientFound = packet.ReadBool();
            propOwnerID = packet.ReadUShort();
        }
    }

    public class GameOverData : NetworkData
    {
        public string winnerUsername { get; set; }
        public override void WriteData(IPacket packet)
        {
        }

        public override void ReadData(IPacket packet)
        {
            winnerUsername = packet.ReadString();
        }
    }
    static class Network
    {
        static IClientAddonNetworkSender<CustomPackets> sender;
        static IClientAddonNetworkReceiver<CustomPackets> receiver;
        public static void SendPropSwap(string propName)
        {
            sender.SendSingleData(CustomPackets.PropSwap, new PropSwapData
            {
                propName = propName
            });
        }

        public static void SendPropLocation(Vector3 propPosition, float propRotation)
        {
            sender.SendSingleData(CustomPackets.PropLocation, new PropLocationData
            {
                propPosition = propPosition,
                propRotation = propRotation
            });
        }

        public static void SendHideStatus(bool isHiding)
        {
            sender.SendSingleData(CustomPackets.HideStatus, new HideStatusData
            {
                isHiding = isHiding
            });
        }

        public static void SendPropFound(ushort propOwnerID)
        {
            sender.SendSingleData(CustomPackets.PropFound, new PropFoundData
            {
                propOwnerID = propOwnerID
            });
        }

        public static void Init(IClientApi clientApi, ClientAddon clientAddon)
        {
            sender = clientApi.NetClient.GetNetworkSender<CustomPackets>(clientAddon);
            receiver = clientApi.NetClient.GetNetworkReceiver<CustomPackets>(clientAddon, InstantiatePacket);

            receiver.RegisterPacketHandler<PropSwapData>(CustomPackets.PropSwap, OnPropSwap);
            receiver.RegisterPacketHandler<ForcePropSwapData>(CustomPackets.ForcePropSwap, OnForcePropSwap);
            receiver.RegisterPacketHandler<PropLocationData>(CustomPackets.PropSwap, OnPropLocation);
            receiver.RegisterPacketHandler<HideStatusData>(CustomPackets.PropSwap, OnHideStatus);
            receiver.RegisterPacketHandler<PropFoundData>(CustomPackets.PropSwap, OnPropFound);
            receiver.RegisterPacketHandler<GameOverData>(CustomPackets.GameOver, OnGameOver);
        }

        static void OnPropSwap(PropSwapData data)
        {
            string propName = data.propName;
            PlayerManager player = PlayerManager.GetPlayerManager(data.Id);
            player.currentCoverObjLocation = null;

            if (propName == "")
            {
                player.currentCoverObjName = null;
            }
            else
            {
                player.currentCoverObjName = propName;
            }

            player.EnsurePropCover();
        }

        static void OnForcePropSwap(ForcePropSwapData data)
        {
            PropHuntMod.cover.EnableProp();
        }

        static void OnPropLocation(PropLocationData data)
        {
            PlayerManager player = PlayerManager.GetPlayerManager(data.Id);

            player.currentCoverObjLocation = data.propPosition;
            player.currentCoverObjRotation = data.propRotation;

            if (PlayerManager.IsHostInSameRoom(data.Id))
            {
                player.coverManager.SetPropLocation(data.propPosition);
            }
            Log.LogInfo($"{data.Id} prop moved to {data.propPosition}, {data.propRotation}");
        }

        static void OnHideStatus(HideStatusData data)
        {
            PlayerManager player = PlayerManager.GetPlayerManager(data.Id);

            player.currentHideState = data.isHiding;

            if (PlayerManager.IsHostInSameRoom(data.Id))
            {
                player.hornetManager.ToggleHornet(!data.isHiding);
            }

            Log.LogInfo($"{data.Id} hiding status set to {data.isHiding}");
        }

        static void OnPropFound(PropFoundData data)
        {
            var isClientFound = data.isClientFound;
            if (isClientFound)
            {
                Log.LogInfo("I've been found!");
                PropHuntMod.cover.DisableProp(PropHuntMod.hornet);
            }
            else
            {
                var player = PlayerManager.GetPlayerManager(data.propOwnerID);
                player.coverManager.DisableProp(player.hornetManager);
                Log.LogInfo($"{player.playerAvatar.Username} has been found");
            }
        }

        static void OnGameOver(GameOverData data)
        {
            PropHuntMod.cover.DisableProp(PropHuntMod.hornet);
            string winner = data.winnerUsername;
        }


        internal static IPacketData InstantiatePacket(CustomPackets packetID)
        {
            switch (packetID) {
                case CustomPackets.PropSwap:
                    return new PropSwapData();
                case CustomPackets.PropLocation:
                    return new PropLocationData();
                case CustomPackets.HideStatus:
                    return new HideStatusData();
                case CustomPackets.PropFound:
                    return new PropFoundData();
                default:
                    return null;
            }
        }
    }
}
