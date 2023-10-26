using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Devices.Displays
{
    public class AvocorDisplayFactory : EssentialsDeviceFactory<AvocorDisplay>
    {
        public AvocorDisplayFactory()
        {
            TypeNames = new List<string>() { "avocorvtf" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Generic Comm Device");
            var comm = CommFactory.CreateCommForDevice(dc);
            if (comm != null)
                return new AvocorDisplay(dc.Key, dc.Name, comm, null);
            else
                return null;

        }
    }
}