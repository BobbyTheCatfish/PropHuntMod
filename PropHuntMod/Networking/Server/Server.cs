using PropHuntMod.Utils;
using PropHuntMod.Networking;
using SSMP.Api.Server;
using SSMP.Game.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PropHuntMod.Networking.Server
{
    internal class Server : ServerAddon
    {
        protected override string Name => Config.ModName;
        protected override string Version => Config.ModVersion;
        public override uint ApiVersion => Config.SSMPApiVersion;
        public override bool NeedsNetwork => true;

        public static Server instance;

        public static GameState GameState = GameState.NotStarted;
        public SeekerTimer SeekerTimer { get; private set; }

        readonly static Dictionary<ushort, ServerPlayer> players = new Dictionary<ushort, ServerPlayer>();


        internal static IServerApi api = null;

        public override void Initialize(IServerApi serverApi)
        {
            instance = this;
            api = serverApi;
            Log.SetLogger(Logger);
            Log.LogInfo("Prop Hunt Server Loaded.");

            Reset();

            ServerNetwork.Init(serverApi, this);

            serverApi.CommandManager.RegisterCommand(new Commands.ServerCommands());

            serverApi.ServerManager.PlayerConnectEvent += player =>
            {
                var p = GetPlayer(player.Id);
                if (GameState == GameState.SeekerWait)
                {
                    p.seeker = true;
                    ServerNetwork.SendRoundStart(player.Id, GameState == GameState.Playing);
                }
            };

            serverApi.ServerManager.PlayerDisconnectEvent += player =>
            {
                players.Remove(player.Id);

                CheckGameOver(player);
            };
        }

        public void Reset(bool disconnect = false)
        {
            Log.LogInfo("Resetting server");
            GameState = GameState.NotStarted;
            SeekerTimer?.CancelTimer();
            SeekerTimer = null;

            if (disconnect)
            {
                foreach (var player in api.ServerManager.Players)
                {
                    api.ServerManager.DisconnectPlayer(player.Id, SSMP.Networking.Packet.Data.DisconnectReason.Shutdown);
                }
            }

            players.Clear();
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
            Log.LogDebug($"Announcement: {announcement}");
            api.ServerManager.BroadcastMessage(announcement);
        }
        public void Message(ushort recipientID, string message)
        {
            Log.LogDebug($"Message to {recipientID}: {message}");
            api.ServerManager.SendMessage(recipientID, message);
        }
        public string DetermineWinner()
        {
            string winnerName = null;
            foreach (var player in players.Values)
            {
                if (!string.IsNullOrEmpty(player.propName))
                {
                    if (winnerName != null) return null;
                    winnerName = player.PlayerAvatar.Username;
                }
            }

            return winnerName;
        }
        void PickSeekers()
        {
            var count = Mathf.Clamp(Config.SeekerCount, 1, players.Count - 1);
            Log.LogDebug($"Choosing {count} seekers");
            for (int i = 0; i < count; i++)
            {
                var validSeekers = players.Values.Where(p => p.seeker == false).ToList();
                if (validSeekers.Count == 0)
                {
                    Log.LogDebug($"Ran out of seekers to choose (Picked {i}/{count})");
                    return;
                }

                var seeker = validSeekers.GetRandomElement();
                seeker.seeker = true;
                Log.LogDebug($"{seeker.PlayerAvatar.Username} is a seeker");
            }
        }
        void ResetPlayers()
        {
            foreach (var player in players.Values)
            {
                player.seeker = false;
                player.hidden = false;
                player.swapCount = 0;
                player.ResetProp();
            }
        }
        public void GameStart()
        {
            if (players.Count < 2)
            {
                Announce("...Well that's awkward. I can't start a game without two people!");
                return;
            }
            ResetPlayers();
            PickSeekers();

            EnsureSettings();
            
            GameState = GameState.SeekerWait;
            int seekerCountdown = Config.SeekerCountdown;
            Announce($"The game has begun! Hiders have a {seekerCountdown} second head start. Good luck!");
            
            SeekerTimer = new SeekerTimer(seekerCountdown);
            ServerNetwork.BroadcastRoundStart();
            

        }
        public void CheckGameOver(IServerPlayer playerHit, bool canceled = false)
        {
            if (GameState == GameState.NotStarted) return;

            bool winner = players.Values.All(p => string.IsNullOrEmpty(p.propName));
            if (!winner && !canceled) return;

            ServerNetwork.BroadcastGameOver(playerHit, canceled);

            if (canceled)
            {
                Announce("The game has been stopped early!");
                SeekerTimer?.CancelTimer();
            }
            else Announce($"{playerHit.Username} was the last bug standing! Congrats!");
            
            GameState = GameState.NotStarted;

            ResetPlayers();
        }

        void EnsureSettings()
        {
            var settings = ServerApi.ServerManager.ServerSettings;
            ServerSettings newSettings = new ServerSettings
            {
                AllowSkins = settings.AllowSkins,
                DisplayNames = settings.DisplayNames,
                OnlyBroadcastMapIconWithCompass = settings.OnlyBroadcastMapIconWithCompass,

                TeamsEnabled = settings.TeamsEnabled, // true; // teams aren't supported yet?
                AlwaysShowMapIcons = false,
                IsPvpEnabled = false
            };

            ServerApi.ServerManager.ApplyServerSettings(newSettings);
        }
    }
}
