using PropHuntMod.Utils;
using PropHuntMod.Utils.Networking;
using SSMP.Api.Server;
using SSMP.Api.Server.Networking;
using SSMP.Game.Settings;
using SSMP.Networking.Packet;
using System.Collections.Generic;
using UnityEngine;

namespace PropHuntMod
{
    internal class ServerPlayer
    {
        public ushort id;
        public bool hidden = false;
        public string propName;
        public Vector3? propLocation;
        public float? propRotation;
        public IServerPlayer playerAvatar => PropHuntServer._serverApi.ServerManager.GetPlayer(id);
        public int swapCount = 0;
        public ServerPlayer(ushort id)
        {
            this.id = id;
        }
    }


    public class PropHuntServer : ServerAddon
    {
        protected override string Name => Config.ModName;
        protected override string Version => Config.ModVersion;
        public override uint ApiVersion => Config.SSMPApiVersion;
        public override bool NeedsNetwork => true;

        internal static bool started = false;

        public static PropHuntServer instance;


        readonly static Dictionary<ushort, ServerPlayer> players = new Dictionary<ushort, ServerPlayer>();


        internal static IServerApi _serverApi = null;

        public override void Initialize(IServerApi serverApi)
        {
            instance = this;
            _serverApi = serverApi;
            this.Logger.Info("Prop Hunt Loaded.");


            ServerNetwork.Init(serverApi, this);

            serverApi.CommandManager.RegisterCommand(new Commands.StartGameCommand());
            serverApi.CommandManager.RegisterCommand(new Commands.StopGameCommand());

            serverApi.ServerManager.PlayerConnectEvent += (IServerPlayer player) =>
            {
                GetPlayer(player.Id);
            };

            serverApi.ServerManager.PlayerDisconnectEvent += (IServerPlayer player) =>
            {
                players.Remove(player.Id);
            };

        }

        internal static ServerPlayer GetPlayer(ushort playerID)
        {
            var hasPlayer = players.TryGetValue(playerID, out var player);
            if (!hasPlayer)
            {
                player = new ServerPlayer(playerID);
                players.Add(playerID, player);
            }

            return player;
        }

        public void Announce(string announcement)
        {
            _serverApi.ServerManager.BroadcastMessage(announcement);
        }
        public void Message(ushort recipientID, string message)
        {
            _serverApi.ServerManager.SendMessage(recipientID, message);
        }
        public string DetermineWinner()
        {
            string winnerName = null;
            foreach (var player in players.Values)
            {
                if (player.propName != null)
                {
                    if (winnerName != null) return null;
                    winnerName = player.playerAvatar.Username;
                }
            }

            return winnerName;
        }
        public void ResetSwapCounts()
        {
            foreach (var player in players.Values)
            {
                player.swapCount = 0;
            }
        }
        public void GameStart()
        {
            ResetSwapCounts();
            EnsureSettings();
            ServerNetwork.BroadcastForcePropSwap();
            started = true;
            Announce("The game has begun! Good luck!");
        }
        public void GameOver(string winner, bool canceled = false)
        {
            ServerNetwork.BroadcastGameOver(winner);
            
            if (canceled) Announce("The game has been stopped!");
            else Announce($"{winner} was the last bug standing! Congrats!");
            
            started = false;
            ResetSwapCounts();            
        }

        void EnsureSettings()
        {
            var settings = ServerApi.ServerManager.ServerSettings;
            ServerSettings newSettings = new ServerSettings();

            newSettings.AllowSkins = settings.AllowSkins;
            newSettings.DisplayNames = settings.DisplayNames;
            newSettings.OnlyBroadcastMapIconWithCompass = settings.OnlyBroadcastMapIconWithCompass;

            newSettings.TeamsEnabled = settings.TeamsEnabled; // true; // teams aren't supported yet?
            newSettings.AlwaysShowMapIcons = false;
            newSettings.IsPvpEnabled = false;

            ServerApi.ServerManager.ApplyServerSettings(newSettings);
        }
    }
}
