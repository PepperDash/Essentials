using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// DeviceFeedbackExtensions class
    /// </summary>
    public static class DeviceFeedbackExtensions
    {
        /// <summary>
        /// Attempts to get and return a feedback property from a device by name.
        /// If unsuccessful, returns null.
        /// </summary>
        /// <param name="device">device to get feedback from</param>
        /// <param name="propertyName">name of the feedback property</param>
        /// <returns>Feedback property if found, otherwise null</returns>
        public static Feedback GetFeedbackProperty(this Device device, string propertyName)
        {
            var feedback = DeviceJsonApi.GetPropertyByName(device.Key, propertyName) as Feedback;

            if (feedback != null)
            {
                return feedback;
            }

            return null;
        }
    }
}