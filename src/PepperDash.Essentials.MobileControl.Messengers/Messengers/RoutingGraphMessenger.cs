using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Web;
using PepperDash.Essentials.Core.Web.RequestHandlers;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Serves the system's routing graph (devices, ports, tie lines and current routes) over Mobile
    /// Control, and runs routing commands, so a touchpanel can show the same routing view as the Dev
    /// Tools app without reaching the Essentials web API.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>/fullStatus</c> replies with the graph and the current routes. After that, every route or
    /// layout change posts the full set of current routes again (debounced). Every midpoint and sink
    /// is always included, with an empty list when nothing is routed, because clients merge each
    /// status message into the last and would otherwise keep showing a cleared route.
    /// </para>
    /// <para>
    /// <c>/routingCommand</c> takes the same body as the web API's <c>routingCommand</c> endpoint plus
    /// a client-chosen <c>requestId</c>, and replies to that client only with
    /// <c>/event/device/routingGraph/commandResult</c>: <c>{ requestId, statusCode, response }</c>.
    /// </para>
    /// </remarks>
    public class RoutingGraphMessenger : MessengerBase
    {
        /// <summary>
        /// The message type of a routing command reply.
        /// </summary>
        public const string CommandResultType = "/event/device/routingGraph/commandResult";

        private const double DebounceMs = 100;

        private readonly object _subscriptionLock = new object();
        private bool _subscribed;

        private readonly object _debounceLock = new object();
        private Timer _debounceTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutingGraphMessenger"/> class.
        /// </summary>
        /// <param name="key">The messenger's key.</param>
        /// <param name="messagePath">The message path, normally <c>/device/routingGraph</c>.</param>
        public RoutingGraphMessenger(string key, string messagePath)
            : base(key, messagePath)
        {
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            AddAction("/routingCommand", (id, content) => RunRoutingCommand(id, content));
        }

        private void SendFullStatus(string clientId)
        {
            // Devices are all built by the time a client can ask, so attach the handlers here.
            EnsureSubscribed();

            var graph = GetRoutingDevicesAndTieLinesHandler.BuildRoutingSystemInfo();
            var routes = BuildRoutes();

            PostStatusMessage(JToken.FromObject(new RoutingGraphStateMessage
            {
                Devices = graph.Devices,
                TieLines = graph.TieLines,
                MidpointRoutes = routes.MidpointRoutes,
                SinkRoutes = routes.SinkRoutes,
                Layouts = routes.Layouts
            }), clientId: clientId);
        }

        private void RunRoutingCommand(string clientId, JToken content)
        {
            // Only an object can carry a requestId. Anything else (an array, a bare value, null) still
            // goes through deserialization and validation, so the sender gets the web API's 400.
            var requestId = (content as JObject)?["requestId"]?.ToString();

            RoutingCommandRequest request;
            try
            {
                request = content?.ToObject<RoutingCommandRequest>();
            }
            catch (Exception ex) when (ex is JsonException || ex is ArgumentException)
            {
                ReplyToCommand(clientId, requestId, 400, new RoutingCommandResponse
                {
                    Status = "error",
                    Error = new RoutingCommandError { Code = "invalidJson", Message = ex.Message }
                });
                return;
            }

            this.LogDebug("Routing command from {clientId}: {@command}", clientId, request);

            var (statusCode, response) = RoutingCommandExecutor.Execute(request);
            ReplyToCommand(clientId, requestId, statusCode, response);
        }

        private void ReplyToCommand(string clientId, string requestId, int statusCode, RoutingCommandResponse response)
        {
            // PostEventMessage needs a device, and this messenger has none, so the event goes out as a
            // status message with an explicit type.
            PostStatusMessage(JToken.FromObject(new RoutingCommandResultMessage
            {
                RequestId = requestId,
                StatusCode = statusCode,
                Response = response
            }), CommandResultType, clientId);
        }

        private static RoutingSnapshot BuildRoutes() =>
            RoutingGraphHelpers.BuildRoutingSnapshot(RoutingGraphHelpers.BuildTileChildMap(), includeUnrouted: true);

        private void EnsureSubscribed()
        {
            lock (_subscriptionLock)
            {
                if (_subscribed)
                    return;

                _subscribed = true;

                foreach (var device in DeviceManager.AllDevices.OfType<IRoutingMidpointWithFeedback>())
                    device.RouteChanged += (midpoint, route) => QueueRoutesUpdate();

                foreach (var device in DeviceManager.AllDevices.OfType<IRoutingSinkWithFeedback>())
                {
                    device.InputChanged += (sink, port) => QueueRoutesUpdate();
                    // A cleared route changes the sink's current source without switching its input.
                    device.CurrentSourcesChanged += (sender, args) => QueueRoutesUpdate();
                }

                foreach (var device in DeviceManager.AllDevices.OfType<IRoutingSinkWithLayoutState>())
                    device.LayoutChanged += (sender, args) => QueueRoutesUpdate();
            }
        }

        private void QueueRoutesUpdate()
        {
            // One route usually raises several events (each midpoint, then the sink), so send once
            // after they settle.
            lock (_debounceLock)
            {
                if (_debounceTimer == null)
                {
                    _debounceTimer = new Timer(DebounceMs) { AutoReset = false };
                    _debounceTimer.Elapsed += (s, e) => SendRoutesUpdate();
                }

                _debounceTimer.Stop();
                _debounceTimer.Start();
            }
        }

        private void SendRoutesUpdate()
        {
            try
            {
                PostStatusMessage(JToken.FromObject(BuildRoutes()));
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error sending routing graph update: {message}", ex.Message);
            }
        }
    }

    /// <summary>
    /// The routing graph and current routes, sent in reply to <c>/fullStatus</c>.
    /// </summary>
    public class RoutingGraphStateMessage : RoutingSnapshot
    {
        /// <summary>Every device with routing inputs or outputs, and its ports.</summary>
        [JsonProperty("devices")]
        public List<RoutingDeviceInfo> Devices { get; set; }

        /// <summary>Every tie line.</summary>
        [JsonProperty("tieLines")]
        public List<TieLineInfo> TieLines { get; set; }
    }

    /// <summary>
    /// The reply to one <c>/routingCommand</c>.
    /// </summary>
    public class RoutingCommandResultMessage
    {
        /// <summary>The <c>requestId</c> the client sent with the command.</summary>
        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        /// <summary>The status the web API would return: 200, 202, 400, 404, 409, 422 or 500.</summary>
        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        /// <summary>The same response body the web API returns.</summary>
        [JsonProperty("response")]
        public RoutingCommandResponse Response { get; set; }
    }
}
