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
                PropPosition = propPosition,
                PropRotation = propRotation
            });
        }

        public static void SendHideStatus(bool isHiding)
        {
            Log.LogInfo($"Sending hide status: {isHiding}");
            sender.SendSingleData(CustomPackets.HideStatus, new FromClient.HideStatus
            {
                IsHiding = isHiding
            });
        }

        public static void SendPropFound(ushort propOwnerID)
        {
            Log.LogInfo($"Sending prop found: {propOwnerID}");
            sender.SendSingleData(CustomPackets.PropFound, new FromClient.PropFound
            {
                PropOwnerID = propOwnerID
            });
        }

        public static void Init(IClientApi clientApi, ClientAddon clientAddon)
        {
            sender = clientApi.NetClient.GetNetworkSender<CustomPackets>(clientAddon);
            receiver = clientApi.NetClient.GetNetworkReceiver<CustomPackets>(clientAddon, FromServer.Packets.Instantiate);

            receiver.RegisterPacketHandler<FromServer.PropSwap>(CustomPackets.PropSwap, OnPropSwap);
            receiver.RegisterPacketHandler<FromServer.PropLocation>(CustomPackets.PropLocation, OnPropLocation);
            receiver.RegisterPacketHandler<FromServer.HideStatus>(CustomPackets.HideStatus, OnHideStatus);
            receiver.RegisterPacketHandler<FromServer.PropFound>(CustomPackets.PropFound, OnPropFound);
            receiver.RegisterPacketHandler<FromServer.RoundStart>(CustomPackets.RoundStart, OnRoundStart);
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

        static void OnRoundStart(FromServer.RoundStart data)
        {
            PropHuntClient.roundStarted = true;
            PropHuntClient.isSeeker = data.IsSeeker;
            PropHuntClient.propSwaps = 0;
            PropHuntClient.maxPropSwaps = data.PropSwapLimit;

            if (data.IsSeeker)
            {
                PropHuntClient.LocalMessage("You're a seeker!");
                SelfCoverManager.instance.DisableProp(false);
                return;
            }

            SelfCoverManager.instance.EnableProp();
            PropHuntClient.LocalMessage("You're hiding this round!");
        }

        public static void OnPropLocation(FromServer.PropLocation data)
        {
            PlayerManager player = PlayerManager.GetPlayerManager(data.Id);
            player.SetPropLocation(data.PropPosition, data.PropRotation);

            Log.LogInfo($"{data.Id} prop moved to {data.PropPosition}, {data.PropRotation}");
        }

        static void OnHideStatus(FromServer.HideStatus data)
        {
            PlayerManager player = PlayerManager.GetPlayerManager(data.Id);
            player.SetHideStatus(data.IsHiding);

            Log.LogInfo($"{data.Id} hiding status set to {data.IsHiding}");
        }

        static void OnPropFound(FromServer.PropFound data)
        {
            if (data.IsClientFound)
            {
                Log.LogInfo("I've been found!");
                SelfCoverManager.instance.DisableProp();
            }
            else
            {
                var player = PlayerManager.GetPlayerManager(data.PropOwnerID);
                player.SetProp("");
                Log.LogInfo($"{player.playerAvatar.Username} has been found");
            }
        }

        static void OnGameOver(FromServer.GameOver data)
        {
            PropHuntClient.roundStarted = false;
            PropHuntClient.isSeeker = false;
            PropHuntClient.propSwaps = 0;

            SelfCoverManager.instance.DisableProp();
            //string winner = data.winnerUsername;
        }
    }
}
