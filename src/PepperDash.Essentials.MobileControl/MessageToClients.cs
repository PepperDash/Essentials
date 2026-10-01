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
using Stopwatch = System.Diagnostics.Stopwatch;

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

    // Perf: the batch-sync terminator's time in the transmit queue is logged when it is sent. It is
    // queued after every reply in its batch, so that wait is how long the batch took to go out.
    private const string InitialSyncCompleteType = "/system/initialSyncComplete";
    private readonly string _type;
    private readonly long _createdTimestamp = Stopwatch.GetTimestamp();

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
      _type = (msg as MobileControlMessage)?.Type;
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

          var sendStart = Stopwatch.GetTimestamp();
          _server.SendMessageToClient(_clientId, _serializedMessage);

          if (_type == InitialSyncCompleteType)
          {
            _server.LogDebug("Perf: initialSyncComplete sent to client {clientId} after {queueMs:F1} ms in the transmit queue ({sendMs:F1} ms to send)",
              _clientId, ElapsedMs(_createdTimestamp, sendStart), ElapsedMs(sendStart, Stopwatch.GetTimestamp()));
          }

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

    private static double ElapsedMs(long from, long to) => (to - from) * 1000.0 / Stopwatch.Frequency;
  }

}