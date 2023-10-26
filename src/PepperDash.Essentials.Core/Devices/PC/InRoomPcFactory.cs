using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.Devices
{
    public class InRoomPcFactory : EssentialsDeviceFactory<InRoomPc>
    {
        public InRoomPcFactory()
        {
            TypeNames = new List<string>() { "inroompc" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new InRoomPc Device");
            return new InRoomPc(dc.Key, dc.Name);
        }
    }
}