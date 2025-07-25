using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Represents a EthernetAdapterInfo
    /// </summary>
    public class EthernetAdapterInfo
    {
        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        public EthernetAdapterType Type { get; set; }
        /// <summary>
        /// Gets or sets the DhcpIsOn
        /// </summary>
        public bool DhcpIsOn { get; set; }
        /// <summary>
        /// Gets or sets the Hostname
        /// </summary>
        public string Hostname { get; set; }
        /// <summary>
        /// Gets or sets the MacAddress
        /// </summary>
        public string MacAddress { get; set; }
        /// <summary>
        /// Gets or sets the IpAddress
        /// </summary>
        public string IpAddress { get; set; }
        /// <summary>
        /// Gets or sets the Subnet
        /// </summary>
        public string Subnet { get; set; }
        /// <summary>
        /// Gets or sets the Gateway
        /// </summary>
        public string Gateway { get; set; }
        /// <summary>
        /// Gets or sets the Dns1
        /// </summary>
        public string Dns1 { get; set; }
        /// <summary>
        /// Gets or sets the Dns2
        /// </summary>
        public string Dns2 { get; set; }
        /// <summary>
        /// Gets or sets the Dns3
        /// </summary>
        public string Dns3 { get; set; }
        /// <summary>
        /// Gets or sets the Domain
        /// </summary>
        public string Domain { get; set; }
    }
}