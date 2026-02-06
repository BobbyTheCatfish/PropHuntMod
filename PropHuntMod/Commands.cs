using PropHuntMod.Modifications;
using PropHuntMod.Utils.Networking;
using SSMP.Api.Command.Client;
using SSMP.Api.Command.Server;

namespace PropHuntMod.Commands
{
    internal class StartGameCommand : IServerCommand
    {
        public bool AuthorizedOnly => false;
        public string Trigger => "/hunt";
        public string[] Aliases => new string[] { };
        public void Execute(ICommandSender sender, string[] args)
        {
            if (PropHuntServer.GameState != Utils.GameState.NotStarted)
            {
                sender.SendMessage("The game has already started!");
                return;
            }

            PropHuntServer.instance.GameStart();
        }
    }

    internal class StopGameCommand : IServerCommand
    {
        public bool AuthorizedOnly => false;
        public string Trigger => "/stop";
        public string[] Aliases => new string[] { };
        public void Execute(ICommandSender sender, string[] args)
        {
            if (PropHuntServer.GameState == Utils.GameState.NotStarted)
            {
                sender.SendMessage("The game hasn't started yet!");
                return;
            }

            PropHuntServer.instance.CheckGameOver(null, true);
        }
    }

    internal class ShowHitboxes : IClientCommand
    {
        public bool AuthorizedOnly => false;
        public string Trigger => "/hitboxes";
        public string[] Aliases => new string[] { };
        public void Execute(string[] args)
        {
            if (!PropHuntClient.isSeeker && !PropHuntMod.showHitboxes)
            {
                PropHuntClient.LocalMessage("Cannot enable hitboxes when seeking.");
                return;
            }
            PropHuntClient.LocalMessage("Turning hitboxes " + (PropHuntMod.showHitboxes ? "off": "on"));
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
            PropHuntClient.LocalMessage($"Syncing props for {PropHuntMod.playerManager.Count} players");
            PlayerManager.EnsureAllPropCovers();

            var cover = SelfCoverManager.instance;
            ClientNetwork.SendSync(cover.cover?.name ?? "", cover.Position, cover.Rotation);
        }
    }
}
