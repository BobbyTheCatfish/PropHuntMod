using HarmonyLib;
using PropHuntMod.Networking.Client;
using PropHuntMod.Networking.Server;
using PropHuntMod.Props;
using PropHuntMod.Utils;
using SSMP.Api.Server.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PropHuntMod.Tests
{
    internal class TestRoundStartHunter : BaseTest
    {
        public override KeyCode KeyCode => KeyCode.Alpha8;
        public override string Name => "Round Start Hunter";
        public override bool Enabled => true;

        public override bool Execute()
        {
            var packet = new RoundStart
            {
                IsSeeker = true,
                PropSwapLimit = 0,
                SeekerWaitTime = Config.SeekerCountdown
            };

            var sender = typeof(ServerNetwork).GetField("sender").GetValue(null) as IServerAddonNetworkSender<CustomPackets>;
            sender.BroadcastSingleData(CustomPackets.RoundStart, packet);

            var timer = new SeekerTimer(Config.SeekerCountdown);

            return true;
        }
    }
}
