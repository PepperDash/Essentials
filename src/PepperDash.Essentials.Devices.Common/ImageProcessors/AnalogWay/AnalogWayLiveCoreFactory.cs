using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Devices.Common
{
    public class AnalogWayLiveCoreFactory : EssentialsDeviceFactory<AnalogWayLiveCore>
    {
        public AnalogWayLiveCoreFactory()
        {
            TypeNames = new List<string>() { "analogwaylivecore" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new AnalogWayLiveCore Device");
            var comm = CommFactory.CreateCommForDevice(dc);
            var props = Newtonsoft.Json.JsonConvert.DeserializeObject<AnalogWayLiveCorePropertiesConfig>(
                dc.Properties.ToString());
            return new AnalogWayLiveCore(dc.Key, dc.Name, comm, props);
        }
    }
}