using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Devices
{
    /// <summary>
    /// Base class for all Device APIs
    /// </summary>
    public abstract class DeviceApiBase
    {
        public Dictionary<string, Object> ActionApi { get; protected set; }
        public Dictionary<string, Feedback> FeedbackApi { get; protected set; }
    }
}