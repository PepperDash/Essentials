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

        public bool UsageTrackingStarted { get; protected set; }
        public DateTime UsageStartTime { get; protected set; }
        public DateTime UsageEndTime { get; protected set; }

        public Device Parent { get; private set; }

        public UsageTracking(Device parent)
        {
            Parent = parent;
   
            InUseTracker = new InUseTracking();

            InUseTracker.InUseFeedback.OutputChange += InUseFeedback_OutputChange; //new EventHandler<EventArgs>();
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
            UsageTrackingStarted = true;
            UsageStartTime = DateTime.Now;
        }

        /// <summary>
        /// Calculates the difference between the usage start and end times, gets the total minutes used and fires an event to pass that info to a consumer
        /// </summary>
        public void EndDeviceUsage()
        {
            try
            {
                UsageTrackingStarted = false;

                UsageEndTime = DateTime.Now;

                if (UsageStartTime != null)
                {
                    var timeUsed = UsageEndTime - UsageStartTime;

                    var handler = DeviceUsageEnded;

                    if (handler != null)
                    {
                        Debug.Console(1, "Device Usage Ended for: {0} at {1}.  In use for {2} minutes.", Parent.Name, UsageEndTime, timeUsed.Minutes);
                        handler(this, new DeviceUsageEventArgs() { UsageEndTime = UsageEndTime, MinutesUsed = timeUsed.Minutes });
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Console(1, "Error ending device usage: {0}", e);
            }
        }
    }

    public class DeviceUsageEventArgs : EventArgs
    {
        public DateTime UsageEndTime { get; set; }
        public int MinutesUsed { get; set; }
    }
}