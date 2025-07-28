using System;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Communications
{
    /// <summary>
    /// An incoming communication stream
    /// </summary>
    public interface ICommunicationReceiver : IKeyed
    {
        /// <summary>
        /// Notifies of bytes received
        /// </summary>
        event EventHandler<GenericCommMethodReceiveBytesArgs> BytesReceived;
        /// <summary>
        /// Notifies of text received
        /// </summary>
        event EventHandler<GenericCommMethodReceiveTextArgs> TextReceived;

        /// <summary>
        /// Indicates connection status
        /// </summary>
        [JsonProperty("isConnected")]
        bool IsConnected { get; }
        /// <summary>
        /// Connect to the device
        /// </summary>
        void Connect();
        /// <summary>
        /// Disconnect from the device
        /// </summary>
        void Disconnect();
    }
}