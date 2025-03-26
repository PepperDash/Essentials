using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Core
{
    /// <summary>
    /// Tcp Server Config object with properties for a tcp server with shared key and heartbeat capabilities
    /// </summary>
    public class TcpServerConfigObject
    {
        /// <summary>
        /// Uique key
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// Max Clients that the server will allow to connect. 
        /// </summary>
        public ushort MaxClients { get; set; }
        /// <summary>
        /// Bool value for secure. Currently not implemented in TCP sockets as they are not dynamic
        /// </summary>
        public bool Secure { get; set; }
        /// <summary>
        /// Port for the server to listen on
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// Require a shared key that both server and client negotiate. If negotiation fails server disconnects the client
        /// </summary>
        public bool SharedKeyRequired { get; set; }
        /// <summary>
        /// The shared key that must match on the server and client
        /// </summary>
        public string SharedKey { get; set; }
        /// <summary>
        /// Require a heartbeat on the client/server connection that will cause the server/client to disconnect if the heartbeat is not received. 
        /// heartbeats do not raise received events. 
        /// </summary>
        public bool HeartbeatRequired { get; set; }
        /// <summary>
        /// The interval in seconds for the heartbeat from the client. If not received client is disconnected
        /// </summary>
        public ushort HeartbeatRequiredIntervalInSeconds { get; set; }
        /// <summary>
        /// HeartbeatString that will be checked against the message received. defaults to heartbeat if no string is provided. 
        /// </summary>
        public string HeartbeatStringToMatch { get; set; }
        /// <summary>
        /// Client buffer size. See Crestron help. defaults to 2000 if not greater than 2000
        /// </summary>
        public int BufferSize { get; set; }
        /// <summary>
        /// Receive Queue size must be greater than 20 or defaults to 20
        /// </summary>
        public int ReceiveQueueSize { get; set; }
    }
}