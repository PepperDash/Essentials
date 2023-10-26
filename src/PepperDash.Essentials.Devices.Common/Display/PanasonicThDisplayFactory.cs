using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Devices.Displays
{
    public class PanasonicThDisplayFactory : EssentialsDeviceFactory<PanasonicThDisplay>
    {
        public PanasonicThDisplayFactory()
        {
            TypeNames = new List<string>() { "panasonicthef" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Generic Comm Device");
            var comm = CommFactory.CreateCommForDevice(dc);
            if (comm != null)
                return new PanasonicThDisplay(dc.Key, dc.Name, comm);
            else
                return null;
        }
    }
}