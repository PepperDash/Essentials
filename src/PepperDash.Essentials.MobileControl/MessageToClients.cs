using System;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core.Queues;
using PepperDash.Essentials.WebSocketServer;
using Serilog.Events;

namespace PepperDash.Essentials
{
  /// <summary>
  /// Represents a MessageToClients
  /// </summary>
  public class MessageToClients : IQueueMessage
  {
    private readonly MobileControlWebsocketServer _server;
    private readonly object msgToSend;

    /// <summary>
    /// Message to send to Direct Server Clients
    /// </summary>
    /// <param name="msg">message object to send</param>
    /// <param name="server">WebSocket server instance</param>
    public MessageToClients(object msg, MobileControlWebsocketServer server)
    {
      _server = server;
      msgToSend = msg;
    }

    /// <summary>
    /// Message to send to Direct Server Clients
    /// </summary>
    /// <param name="msg">message object to send</param>
    /// <param name="server">WebSocket server instance</param>
    public MessageToClients(DeviceStateMessageBase msg, MobileControlWebsocketServer server)
    {
      _server = server;
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

          _server.LogVerbose("Message TX To client {clientId}: {message}", clientId, message);

          _server.SendMessageToClient(clientId, message);

          return;
        }

        _server.SendMessageToAllClients(message);

        _server.LogVerbose("Message TX To all clients: {message}", message);
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