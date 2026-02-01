using PropHuntMod.Modifications;
using SSMP.Api.Server;
using SSMP.Api.Server.Networking;
using SSMP.Networking.Packet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PropHuntMod.Utils.Networking
{
    static class ServerNetwork
    {
        static IServerAddonNetworkSender<CustomPackets> sender;
        static IServerAddonNetworkReceiver<CustomPackets> receiver;

        public static void Broadcast(ushort senderID, CustomPackets packetID, IPacketData data)
        {
            foreach (var player in PropHuntServer._serverApi.ServerManager.Players)
            {
                if (player.Id == senderID) continue;

                sender.SendSingleData(packetID, data, player.Id);
            }
        }

        /******************
         * PACKET SENDERS *
         ******************/
        public static void ForwardPropSwap(ushort id, string propName)
        {
            Log.LogInfo($"Broadcasting prop swap from {id}: {propName}");
            Broadcast(id, CustomPackets.PropSwap, new FromServer.PropSwap
            {
                Id = id,
                propName = propName
            });
        }

        public static void BroadcastRoundStart()
        {
            Log.LogInfo("Broadcasting round start");
            foreach (var player in PropHuntServer._serverApi.ServerManager.Players)
            {
                SendRoundStart(player.Id);
            }
        }

        public static void SendRoundStart(ushort id)
        {
            var player = PropHuntServer.GetPlayer(id);
            var data = new FromServer.RoundStart
            {
                IsSeeker = player.seeker,
                PropSwapLimit = Config.MaxSwapCount
            };

            sender.SendSingleData(CustomPackets.RoundStart, data);
        }


        public static void ForwardPropLocation(ushort id, Vector3 propPosition, float propRotation)
        {
            Log.LogInfo($"Broadcasting prop location from {id}: {propPosition}, {propRotation}");

            BaseCoverManager.ConstrainPropLocation(ref propPosition, ref propRotation);
            Broadcast(id, CustomPackets.PropLocation, new FromServer.PropLocation
            {
                Id= id,
                PropPosition = propPosition,
                PropRotation = propRotation
            });
        }

        public static void ForwardHideStatus(ushort id, bool isHiding)
        {
            Log.LogInfo($"Broadcasting hide status from {id}: {isHiding}");
            Broadcast(id, CustomPackets.HideStatus, new FromServer.HideStatus
            {
                Id = id,
                IsHiding = isHiding
            });
        }

        public static void ForwardPropFound(ushort id, ushort propOwnerID)
        {
            Log.LogInfo($"Broadcasting prop found from {id}: {propOwnerID}");
            foreach (var player in PropHuntServer._serverApi.ServerManager.Players)
            {
                if (player.Id == id) continue;

                FromServer.PropFound sendData = new FromServer.PropFound
                {
                    IsClientFound = player.Id == propOwnerID,
                    PropOwnerID = propOwnerID,
                };

                sender.SendSingleData(CustomPackets.PropFound, sendData, player.Id);
            }
        }

        public static void BroadcastGameOver(string winner)
        {
            Log.LogInfo("Broadcasting game over");
            sender.BroadcastSingleData(CustomPackets.GameOver, new FromServer.GameOver
            {
                WinnerUsername = winner
            });
        }

        public static void Init(IServerApi serverApi, ServerAddon serverAddon)
        {
            sender = serverApi.NetServer.GetNetworkSender<CustomPackets>(serverAddon);
            receiver = serverApi.NetServer.GetNetworkReceiver<CustomPackets>(serverAddon, FromClient.Packets.Instantiate);

            receiver.RegisterPacketHandler<FromClient.PropSwap>(CustomPackets.PropSwap, OnPropSwap);
            receiver.RegisterPacketHandler<FromClient.PropLocation>(CustomPackets.PropLocation, OnPropLocation);
            receiver.RegisterPacketHandler<FromClient.HideStatus>(CustomPackets.HideStatus, OnHideStatus);
            receiver.RegisterPacketHandler<FromClient.PropFound>(CustomPackets.PropFound, OnPropFound);
        }

        /********************
         * PACKET RECEIVERS *
         ********************/
        static void OnPropSwap(ushort id, FromClient.PropSwap data)
        {
            var player = PropHuntServer.GetPlayer(id);

            if (Config.MaxSwapCount > 0 && player.swapCount >= Config.MaxSwapCount && PropHuntServer.started)
            {
                PropHuntServer.instance.Message(id, "You ran out of prop swaps and are de-synced! Is your mod/config up to date?");
                return;
            }

            if (player.seeker)
            {
                PropHuntServer.instance.Message(id, "You're a seeker! You can't hide this round.");
                return;
            }

            player.propName = string.IsNullOrEmpty(data.propName) ? null : data.propName;
            player.swapCount++;

            ForwardPropSwap(id, data.propName);
        }

        static void OnPropLocation(ushort id, FromClient.PropLocation data)
        {
            var player = PropHuntServer.GetPlayer(id);

            if (player.seeker)
            {
                PropHuntServer.instance.Message(id, "You're a seeker! You can't hide this round.");
                return;
            }

            player.propLocation = data.PropPosition;
            player.propRotation = data.PropRotation;

            ForwardPropLocation(id, data.PropPosition, data.PropRotation);
        }

        static void OnHideStatus(ushort id, FromClient.HideStatus data)
        {
            var player = PropHuntServer.GetPlayer(id);
            if (player.seeker && data.IsHiding)
            {
                PropHuntServer.instance.Message(id, "You're a seeker! You can't hide this round.");
                return;
            }
            player.hidden = data.IsHiding;
            ForwardHideStatus(id, data.IsHiding);
        }

        static void OnPropFound(ushort id, FromClient.PropFound data)
        {
            var owner = PropHuntServer.GetPlayer(data.PropOwnerID);
            var finder = PropHuntServer.GetPlayer(id);

            if (PropHuntServer.started && owner.seeker)
            {
                PropHuntServer.instance.Message(id, "That player is a seeker!");
                return;
            }
            else if (PropHuntServer.started && !finder.seeker)
            {
                PropHuntServer.instance.Message(id, "You're not a seeker! You can't find people this round.");
                return;
            }

            owner.propName = null;
            owner.propLocation = Vector3.zero;
            owner.propRotation = 0;
            owner.seeker = true;

            ForwardPropFound(id, data.PropOwnerID);

            
            PropHuntServer.instance.Announce($"{owner.PlayerAvatar.Username} was found by {finder.PlayerAvatar.Username}!");

            if (PropHuntServer.started)
            {
                PropHuntServer.instance.CheckGameOver(owner.PlayerAvatar.Username);
            }
        }
    }
}
