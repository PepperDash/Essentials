using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core
{
    public class BasicIrDisplayFactory : EssentialsDeviceFactory<BasicIrDisplay>
    {
        public BasicIrDisplayFactory()
        {
            TypeNames = new List<string>() { "basicirdisplay" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new BasicIrDisplay Device");
            var ir = IRPortHelper.GetIrPort(dc.Properties);
            if (ir != null)
            {
                var display = new BasicIrDisplay(dc.Key, dc.Name, ir.Port, ir.FileName);
                display.IrPulseTime = 200;       // Set default pulse time for IR commands.
                return display;
            }

            return null;
        }
    }
}