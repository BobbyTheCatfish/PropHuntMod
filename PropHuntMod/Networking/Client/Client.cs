using PropHuntMod.Utils;
using SSMP.Api.Client;
using SSMP.Game;
using PropHuntMod.Players;
using PropHuntMod.Commands;

namespace PropHuntMod.Networking.Client
{
    internal enum Teams
    {
        Hunter = Team.Lifeblood,
        Seeker = Team.Grimm
    }

    internal class Client : ClientAddon
    {
        protected override string Name => Config.ModName;
        protected override string Version => Config.ModVersion;
        public override uint ApiVersion => Config.SSMPApiVersion;
        public override bool NeedsNetwork =>  true;

        public static GameState GameState = GameState.NotStarted;
        public static bool isSeeker = false;
        public static ushort propSwaps = 0;
        public static ushort maxPropSwaps = 0;
        public static float seekerAttackCooldown = 0;
        public override void Initialize(IClientApi clientApi)
        {
            PropHuntMod.Initialize(clientApi);
            
            Log.SetLogger(Logger);
            Log.LogInfo("Prop Hunt Client Loaded.");
            ClientNetwork.Init(clientApi, this);

            clientApi.ClientManager.ConnectEvent += () =>
            {
                string[] helpTextCommands = new string[]
                {
                    "Prop Hunt Commands:",
                    $"/ph {ServerCommands.START}: starts a round",
                    $"/ph {ServerCommands.STOP}: stops a round",
                    $"/ph {ServerCommands.SETTINGS}: configure server settings",
                    "/sync: syncs other player's props",
                };

                foreach (string command in helpTextCommands)
                    LocalMessage(command);
            };

            clientApi.ClientManager.DisconnectEvent += () =>
            {
                GameState = GameState.NotStarted;
                isSeeker = false;
                propSwaps = 0;
                maxPropSwaps = 0;
                PropHuntMod.playerManager.Clear();
                Server.Server.instance.Reset(true);
            };

            // Handle connects and disconnects
            clientApi.ClientManager.PlayerConnectEvent += player =>
            {
                PlayerManager.GetPlayerManager(player.Id);
            };

            clientApi.ClientManager.PlayerDisconnectEvent += player =>
            {
                PropHuntMod.playerManager.Remove(player.Id);
            };

            clientApi.ClientManager.PlayerEnterSceneEvent += player =>
            {
                var manager = PlayerManager.GetPlayerManager(player.Id);
                manager.hornetManager.SetHornet();
                manager.EnsurePropCover();
            };

            clientApi.CommandManager.RegisterCommand(new Commands.ShowHitboxes());
            clientApi.CommandManager.RegisterCommand(new Commands.Sync());

#if DEBUG
            clientApi.CommandManager.RegisterCommand(new Commands.ToScene());
#endif

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