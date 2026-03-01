using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core.Devices
{
    /// <summary>
    /// Base class for all Device APIs
    /// </summary>
    public abstract class DeviceApiBase
    {
        /// <summary>
        /// Action API dictionary
        /// </summary>
        public Dictionary<string, Object> ActionApi { get; protected set; }

        /// <summary>
        /// Feedback API dictionary
        /// </summary>
        public Dictionary<string, Feedback> FeedbackApi { get; protected set; }
    }
}