using PropHuntMod.Utils;
using PropHuntMod.Utils.Networking;
using SSMP.Api.Server;
using SSMP.Api.Server.Networking;
using SSMP.Game.Settings;
using SSMP.Networking.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using UnityEngine;

namespace PropHuntMod
{
    internal class ServerPlayer
    {
        public ushort id;
        public bool hidden = false;
        public bool seeker = false;
        public string propName;
        public Vector3 propLocation = Vector3.zero;
        public float propRotation = 0;
        public IServerPlayer PlayerAvatar => PropHuntServer._serverApi.ServerManager.GetPlayer(id);
        public int swapCount = 0;
        public ServerPlayer(ushort id)
        {
            this.id = id;
        }
    }

    public class SeekerTimer
    {
        int seconds = 5;
        Timer timer;

        void SetTimer(int seconds, Action cb)
        {
            timer = new Timer(seconds * 1000);
            timer.Elapsed += new ElapsedEventHandler((a, b) => {
                timer.Stop();
                timer = null;
                cb.Invoke();
            });

            timer.AutoReset = false;
            timer.Start();
        }

        void SetSecondsTimer()
        {
            if (seconds == 0)
            {
                PropHuntServer.instance.Announce("[Seekers]: Ready or not, here we come!");
                ServerNetwork.BroadcastSeekerStart();
            }
            else
            {
                PropHuntServer.instance.Announce($"[Seekers]: {seconds}...");
                SetTimer(1, () => SetSecondsTimer());
                seconds--;
            }
        }

        public SeekerTimer(int seconds)
        {
            SetTimer(seconds, SetSecondsTimer);
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


        SeekerTimer seekerTimer;

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

            serverApi.ServerManager.PlayerConnectEvent += player =>
            {
                var p = GetPlayer(player.Id);
                if (started)
                {
                    p.seeker = true;
                    ServerNetwork.SendRoundStart(player.Id);
                }
            };

            serverApi.ServerManager.PlayerDisconnectEvent += player =>
            {
                players.Remove(player.Id);
                CheckGameOver(player.Username);
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
                if (!string.IsNullOrEmpty(player.propName))
                {
                    if (winnerName != null) return null;
                    winnerName = player.PlayerAvatar.Username;
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
        void PickSeekers(int count = 1)
        {
            ResetSeekers();
            count = Mathf.Clamp(count, 1, players.Count - 1);
            Log.LogInfo($"Choosing {count} seekers");
            for (int i = 0; i < count; i++)
            {
                var validSeekers = players.Values.Where(p => p.seeker == false).ToList();
                if (validSeekers.Count == 0)
                {
                    Log.LogInfo($"Ran out of seekers to choose (Picked {i}/{count})");
                    return;
                }

                var seeker = validSeekers.GetRandomElement();
                seeker.seeker = true;
                Log.LogDebug($"{seeker} is a seeker");
            }
        }
        void ResetSeekers()
        {
            foreach (var player in players.Values)
            {
                player.seeker = false;
            }
        }
        public void GameStart()
        {
            if (players.Count < 2)
            {
                Announce("...Well that's awkward. I can't start a game without two people!");
                return;
            }
            ResetSwapCounts();
            PickSeekers();

            EnsureSettings();
            
            started = true;
            int seekerCountdown = Config.SeekerCountdown;
            Announce($"The game has begun! Hiders have a {seekerCountdown} second head start. Good luck!");
            
            ServerNetwork.BroadcastRoundStart();
            
            seekerTimer = new SeekerTimer(seekerCountdown);

        }
        public void CheckGameOver(string usernameHit, bool canceled = false)
        {
            bool winner = players.Values.All(p => string.IsNullOrEmpty(p.propName));
            if (!winner && !canceled) return;

            ServerNetwork.BroadcastGameOver(usernameHit);
            
            if (canceled) Announce("The game has been stopped early!");
            else Announce($"{usernameHit} was the last bug standing! Congrats!");
            
            started = false;
            ResetSwapCounts();
            ResetSeekers();
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
