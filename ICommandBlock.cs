using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace avaness.AdminTimers
{
    public interface ICommandBlock
    {
        void Trigger(string arg = null);

        bool IsFunctional { get; }
    }
}
