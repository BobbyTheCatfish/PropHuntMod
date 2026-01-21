using PropHuntMod.Utils;
using PropHuntMod.Utils.Networking;
using SSMP.Api.Client;
using SSMP.Api.Command.Server;
using SSMP.Api.Server;
using SSMP.Api.Server.Networking;
using SSMP.Networking.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PropHuntMod
{
    internal class ServerPlayer
    {
        public ushort id;
        public bool hidden = false;
        public string propName;
        public Vector3? propLocation;
        public float? propRotation;
        public IServerPlayer playerAvatar => PropHuntServer._serverApi.ServerManager.GetPlayer(id);
        public ServerPlayer(ushort id)
        {
            this.id = id;
        }
    }

    internal class ServerStartCommand : IServerCommand
    {
        public bool AuthorizedOnly => false;
        public string Trigger => "/prophunt start";
        public string[] Aliases => new[] { "" };
        public void Execute(ICommandSender sender, string[] args)
        {
            PropHuntServer.sender.BroadcastSingleData(CustomPackets.ForcePropSwap, new ForcePropSwapData());
        }
    }

    public class PropHuntServer : ServerAddon
    {
        protected override string Name => Config.ModName;
        protected override string Version => Config.ModVersion;
        public override uint ApiVersion => Config.SSMPApiVersion;
        public override bool NeedsNetwork => true;


        readonly Dictionary<ushort, ServerPlayer> players = new Dictionary<ushort, ServerPlayer>();


        internal static IServerAddonNetworkReceiver<CustomPackets> receiver = null;
        internal static IServerAddonNetworkSender<CustomPackets> sender = null;
        internal static IServerApi _serverApi = null;

        public override void Initialize(IServerApi serverApi)
        {
            //instance = this;
            _serverApi = serverApi;
            Log.LogInfo("Prop Hunt Loaded.");

            receiver = serverApi.NetServer.GetNetworkReceiver<CustomPackets>(this, Network.InstantiatePacket);
            sender = serverApi.NetServer.GetNetworkSender<CustomPackets>(this);

            receiver.RegisterPacketHandler<PropSwapData>(CustomPackets.PropSwap, OnPropSwap);
            receiver.RegisterPacketHandler<PropLocationData>(CustomPackets.PropLocation, OnPropLocation);
            receiver.RegisterPacketHandler<HideStatusData>(CustomPackets.HideStatus, OnHideStatus);
            receiver.RegisterPacketHandler<PropFoundData>(CustomPackets.PropFound, OnPropFound);

            serverApi.CommandManager.RegisterCommand(new ServerStartCommand());

            serverApi.ServerManager.PlayerConnectEvent += (IServerPlayer player) =>
            {
                GetPlayer(player.Id);
            };

            serverApi.ServerManager.PlayerDisconnectEvent += (IServerPlayer player) =>
            {
                players.Remove(player.Id);
            };

        }

        public void Broadcast(ushort senderID, CustomPackets packetID, IPacketData data)
        {
            foreach (var player in this.ServerApi.ServerManager.Players)
            {
                if (player.Id == senderID) continue;

                sender.SendSingleData(packetID, data);
            }
        }

        ServerPlayer GetPlayer(ushort playerID)
        {
            var hasPlayer = players.TryGetValue(playerID, out var player);
            if (!hasPlayer)
            {
                player = new ServerPlayer(playerID);
                players.Add(playerID, player);
            }

            return player;
        }

        void OnPropSwap(ushort id, PropSwapData data)
        {
            var player = GetPlayer(id);
            player.propName = string.IsNullOrEmpty(data.propName) ? null : data.propName;


            PropSwapData sendData = new PropSwapData
            {
                Id = id,
                propName = data.propName
            };
            Broadcast(id, CustomPackets.PropSwap, sendData);
        }

        void OnPropLocation(ushort id, PropLocationData data)
        {
            var player = GetPlayer(id);
            player.propLocation = data.propPosition;
            player.propRotation = data.propRotation;

            PropLocationData sendData = new PropLocationData
            {
                Id = id,
                propPosition = data.propPosition,
                propRotation = data.propRotation
            };
            Broadcast(id, CustomPackets.PropLocation, sendData);
        }
        void OnHideStatus(ushort id, HideStatusData data)
        {
            GetPlayer(id).hidden = data.isHiding;

            HideStatusData sendData = new HideStatusData
            {
                Id = id,
                isHiding = data.isHiding
            };
            Broadcast(id, CustomPackets.HideStatus, sendData);
        }
        void OnPropFound(ushort id, PropFoundData data)
        {
            var owner = GetPlayer(data.propOwnerID);
            owner.propName = null;
            owner.propLocation = null;
            owner.propRotation = null;
            foreach (var player in this.ServerApi.ServerManager.Players)
            {
                if (player.Id == id) continue;

                PropFoundData sendData = new PropFoundData
                {
                    Id = id,
                    isClientFound = player.Id == data.propOwnerID,
                    propOwnerID = data.propOwnerID,
                };

                sender.SendSingleData(CustomPackets.PropFound, sendData);
            }

            string winner = DetermineWinner();
            if (winner != null)
            {
                sender.BroadcastSingleData(CustomPackets.GameOver, new GameOverData { winnerUsername = winner });
            }
        }

        string DetermineWinner()
        {
            string winnerName = null;
            foreach (var player in players.Values)
            {
                if (player.propName != null)
                {
                    if (winnerName != null) return null;
                    winnerName = player.playerAvatar.Username;
                }
            }

            return winnerName;
        }
    }
}
