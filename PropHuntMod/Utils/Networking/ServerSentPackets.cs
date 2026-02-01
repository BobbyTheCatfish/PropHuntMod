using SSMP.Networking.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PropHuntMod.Utils.Networking.FromServer
{
    public class PropSwap : FromClient.PropSwap
    {
        public ushort Id { get; set; }
        public override void WriteData(IPacket packet)
        {
            packet.Write(Id);
            base.WriteData(packet);
        }

        public override void ReadData(IPacket packet)
        {
            Id = packet.ReadUShort();
            base.ReadData(packet);
        }
    }
    public class RoundStart : IPacketData
    {
        public bool IsReliable => true;
        public bool DropReliableDataIfNewerExists => true;
        public bool IsSeeker;
        public ushort PropSwapLimit;
        public void WriteData(IPacket packet)
        {
            packet.Write(IsSeeker);
            packet.Write(PropSwapLimit);
        }
        public void ReadData(IPacket packet)
        {
            IsSeeker = packet.ReadBool();
            PropSwapLimit = packet.ReadUShort();
        }
    }
    public class PropLocation : FromClient.PropLocation
    {
        public ushort Id { get; set; }
        public override void WriteData(IPacket packet)
        {
            packet.Write(Id);
            base.WriteData(packet);
        }

        public override void ReadData(IPacket packet)
        {
            Id = packet.ReadUShort();
            base.ReadData(packet);
        }
    }
    public class HideStatus : FromClient.HideStatus
    {
        public ushort Id { get; set; }
        public override void WriteData(IPacket packet)
        {
            packet.Write(Id);
            base.WriteData(packet);
        }

        public override void ReadData(IPacket packet)
        {
            Id = packet.ReadUShort();
            base.ReadData(packet);
        }
    }
    public class PropFound : FromClient.PropFound
    {
        public bool IsClientFound { get; set; }
        public override void WriteData(IPacket packet)
        {
            packet.Write(IsClientFound);
            base.WriteData(packet);
        }

        public override void ReadData(IPacket packet)
        {
            IsClientFound = packet.ReadBool();
            base.ReadData(packet);
        }
    }
    public class GameOver : IPacketData
    {
        public bool IsReliable => true;
        public bool DropReliableDataIfNewerExists => true;
        public string WinnerUsername { get; set; } = "";
        public void WriteData(IPacket packet)
        {
            packet.Write(WinnerUsername);
        }

        public void ReadData(IPacket packet)
        {
            WinnerUsername = packet.ReadString();
        }
    }
    public static class Packets
    {
        internal static IPacketData Instantiate(CustomPackets packetID)
        {
            switch (packetID)
            {
                case CustomPackets.PropSwap:
                    return new PropSwap();
                case CustomPackets.RoundStart:
                    return new RoundStart();
                case CustomPackets.PropLocation:
                    return new PropLocation();
                case CustomPackets.HideStatus:
                    return new HideStatus();
                case CustomPackets.PropFound:
                    return new PropFound();
                case CustomPackets.GameOver:
                    return new GameOver();
                default:
                    throw new NotImplementedException(packetID.ToString());
            }
        }
    }
}
