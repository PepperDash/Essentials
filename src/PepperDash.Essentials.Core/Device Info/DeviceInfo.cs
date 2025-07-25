namespace PepperDash.Essentials.Core.DeviceInfo
{
    /// <summary>
    /// Represents a DeviceInfo
    /// </summary>
    public class DeviceInfo
    {
        /// <summary>
        /// Gets or sets the HostName
        /// </summary>
        public string HostName { get; set; } 
        /// <summary>
        /// Gets or sets the IpAddress
        /// </summary>
        public string IpAddress { get; set; }
        /// <summary>
        /// Gets or sets the MacAddress
        /// </summary>
        public string MacAddress { get; set; }
        /// <summary>
        /// Gets or sets the SerialNumber
        /// </summary>
        public string SerialNumber { get; set; }
        /// <summary>
        /// Gets or sets the FirmwareVersion
        /// </summary>
        public string FirmwareVersion { get; set; }
    }
}