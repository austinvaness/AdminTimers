using Sandbox.Game;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.Components;
using VRage.Game.ModAPI;

namespace avaness.AdminTimers
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class MySession : MySessionComponentBase
    {
        public static MySession Instance;

        private readonly Dictionary<long, Dictionary<string, HashSet<IMyTimerBlock>>> timers = new Dictionary<long, Dictionary<string, HashSet<IMyTimerBlock>>>();
        private bool init = false;

        public MySession()
        {
            Instance = this;
        }

        public void Register(string cmd, IMyTimerBlock timer, long fId = Constants.noFactionId)
        {
            Dictionary<string, HashSet<IMyTimerBlock>> faction;
            if (timers.TryGetValue(fId, out faction))
            {
                HashSet<IMyTimerBlock> timers;
                if(faction.TryGetValue(cmd, out timers))
                {
                    timers.Add(timer);
                }
                else
                {
                    faction[cmd] = new HashSet<IMyTimerBlock>()
                    {
                        timer
                    };
                }
            }
            else
            {
                timers[fId] = new Dictionary<string, HashSet<IMyTimerBlock>>()
                {
                    { cmd, new HashSet<IMyTimerBlock>() { timer } }
                };
            }
        }

        public void Unregister(string cmd, IMyTimerBlock timer, long fId = Constants.noFactionId)
        {
            Dictionary<string, HashSet<IMyTimerBlock>> faction;
            if (timers.TryGetValue(fId, out faction))
            {
                HashSet<IMyTimerBlock> timers;
                if (faction.TryGetValue(cmd, out timers))
                {
                    timers.Remove(timer);
                    if (timers.Count == 0)
                        faction.Remove(cmd);
                    if (faction.Count == 0)
                        this.timers.Remove(fId);
                }
            }
        }

        private bool TryGetValue(string cmd, out HashSet<IMyTimerBlock> timers, long fId = Constants.noFactionId)
        {
            Dictionary<string, HashSet<IMyTimerBlock>> faction;
            if (this.timers.TryGetValue(fId, out faction) && faction.TryGetValue(cmd, out timers))
                return true;

            timers = null;
            return false;
        }

        public override void LoadData()
        {
            Instance = this;
        }

        protected override void UnloadData()
        {
            if (Constants.IsServer)
                MyAPIGateway.Multiplayer.UnregisterMessageHandler(Constants.chatPacket, ChatPacketReceived);
            MyAPIGateway.Utilities.MessageEntered -= ChatMessageEntered;
            Instance = null;
        }

        private void Start()
        {
            if (Constants.IsServer)
                MyAPIGateway.Multiplayer.RegisterMessageHandler(Constants.chatPacket, ChatPacketReceived);
            MyAPIGateway.Utilities.MessageEntered += ChatMessageEntered;
            init = true;
        }

        private void ChatPacketReceived(byte[] data)
        {
            ChatMessage msg = MyAPIGateway.Utilities.SerializeFromBinary<ChatMessage>(data);
            IMyPlayer sender = Constants.GetPlayer(msg.steamUserId);
            bool b = true;
            if (sender != null)
                ChatMessageEntered(sender, msg.msg, ref b);

        }

        private void ChatMessageEntered(string msg, ref bool sendToOthers)
        {
            if (MyAPIGateway.Session?.Player != null)
                ChatMessageEntered(MyAPIGateway.Session.Player, msg, ref sendToOthers);
        }

        private void ChatMessageEntered(IMyPlayer sender, string msg, ref bool sendToOthers)
        {
            if(Constants.IsAdmin(sender))
            {
                // Admin
                if(msg == "/cmdlist")
                {
                    sendToOthers = false;
                    if (!Constants.IsServer)
                    {
                        new ChatMessage(sender.SteamUserId, msg).Send();
                        return;
                    }

                    StringBuilder sb = new StringBuilder();
                    int count = 0;
                    sb.AppendLine("Available commands:");

                    PrintCmds(sb, ref count, Constants.noFactionId, "Admin:");

                    IMyFaction fac = MyAPIGateway.Session.Factions.TryGetPlayerFaction(sender.IdentityId);
                    Dictionary<string, HashSet<IMyTimerBlock>> fCmds;
                    if (fac != null)
                        PrintCmds(sb, ref count, fac.FactionId, "Faction:");

                    if (count == 0)
                        sb.AppendLine("No commands.");

                    Notify(sb.ToString(), sender);
                }
                else if(msg.StartsWith("/cmd"))
                {
                    sendToOthers = false;
                    if (!Constants.IsServer)
                    {
                        new ChatMessage(sender.SteamUserId, msg).Send();
                        return;
                    }

                    string[] args = msg.Split(new[] { ' ' }, 2);
                    if (args.Length < 2)
                    {
                        Notify("Usage: /cmd command_name", sender);
                        return;
                    }

                    TryTriggerTimers(sender, args[1]);
                }
                else if(msg.StartsWith("/fcmd"))
                {
                    sendToOthers = false;
                    if (!Constants.IsServer)
                    {
                        new ChatMessage(sender.SteamUserId, msg).Send();
                        return;
                    }

                    string[] args = msg.Split(new[] { ' ' }, 3);
                    if (args.Length < 3)
                    {
                        Notify("Usage: /fcmd faction_tag command_name", sender);
                        return;
                    }

                    TryTriggerFacTimers(sender, args[2], args[1]);
                }
                else if(IsCommand(msg))
                {
                    sendToOthers = false;
                    if (!Constants.IsServer)
                    {
                        new ChatMessage(sender.SteamUserId, msg).Send();
                        return;
                    }

                    if (!TryTriggerTimers(sender, msg))
                        TryTriggerFacTimers(sender, msg);
                }
            }
            else
            {
                // Not Admin
                if (msg == "/cmdlist")
                {
                    sendToOthers = false;
                    if (!Constants.IsServer)
                    {
                        new ChatMessage(sender.SteamUserId, msg).Send();
                        return;
                    }

                    StringBuilder sb = new StringBuilder();
                    int count = 0;
                    sb.AppendLine("Available commands:");

                    IMyFaction fac = MyAPIGateway.Session.Factions.TryGetPlayerFaction(sender.IdentityId);
                    if (fac != null)
                        PrintCmds(sb, ref count, fac.FactionId);

                    if (count == 0)
                        sb.AppendLine("No commands.");

                    Notify(sb.ToString(), sender);
                }
                else if (IsCommand(msg))
                {
                    sendToOthers = false;
                    if (!Constants.IsServer)
                    {
                        new ChatMessage(sender.SteamUserId, msg).Send();
                        return;
                    }

                    TryTriggerFacTimers(sender, msg);
                }
            }
        }

        private void PrintCmds(StringBuilder sb, ref int count, long fId = Constants.noFactionId, string label = null)
        {
            Dictionary<string, HashSet<IMyTimerBlock>> cmdList;
            if (timers.TryGetValue(fId, out cmdList))
            {
                if(label != null)
                    sb.AppendLine(label);
                foreach (string cmd in cmdList.Keys)
                {
                    count++;
                    sb.Append("//").Append(cmd).Append(", ");
                }
                sb.Length -= 2;
                sb.AppendLine();
            }
        }

        private bool IsCommand(string s)
        {
            int len = s.Length;
            return len > 1 && s[0] == '/' && s[1] == '/' && len < Constants.maxCmdLen + 2;
        }

        private bool TryTriggerTimers(IMyPlayer p, string cmd, long fId = Constants.noFactionId)
        {
            HashSet<IMyTimerBlock> temp;
            if(TryGetValue(cmd.TrimStart('/').ToLower(), out temp, fId))
            {
                foreach (IMyTimerBlock t in temp)
                    t.Trigger();
                Notify(temp.Count + " timers triggered.", p);
                return true;
            }
            return false;
        }

        private bool TryTriggerFacTimers(IMyPlayer p, string cmd)
        {
            IMyFaction fac = MyAPIGateway.Session.Factions.TryGetPlayerFaction(p.IdentityId);
            if(fac == null)
                return false;
            return TryTriggerTimers(p, cmd, fac.FactionId);
        }

        private bool TryTriggerFacTimers(IMyPlayer p, string cmd, string tag)
        {
            IMyFaction fac;
            if (tag.Equals("me", StringComparison.OrdinalIgnoreCase))
                fac = MyAPIGateway.Session.Factions.TryGetPlayerFaction(p.IdentityId);
            else
                fac = MyAPIGateway.Session.Factions.TryGetFactionByTag(tag);
            if(fac == null)
                return false;
            return TryTriggerTimers(p, cmd, fac.FactionId);
        }

        private void Notify(string s, IMyPlayer p)
        {
            MyVisualScriptLogicProvider.SendChatMessage(s, "Admin Timers", p.IdentityId);
        }

        public override void UpdateAfterSimulation()
        {
            if (MyAPIGateway.Session == null)
                return;
            if (!init)
            {
                Start();
                MyAPIGateway.Utilities.InvokeOnGameThread(() => SetUpdateOrder(MyUpdateOrder.NoUpdate));
            }

        }
    }
}