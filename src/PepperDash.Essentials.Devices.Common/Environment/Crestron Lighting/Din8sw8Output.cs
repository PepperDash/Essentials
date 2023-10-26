using System;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.CrestronIO;

namespace PepperDash.Essentials.Devices.Common.Environment.Lighting
{
    /// <summary>
    /// Wrapper class to 
    /// </summary>
    public class Din8sw8Output :  ISwitchedOutput
    {
        SwitchedLoadWithOverrideParameter SwitchedOutput;

        public BoolFeedback OutputIsOnFeedback { get; protected set; }

        public Din8sw8Output(SwitchedLoadWithOverrideParameter switchedOutput)
        {
            SwitchedOutput = switchedOutput;

            OutputIsOnFeedback = new BoolFeedback(new Func<bool>(() => SwitchedOutput.IsOn)); 
        }

        public void On()
        {
            SwitchedOutput.FullOn();
        }

        public void Off()
        {
            SwitchedOutput.FullOff();
        }
    }
}