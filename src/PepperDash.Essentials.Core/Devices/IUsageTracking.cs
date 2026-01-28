using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines the contract for IUsageTracking
    /// </summary>
    public interface IUsageTracking
    {
        /// <summary>
        /// Gets or sets the UsageTracker
        /// </summary>
        UsageTracking UsageTracker { get; set; }
    }

    //public static class IUsageTrackingExtensions
    //{
    //    public static void EnableUsageTracker(this IUsageTracking device)
    //    {
    //        device.UsageTracker = new UsageTracking();
    //    }
    //}

    /// <summary>
    /// Represents a UsageTracking
    /// </summary>
    public class UsageTracking
    {
        /// <summary>
        /// Event fired when device usage ends
        /// </summary>
        public event EventHandler<DeviceUsageEventArgs> DeviceUsageEnded;

        /// <summary>
        /// Gets or sets the InUseTracker
        /// </summary>
        public InUseTracking InUseTracker { get; protected set; }

        /// <summary>
        /// Gets or sets the UsageIsTracked
        /// </summary>
        public bool UsageIsTracked { get; set; }

        /// <summary>
        /// Gets or sets the UsageTrackingStarted
        /// </summary>
        public bool UsageTrackingStarted { get; protected set; }
        /// <summary>
        /// Gets or sets the UsageStartTime
        /// </summary>
        public DateTime UsageStartTime { get; protected set; }
        /// <summary>
        /// Gets or sets the UsageEndTime
        /// </summary>
        public DateTime UsageEndTime { get; protected set; }

        /// <summary>
        /// Gets or sets the Parent
        /// </summary>
        public Device Parent { get; private set; }

        /// <summary>
        /// Constructor for UsageTracking class
        /// </summary>
        /// <param name="parent">The parent device</param>
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
        /// StartDeviceUsage method
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
                        Debug.LogMessage(LogEventLevel.Debug, "Device Usage Ended for: {0} at {1}.  In use for {2} minutes.", Parent.Name, UsageEndTime, timeUsed.Minutes);
                        handler(this, new DeviceUsageEventArgs() { UsageEndTime = UsageEndTime, MinutesUsed = timeUsed.Minutes });
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogMessage(LogEventLevel.Debug, "Error ending device usage: {0}", e);
            }
        }
    }

    /// <summary>
    /// Represents a DeviceUsageEventArgs
    /// </summary>
    public class DeviceUsageEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the UsageEndTime
        /// </summary>
        public DateTime UsageEndTime { get; set; }
        /// <summary>
        /// Gets or sets the MinutesUsed
        /// </summary>
        public int MinutesUsed { get; set; }
    }
}