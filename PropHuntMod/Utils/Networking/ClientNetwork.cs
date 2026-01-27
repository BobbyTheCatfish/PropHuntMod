using PropHuntMod.Modifications;
using SSMP.Api.Client;
using SSMP.Api.Client.Networking;
using UnityEngine;

namespace PropHuntMod.Utils.Networking
{
    static class ClientNetwork
    {
        static IClientAddonNetworkSender<CustomPackets> sender;
        static IClientAddonNetworkReceiver<CustomPackets> receiver;

        /******************
         * PACKET SENDERS *
         ******************/
        public static void SendPropSwap(string propName)
        {
            Log.LogInfo($"Sending prop swap: {propName}");
            sender.SendSingleData(CustomPackets.PropSwap, new FromClient.PropSwap
            {
                propName = propName
            });
        }

        public static void SendPropLocation(Vector3 propPosition, float propRotation)
        {
            Log.LogInfo($"Sending prop location: {propPosition}, {propRotation}");
            sender.SendSingleData(CustomPackets.PropLocation, new FromClient.PropLocation
            {
                propPosition = propPosition,
                propRotation = propRotation
            });
        }

        public static void SendHideStatus(bool isHiding)
        {
            Log.LogInfo($"Sending hide status: {isHiding}");
            sender.SendSingleData(CustomPackets.HideStatus, new FromClient.HideStatus
            {
                isHiding = isHiding
            });
        }

        public static void SendPropFound(ushort propOwnerID)
        {
            Log.LogInfo($"Sending prop found: {propOwnerID}");
            sender.SendSingleData(CustomPackets.PropFound, new FromClient.PropFound
            {
                propOwnerID = propOwnerID
            });
        }

        public static void Init(IClientApi clientApi, ClientAddon clientAddon)
        {
            sender = clientApi.NetClient.GetNetworkSender<CustomPackets>(clientAddon);
            receiver = clientApi.NetClient.GetNetworkReceiver<CustomPackets>(clientAddon, FromServer.Packets.Instantiate);

            receiver.RegisterPacketHandler<FromServer.PropSwap>(CustomPackets.PropSwap, OnPropSwap);
            receiver.RegisterPacketHandler<FromServer.ForcePropSwap>(CustomPackets.ForcePropSwap, OnForcePropSwap);
            receiver.RegisterPacketHandler<FromServer.PropLocation>(CustomPackets.PropLocation, OnPropLocation);
            receiver.RegisterPacketHandler<FromServer.HideStatus>(CustomPackets.HideStatus, OnHideStatus);
            receiver.RegisterPacketHandler<FromServer.PropFound>(CustomPackets.PropFound, OnPropFound);
            receiver.RegisterPacketHandler<FromServer.GameOver>(CustomPackets.GameOver, OnGameOver);
        }

        /********************
         * PACKET RECEIVERS *
         ********************/
        static void OnPropSwap(FromServer.PropSwap data)
        {
            PlayerManager player = PlayerManager.GetPlayerManager(data.Id);
            player.SetProp(data.propName);

            Log.LogInfo($"{data.Id} prop set to {data.propName}");
        }

        static void OnForcePropSwap(FromServer.ForcePropSwap data)
        {
            SelfCoverManager.instance.EnableProp();
            Log.LogInfo("Forced prop sawp");
        }

        public static void OnPropLocation(FromServer.PropLocation data)
        {
            PlayerManager player = PlayerManager.GetPlayerManager(data.Id);
            player.SetPropLocation(data.propPosition, data.propRotation);

            Log.LogInfo($"{data.Id} prop moved to {data.propPosition}, {data.propRotation}");
        }

        static void OnHideStatus(FromServer.HideStatus data)
        {
            PlayerManager player = PlayerManager.GetPlayerManager(data.Id);
            player.SetHideStatus(data.isHiding);

            Log.LogInfo($"{data.Id} hiding status set to {data.isHiding}");
        }

        static void OnPropFound(FromServer.PropFound data)
        {
            if (data.isClientFound)
            {
                Log.LogInfo("I've been found!");
                SelfCoverManager.instance.DisableProp();
            }
            else
            {
                var player = PlayerManager.GetPlayerManager(data.propOwnerID);
                player.SetProp("");
                Log.LogInfo($"{player.playerAvatar.Username} has been found");
            }
        }

        static void OnGameOver(FromServer.GameOver data)
        {
            SelfCoverManager.instance.DisableProp();
            //string winner = data.winnerUsername;
        }
    }
}
