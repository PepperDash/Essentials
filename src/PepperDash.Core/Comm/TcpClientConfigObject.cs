using Newtonsoft.Json;

namespace PepperDash.Core
{
    /// <summary>
    /// Client config object for TCP client with server that inherits from TcpSshPropertiesConfig and adds properties for shared key and heartbeat
    /// </summary>
    public class TcpClientConfigObject
    {
        /// <summary>
        /// TcpSsh Properties 
        /// </summary>
        [JsonProperty("control")]
        public ControlPropertiesConfig Control { get; set; }

        /// <summary>
        /// Bool value for secure. Currently not implemented in TCP sockets as they are not dynamic
        /// </summary>
        [JsonProperty("secure")]
        public bool Secure { get; set; }

        /// <summary>
        /// Require a shared key that both server and client negotiate. If negotiation fails server disconnects the client
        /// </summary>
        [JsonProperty("sharedKeyRequired")]
        public bool SharedKeyRequired { get; set; }

        /// <summary>
        /// The shared key that must match on the server and client
        /// </summary>
        [JsonProperty("sharedKey")]
        public string SharedKey { get; set; }

        /// <summary>
        /// Require a heartbeat on the client/server connection that will cause the server/client to disconnect if the heartbeat is not received. 
        /// heartbeats do not raise received events. 
        /// </summary>
        [JsonProperty("heartbeatRequired")]
        public bool HeartbeatRequired { get; set; }

        /// <summary>
        /// The interval in seconds for the heartbeat from the client. If not received client is disconnected
        /// </summary>
        [JsonProperty("heartbeatRequiredIntervalInSeconds")]
        public ushort HeartbeatRequiredIntervalInSeconds { get; set; }

        /// <summary>
        /// HeartbeatString that will be checked against the message received. defaults to heartbeat if no string is provided. 
        /// </summary>
        [JsonProperty("heartbeatStringToMatch")]
        public string HeartbeatStringToMatch { get; set; }

        /// <summary>
        /// Receive Queue size must be greater than 20 or defaults to 20
        /// </summary>
        [JsonProperty("receiveQueueSize")]
        public int ReceiveQueueSize { get; set; }
    }
}