using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace avaness.AdminTimers
{
    public class CommandTimer : ICommandBlock, IEquatable<CommandTimer>
    {
        private readonly IMyTimerBlock block;

        public bool IsFunctional => block.IsWorking;

        public CommandTimer(IMyTimerBlock block)
        {
            this.block = block;
        }

        public void Trigger(string arg = null)
        {
            block.Trigger();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CommandTimer);
        }

        public bool Equals(CommandTimer other)
        {
            return other != null &&
                   EqualityComparer<IMyTimerBlock>.Default.Equals(block, other.block);
        }

        public override int GetHashCode()
        {
            return -344596814 + EqualityComparer<IMyTimerBlock>.Default.GetHashCode(block);
        }

        public static bool operator ==(CommandTimer left, CommandTimer right)
        {
            return EqualityComparer<CommandTimer>.Default.Equals(left, right);
        }

        public static bool operator !=(CommandTimer left, CommandTimer right)
        {
            return !(left == right);
        }
    }
}
