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
    private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
    {
      NullValueHandling = NullValueHandling.Ignore,
      Converters = { new IsoDateTimeConverter() }
    };

    private readonly MobileControlWebsocketServer _server;
    private readonly string _serializedMessage;
    private readonly string _clientId;

    /// <summary>
    /// Message to send to Direct Server Clients.
    /// Serialization occurs here in the caller's thread context (parallel) rather than on the queue thread (sequential).
    /// </summary>
    /// <param name="msg">message object to send</param>
    /// <param name="server">WebSocket server instance</param>
    public MessageToClients(object msg, MobileControlWebsocketServer server)
    {
      _server = server;
      _serializedMessage = JsonConvert.SerializeObject(msg, Formatting.None, SerializerSettings);
      _clientId = (msg as MobileControlMessage)?.ClientId;
    }

    /// <summary>
    /// Message to send to Direct Server Clients.
    /// Serialization occurs here in the caller's thread context (parallel) rather than on the queue thread (sequential).
    /// </summary>
    /// <param name="msg">message object to send</param>
    /// <param name="server">WebSocket server instance</param>
    public MessageToClients(DeviceStateMessageBase msg, MobileControlWebsocketServer server)
    {
      _server = server;
      _serializedMessage = JsonConvert.SerializeObject(msg, Formatting.None, SerializerSettings);
      _clientId = null;
    }

    #region Implementation of IQueueMessage

    /// <summary>
    /// Dispatch method - only handles WebSocket send since serialization was done at construction time
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

        if (_clientId != null)
        {
          _server.LogVerbose("Message TX To client {clientId}: {message}", _clientId, _serializedMessage);

          _server.SendMessageToClient(_clientId, _serializedMessage);

          return;
        }

        _server.SendMessageToAllClients(_serializedMessage);

        _server.LogVerbose("Message TX To all clients: {message}", _serializedMessage);
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