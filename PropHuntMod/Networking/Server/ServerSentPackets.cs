using PropHuntMod.Utils;
using SSMP.Networking.Packet;
using System;

namespace PropHuntMod.Networking.Server
{
    public class PropSwap : Client.PropSwap
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
        public int SeekerWaitTime;
        public float SeekerAttackCooldown;
        public void WriteData(IPacket packet)
        {
            packet.Write(IsSeeker);
            packet.Write(PropSwapLimit);
            packet.Write(SeekerWaitTime);
            packet.Write(SeekerAttackCooldown);
        }
        public void ReadData(IPacket packet)
        {
            IsSeeker = packet.ReadBool();
            PropSwapLimit = packet.ReadUShort();
            SeekerWaitTime = packet.ReadInt();
            SeekerAttackCooldown = packet.ReadFloat();
        }
    }
    public class PropLocation : Client.PropLocation
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
    public class HideStatus : Client.HideStatus
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
    public class PropFound : Client.PropFound
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

    public class SeekerStart : IPacketData
    {
        public bool IsReliable => true;
        public bool DropReliableDataIfNewerExists => true;
        public void WriteData(IPacket packet)
        {
        }

        public void ReadData(IPacket packet)
        {
        }
    }
    public class GameOver : IPacketData
    {
        public bool IsReliable => true;
        public bool DropReliableDataIfNewerExists => true;
        public string WinnerUsername { get; set; }
        public bool IsWinner { get; set; }
        public bool WasCanceled { get; set; }
        public void WriteData(IPacket packet)
        {
            packet.Write(WinnerUsername);
            packet.Write(IsWinner);
            packet.Write(WasCanceled);
        }

        public void ReadData(IPacket packet)
        {
            WinnerUsername = packet.ReadString();
            IsWinner = packet.ReadBool();
            WasCanceled = packet.ReadBool();
        }
    }

    public class FailedAction : IPacketData
    {
        public bool IsReliable => true;
        public bool DropReliableDataIfNewerExists => true;
        public CustomPackets FailedPacket;
        public CorrectiveActions FixMethod;
        public ushort AffectedID;
        public int BypassTicketID;

        public void WriteData(IPacket packet)
        {
            packet.Write((int)FailedPacket);
            packet.Write((int)FixMethod);
            packet.Write(AffectedID);
            packet.Write(BypassTicketID);
        }

        public void ReadData(IPacket packet)
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
                case CustomPackets.SeekerStart:
                    return new SeekerStart();
                case CustomPackets.FailedAction:
                    return new FailedAction();
                default:
                    throw new NotImplementedException(packetID.ToString());
            }
        }
    }
}
