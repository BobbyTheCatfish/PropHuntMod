using PropHuntMod.Props;
using PropHuntMod.Utils;
using SSMP.Api.Server;
using SSMP.Api.Server.Networking;
using SSMP.Networking.Packet;
using System.Collections.Generic;
using System.Linq;

using Vector3 = SSMP.Math.Vector3;

namespace PropHuntMod.Networking.Server
{
    struct BypassTicket
    {
        public int Id;
        public CorrectiveActions action;

        public BypassTicket(int Id = -1, CorrectiveActions action = CorrectiveActions.None)
        {
            this.Id = Id;
            this.action = action;
        }
    }

    static class ServerNetwork
    {
        static IServerAddonNetworkSender<CustomPackets> sender;
        static IServerAddonNetworkReceiver<CustomPackets> receiver;
        static readonly Dictionary<ushort, List<BypassTicket>> tickets = new Dictionary<ushort, List<BypassTicket>>();
        static int TicketID = 0;

        static BypassTicket GenerateTicket(ushort playerID, CorrectiveActions action)
        {
            if (!tickets.ContainsKey(playerID))
            {
                tickets[playerID] = new List<BypassTicket>();
            }

            var ticket = new BypassTicket { action = action, Id = TicketID };
            tickets[playerID].Add(ticket);

            TicketID++;
            return ticket;
        }

        static bool IsValidTicket(ushort playerID, int ticketID, params CorrectiveActions[] action)
        {
            if (ticketID == -1) return false;
            if (!tickets.ContainsKey(playerID)) return false;

            var ticketIndex = tickets[playerID].FindIndex(t => t.Id == ticketID && action.Contains(t.action));
            if (ticketIndex == -1) return false;

            tickets[playerID].RemoveAt(ticketIndex);
            return true;
        }

        public static void Broadcast(ushort senderID, CustomPackets packetID, Client.Packet data, bool collection)
        {
            foreach (var player in Server.api.ServerManager.Players)
            {
                if (player.Id == senderID) continue;

                if (collection) sender.SendCollectionData(packetID, data, player.Id);
                else sender.SendSingleData(packetID, data, player.Id);
            }
        }

        static void FailedAction(ushort senderID, ushort affectedID, CustomPackets packetType, CorrectiveActions fix)
        {
            // Generate ticket if needed
            int ticketId = -1;
            if (
                fix == CorrectiveActions.ToggleHornetFalse ||
                fix == CorrectiveActions.PreviousScene ||
                fix == CorrectiveActions.RestoreLastProp
            )
            {
                ticketId = GenerateTicket(senderID, fix).Id;
            }

            sender.SendCollectionData(CustomPackets.FailedAction, new FailedAction
            {
                AffectedID = affectedID,
                FailedPacket = packetType,
                FixMethod = fix,
                BypassTicketID = ticketId
            }, senderID);
        }

        /******************
         * PACKET SENDERS *
         ******************/
        public static void ForwardPropSwap(ushort id, string propName, string propPath)
        {
            Log.LogInfo($"Broadcasting prop swap from {id}: {propName}");
            Broadcast(id, CustomPackets.PropSwap, new PropSwap
            {
                Id = id,
                propName = propName,
                propPath = propPath
            }, true);
        }

        public static void BroadcastRoundStart()
        {
            Log.LogInfo("Broadcasting round start");
            foreach (var player in Server.api.ServerManager.Players)
            {
                SendRoundStart(player.Id);
            }
        }

        public static void SendRoundStart(ushort id, bool started = false)
        {
            var player = Server.GetPlayer(id);
            var data = new RoundStart
            {
                IsSeeker = player.seeker,
                PropSwapLimit = Config.MaxSwapCount,
                SeekerWaitTime = started ? 0 : Server.instance.SeekerTimer.seconds,
                SeekerAttackCooldown = Config.AttackCooldown,
            };

            sender.SendSingleData(CustomPackets.RoundStart, data, id);
        }


        public static void ForwardPropLocation(ushort id, Vector3 propPosition, float propRotation, float propScale)
        {
            Log.LogInfo($"Broadcasting prop location from {id}: {propPosition}, {propRotation}, {propScale}");

            BaseCoverManager.ConstrainPropLocation(ref propPosition, ref propRotation, ref propScale);
            Broadcast(id, CustomPackets.PropLocation, new PropLocation
            {
                Id= id,
                PropPosition = propPosition,
                PropRotation = propRotation,
                PropScale = propScale
            }, true);
        }

        public static void ForwardHideStatus(ushort id, bool isHiding)
        {
            Log.LogInfo($"Broadcasting hide status from {id}: {isHiding}");
            Broadcast(id, CustomPackets.HideStatus, new HideStatus
            {
                Id = id,
                IsHiding = isHiding
            }, true);
        }

        public static void ForwardPropFound(ushort id, ushort propOwnerID)
        {
            Log.LogInfo($"Broadcasting prop found from {id}: {propOwnerID}");
            foreach (var player in Server.api.ServerManager.Players)
            {
                PropFound sendData = new PropFound
                {
                    IsClientFound = player.Id == propOwnerID,
                    PropOwnerID = propOwnerID,
                };

                sender.SendCollectionData(CustomPackets.PropFound, sendData, player.Id);
            }
        }

        public static void BroadcastGameOver(IServerPlayer winner, bool canceled)
        {
            Log.LogInfo("Broadcasting game over");
            foreach (var player in Server.api.ServerManager.Players)
            {
                var data = new GameOver
                {
                    IsWinner = !canceled && winner?.Id == player.Id,
                    WasCanceled = canceled,
                    WinnerUsername = winner?.Username ?? "Nobody"
                };
                sender.SendSingleData(CustomPackets.GameOver, data, player.Id);
            }
        }

        public static void BroadcastSeekerStart()
        {
            Log.LogInfo("Broadcasting seeker start");
            sender.BroadcastSingleData(CustomPackets.SeekerStart, new SeekerStart());
        }

        public static void Init(IServerApi serverApi, ServerAddon serverAddon)
        {
            sender = serverApi.NetServer.GetNetworkSender<CustomPackets>(serverAddon);
            receiver = serverApi.NetServer.GetNetworkReceiver<CustomPackets>(serverAddon, Client.Packets.Instantiate);

            receiver.RegisterPacketHandler<Client.PropSwap>(CustomPackets.PropSwap, OnPropSwap);
            receiver.RegisterPacketHandler<Client.PropLocation>(CustomPackets.PropLocation, OnPropLocation);
            receiver.RegisterPacketHandler<Client.PropFound>(CustomPackets.PropFound, OnPropFound);
            receiver.RegisterPacketHandler<Client.Sync>(CustomPackets.Sync, OnSync);

            if (Config.AllowDebugFeatures) receiver.RegisterPacketHandler<Client.HideStatus>(CustomPackets.HideStatus, OnHideStatus);
        }

        /********************
         * PACKET RECEIVERS *
         ********************/
        static void OnPropSwap(ushort id, Client.PropSwap data)
        {
            var player = Server.GetPlayer(id);

            var disabling = string.IsNullOrEmpty(data.propPath);

            // Seekers can't hide
            if (player.seeker && Server.GameState != GameState.NotStarted && !disabling)
            {
                Server.instance.Message(id, "You're a seeker! You can't hide this round.");
                FailedAction(id, id, CustomPackets.PropSwap, CorrectiveActions.DisableClientProp);
                return;
            }

            var swapCountExempt = IsValidTicket(id, data.TicketID, CorrectiveActions.RestoreLastProp, CorrectiveActions.PreviousScene);

            // Limit number of prop swaps
            if (Config.MaxSwapCount > 0 && player.swapCount >= Config.MaxSwapCount && Server.GameState != GameState.NotStarted)
            {
                if (!swapCountExempt)
                {
                    Server.instance.Message(id, "You ran out of prop swaps and are de-synced! Is your mod/config up to date?");
                    FailedAction(id, id, CustomPackets.PropSwap, CorrectiveActions.RestoreLastProp);
                    return;
                }
            }

            if (disabling)
            {
                player.propName = "";
                player.propPath = "";
            }
            else
            {
                player.propName = data.propName;
                player.propPath = data.propPath;
            }

            if (!swapCountExempt) player.swapCount++;

            ForwardPropSwap(id, player.propName, player.propPath);
        }

        static void OnPropLocation(ushort id, Client.PropLocation data)
        {
            var player = Server.GetPlayer(id);

            // Seekers can't prop
            if (player.seeker && Server.GameState != GameState.NotStarted)
            {
                Server.instance.Message(id, "You're a seeker! You can't hide this round.");
                FailedAction(id, id, CustomPackets.PropSwap, CorrectiveActions.DisableClientProp);
                return;
            }

            player.propLocation = data.PropPosition;
            player.propRotation = data.PropRotation;
            player.propScale = data.PropScale;

            // Constrain location
            BaseCoverManager.ConstrainPropLocation(ref player.propLocation, ref player.propRotation, ref player.propScale);

            ForwardPropLocation(id, player.propLocation, player.propRotation, player.propScale);
        }
        
        static void OnSync(ushort id, Client.Sync data)
        {
            var player = Server.GetPlayer(id);

            if (player.seeker)
            {
                player.ResetProp();
            }
            else
            {
                player.propName = data.PropName ?? "";
                player.propPath = data.PropPath ?? "";
                player.propLocation = data.PropLocation;
                player.propRotation = data.PropRotation;
                player.propScale = data.PropRotation;
            }

            BaseCoverManager.ConstrainPropLocation(ref player.propLocation, ref player.propRotation, ref player.propScale);

            ForwardPropSwap(id, player.propName, player.propPath);
            ForwardPropLocation(id, player.propLocation, player.propRotation, player.propScale);

            Server.instance.CheckGameOver(player.PlayerAvatar);
        }

        static void OnHideStatus(ushort id, Client.HideStatus data)
        {
            var player = Server.GetPlayer(id);
            if (Server.GameState != GameState.NotStarted)
            {
                if (player.seeker && data.IsHiding && !IsValidTicket(id, data.TicketID, CorrectiveActions.ToggleHornetFalse))
                {
                    Server.instance.Message(id, "You're a seeker! You can't hide this round.");
                    FailedAction(id, id, CustomPackets.HideStatus, CorrectiveActions.ToggleHornetTrue);
                    return;
                }
            }

            player.hidden = data.IsHiding;
            ForwardHideStatus(id, data.IsHiding);
        }

        static void OnPropFound(ushort id, Client.PropFound data)
        {
            var owner = Server.GetPlayer(data.PropOwnerID);
            var finder = Server.GetPlayer(id);

            if (Server.GameState == GameState.SeekerWait)
            {
                Server.instance.Message(id, "You're still in the waiting period. No seeking yet!");
                FailedAction(id, data.PropOwnerID, CustomPackets.PropFound, CorrectiveActions.RestoreTriggerHandler);
                return;
            }

            if (Server.GameState == GameState.Playing)
            {
                if (owner.seeker)
                {
                    Server.instance.Message(id, "That player is a seeker!");
                    FailedAction(id, data.PropOwnerID, CustomPackets.PropFound, CorrectiveActions.DisablePlayerProp);
                    return;
                }
                else if (!finder.seeker)
                {
                    Server.instance.Message(id, "You're not a seeker! You can't find people this round.");
                    FailedAction(id, data.PropOwnerID, CustomPackets.PropFound, CorrectiveActions.RestoreTriggerHandler);
                    return;
                }
                else owner.seeker = true;
            }

            if (string.IsNullOrEmpty(owner.propName))
            {
                Server.instance.Message(id, "That player has already been found. You might be out of sync.");
                FailedAction(id, data.PropOwnerID, CustomPackets.PropFound, CorrectiveActions.DisablePlayerProp);
                return;
            }

            owner.ResetProp();

            ForwardPropFound(id, data.PropOwnerID);

            
            Server.instance.Announce($"{owner.PlayerAvatar.Username} was found by {finder.PlayerAvatar.Username}!");

            if (Server.GameState == GameState.Playing)
            {
                Server.instance.CheckGameOver(owner.PlayerAvatar);
            }
        }
    }
}
