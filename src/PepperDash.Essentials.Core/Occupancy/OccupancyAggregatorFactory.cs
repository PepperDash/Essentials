using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core
{
    public class OccupancyAggregatorFactory : EssentialsDeviceFactory<IOccupancyStatusProviderAggregator>
    {
        public OccupancyAggregatorFactory()
        {
            TypeNames = new List<string> { "occupancyAggregator", "occAggregate" };
        }


        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new GlsOccupancySensorBaseController Device");

            var config = dc.Properties.ToObject<OccupancyAggregatorConfig>();
                
            return new IOccupancyStatusProviderAggregator(dc.Key, dc.Name, config);
        }
    }
}