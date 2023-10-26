extern alias Full;
using System.Collections.Generic;
using Full::Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.CrestronIO
{
    public class GenericVersiportAbalogInputDeviceFactory : EssentialsDeviceFactory<GenericVersiportAnalogInputDevice>
    {
        public GenericVersiportAbalogInputDeviceFactory()
        {
            TypeNames = new List<string>() { "versiportanaloginput" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Generic Versiport Device");

            var props = JsonConvert.DeserializeObject<IOPortConfig>(dc.Properties.ToString());

            if (props == null) return null;

            var portDevice = new GenericVersiportAnalogInputDevice(dc.Key, dc.Name, GenericVersiportAnalogInputDevice.GetVersiportDigitalInput, props);

            return portDevice;
        }
    }
}