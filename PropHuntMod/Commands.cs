using PropHuntMod.Utils;
using PropHuntMod.Utils.Networking;
using SSMP.Api.Command.Client;
using SSMP.Api.Command.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
