using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.GeneralIO;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Aggregates the RoomIsOccupied feedbacks of one or more IOccupancyStatusProvider objects
    /// </summary>
    public class IOccupancyStatusProviderAggregator : EssentialsDevice, IOccupancyStatusProvider
    {
        /// <summary>
        /// Aggregated feedback of all linked IOccupancyStatusProvider devices
        /// </summary>
        public BoolFeedback RoomIsOccupiedFeedback 
        {
            get
            {
                return _aggregatedOccupancyStatus.Output;
            }
        }

        private readonly BoolFeedbackOr _aggregatedOccupancyStatus;

        public IOccupancyStatusProviderAggregator(string key, string name) 
            : base(key, name)
        {
            _aggregatedOccupancyStatus = new BoolFeedbackOr();
        }

        public IOccupancyStatusProviderAggregator(string key, string name, OccupancyAggregatorConfig config)
            : this(dc.Key, dc.Name)
        {
            
        }

        /// <summary>
        /// Adds an IOccupancyStatusProvider device
        /// </summary>
        /// <param name="statusProvider"></param>
        public void AddOccupancyStatusProvider(IOccupancyStatusProvider statusProvider)
        {
            _aggregatedOccupancyStatus.AddOutputIn(statusProvider.RoomIsOccupiedFeedback);
        }

        public void RemoveOccupancyStatusProvider(IOccupancyStatusProvider statusProvider)
        {
            _aggregatedOccupancyStatus.RemoveOutputIn(statusProvider.RoomIsOccupiedFeedback);
        }
    }

    public class OccupancyAggregatorFactory : EssentialsDeviceFactory<IOccupancyStatusProviderAggregator>
    {
        public OccupancyAggregatorFactory()
        {
            TypeNames = new List<string> { "glsodtccn" };
        }


        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new GlsOccupancySensorBaseController Device");

            var config = dc.Properties.ToObject<OccupancyAggregatorConfig>();
                
            return new IOccupancyStatusProviderAggregator(dc.Key, dc.Name, config);
        }
    }
}