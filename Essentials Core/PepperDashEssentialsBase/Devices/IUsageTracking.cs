using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    public interface IUsageTracking
    {
        UsageTracking UsageTracker { get; set; }
    }

    //public static class IUsageTrackingExtensions
    //{
    //    public static void EnableUsageTracker(this IUsageTracking device)
    //    {
    //        device.UsageTracker = new UsageTracking();
    //    }
    //}

    public class UsageTracking
    {
        public event EventHandler<DeviceUsageEventArgs> DeviceUsageEnded;

        public InUseTracking InUseTracker { get; protected set; }

        public bool UsageIsTracked { get; set; }
        public DateTime UsageStartTime { get; protected set; }
        public DateTime UsageEndTime { get; protected set; }

        public UsageTracking()
        {
            InUseTracker = new InUseTracking();

            InUseTracker.InUseFeedback.OutputChange +=new EventHandler<EventArgs>(InUseFeedback_OutputChange);
        }

        void  InUseFeedback_OutputChange(object sender, EventArgs e)
        {
 	        if(InUseTracker.InUseFeedback.BoolValue)
            {
                StartDeviceUsage();
            }
            else
            {
                EndDeviceUsage();
            }
        }


        /// <summary>
        /// Stores the usage start time
        /// </summary>
        public void StartDeviceUsage()
        {
            UsageStartTime = DateTime.Now;
        }

        /// <summary>
        /// Calculates the difference between the usage start and end times, gets the total minutes used and fires an event to pass that info to a consumer
        /// </summary>
        public void EndDeviceUsage()
        {
            UsageEndTime = DateTime.Now;

            var timeUsed = UsageEndTime - UsageStartTime;

            var handler = DeviceUsageEnded;

            if (handler != null)
            {
                Debug.Console(1, "Device Usage Ended at {1}.  In use for {2} minutes.", UsageEndTime, timeUsed.Minutes);
                handler(this, new DeviceUsageEventArgs() { UsageEndTime = UsageEndTime, MinutesUsed = timeUsed.Minutes });
            }
        }
    }

    public class DeviceUsageEventArgs : EventArgs
    {
        public DateTime UsageEndTime { get; set; }
        public int MinutesUsed { get; set; }
    }
}