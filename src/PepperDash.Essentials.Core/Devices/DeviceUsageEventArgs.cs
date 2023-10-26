using System;

namespace PepperDash.Essentials.Core
{
    public class DeviceUsageEventArgs : EventArgs
    {
        public DateTime UsageEndTime { get; set; }
        public int MinutesUsed { get; set; }
    }
}