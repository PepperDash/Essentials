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

        /// <summary>
        /// Constructor
        /// </summary>
        public DeviceInfoEventArgs()
        {
            
        }

        /// <summary>
        /// Constructor with DeviceInfo
        /// </summary>
        /// <param name="devInfo">the DeviceInfo instance</param>
        public DeviceInfoEventArgs(DeviceInfo devInfo)
        {
            DeviceInfo = devInfo;
        }
    }
}