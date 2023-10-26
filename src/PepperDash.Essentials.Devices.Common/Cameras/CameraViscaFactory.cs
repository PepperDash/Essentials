using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Devices.Common.Cameras
{
    public class CameraViscaFactory : EssentialsDeviceFactory<CameraVisca>
    {
        public CameraViscaFactory()
        {
            TypeNames = new List<string>() { "cameravisca" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new CameraVisca Device");
            var comm = CommFactory.CreateCommForDevice(dc);
            var props = Newtonsoft.Json.JsonConvert.DeserializeObject<Cameras.CameraViscaPropertiesConfig>(
                dc.Properties.ToString());
            return new Cameras.CameraVisca(dc.Key, dc.Name, comm, props);
        }
    }
}