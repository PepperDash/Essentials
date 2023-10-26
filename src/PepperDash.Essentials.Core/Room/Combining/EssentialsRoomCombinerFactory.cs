using System.Collections.Generic;
using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    public class EssentialsRoomCombinerFactory : EssentialsDeviceFactory<EssentialsRoomCombiner>
    {
        public EssentialsRoomCombinerFactory()
        {
            TypeNames = new List<string> { "essentialsroomcombiner" };
        }

        public override EssentialsDevice BuildDevice(PepperDash.Essentials.Core.Config.DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new EssentialsRoomCombiner Device");

            var props = dc.Properties.ToObject<EssentialsRoomCombinerPropertiesConfig>();

            return new EssentialsRoomCombiner(dc.Key, props);
        }
    }
}