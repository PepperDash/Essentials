using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.Shades
{
    public class ShadeControllerFactory : EssentialsDeviceFactory<ShadeController>
    {
        public ShadeControllerFactory()
        {
            TypeNames = new List<string>() { "shadecontroller" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new ShadeController Device");
            var props = Newtonsoft.Json.JsonConvert.DeserializeObject<Core.Shades.ShadeControllerConfigProperties>(dc.Properties.ToString());

            return new Core.Shades.ShadeController(dc.Key, dc.Name, props);
        }
    }
}