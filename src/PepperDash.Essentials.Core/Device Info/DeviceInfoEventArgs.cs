using System;

namespace PepperDash.Essentials.Core.DeviceInfo
{
    /// <summary>
    /// Represents a DeviceInfoEventArgs
    /// </summary>
    public class DeviceInfoEventArgs:EventArgs
    {
        /// <summary>
        /// Gets or sets the DeviceInfo
        /// </summary>
        public DeviceInfo DeviceInfo { get; set; }

        public DeviceInfoEventArgs()
        {
            
        }

        public DeviceInfoEventArgs(DeviceInfo devInfo)
        {
            DeviceInfo = devInfo;
        }
    }
}