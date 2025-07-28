using System;
using Crestron.SimplSharp.CrestronSockets;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Communications
{
    /// <summary>
    /// For IBasicCommunication classes that have SocketStatus. GenericSshClient,
    /// GenericTcpIpClient
    /// </summary>
    public interface ISocketStatus : IBasicCommunication
	{
        /// <summary>
        /// Notifies of socket status changes
        /// </summary>
		event EventHandler<GenericSocketStatusChageEventArgs> ConnectionChange;

        /// <summary>
        /// The current socket status of the client
        /// </summary>
        [JsonProperty("clientStatus")]
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        SocketStatus ClientStatus { get; }
	}
}