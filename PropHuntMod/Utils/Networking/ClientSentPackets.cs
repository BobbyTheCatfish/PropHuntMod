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
        public Vector3 propPosition { get; set; }
        public float propRotation { get; set; }
        public virtual void WriteData(IPacket packet)
        {
            packet.Write(propPosition.x);
            packet.Write(propPosition.y);
            packet.Write(propPosition.z);

            packet.Write(propRotation);
        }

        public virtual void ReadData(IPacket packet)
        {
            propPosition = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());
            propRotation = packet.ReadFloat();
        }
    }
    public class HideStatus : IPacketData
    {
        public bool IsReliable => true;
        public bool DropReliableDataIfNewerExists => true;
        public bool isHiding { get; set; }

        public virtual void WriteData(IPacket packet)
        {
            packet.Write(isHiding);
        }

        public virtual void ReadData(IPacket packet)
        {
            isHiding = packet.ReadBool();
        }
    }
    public class PropFound : IPacketData
    {
        public bool IsReliable => true;
        public bool DropReliableDataIfNewerExists => true;
        public ushort propOwnerID { get; set; }
        public virtual void WriteData(IPacket packet)
        {
            packet.Write(propOwnerID);
        }
        public virtual void ReadData(IPacket packet)
        {
            propOwnerID = packet.ReadUShort();
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
