using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core
{
    public class RoomOnToDefaultSourceWhenOccupiedFactory : EssentialsDeviceFactory<RoomOnToDefaultSourceWhenOccupied>
    {
        public RoomOnToDefaultSourceWhenOccupiedFactory()
        {
            TypeNames = new List<string>() { "roomonwhenoccupancydetectedfeature" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new RoomOnToDefaultSourceWhenOccupied Device");
            return new RoomOnToDefaultSourceWhenOccupied(dc);
        }
    }
}