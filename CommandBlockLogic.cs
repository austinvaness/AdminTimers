using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Text;
using System.Windows.Input;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace avaness.AdminTimers
{
    public abstract class CommandBlockLogic : MyGameLogicComponent
    {
        private IMyTerminalBlock block;
        private ICommandBlock cmdBlock;
        private bool adminOwned;
        private string cmd;
        private IMyFaction fac;
        private bool fake;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            block = (IMyTerminalBlock)Entity;
            cmdBlock = GetCommandBlock(block);
            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        protected abstract ICommandBlock GetCommandBlock(IMyTerminalBlock block);

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
            if (cmd != null)
            {
                sb.Append("Chat command:").AppendLine();
                sb.Append("//").Append(cmd).AppendLine();
                if (fac != null)
                {
                    sb.AppendLine("Faction:");
                    sb.AppendLine(fac.Name);
                }
            }
        }

        private void OwnerModified(IMyTerminalBlock block)
        {
            adminOwned = Constants.IsAdmin(block.OwnerId);
            if (adminOwned)
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
            if (!TryParseCmd(s))
                TryParseFCmd(s);
        }

        private bool TryParseCmd(string s)
        {
            int start = s.IndexOf("[cmd:");
            if (start > 0)
            {
                int end = s.IndexOf(']', start);
                if (end > 0)
                {
                    int len = end - (start + 5);
                    if (len > 0)
                    {
                        string cmd = s.Substring(start + 5, len).TrimStart(' ', '/').TrimEnd().ToLower();
                        if (IsValid(cmd))
                        {
                            this.cmd = cmd;
                            fac = null;
                            block.RefreshCustomInfo();
                            if (!fake)
                                MySession.Instance.Register(this.cmd, cmdBlock);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool IsValid(string cmd)
        {
            return cmd.Length > 0 && cmd.Length <= Constants.maxCmdLen && cmd.IndexOf(' ') == -1;
        }

        private bool TryParseFCmd(string s)
        {
            int start = s.IndexOf("[fcmd:");
            if (start > 0 && start + 6 < s.Length)
            {
                start += 6;
                int mid = s.IndexOf(':', start);
                if (mid > 0 && mid + 1 < s.Length)
                {
                    mid++;
                    int end = s.IndexOf(']', mid);
                    if (end > 0)
                    {
                        int tagLen = mid - start - 1;
                        if (tagLen > 0)
                        {
                            string tag = s.Substring(start, tagLen).Trim();
                            int cmdLen = end - mid;
                            if (tag.Length > 0 && cmdLen > 0)
                            {
                                string cmd = s.Substring(mid, cmdLen).TrimStart(' ', '/').TrimEnd().ToLower();
                                if (IsValid(cmd))
                                {
                                    IMyFaction fac = MyAPIGateway.Session.Factions.TryGetFactionByTag(tag);
                                    if (fac != null)
                                    {
                                        this.cmd = cmd;
                                        this.fac = fac;
                                        block.RefreshCustomInfo();
                                        if (!fake)
                                            MySession.Instance.Register(this.cmd, cmdBlock, fac.FactionId);
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        private void Unregister()
        {
            if (cmd != null)
            {
                if (!fake)
                {
                    if (fac == null)
                        MySession.Instance.Unregister(cmd, cmdBlock);
                    else
                        MySession.Instance.Unregister(cmd, cmdBlock, fac.FactionId);
                }
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