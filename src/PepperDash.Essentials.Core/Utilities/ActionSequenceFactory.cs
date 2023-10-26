using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.Utilities
{
    /// <summary>
    /// Factory class
    /// </summary>
    public class ActionSequenceFactory : EssentialsDeviceFactory<ActionSequence>
    {
        public ActionSequenceFactory()
        {
            TypeNames = new List<string>() { "actionsequence" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new ActionSequence Device");

            return new ActionSequence(dc.Key, dc);
        }
    }
}