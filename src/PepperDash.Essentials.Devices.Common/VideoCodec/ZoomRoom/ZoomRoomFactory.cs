using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.ZoomRoom
{
    public class ZoomRoomFactory : EssentialsDeviceFactory<ZoomRoom>
    {
        public ZoomRoomFactory()
        {
            TypeNames = new List<string> {"zoomroom"};
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new ZoomRoom Device");
            var comm = CommFactory.CreateCommForDevice(dc);
            return new ZoomRoom(dc, comm);
        }
    }
}