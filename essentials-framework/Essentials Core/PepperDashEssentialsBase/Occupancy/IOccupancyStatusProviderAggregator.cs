using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Aggregates the RoomIsOccupied feedbacks of one or more IOccupancyStatusProvider objects
    /// </summary>
    public class IOccupancyStatusProviderAggregator : Device, IOccupancyStatusProvider
    {
        /// <summary>
        /// Aggregated feedback of all linked IOccupancyStatusProvider devices
        /// </summary>
        public BoolFeedback RoomIsOccupiedFeedback 
        {
            get
            {
                return AggregatedOccupancyStatus.Output;
            }
        }

        private BoolFeedbackOr AggregatedOccupancyStatus;

        public IOccupancyStatusProviderAggregator(string key, string name) 
            : base(key, name)
        {
            AggregatedOccupancyStatus = new BoolFeedbackOr();
        }

        /// <summary>
        /// Adds an IOccupancyStatusProvider device
        /// </summary>
        /// <param name="statusProvider"></param>
        public void AddOccupancyStatusProvider(IOccupancyStatusProvider statusProvider)
        {
            AggregatedOccupancyStatus.AddOutputIn(statusProvider.RoomIsOccupiedFeedback);
        }
    }
}