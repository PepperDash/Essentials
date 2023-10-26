extern alias Full;
using System.Collections.Generic;
using Full::Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.CrestronIO
{
    public class GenericVersiportDigitalOutputDeviceFactory : EssentialsDeviceFactory<GenericVersiportDigitalInputDevice>
    {
        public GenericVersiportDigitalOutputDeviceFactory()
        {
            TypeNames = new List<string>() { "versiportoutput" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Generic Versiport Device");

            var props = JsonConvert.DeserializeObject<IOPortConfig>(dc.Properties.ToString());

            if (props == null) return null;

            var portDevice = new GenericVersiportDigitalOutputDevice(dc.Key, dc.Name, GenericVersiportDigitalOutputDevice.GetVersiportDigitalOutput, props);

            return portDevice;
        }
    }
}