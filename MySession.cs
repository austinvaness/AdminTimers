using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;

namespace avaness.AdminTimers
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class MySession : MySessionComponentBase
    {
        public static MySession Instance;

        private readonly Dictionary<string, HashSet<IMyTimerBlock>> timers = new Dictionary<string, HashSet<IMyTimerBlock>>();
        private bool init = false;

        public MySession()
        {
            Instance = this;
        }

        public void Register(string cmd, IMyTimerBlock timer)
        {
            HashSet<IMyTimerBlock> temp;
            if (timers.TryGetValue(cmd, out temp))
            {
                temp.Add(timer);
            }
            else
            {
                timers[cmd] = new HashSet<IMyTimerBlock>
                {
                    timer
                };
            }
        }

        public void Unregister(string cmd, IMyTimerBlock timer)
        {
            HashSet<IMyTimerBlock> temp;
            if(timers.TryGetValue(cmd, out temp))
            {
                temp.Remove(timer);
                if (temp.Count == 0)
                    timers.Remove(cmd);
            }
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
                int len = msg.Length;
                if (len > 1 && msg[0] == '/' && msg[1] == '/' && len < Constants.maxCmdLen + 2)
                {
                    sendToOthers = false;
                    if (!Constants.IsServer)
                    {
                        new ChatMessage(sender.SteamUserId, msg).Send();
                        return;
                    }

                    HashSet<IMyTimerBlock> temp;
                    if(timers.TryGetValue(msg, out temp))
                    {
                        foreach (IMyTimerBlock t in temp)
                            t.Trigger();
                        MyAPIGateway.Utilities.ShowMessage("Admin Timers", temp.Count + " timers triggered.");
                    }
                }
            }
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