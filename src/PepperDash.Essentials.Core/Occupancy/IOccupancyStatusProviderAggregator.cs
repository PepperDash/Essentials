using System;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.GeneralIO;
using PepperDash.Core;
using PepperDash.Essentials.Core;

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
            : this(key, name)
        {
            AddPostActivationAction(() =>
            {
                if (config.DeviceKeys.Count == 0)
                {
                    return;
                }

                foreach (var deviceKey in config.DeviceKeys)
                {
                    var device = DeviceManager.GetDeviceForKey(deviceKey);

                    if (device == null)
                    {
                        Debug.Console(0, this, Debug.ErrorLogLevel.Notice,
                            "Unable to retrieve Occupancy provider with key {0}", deviceKey);
                        continue;
                    }

                    var provider = device as IOccupancyStatusProvider;

                    if (provider == null)
                    {
                        Debug.Console(0, this, Debug.ErrorLogLevel.Notice,
                            "Device with key {0} does NOT implement IOccupancyStatusProvider. Please check configuration.");
                        continue;
                    }

                    AddOccupancyStatusProvider(provider);
                }
            });
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

        public void ClearOccupancyStatusProviders()
        {
            _aggregatedOccupancyStatus.ClearOutputs();
        }
    }
}