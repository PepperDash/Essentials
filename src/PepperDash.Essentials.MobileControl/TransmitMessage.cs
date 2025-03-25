using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core.Queues;
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

                //Debug.Console(2, "Dispatching message type: {0}", msgToSend.GetType());

                //Debug.Console(2, "Message: {0}", msgToSend.ToString());

                //var messageToSend = JObject.FromObject(msgToSend);

                if (_ws != null && _ws.IsAlive)
                {
                    var message = JsonConvert.SerializeObject(msgToSend, Formatting.None,
                        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Converters = { new IsoDateTimeConverter() } });

                    Debug.Console(2, "Message TX: {0}", message);

                    _ws.Send(message);
                }
                else if (_ws == null)
                {
                    Debug.Console(1, "Cannot send. No client.");
                }
            }
            catch (Exception ex)
            {
                Debug.Console(0, Debug.ErrorLogLevel.Error, "Caught an exception in the Transmit Processor {0}\r{1}\r{2}", ex.Message, ex.InnerException, ex.StackTrace);
                Debug.Console(2, Debug.ErrorLogLevel.Error, "Stack Trace: {0}", ex.StackTrace);

                if (ex.InnerException != null)
                {
                    Debug.Console(0, Debug.ErrorLogLevel.Error, "Inner Exception: {0}", ex.InnerException.Message);
                    Debug.Console(2, Debug.ErrorLogLevel.Error, "Stack Trace: {0}", ex.InnerException.StackTrace);
                }
            }


        }
        #endregion
    }


#if SERIES4
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
                //Debug.Console(2, "Message: {0}", msgToSend.ToString());

                if (_server != null)
                {
                    Debug.Console(2, _server, Debug.ErrorLogLevel.Notice, "Dispatching message type: {0}", msgToSend.GetType());

                    var message = JsonConvert.SerializeObject(msgToSend, Formatting.None,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Converters = { new IsoDateTimeConverter() } });

                    var clientSpecificMessage = msgToSend as MobileControlMessage;
                    if (clientSpecificMessage.ClientId != null)
                    {
                        var clientId = clientSpecificMessage.ClientId;

                        Debug.Console(2, _server, "Message TX To Client ID: {0} Message: {1}", clientId, message);

                        _server.SendMessageToClient(clientId, message);
                    }
                    else
                    {
                        _server.SendMessageToAllClients(message);

                        Debug.Console(2, "Message TX To Clients: {0}", message);
                    }
                }
                else if (_server == null)
                {
                    Debug.Console(1, "Cannot send. No server.");
                }
            }
            catch (ThreadAbortException)
            {
                //Swallowing this exception, as it occurs on shutdown and there's no need to print out a scary stack trace
            }
            catch (Exception ex)
            {
                Debug.Console(0, Debug.ErrorLogLevel.Error, "Caught an exception in the Transmit Processor {0}", ex.Message);
                Debug.Console(2, Debug.ErrorLogLevel.Error, "Stack Trace: {0}", ex.StackTrace);

                if (ex.InnerException != null)
                {
                    Debug.Console(0, Debug.ErrorLogLevel.Error, "----\r\n{0}", ex.InnerException.Message);
                    Debug.Console(2, Debug.ErrorLogLevel.Error, "Stack Trace: {0}", ex.InnerException.StackTrace);
                }
            }


        }
        #endregion
    }

#endif
}