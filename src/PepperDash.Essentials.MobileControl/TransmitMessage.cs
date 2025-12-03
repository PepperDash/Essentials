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
        private readonly WebSocket _ws;
        private readonly object msgToSend;

        /// <summary>
        /// Initialize a message to send
        /// </summary>
        /// <param name="msg">message object to send</param>
        /// <param name="ws">WebSocket instance</param>
        public TransmitMessage(object msg, WebSocket ws)
        {
            _ws = ws;
            msgToSend = msg;
        }

        /// <summary>
        /// Initialize a message to send
        /// </summary>
        /// <param name="msg">message object to send</param>
        /// <param name="ws">WebSocket instance</param>
        public TransmitMessage(DeviceStateMessageBase msg, WebSocket ws)
        {
            _ws = ws;
            msgToSend = msg;
        }

        #region Implementation of IQueueMessage

        /// <summary>
        /// Dispatch method
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


                var message = JsonConvert.SerializeObject(msgToSend, Formatting.None,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Converters = { new IsoDateTimeConverter() } });

                Debug.LogVerbose("Message TX: {0}", message);

                _ws.Send(message);
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