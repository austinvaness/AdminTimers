using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace avaness.AdminTimers
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_MyProgrammableBlock), false)]
    public class ProgramBlockLogic : CommandBlockLogic
    {
        protected override ICommandBlock GetCommandBlock(IMyTerminalBlock block)
        {
            return new CommandProgramBlock((IMyProgrammableBlock)block);
        }
    }
}