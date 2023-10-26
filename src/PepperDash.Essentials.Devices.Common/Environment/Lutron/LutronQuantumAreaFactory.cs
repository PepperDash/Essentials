using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Devices.Common.Environment.Lutron
{
    public class LutronQuantumAreaFactory : EssentialsDeviceFactory<LutronQuantumArea>
    {
        public LutronQuantumAreaFactory()
        {
            TypeNames = new List<string>() { "lutronqs" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new LutronQuantumArea Device");
            var comm = CommFactory.CreateCommForDevice(dc);

            var props = Newtonsoft.Json.JsonConvert.DeserializeObject<Environment.Lutron.LutronQuantumPropertiesConfig>(dc.Properties.ToString());

            return new LutronQuantumArea(dc.Key, dc.Name, comm, props);
        }
    }
}