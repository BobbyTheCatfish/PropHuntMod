using SSMP.Networking.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PropHuntMod.Utils.Networking.FromClient
{
    public class PropSwap : IPacketData
    {
        public bool IsReliable => true;
        public bool DropReliableDataIfNewerExists => true;
        public string propName = "";
        public virtual void WriteData(IPacket packet)
        {
            packet.Write(propName);
        }
        public virtual void ReadData(IPacket packet)
        {
            propName = packet.ReadString();
        }
    }
    public class PropLocation : IPacketData
    {
        public bool IsReliable => true;
        public bool DropReliableDataIfNewerExists => true;
        public Vector3 PropPosition { get; set; }
        public float PropRotation { get; set; }
        public virtual void WriteData(IPacket packet)
        {
            packet.Write(PropPosition.x);
            packet.Write(PropPosition.y);
            packet.Write(PropPosition.z);

            packet.Write(PropRotation);
        }

        public virtual void ReadData(IPacket packet)
        {
            PropPosition = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());
            PropRotation = packet.ReadFloat();
        }
    }
    public class HideStatus : IPacketData
    {
        public bool IsReliable => true;
        public bool DropReliableDataIfNewerExists => true;
        public bool IsHiding { get; set; }

        public virtual void WriteData(IPacket packet)
        {
            packet.Write(IsHiding);
        }

        public virtual void ReadData(IPacket packet)
        {
            IsHiding = packet.ReadBool();
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
                default:
                    throw new NotImplementedException(packetID.ToString());
            }
        }
    }
}
