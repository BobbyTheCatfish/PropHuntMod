using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using PropHuntMod.Utils;
using SSMP.Api.Client;
using SSMP.Game;
using System.Linq;
using PropHuntMod.Utils.Networking;
using PropHuntMod.Modifications;
using UnityEngine.SceneManagement;

namespace PropHuntMod
{
    public enum Teams
    {
        Hunter = SSMP.Game.Team.Lifeblood,
        Seeker = SSMP.Game.Team.Grimm
    }

    public class PropHuntClient : ClientAddon
    {
        protected override string Name => Config.ModName;
        protected override string Version => Config.ModVersion;
        public override uint ApiVersion => Config.SSMPApiVersion;
        public override bool NeedsNetwork =>  true;

        public static bool roundStarted = false;
        public static bool isSeeker = false;
        public static ushort propSwaps = 0;
        public static ushort maxPropSwaps = 0;

        public override void Initialize(IClientApi clientApi)
        {
            PropHuntMod.Initialize(clientApi);
            
            Log.LogInfo("Prop Hunt Loaded.");
            ClientNetwork.Init(clientApi, this);

            // Handle connects and disconnects
            clientApi.ClientManager.PlayerConnectEvent += player =>
            {
                Log.LogInfo($"Player {player.Username} connected");
                PlayerManager.GetPlayerManager(player.Id);
            };

            clientApi.ClientManager.PlayerDisconnectEvent += player =>
            {
                Log.LogInfo($"Player {player.Username} disconnected");
                PropHuntMod.playerManager.Remove(player.Id);
            };

            clientApi.ClientManager.PlayerEnterSceneEvent += player =>
            {
                Log.LogInfo($"Player {player.Username} entered your scene");

                var manager = PlayerManager.GetPlayerManager(player.Id);
                manager.hornetManager.SetHornet();
                manager.EnsurePropCover();
            };

            clientApi.CommandManager.RegisterCommand(new Commands.ShowHitboxes());
            clientApi.CommandManager.RegisterCommand(new Commands.Sync());

            //clientApi.CommandManager.RegisterCommand(new Commands.ClientStartCommand());
            //clientApi.CommandManager.RegisterCommand(new Commands.ClientStopCommand());
        }

        public static void LocalMessage(string message)
        {
            PropHuntMod.client.UiManager.ChatBox.AddMessage(message);
        }
        //public static bool IsHunter()
        //{
        //    return PropHuntMod.client.ClientManager.Team == Team.Grimm;
        //}
    }
}