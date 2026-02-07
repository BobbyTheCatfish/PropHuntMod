using PropHuntMod.Modifications;
using SSMP.Api.Server;
using SSMP.Api.Server.Networking;
using SSMP.Networking.Packet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PropHuntMod.Utils.Networking
{
    struct BypassTicket
    {
        public int Id;
        public CorrectionActions action;

        public BypassTicket(int Id = -1, CorrectionActions action = CorrectionActions.None)
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

        static BypassTicket GenerateTicket(ushort playerID, CorrectionActions action)
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

        static bool IsValidTicket(ushort playerID, int ticketID, params CorrectionActions[] action)
        {
            if (ticketID == -1) return false;
            if (!tickets.ContainsKey(playerID)) return false;

            var ticketIndex = tickets[playerID].FindIndex(t => t.Id == ticketID && action.Contains(t.action));
            if (ticketIndex == -1) return false;

            tickets[playerID].RemoveAt(ticketIndex);
            return true;
        }

        public static void Broadcast(ushort senderID, CustomPackets packetID, IPacketData data)
        {
            foreach (var player in PropHuntServer._serverApi.ServerManager.Players)
            {
                if (player.Id == senderID) continue;

                sender.SendSingleData(packetID, data, player.Id);
            }
        }

        static void FailedAction(ushort senderID, ushort affectedID, CustomPackets packetType, CorrectionActions fix)
        {
            // Generate ticket if needed
            int ticketId = -1;
            if (
                fix == CorrectionActions.ToggleHornetFalse ||
                fix == CorrectionActions.PreviousScene ||
                fix == CorrectionActions.RestoreLastProp
            )
            {
                ticketId = GenerateTicket(senderID, fix).Id;
            }

            sender.SendSingleData(CustomPackets.FailedAction, new FromServer.FailedAction
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
            Broadcast(id, CustomPackets.PropSwap, new FromServer.PropSwap
            {
                Id = id,
                propName = propName,
                propPath = propPath
            });
        }

        public static void BroadcastRoundStart()
        {
            Log.LogInfo("Broadcasting round start");
            foreach (var player in PropHuntServer._serverApi.ServerManager.Players)
            {
                SendRoundStart(player.Id);
            }
        }

        public static void SendRoundStart(ushort id, bool started = false)
        {
            var player = PropHuntServer.GetPlayer(id);
            var data = new FromServer.RoundStart
            {
                IsSeeker = player.seeker,
                PropSwapLimit = Config.MaxSwapCount,
                SeekerWaitTime = started ? 0 : PropHuntServer.instance.SeekerTimer.seconds
            };

            sender.SendSingleData(CustomPackets.RoundStart, data, id);
        }


        public static void ForwardPropLocation(ushort id, Vector3 propPosition, float propRotation)
        {
            Log.LogInfo($"Broadcasting prop location from {id}: {propPosition}, {propRotation}");

            BaseCoverManager.ConstrainPropLocation(ref propPosition, ref propRotation);
            Broadcast(id, CustomPackets.PropLocation, new FromServer.PropLocation
            {
                Id= id,
                PropPosition = propPosition,
                PropRotation = propRotation
            });
        }

        public static void ForwardHideStatus(ushort id, bool isHiding)
        {
            Log.LogInfo($"Broadcasting hide status from {id}: {isHiding}");
            Broadcast(id, CustomPackets.HideStatus, new FromServer.HideStatus
            {
                Id = id,
                IsHiding = isHiding
            });
        }

        public static void ForwardPropFound(ushort id, ushort propOwnerID)
        {
            Log.LogInfo($"Broadcasting prop found from {id}: {propOwnerID}");
            foreach (var player in PropHuntServer._serverApi.ServerManager.Players)
            {
                FromServer.PropFound sendData = new FromServer.PropFound
                {
                    IsClientFound = player.Id == propOwnerID,
                    PropOwnerID = propOwnerID,
                };

                sender.SendSingleData(CustomPackets.PropFound, sendData, player.Id);
            }
        }

        public static void BroadcastGameOver(IServerPlayer winner, bool canceled)
        {
            Log.LogInfo("Broadcasting game over");
            foreach (var player in PropHuntServer._serverApi.ServerManager.Players)
            {
                var data = new FromServer.GameOver
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
            sender.BroadcastSingleData(CustomPackets.SeekerStart, new FromServer.SeekerStart());
        }

        public static void Init(IServerApi serverApi, ServerAddon serverAddon)
        {
            sender = serverApi.NetServer.GetNetworkSender<CustomPackets>(serverAddon);
            receiver = serverApi.NetServer.GetNetworkReceiver<CustomPackets>(serverAddon, FromClient.Packets.Instantiate);

            receiver.RegisterPacketHandler<FromClient.PropSwap>(CustomPackets.PropSwap, OnPropSwap);
            receiver.RegisterPacketHandler<FromClient.PropLocation>(CustomPackets.PropLocation, OnPropLocation);
            receiver.RegisterPacketHandler<FromClient.PropFound>(CustomPackets.PropFound, OnPropFound);
            receiver.RegisterPacketHandler<FromClient.Sync>(CustomPackets.Sync, OnSync);

            if (Config.AllowDebugFeatures) receiver.RegisterPacketHandler<FromClient.HideStatus>(CustomPackets.HideStatus, OnHideStatus);
        }

        /********************
         * PACKET RECEIVERS *
         ********************/
        static void OnPropSwap(ushort id, FromClient.PropSwap data)
        {
            var player = PropHuntServer.GetPlayer(id);

            // Seekers can't hide
            if (player.seeker && PropHuntServer.GameState != GameState.NotStarted)
            {
                PropHuntServer.instance.Message(id, "You're a seeker! You can't hide this round.");
                FailedAction(id, id, CustomPackets.PropSwap, CorrectionActions.DisableClientProp);
                return;
            }

            var swapCountExempt = IsValidTicket(id, data.TicketID, CorrectionActions.RestoreLastProp, CorrectionActions.PreviousScene);

            // Limit number of prop swaps
            if (Config.MaxSwapCount > 0 && player.swapCount >= Config.MaxSwapCount && PropHuntServer.GameState != GameState.NotStarted)
            {
                if (!swapCountExempt)
                {
                    PropHuntServer.instance.Message(id, "You ran out of prop swaps and are de-synced! Is your mod/config up to date?");
                    FailedAction(id, id, CustomPackets.PropSwap, CorrectionActions.RestoreLastProp);
                    return;
                }
            }

            if (string.IsNullOrEmpty(data.propName))
            {
                player.propName = null;
                player.propPath = null;
            }
            else
            {
                player.propName = data.propName;
                player.propPath = data.propPath;
            }

            if (!swapCountExempt) player.swapCount++;

            ForwardPropSwap(id, data.propName, data.propPath);
        }

        static void OnPropLocation(ushort id, FromClient.PropLocation data)
        {
            var player = PropHuntServer.GetPlayer(id);

            // Seekers can't prop
            if (player.seeker && PropHuntServer.GameState != GameState.NotStarted)
            {
                PropHuntServer.instance.Message(id, "You're a seeker! You can't hide this round.");
                FailedAction(id, id, CustomPackets.PropSwap, CorrectionActions.DisableClientProp);
                return;
            }

            player.propLocation = data.PropPosition;
            player.propRotation = data.PropRotation;

            // Constrain location
            BaseCoverManager.ConstrainPropLocation(ref player.propLocation, ref player.propRotation);

            ForwardPropLocation(id, player.propLocation, player.propRotation);
        }
        
        static void OnSync(ushort id, FromClient.Sync data)
        {
            var player = PropHuntServer.GetPlayer(id);

            if (player.seeker)
            {
                player.propName = "";
                player.propPath = "";
                player.propRotation = 0;
                player.propLocation = Vector3.zero;
            }
            else
            {
                player.propName = data.PropName;
                player.propPath = data.PropPath;
                player.propLocation = data.PropLocation;
                player.propRotation = data.PropRotation;
            }

            BaseCoverManager.ConstrainPropLocation(ref player.propLocation, ref player.propRotation);

            ForwardPropSwap(id, player.propName, player.propPath);
            ForwardPropLocation(id, player.propLocation, player.propRotation);

            PropHuntServer.instance.CheckGameOver(player.PlayerAvatar);
        }

        static void OnHideStatus(ushort id, FromClient.HideStatus data)
        {
            var player = PropHuntServer.GetPlayer(id);
            if (PropHuntServer.GameState != GameState.NotStarted)
            {
                if (player.seeker && data.IsHiding && !IsValidTicket(id, data.TicketID, CorrectionActions.ToggleHornetFalse))
                {
                    PropHuntServer.instance.Message(id, "You're a seeker! You can't hide this round.");
                    FailedAction(id, id, CustomPackets.HideStatus, CorrectionActions.ToggleHornetTrue);
                    return;
                }
            }

            player.hidden = data.IsHiding;
            ForwardHideStatus(id, data.IsHiding);
        }

        static void OnPropFound(ushort id, FromClient.PropFound data)
        {
            var owner = PropHuntServer.GetPlayer(data.PropOwnerID);
            var finder = PropHuntServer.GetPlayer(id);

            if (PropHuntServer.GameState == GameState.SeekerWait)
            {
                PropHuntServer.instance.Message(id, "You're still in the waiting period. No seeking yet!");
                FailedAction(id, data.PropOwnerID, CustomPackets.PropFound, CorrectionActions.RestoreTriggerHandler);
                return;
            }

            if (PropHuntServer.GameState == GameState.Playing)
            {
                if (owner.seeker)
                {
                    PropHuntServer.instance.Message(id, "That player is a seeker!");
                    FailedAction(id, data.PropOwnerID, CustomPackets.PropFound, CorrectionActions.DisablePlayerProp);
                    return;
                }
                else if (!finder.seeker)
                {
                    PropHuntServer.instance.Message(id, "You're not a seeker! You can't find people this round.");
                    FailedAction(id, data.PropOwnerID, CustomPackets.PropFound, CorrectionActions.RestoreTriggerHandler);
                    return;
                }
                else owner.seeker = true;
            }

            if (string.IsNullOrEmpty(owner.propName))
            {
                PropHuntServer.instance.Message(id, "That player has already been found. They might be out of sync.");
                FailedAction(id, data.PropOwnerID, CustomPackets.PropFound, CorrectionActions.DisablePlayerProp);
                return;
            }

            owner.propName = null;
            owner.propLocation = Vector3.zero;
            owner.propRotation = 0;

            ForwardPropFound(id, data.PropOwnerID);

            
            PropHuntServer.instance.Announce($"{owner.PlayerAvatar.Username} was found by {finder.PlayerAvatar.Username}!");

            if (PropHuntServer.GameState == GameState.Playing)
            {
                PropHuntServer.instance.CheckGameOver(owner.PlayerAvatar);
            }
        }
    }
}
