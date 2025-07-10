using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core.Queues;
using Serilog.Events;
using WebSocketSharp;

namespace PepperDash.Essentials
{
    public class TransmitMessage : IQueueMessage
    {
        private readonly WebSocket _ws;
        private readonly object msgToSend;

        public TransmitMessage(object msg, WebSocket ws)
        {
            _ws = ws;
            msgToSend = msg;
        }

        public TransmitMessage(DeviceStateMessageBase msg, WebSocket ws)
        {
            _ws = ws;
            msgToSend = msg;
        }

        #region Implementation of IQueueMessage

        public void Dispatch()
        {
            try
            {
                if (_ws == null)
                {
                    Debug.LogMessage(LogEventLevel.Warning, "Cannot send message.  Websocket client is null");
                    return;
                }

                if (!_ws.IsAlive)
                {
                    Debug.LogMessage(LogEventLevel.Warning, "Cannot send message.  Websocket client is not connected");
                    return;
                }


                var message = JsonConvert.SerializeObject(msgToSend, Formatting.None,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Converters = { new IsoDateTimeConverter() } });

                Debug.LogMessage(LogEventLevel.Verbose, "Message TX: {0}", null, message);

                _ws.Send(message);


            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Caught an exception in the Transmit Processor");
            }
        }
        #endregion
    }

}