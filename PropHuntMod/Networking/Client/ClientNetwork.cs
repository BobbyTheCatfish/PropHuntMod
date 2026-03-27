using PropHuntMod.Players;
using PropHuntMod.Props;
using PropHuntMod.Utils;
using SSMP.Api.Client;
using SSMP.Api.Client.Networking;
using SSMP.Networking.Packet;
using Vector3 = SSMP.Math.Vector3;

namespace PropHuntMod.Networking.Client
{
    internal static class ClientNetwork
    {
        static IClientApi api;
        static IClientAddonNetworkSender<CustomPackets> sender;
        static IClientAddonNetworkReceiver<CustomPackets> receiver;

        /******************
         * PACKET SENDERS *
         ******************/

        static void SendData(CustomPackets packetId, IPacketData data)
        {
            if (api.NetClient.IsConnected && sender != null)
            {
                sender.SendSingleData(packetId, data);
            }
        }

        static void SendCollectionData(CustomPackets packetId, Packet data)
        {
            if (api.NetClient.IsConnected && sender != null)
            {
                sender.SendCollectionData(packetId, data);
            }
        }

        public static void SendPropSwap(Prop prop, int ticket = -1)
        {
            Log.LogInfo($"Sending prop swap: {prop?.name}");
            SendData(CustomPackets.PropSwap, new PropSwap
            {
                propName = prop?.name ?? "",
                propPath = prop?.path ?? "",
                TicketID = ticket,
            });
        }

        public static void SendPropLocation(Vector3 propPosition, float propRotation, float propScale)
        {
            Log.LogInfo($"Sending prop location: {propPosition}, {propRotation}, {propScale}");
            SendData(CustomPackets.PropLocation, new PropLocation
            {
                PropPosition = propPosition,
                PropRotation = propRotation,
                PropScale = propScale
            });
        }

        public static void SendHideStatus(bool isHiding, int ticket = -1)
        {
            Log.LogInfo($"Sending hide status: {isHiding}");
            SendData(CustomPackets.HideStatus, new HideStatus
            {
                IsHiding = isHiding,
                TicketID = ticket
            });
        }

        public static void SendPropFound(ushort propOwnerID)
        {
            Log.LogInfo($"Sending prop found: {propOwnerID}");
            SendCollectionData(CustomPackets.PropFound, new PropFound
            {
                PropOwnerID = propOwnerID
            });
        }

        public static void SendSync(string propName, string propPath, Vector3 propPosition, float propRotation, float propScale)
        {
            Log.LogInfo($"Sending sync data: {propName}, {propPosition}, {propRotation}, {propScale}");
            SendData(CustomPackets.Sync, new Sync
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
            api = clientApi;
            sender = clientApi.NetClient.GetNetworkSender<CustomPackets>(clientAddon);
            receiver = clientApi.NetClient.GetNetworkReceiver<CustomPackets>(clientAddon, Server.Packets.Instantiate);

            receiver.RegisterPacketHandler<Server.PropSwap>(CustomPackets.PropSwap, OnPropSwap);
            receiver.RegisterPacketHandler<Server.PropLocation>(CustomPackets.PropLocation, OnPropLocation);
            receiver.RegisterPacketHandler<Server.PropFound>(CustomPackets.PropFound, OnPropFound);
            receiver.RegisterPacketHandler<Server.RoundStart>(CustomPackets.RoundStart, OnRoundStart);
            receiver.RegisterPacketHandler<Server.GameOver>(CustomPackets.GameOver, OnGameOver);
            receiver.RegisterPacketHandler<Server.SeekerStart>(CustomPackets.SeekerStart, OnSeekerStart);
            receiver.RegisterPacketHandler<Server.FailedAction>(CustomPackets.FailedAction, ClientErrorCorrection.DiagnoseError);

            if (Config.AllowDebugFeatures) receiver.RegisterPacketHandler<Server.HideStatus>(CustomPackets.HideStatus, OnHideStatus);
        }

        /********************
         * PACKET RECEIVERS *
         ********************/
        static void OnPropSwap(Server.PropSwap data)
        {
            PlayerManager player = PlayerManager.GetPlayerManager(data.Id);
            player.SetProp(data.propName, data.propPath);

            Log.LogInfo($"{data.Id} prop set to {data.propName}");
        }

        static void OnRoundStart(Server.RoundStart data)
        {
            // Set game state
            bool alreadyPlaying = data.SeekerWaitTime == 0;
            if (alreadyPlaying) Client.GameState = GameState.Playing;
            else Client.GameState = GameState.SeekerWait;

            // Set hiding settings
            Client.isSeeker = data.IsSeeker;
            Client.propSwaps = 0;
            Client.maxPropSwaps = data.PropSwapLimit;
            Client.seekerAttackCooldown = data.SeekerAttackCooldown;

            // Display overlays and effects
            if (data.IsSeeker)
            {
                Client.LocalMessage("You're a seeker!");
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
                Client.LocalMessage("You're hiding this round!");
            }

        }

        static void OnPropLocation(Server.PropLocation data)
        {
            PlayerManager player = PlayerManager.GetPlayerManager(data.Id);
            player.SetPropLocation(data.PropPosition, data.PropRotation, data.PropScale);

            Log.LogInfo($"{data.Id} prop moved to {data.PropPosition}, {data.PropRotation}, {data.PropScale}");
        }

        static void OnHideStatus(Server.HideStatus data)
        {
            PlayerManager player = PlayerManager.GetPlayerManager(data.Id);
            player.SetHideStatus(data.IsHiding);

            Log.LogInfo($"{data.Id} hiding status set to {data.IsHiding}");
        }

        static void OnPropFound(Server.PropFound data)
        {
            if (data.IsClientFound)
            {
                Log.LogInfo("I've been found!");
                SelfCoverManager.instance.FindProp(SelfHornetManager.instance);
                //SelfCoverManager.instance.DisableProp();
                if (Client.GameState == GameState.Playing) Client.isSeeker = true;
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

        static void OnGameOver(Server.GameOver data)
        {
            Client.GameState = GameState.NotStarted;
            Client.isSeeker = false;
            Client.propSwaps = 0;

            SelfHornetManager.instance.SetSeekerObscure(false);
            SelfCoverManager.instance.DisableProp();
            EffectsManager.PlayGameOverSound(!data.WasCanceled && data.IsWinner);

            var mainText = data.WasCanceled ? "Round Canceled" : data.IsWinner ? "You Won!" : "Round Complete";
            var subText = data.IsWinner || data.WasCanceled ? "" : $"{data.WinnerUsername} won!";
            EffectsManager.SetTitle(mainText, subText);

            if (data.IsWinner) EffectsManager.PlayConfetti();
            GameManager.instance.FreezeMoment(GlobalEnums.FreezeMomentTypes.BossDeathSlow);
        }

        static void OnSeekerStart(Server.SeekerStart data)
        {
            Client.GameState = GameState.Playing;

            if (!Client.isSeeker) return;
            SelfHornetManager.instance.SetSeekerObscure(false);
        }
    }
}
