using PropHuntMod.Modifications;
using SSMP.Api.Client;
using SSMP.Api.Client.Networking;
using System.Reflection;
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

        public static void SendSync(string propName, Vector3 propPosition, float propRotation)
        {
            Log.LogInfo($"Sending sync data: {propName}, {propPosition}, {propRotation}");
            sender.SendSingleData(CustomPackets.Sync, new FromClient.Sync
            {
                PropName = propName,
                PropLocation = propPosition,
                PropRotation = propRotation
            });
        }

        public static void Init(IClientApi clientApi, ClientAddon clientAddon)
        {
            sender = clientApi.NetClient.GetNetworkSender<CustomPackets>(clientAddon);
            receiver = clientApi.NetClient.GetNetworkReceiver<CustomPackets>(clientAddon, FromServer.Packets.Instantiate);

            receiver.RegisterPacketHandler<FromServer.PropSwap>(CustomPackets.PropSwap, OnPropSwap);
            receiver.RegisterPacketHandler<FromServer.PropLocation>(CustomPackets.PropLocation, OnPropLocation);
            receiver.RegisterPacketHandler<FromServer.PropFound>(CustomPackets.PropFound, OnPropFound);
            receiver.RegisterPacketHandler<FromServer.RoundStart>(CustomPackets.RoundStart, OnRoundStart);
            receiver.RegisterPacketHandler<FromServer.GameOver>(CustomPackets.GameOver, OnGameOver);
            receiver.RegisterPacketHandler<FromServer.SeekerStart>(CustomPackets.SeekerStart, OnSeekerStart);

            if (Config.AllowDebugFeatures) receiver.RegisterPacketHandler<FromServer.HideStatus>(CustomPackets.HideStatus, OnHideStatus);
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

        public static void OnRoundStart(FromServer.RoundStart data)
        {
            PropHuntClient.roundStarted = true;
            PropHuntClient.isSeeker = data.IsSeeker;
            PropHuntClient.propSwaps = 0;
            PropHuntClient.maxPropSwaps = data.PropSwapLimit;

            if (data.IsSeeker)
            {
                PropHuntClient.LocalMessage("You're a seeker!");
                SelfHornetManager.instance.SetSeekerObscure(true);
                SelfCoverManager.instance.DisableProp(false);
                EffectsManager.SetTitle("SEEKER", $"Wait time: {data.SeekerWaitTime} seconds", "YOUR ROLE:", true, 10);
                return;
            }
            else
            {
                EffectsManager.SetTitle("HIDER", "", "YOUR ROLE:");
            }

            SelfCoverManager.instance.EnableProp();
            PropHuntClient.LocalMessage("You're hiding this round!");
        }

        static void OnPropLocation(FromServer.PropLocation data)
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
                if (PropHuntClient.roundStarted) PropHuntClient.isSeeker = true;
            }
            else
            {
                var player = PlayerManager.GetPlayerManager(data.PropOwnerID);
                player.SetProp("");
                Log.LogInfo($"{player.PlayerAvatar.Username} has been found");
            }
            EffectsManager.PlayFoundSound(data.IsClientFound);
        }

        static void OnGameOver(FromServer.GameOver data)
        {
            PropHuntClient.roundStarted = false;
            PropHuntClient.isSeeker = false;
            PropHuntClient.propSwaps = 0;

            SelfCoverManager.instance.DisableProp();
            EffectsManager.PlayGameOverSound(!data.WasCanceled && data.IsWinner);

            var mainText = data.WasCanceled ? "Round Canceled" : data.IsWinner ? "You Won!" : "Round Complete";
            var subText = data.IsWinner || data.WasCanceled ? "" : $"{data.WinnerUsername} won!";
            EffectsManager.SetTitle(mainText, subText);
            if (data.IsWinner) EffectsManager.PlayConfetti();
            GameManager.instance.FreezeMoment(GlobalEnums.FreezeMomentTypes.BossDeathSlow);
        }

        static void OnSeekerStart(FromServer.SeekerStart data)
        {
            if (!PropHuntClient.isSeeker)
            {
                Log.LogFatal("OnSeekerStart received, but I'm not a seeker.");
                //return;
            }

            SelfHornetManager.instance.SetSeekerObscure(false);
        }
    }
}
