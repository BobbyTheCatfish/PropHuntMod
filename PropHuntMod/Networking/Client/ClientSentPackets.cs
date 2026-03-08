using PropHuntMod.Utils;
using SSMP.Networking.Packet;
using System;

using Vector3 = SSMP.Math.Vector3;

namespace PropHuntMod.Networking.Client
{
    public class PropSwap : IPacketData
    {
        public bool IsReliable => true;
        public bool DropReliableDataIfNewerExists => true;
        public string propName = "";
        public string propPath = "";
        public int TicketID = -1;
        public virtual void WriteData(IPacket packet)
        {
            packet.Write(propName);
            packet.Write(propPath);
            packet.Write(TicketID);
        }
        public virtual void ReadData(IPacket packet)
        {
            propName = packet.ReadString();
            propPath = packet.ReadString();
            TicketID = packet.ReadInt();
        }
    }
    public class PropLocation : IPacketData
    {
        public bool IsReliable => true;
        public bool DropReliableDataIfNewerExists => true;
        public Vector3 PropPosition { get; set; }
        public float PropRotation { get; set; }
        public float PropScale { get; set; }
        public virtual void WriteData(IPacket packet)
        {
            packet.Write(PropPosition);

            packet.Write(PropRotation);
            packet.Write(PropScale);
        }

        public virtual void ReadData(IPacket packet)
        {
            PropPosition = packet.ReadVector3();
            PropRotation = packet.ReadFloat();
            PropScale = packet.ReadFloat();
        }
    }
    public class HideStatus : IPacketData
    {
        public bool IsReliable => true;
        public bool DropReliableDataIfNewerExists => true;
        public bool IsHiding { get; set; }
        public int TicketID = -1;
        public virtual void WriteData(IPacket packet)
        {
            packet.Write(IsHiding);
            packet.Write(TicketID);
        }

        public virtual void ReadData(IPacket packet)
        {
            IsHiding = packet.ReadBool();
            TicketID = packet.ReadInt();
        }
    }
    public class PropFound : IPacketData
    {
        public bool IsReliable => true;
        public bool DropReliableDataIfNewerExists => true;
        public ushort PropOwnerID { get; set; }
        public virtual void WriteData(IPacket packet)
        {
            packet.Write(PropOwnerID);
        }
        public virtual void ReadData(IPacket packet)
        {
            PropOwnerID = packet.ReadUShort();
        }
    }

    public class Sync : IPacketData
    {
        public bool IsReliable => true;
        public bool DropReliableDataIfNewerExists => true;
        public string PropName { get; set; }
        public string PropPath { get; set; }
        public Vector3 PropLocation { get; set; }
        public float PropRotation { get; set; }
        public float PropScale { get; set; }

        public virtual void WriteData(IPacket packet)
        {
            packet.Write(PropName);
            packet.Write(PropPath);

            packet.Write(PropLocation);
            
            packet.Write(PropRotation);
            packet.Write(PropScale);
        }

        public virtual void ReadData(IPacket packet)
        {
            PropName = packet.ReadString();
            PropPath = packet.ReadString();

            PropLocation = packet.ReadVector3();
            PropRotation = packet.ReadFloat();
            PropScale = packet.ReadFloat();
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
                case CustomPackets.PropLocation:
                    return new PropLocation();
                case CustomPackets.HideStatus:
                    return new HideStatus();
                case CustomPackets.PropFound:
                    return new PropFound();
                case CustomPackets.Sync:
                    return new Sync();
                default:
                    throw new NotImplementedException(packetID.ToString());
            }
        }
    }
}
