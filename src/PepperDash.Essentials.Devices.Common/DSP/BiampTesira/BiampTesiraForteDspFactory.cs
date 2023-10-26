using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Devices.Common.DSP
{
    public class BiampTesiraForteDspFactory : EssentialsDeviceFactory<BiampTesiraForteDsp>
    {
        public BiampTesiraForteDspFactory()
        {
            TypeNames = new List<string>() {"biamptesira"};
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new BiampTesira Device");
            var comm = CommFactory.CreateCommForDevice(dc);
            var props = Newtonsoft.Json.JsonConvert.DeserializeObject<BiampTesiraFortePropertiesConfig>(
                dc.Properties.ToString());
            return new BiampTesiraForteDsp(dc.Key, dc.Name, comm, props);
        }
    }
}