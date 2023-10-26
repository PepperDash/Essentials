using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Devices.Common.Environment.Lighting
{
    public class Din8sw8ControllerFactory : EssentialsDeviceFactory<Din8sw8Controller>
    {
        public Din8sw8ControllerFactory()
        {
            TypeNames = new List<string>() { "din8sw8" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Din8sw8Controller Device");
            var comm = CommFactory.GetControlPropertiesConfig(dc);

            return new Din8sw8Controller(dc.Key, comm.CresnetIdInt);

        }
    }
}