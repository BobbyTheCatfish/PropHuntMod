using PropHuntMod.Utils;
using SSMP.Networking.Packet;
using SSMP.Networking.Packet.Data;
using System;
using Packet = PropHuntMod.Networking.Client.Packet;

namespace PropHuntMod.Networking.Server
{
    public class PropSwap : Client.PropSwap
    {
        public override bool DropReliableDataIfNewerExists => false;
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
    public class RoundStart : Packet
    {
        public bool IsSeeker;
        public ushort PropSwapLimit;
        public int SeekerWaitTime;
        public float SeekerAttackCooldown;
        public override void WriteData(IPacket packet)
        {
            packet.Write(IsSeeker);
            packet.Write(PropSwapLimit);
            packet.Write(SeekerWaitTime);
            packet.Write(SeekerAttackCooldown);
        }
        public override void ReadData(IPacket packet)
        {
            IsSeeker = packet.ReadBool();
            PropSwapLimit = packet.ReadUShort();
            SeekerWaitTime = packet.ReadInt();
            SeekerAttackCooldown = packet.ReadFloat();
        }
    }
    public class PropLocation : Client.PropLocation
    {
        public override bool DropReliableDataIfNewerExists => false;
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
    public class HideStatus : Client.HideStatus
    {
        public override bool DropReliableDataIfNewerExists => false;
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
    public class PropFound : Client.PropFound
    {
        public override bool DropReliableDataIfNewerExists => false;
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

    public class SeekerStart : Packet
    {
    }
    public class GameOver : Packet
    {

        public string WinnerUsername { get; set; }
        public bool IsWinner { get; set; }
        public bool WasCanceled { get; set; }
        public override void WriteData(IPacket packet)
        {
            packet.Write(WinnerUsername);
            packet.Write(IsWinner);
            packet.Write(WasCanceled);
        }

        public override void ReadData(IPacket packet)
        {
            WinnerUsername = packet.ReadString();
            IsWinner = packet.ReadBool();
            WasCanceled = packet.ReadBool();
        }
    }

    public class FailedAction : Packet
    {
        public CustomPackets FailedPacket;
        public CorrectiveActions FixMethod;
        public ushort AffectedID;
        public int BypassTicketID;

        public override void WriteData(IPacket packet)
        {
            packet.Write((int)FailedPacket);
            packet.Write((int)FixMethod);
            packet.Write(AffectedID);
            packet.Write(BypassTicketID);
        }

        public override void ReadData(IPacket packet)
        {
            FailedPacket = (CustomPackets)packet.ReadInt();
            FixMethod = (CorrectiveActions)packet.ReadInt();
            AffectedID = packet.ReadUShort();
            BypassTicketID = packet.ReadUShort();
        }
    }

    public static class Packets
    {
        internal static IPacketData Instantiate(CustomPackets packetID)
        {
            switch (packetID)
            {
                case CustomPackets.PropSwap:
                    return new PacketDataCollection<PropSwap>();
                case CustomPackets.RoundStart:
                    return new RoundStart();
                case CustomPackets.PropLocation:
                    return new PacketDataCollection<PropLocation>();
                case CustomPackets.HideStatus:
                    return new PacketDataCollection<HideStatus>();
                case CustomPackets.PropFound:
                    return new PacketDataCollection<PropFound>();
                case CustomPackets.GameOver:
                    return new GameOver();
                case CustomPackets.SeekerStart:
                    return new SeekerStart();
                case CustomPackets.FailedAction:
                    return new PacketDataCollection<FailedAction>();
                default:
                    throw new NotImplementedException(packetID.ToString());
            }
        }
    }
}
