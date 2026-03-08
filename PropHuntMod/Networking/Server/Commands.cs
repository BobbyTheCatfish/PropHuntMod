using PropHuntMod.Utils;
using PropHuntMod.Networking.Server;
using PropHuntMod.Networking.Client;
using SSMP.Api.Command.Client;
using SSMP.Api.Command.Server;
using PropHuntMod.Players;
using PropHuntMod.Props;

namespace PropHuntMod.Commands
{
    internal class ServerCommands : IServerCommand
    {
        public bool AuthorizedOnly => false;
        public string Trigger => PREFIX;
        public string[] Aliases => new string[] { "/prophunt", "/prop" };

        public const string START = "start";
        public const string STOP = "stop";
        public const string SETTINGS = "config";
        public const string PREFIX = "/ph";
        public void Execute(ICommandSender sender, string[] args)
        {
            var usage = $"Invalid Usage. /{PREFIX} <{START}|{STOP}|{SETTINGS}>";
            if (args.Length < 2)
            {
                sender.SendMessage(usage);
                return;
            }

            switch (args[1])
            {
                case START:
                    StartCommand(sender);
                    break;
                case STOP:
                    StopCommand(sender);
                    break;
                case SETTINGS:
                    SettingsCommand(sender, args);
                    break;
                default:
                    sender.SendMessage(usage);
                    break;
            }
        }

        void StartCommand(ICommandSender sender)
        {
            if (Server.GameState != GameState.NotStarted)
            {
                sender.SendMessage("The game has already started!");
                return;
            }

            Server.instance.GameStart();
        }

        void StopCommand(ICommandSender sender)
        {
            if (Server.GameState == GameState.NotStarted)
            {
                sender.SendMessage("The game hasn't started yet!");
                return;
            }

            Server.instance.CheckGameOver(null, true);
        }

        void SettingsCommand(ICommandSender sender, string[] args)
        {
            if (!sender.IsAuthorized)
            {
                sender.SendMessage("You need to be authorized to use this command.");
                return;
            }
            
            const string SEEKER_WAIT = "seekerwait";
            const string NUM_SEEKERS = "seekercount";
            const string ATTACK_COOL = "attackcooldown";

            var usage = $"Invalid Usage. /{PREFIX} {SETTINGS} <{SEEKER_WAIT}|{NUM_SEEKERS}|{ATTACK_COOL}> <value>";
            if (args.Length < 4)
            {
                sender.SendMessage(usage);
                return;
            }

            var setting = args[2];
            var value = args[3];
            if (!float.TryParse(value, out var floatVal))
            {
                if (setting == "attackcooldown") sender.SendMessage("Value must be a valid number of seconds.");
                else sender.SendMessage("Value must be a valid whole number.");
                return;
            }
            var intVal = (int)floatVal;

            switch (args[2])
            {
                case SEEKER_WAIT:
                    SetSeekerWait(sender, intVal); break;
                case NUM_SEEKERS:
                    SetSeekerCount(sender, intVal); break;
                case ATTACK_COOL:
                    SetAttackCooldown(sender, floatVal); break;
            }
        }

        void SetSeekerWait(ICommandSender sender, int time)
        {
            if (time < 0)
            {
                sender.SendMessage("Seeker wait time must be 0 or higher. Default is 30.");
                return;
            }

            Config.SeekerCountdown = time;
            sender.SendMessage($"Seekers will wait {time} seconds before they can start.");
            Config.SaveServerConfig();
        }

        void SetSeekerCount(ICommandSender sender, int count)
        {
            if (count < 1)
            {
                sender.SendMessage("There needs to be at least one seeker. Default is 1.");
                return;
            }

            Config.SeekerCount = count;
            sender.SendMessage($"There will be up to {count} seekers per round. Note that there will always be at least one hider.");
            Config.SaveServerConfig();
        }

        void SetAttackCooldown(ICommandSender sender, float time)
        {
            if (time < 0)
            {
                sender.SendMessage("Attack cooldown must be 0 or higher. Default is 0 (disabled).");
                return;
            }

            Config.AttackCooldown = time;
            sender.SendMessage($"Seekers will need to wait {time.ToString("F2")} seconds between attacks.");
            Config.SaveServerConfig();
        }
    }
}
