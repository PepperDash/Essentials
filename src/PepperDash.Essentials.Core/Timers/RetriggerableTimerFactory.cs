using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.Timers
{
    /// <summary>
    /// Factory class
    /// </summary>
    public class RetriggerableTimerFactory : EssentialsDeviceFactory<RetriggerableTimer>
    {
        public RetriggerableTimerFactory()
        {
            TypeNames = new List<string>() { "retriggerabletimer" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new RetriggerableTimer Device");

            return new RetriggerableTimer(dc.Key, dc);
        }
    }
}