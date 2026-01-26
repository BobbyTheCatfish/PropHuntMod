using PropHuntMod.Modifications;
using SSMP.Api.Server;
using SSMP.Api.Server.Networking;
using SSMP.Networking.Packet;
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

        public static void BroadcastForcePropSwap()
        {
            Log.LogInfo("Broadcasting force prop swap");
            sender.BroadcastSingleData(CustomPackets.ForcePropSwap, new FromServer.ForcePropSwap());
        }

        public static void ForwardPropLocation(ushort id, Vector3 propPosition, float propRotation)
        {
            Log.LogInfo($"Broadcasting prop location from {id}: {propPosition}, {propRotation}");
            Broadcast(id, CustomPackets.PropLocation, new FromServer.PropLocation
            {
                Id= id,
                propPosition = propPosition,
                propRotation = propRotation
            });
        }

        public static void ForwardHideStatus(ushort id, bool isHiding)
        {
            Log.LogInfo($"Broadcasting hide status from {id}: {isHiding}");
            Broadcast(id, CustomPackets.HideStatus, new FromServer.HideStatus
            {
                Id = id,
                isHiding = isHiding
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
                    isClientFound = player.Id == propOwnerID,
                    propOwnerID = propOwnerID,
                };

                sender.SendSingleData(CustomPackets.PropFound, sendData, player.Id);
            }
        }

        public static void BroadcastGameOver(string winner)
        {
            Log.LogInfo("Broadcasting game over");
            sender.BroadcastSingleData(CustomPackets.GameOver, new FromServer.GameOver
            {
                winnerUsername = winner
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

            PropHuntServer.started = true;
            player.propName = string.IsNullOrEmpty(data.propName) ? null : data.propName;
            player.swapCount++;

            ForwardPropSwap(id, data.propName);
        }

        static void OnPropLocation(ushort id, FromClient.PropLocation data)
        {
            var player = PropHuntServer.GetPlayer(id);
            player.propLocation = data.propPosition;
            player.propRotation = data.propRotation;

            ForwardPropLocation(id, data.propPosition, data.propRotation);
        }

        static void OnHideStatus(ushort id, FromClient.HideStatus data)
        {
            PropHuntServer.GetPlayer(id).hidden = data.isHiding;
            ForwardHideStatus(id, data.isHiding);
        }

        static void OnPropFound(ushort id, FromClient.PropFound data)
        {
            var owner = PropHuntServer.GetPlayer(data.propOwnerID);
            owner.propName = null;
            owner.propLocation = null;
            owner.propRotation = null;

            ForwardPropFound(id, data.propOwnerID);

            var finder = PropHuntServer.GetPlayer(id);
            PropHuntServer.instance.Announce($"{owner.playerAvatar.Username} was found by {finder.playerAvatar.Username}!");

            string winner = PropHuntServer.instance.DetermineWinner();
            if (winner != null)
            {
                PropHuntServer.instance.GameOver(winner);
            }
        }
    }
}
