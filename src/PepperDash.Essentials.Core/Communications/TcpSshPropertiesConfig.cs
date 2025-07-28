using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Communications
{
    /// <summary>
    /// Represents a TcpSshPropertiesConfig
    /// </summary>
    public class TcpSshPropertiesConfig
    {
        /// <summary>
        /// Address to connect to
        /// </summary>
		[JsonProperty(Required = Required.Always)]
        public string Address { get; set; }

        /// <summary>
        /// Port to connect to
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public int Port { get; set; }

        /// <summary>
        /// Username credential
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Gets or sets the Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Defaults to 32768
        /// </summary>
        public int BufferSize { get; set; }

        /// <summary>
        /// Gets or sets the AutoReconnect
        /// </summary>
        public bool AutoReconnect { get; set; }

        /// <summary>
        /// Gets or sets the AutoReconnectIntervalMs
        /// </summary>
        public int AutoReconnectIntervalMs { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
		public TcpSshPropertiesConfig()
        {
            BufferSize = 32768;
            AutoReconnect = true;
            AutoReconnectIntervalMs = 5000;
            Username = "";
            Password = "";
        }

    }
}