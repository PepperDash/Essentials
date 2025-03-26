using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core.Queues;
using PepperDash.Essentials.WebSocketServer;
using Serilog.Events;
using System;
using System.Threading;
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



    public class MessageToClients : IQueueMessage
    {
        private readonly MobileControlWebsocketServer _server;
        private readonly object msgToSend;

        public MessageToClients(object msg, MobileControlWebsocketServer server)
        {
            _server = server;
            msgToSend = msg;
        }

        public MessageToClients(DeviceStateMessageBase msg, MobileControlWebsocketServer server)
        {
            _server = server;
            msgToSend = msg;
        }

        #region Implementation of IQueueMessage

        public void Dispatch()
        {
            try
            {
                if (_server == null)
                {
                    Debug.LogMessage(LogEventLevel.Warning, "Cannot send message. Server is null");
                    return;
                }

                var message = JsonConvert.SerializeObject(msgToSend, Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Converters = { new IsoDateTimeConverter() } });

                var clientSpecificMessage = msgToSend as MobileControlMessage;
                if (clientSpecificMessage.ClientId != null)
                {
                    var clientId = clientSpecificMessage.ClientId;

                    _server.LogVerbose("Message TX To client {clientId} Message: {message}", clientId, message);

                    _server.SendMessageToClient(clientId, message);

                    return;
                }

                _server.SendMessageToAllClients(message);

                _server.LogVerbose("Message TX To all clients: {message}", null, message);



            }
            catch (ThreadAbortException)
            {
                //Swallowing this exception, as it occurs on shutdown and there's no need to print out a scary stack trace
            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Caught an exception in the Transmit Processor");
            }


        }
        #endregion
    }

}