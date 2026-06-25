using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core.Queues;
using WebSocketSharp;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Represents a TransmitMessage
    /// </summary>
    public class TransmitMessage : IQueueMessage
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Converters = { new IsoDateTimeConverter() }
        };

        private readonly WebSocket _ws;
        private readonly string _serializedMessage;

        /// <summary>
        /// Initialize a message to send.
        /// Serialization occurs here in the caller's thread context rather than on the queue thread.
        /// </summary>
        /// <param name="msg">message object to send</param>
        /// <param name="ws">WebSocket instance</param>
        public TransmitMessage(object msg, WebSocket ws)
        {
            _ws = ws;
            _serializedMessage = JsonConvert.SerializeObject(msg, Formatting.None, SerializerSettings);
        }

        /// <summary>
        /// Initialize a message to send.
        /// Serialization occurs here in the caller's thread context rather than on the queue thread.
        /// </summary>
        /// <param name="msg">message object to send</param>
        /// <param name="ws">WebSocket instance</param>
        public TransmitMessage(DeviceStateMessageBase msg, WebSocket ws)
        {
            _ws = ws;
            _serializedMessage = JsonConvert.SerializeObject(msg, Formatting.None, SerializerSettings);
        }

        #region Implementation of IQueueMessage

        /// <summary>
        /// Dispatch method - only handles WebSocket send since serialization was done at construction time
        /// </summary>
        public void Dispatch()
        {
            try
            {
                if (_ws == null)
                {
                    Debug.LogWarning("Cannot send message.  Websocket client is null");
                    return;
                }

                if (!_ws.IsAlive)
                {
                    Debug.LogWarning("Cannot send message.  Websocket client is not connected");
                    return;
                }

                Debug.LogVerbose("Message TX: {0}", _serializedMessage);

                _ws.Send(_serializedMessage);
            }
            catch (Exception ex)
            {
                Debug.LogError("Caught an exception in the Transmit Processor: {message}", ex.Message);
                Debug.LogDebug(ex, "Stack Trace: ");
            }
        }
        #endregion
    }

}