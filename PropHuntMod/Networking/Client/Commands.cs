using PropHuntMod.Utils;
using PropHuntMod.Networking.Server;
using PropHuntMod.Networking.Client;
using SSMP.Api.Command.Client;
using SSMP.Api.Command.Server;
using PropHuntMod.Players;
using PropHuntMod.Props;

namespace PropHuntMod.Commands
{
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
