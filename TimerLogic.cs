using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Text;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;

namespace avaness.AdminTimers
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TimerBlock), false)]
    public class TimerLogic : MyGameLogicComponent
    {
        private IMyTimerBlock block;
        private bool adminOwned;
        private string cmd;
        private bool fake;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            block = (IMyTimerBlock)Entity;
            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            if (block.CubeGrid?.Physics == null)
                return;
            fake = !Constants.IsServer;
            block.AppendingCustomInfo += AppendingCustomInfo;
            OwnerModified(block);
            block.OwnershipChanged += OwnerModified;
            NameModified(block);
            block.CustomNameChanged += NameModified;
        }

        private void AppendingCustomInfo(IMyTerminalBlock block, StringBuilder sb)
        {
            if(cmd != null)
            {
                sb.Append("Chat command:").AppendLine();
                sb.Append(cmd).AppendLine();
            }
        }

        private void OwnerModified(IMyTerminalBlock block)
        {
            adminOwned = Constants.IsAdmin(block.OwnerId);
            if(adminOwned)
                NameModified(block);
            else
                Unregister();
        }

        private void NameModified(IMyTerminalBlock block)
        {
            Unregister();

            if (!adminOwned)
                return;

            string s = block.CustomName;
            int start = s.IndexOf("[cmd:");
            if(start > 0)
            {
                int end = s.IndexOf(']', start);
                if (end > 0)
                {
                    int len = end - (start + 5);
                    if (len > 0)
                    {
                        string cmd = s.Substring(start + 5, len).TrimStart(' ', '/').TrimEnd().ToLower();
                        if(cmd.Length > 0 && cmd.Length <= Constants.maxCmdLen)
                        {
                            this.cmd = "//" + cmd;
                            block.RefreshCustomInfo();
                            if(!fake)
                                MySession.Instance.Register(this.cmd, this.block);
                            return;
                        }
                    }
                }
            }
        }

        private void Unregister()
        {
            if (cmd != null)
            {
                if(!fake)
                    MySession.Instance.Unregister(cmd, block);
                cmd = null;
                block.RefreshCustomInfo();
            }
        }

        public override void Close()
        {
            Unregister();
            block.CustomNameChanged -= NameModified;

        }
    }
}