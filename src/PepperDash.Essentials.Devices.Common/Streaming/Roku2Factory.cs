using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Devices.Common
{
    public class Roku2Factory : EssentialsDeviceFactory<Roku2>
    {
        public Roku2Factory()
        {
            TypeNames = new List<string>() { "roku" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Roku Device");
            var irCont = IRPortHelper.GetIrOutputPortController(dc);
            return new Roku2(dc.Key, dc.Name, irCont);

        }
    }
}