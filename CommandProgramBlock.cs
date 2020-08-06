using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace avaness.AdminTimers
{
    public class CommandProgramBlock : ICommandBlock, IEquatable<CommandProgramBlock>
    {
        private readonly IMyProgrammableBlock block;

        public bool IsFunctional => block.IsWorking;

        public CommandProgramBlock(IMyProgrammableBlock block)
        {
            this.block = block;
        }

        public void Trigger(string arg = null)
        {
            if (arg == null)
                block.Run();
            else
                block.Run(arg);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CommandProgramBlock);
        }

        public bool Equals(CommandProgramBlock other)
        {
            return other != null &&
                   EqualityComparer<IMyProgrammableBlock>.Default.Equals(block, other.block);
        }

        public override int GetHashCode()
        {
            return -344596814 + EqualityComparer<IMyProgrammableBlock>.Default.GetHashCode(block);
        }

        public static bool operator ==(CommandProgramBlock left, CommandProgramBlock right)
        {
            return EqualityComparer<CommandProgramBlock>.Default.Equals(left, right);
        }

        public static bool operator !=(CommandProgramBlock left, CommandProgramBlock right)
        {
            return !(left == right);
        }
    }
}
