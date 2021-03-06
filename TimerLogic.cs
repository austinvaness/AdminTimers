﻿using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Text;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace avaness.AdminTimers
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TimerBlock), false)]
    public class TimerLogic : CommandBlockLogic
    {
        protected override ICommandBlock GetCommandBlock(IMyTerminalBlock block)
        {
            return new CommandTimer((IMyTimerBlock)block);
        }
    }
}