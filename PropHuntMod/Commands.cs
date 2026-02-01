using PropHuntMod.Modifications;
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
            if (PropHuntServer.started)
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
            if (!PropHuntServer.started)
            {
                sender.SendMessage("The game hasn't started yet!");
                return;
            }

            PropHuntServer.instance.GameOver("[canceled]", true);
        }
    }

    internal class ShowHitboxes : IClientCommand
    {
        public bool AuthorizedOnly => false;
        public string Trigger => "/hitboxes";
        public string[] Aliases => new string[] { };
        public void Execute(string[] args)
        {
            if (!SelfCoverManager.instance.IsHiding && !PropHuntMod.showHitboxes)
            {
                PropHuntClient.AddLocalMessage("Cannot enable hitboxes when not hiding.");
                return;
            }
            PropHuntClient.AddLocalMessage("Turning hitboxes " + (PropHuntMod.showHitboxes ? "off": "on"));
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
            PropHuntClient.AddLocalMessage($"Syncing props for {PropHuntMod.playerManager.Count} players");
            PlayerManager.EnsureAllPropCovers();
        }
    }
}
