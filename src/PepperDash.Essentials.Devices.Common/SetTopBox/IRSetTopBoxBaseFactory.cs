using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Devices.Common
{
    public class IRSetTopBoxBaseFactory : EssentialsDeviceFactory<IRSetTopBoxBase>
    {
        public IRSetTopBoxBaseFactory()
        {
            TypeNames = new List<string>() { "settopbox" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new SetTopBox Device");
            var irCont = IRPortHelper.GetIrOutputPortController(dc);
            var config = dc.Properties.ToObject<SetTopBoxPropertiesConfig>();
            var stb    = new IRSetTopBoxBase(dc.Key, dc.Name, irCont, config);

            var listName = dc.Properties.Value<string>("presetsList");
            if (listName != null)
                stb.LoadPresets(listName);
            return stb;

        }
    }
}