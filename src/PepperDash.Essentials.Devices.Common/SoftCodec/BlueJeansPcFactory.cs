using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Devices.Common.SoftCodec
{
    public class BlueJeansPcFactory : EssentialsDeviceFactory<BlueJeansPc>
    {
        public BlueJeansPcFactory()
        {
            TypeNames = new List<string>() { "bluejeanspc" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new BlueJeansPc Device");
            return new SoftCodec.BlueJeansPc(dc.Key, dc.Name);
        }
    }
}