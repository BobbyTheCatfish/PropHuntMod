using PropHuntMod.Utils;
using PropHuntMod.Networking.Server;
using PropHuntMod.Networking.Client;
using SSMP.Api.Command.Client;
using SSMP.Api.Command.Server;
using PropHuntMod.Players;
using PropHuntMod.Props;

namespace PropHuntMod.Commands
{
    internal class StartGameCommand : IServerCommand
    {
        public bool AuthorizedOnly => false;
        public string Trigger => "/hunt";
        public string[] Aliases => new string[] { };
        public void Execute(ICommandSender sender, string[] args)
        {
            if (Server.GameState != Utils.GameState.NotStarted)
            {
                sender.SendMessage("The game has already started!");
                return;
            }

            Server.instance.GameStart();
        }
    }

    internal class StopGameCommand : IServerCommand
    {
        public bool AuthorizedOnly => false;
        public string Trigger => "/stop";
        public string[] Aliases => new string[] { };
        public void Execute(ICommandSender sender, string[] args)
        {
            if (Server.GameState == Utils.GameState.NotStarted)
            {
                sender.SendMessage("The game hasn't started yet!");
                return;
            }

            Server.instance.CheckGameOver(null, true);
        }
    }

    internal class ShowHitboxes : IClientCommand
    {
        public bool AuthorizedOnly => false;
        public string Trigger => "/hitboxes";
        public string[] Aliases => new string[] { };
        public void Execute(string[] args)
        {
            if (Client.isSeeker && !PropHuntMod.showHitboxes)
            {
                Client.LocalMessage("Cannot enable hitboxes when seeking.");
                return;
            }
            Client.LocalMessage("Turning hitboxes " + (PropHuntMod.showHitboxes ? "off": "on"));
            PropHuntMod.showHitboxes = !PropHuntMod.showHitboxes;
        }
    }

    internal class Sync : IClientCommand
    {
        public bool AuthorizedOnly => false;
        public string Trigger => "/sync";
        public string[] Aliases => new string[] { };
        public void Execute(string[] args)
        {
            Client.LocalMessage($"Syncing props for {PropHuntMod.playerManager.Count} players");
            PlayerManager.EnsureAllPropCovers();

            var cover = SelfCoverManager.instance;
            ClientNetwork.SendSync(cover.cover?.name ?? "", cover.CoverPath ?? "", cover.Position, cover.Rotation, cover.Scale);
        }
    }

#if DEBUG
    internal class ToScene : IClientCommand
    {
        public bool AuthorizedOnly => false;
        public string Trigger => "/scene";
        public string[] Aliases => new string[] { };
        public void Execute(string[] args)
        {
            if (args.Length < 2)
            {
                Client.LocalMessage("You need to provide a scene name to go to.");
                return;
            }
            
            if (!PropTesting.GoToScene(args[1]))
            {
                Client.LocalMessage($"I couldn't find the scene for {args[1]}.");
                return;
            }
        }
    }
#endif
}
