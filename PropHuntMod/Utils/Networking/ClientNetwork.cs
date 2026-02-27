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
        public static void SendPropSwap(Prop prop, int ticket = -1)
        {
            Log.LogInfo($"Sending prop swap: {prop?.name}");
            sender.SendSingleData(CustomPackets.PropSwap, new FromClient.PropSwap
            {
                propName = prop?.name ?? "",
                propPath = prop?.path ?? "",
                TicketID = ticket,
            });
        }

        public static void SendPropLocation(Vector3 propPosition, float propRotation, float propScale)
        {
            Log.LogInfo($"Sending prop location: {propPosition}, {propRotation}, {propScale}");
            sender.SendSingleData(CustomPackets.PropLocation, new FromClient.PropLocation
            {
                PropPosition = propPosition,
                PropRotation = propRotation,
                PropScale = propScale
            });
        }

        public static void SendHideStatus(bool isHiding, int ticket = -1)
        {
            Log.LogInfo($"Sending hide status: {isHiding}");
            sender.SendSingleData(CustomPackets.HideStatus, new FromClient.HideStatus
            {
                IsHiding = isHiding,
                TicketID = ticket
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

        public static void SendSync(string propName, string propPath, Vector3 propPosition, float propRotation, float propScale)
        {
            Log.LogInfo($"Sending sync data: {propName}, {propPosition}, {propRotation}, {propScale}");
            sender.SendSingleData(CustomPackets.Sync, new FromClient.Sync
            {
                PropName = propName,
                PropPath = propPath,
                PropLocation = propPosition,
                PropRotation = propRotation,
                PropScale = propScale
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
            receiver.RegisterPacketHandler<FromServer.FailedAction>(CustomPackets.FailedAction, ClientErrorCorrection.DiagnoseError);

            if (Config.AllowDebugFeatures) receiver.RegisterPacketHandler<FromServer.HideStatus>(CustomPackets.HideStatus, OnHideStatus);
        }

        /********************
         * PACKET RECEIVERS *
         ********************/
        static void OnPropSwap(FromServer.PropSwap data)
        {
            PlayerManager player = PlayerManager.GetPlayerManager(data.Id);
            player.SetProp(data.propName, data.propPath);

            Log.LogInfo($"{data.Id} prop set to {data.propName}");
        }

        static void OnRoundStart(FromServer.RoundStart data)
        {
            // Set game state
            bool alreadyPlaying = data.SeekerWaitTime == 0;
            if (alreadyPlaying) PropHuntClient.GameState = GameState.Playing;
            else PropHuntClient.GameState = GameState.SeekerWait;

            // Set hiding settings
            PropHuntClient.isSeeker = data.IsSeeker;
            PropHuntClient.propSwaps = 0;
            PropHuntClient.maxPropSwaps = data.PropSwapLimit;

            // Display overlays and effects
            if (data.IsSeeker)
            {
                PropHuntClient.LocalMessage("You're a seeker!");
                SelfCoverManager.instance.DisableProp(false);
                if (!alreadyPlaying)
                {
                    SelfHornetManager.instance.SetSeekerObscure(true);
                    EffectsManager.SetTitle("SEEKER", $"Wait time: {data.SeekerWaitTime} seconds", "YOUR ROLE:", true, 10);
                }
                else
                {
                    EffectsManager.SetTitle("SEEKER", $"It's go time!", "YOUR ROLE:", true, 10);
                }
                return;
            }
            else
            {
                EffectsManager.SetTitle("HIDER", "", "YOUR ROLE:");
                SelfCoverManager.instance.EnableRandomProp();
                PropHuntClient.LocalMessage("You're hiding this round!");
            }

        }

        static void OnPropLocation(FromServer.PropLocation data)
        {
            PlayerManager player = PlayerManager.GetPlayerManager(data.Id);
            player.SetPropLocation(data.PropPosition, data.PropRotation, data.PropScale);

            Log.LogInfo($"{data.Id} prop moved to {data.PropPosition}, {data.PropRotation}, {data.PropScale}");
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
                SelfCoverManager.instance.FindProp(SelfHornetManager.instance);
                //SelfCoverManager.instance.DisableProp();
                if (PropHuntClient.GameState == GameState.Playing) PropHuntClient.isSeeker = true;
            }
            else
            {
                var player = PlayerManager.GetPlayerManager(data.PropOwnerID);
                player.coverManager.FindProp(player.hornetManager);
                player.SetProp("", "");
                Log.LogInfo($"{player.PlayerAvatar.Username} has been found");
            }
            EffectsManager.PlayFoundSound(data.IsClientFound);
        }

        static void OnGameOver(FromServer.GameOver data)
        {
            PropHuntClient.GameState = GameState.NotStarted;
            PropHuntClient.isSeeker = false;
            PropHuntClient.propSwaps = 0;

            SelfHornetManager.instance.SetSeekerObscure(false);
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
            PropHuntClient.GameState = GameState.Playing;

            if (!PropHuntClient.isSeeker) return;
            SelfHornetManager.instance.SetSeekerObscure(false);
        }
    }
}
