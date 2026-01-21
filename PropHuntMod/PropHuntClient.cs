using BepInEx;
using HarmonyLib;
//using PropHuntMod.Modifications;
using UnityEngine;
using System.Collections.Generic;
//using PropHuntMod.Utils.Networking;
using PropHuntMod.Utils;
using SSMP.Api.Server;
using SSMP.Api.Client;
using SSMP.Game;
using System.Linq;
using PropHuntMod.Utils.Networking;
using PropHuntMod.Modifications;

/**
 * FEATURE LIST
 * Hide/Show Hornet
 * Spawn and attach a game object (prop) to hornet
 * Move the prop in x/y/z
 * Dynamically get game objects in current scene
 * Add more props other than breakable ones (corpses, enemies?, etc)
 * Slow down attacks (Currently disabled)
 *
 * 
 * TODO:
 * Integrate with multiplayer mod
 *  - Find out which player is which
 *  - Send prop information packets
 * 
 */

namespace PropHuntMod
{
    public class PropHuntClient : ClientAddon
    {
        protected override string Name => Config.ModName;
        protected override string Version => Config.ModVersion;
        public override uint ApiVersion => Config.SSMPApiVersion;
        public override bool NeedsNetwork =>  true;

        public override void Initialize(IClientApi clientApi)
        {
            Log.LogInfo("Prop Hunt Loaded.");

            PropHuntMod.Initialize(clientApi);
            Network.Init(clientApi, this);

            clientApi.ClientManager.PlayerConnectEvent += (IClientPlayer player) =>
            {
                PlayerManager.GetPlayerManager(player.Id);
            };

            clientApi.ClientManager.PlayerDisconnectEvent += (IClientPlayer player) =>
            {
                PropHuntMod.playerManager.Remove(player.Id);
            };
        }
    }
}