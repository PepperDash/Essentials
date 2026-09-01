using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Timers;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PepperDash.Core;
using Serilog.Events;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace PepperDash.Essentials.Core.Web;

/// <summary>
/// WebSocket service that broadcasts real-time routing state changes to connected clients.
/// Subscribes to route-changed events on midpoint and sink devices and pushes updates
/// to all connected WebSocket clients.
/// </summary>
public class RoutingFeedbackWebsocket : IKeyed
{
    private HttpServer _httpsServer;
    private readonly string _path = "/routing/join/";
    private const string _certificateName = "selfCres";
    private const string _certificatePassword = "cres12345";
    private const long DEBOUNCE_MS = 200;

    private static string CertPath =>
        $"{Path.DirectorySeparatorChar}user{Path.DirectorySeparatorChar}{_certificateName}.pfx";

    private readonly Dictionary<string, Timer> _debounceTimers = new Dictionary<string, Timer>();

    // Tile-sink children of an IRoutingSinkWithLayouts device (e.g. a multiview decoder's per-window
    // sinks) are reported to clients as synthesized inputs on the parent's node, rather than as their
    // own separate nodes - see RoutingGraphHelpers. Rebuilt whenever the server (re)starts.
    private Dictionary<string, RoutingGraphHelpers.TileChildInfo> _tileChildren = new Dictionary<string, RoutingGraphHelpers.TileChildInfo>();

    private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore
    };

    /// <inheritdoc/>
    public string Key => "RoutingFeedbackWebsocket";

    /// <summary>
    /// Gets the port number on which the server is currently running.
    /// </summary>
    public int Port => _httpsServer?.Port ?? 0;

    /// <summary>
    /// Gets the WebSocket URL for the current server instance.
    /// </summary>
    public string Url
    {
        get
        {
            var host = ProcessorEthernetInfo.GetLanIpAddress() ?? ProcessorEthernetInfo.GetCsLanIpAddress();

            return GetUrlForHost(host);
        }
    }

    /// <summary>
    /// Gets the WebSocket path clients connect to, e.g. <c>/routing/join</c>. Exposed so a client that
    /// already knows the processor's address — a browser on a page the processor served, for instance —
    /// can build the URL itself from its own location.
    /// </summary>
    public string ServicePath
    {
        get
        {
            var service = _httpsServer?.WebSocketServices[_path];

            return service?.Path ?? _path.TrimEnd('/');
        }
    }

    /// <summary>
    /// Builds the WebSocket URL for this server using the supplied host, which lets callers hand back
    /// the address the client actually used to reach the processor.
    /// </summary>
    /// <param name="host">Host name or IP address, without scheme or port. IPv6 literals must already be bracketed.</param>
    /// <returns>The <c>wss://</c> URL, or an empty string when the server is not listening or <paramref name="host"/> is unusable.</returns>
    public string GetUrlForHost(string host)
    {
        if (_httpsServer == null || !_httpsServer.IsListening) return "";

        var service = _httpsServer.WebSocketServices[_path];
        if (service == null) return "";

        host = ProcessorEthernetInfo.NullIfInvalid(host);
        if (host == null) return "";

        return $"wss://{host}:{_httpsServer.Port}{service.Path}";
    }

    /// <summary>
    /// Gets a value indicating whether the server is currently listening.
    /// </summary>
    public bool IsRunning => _httpsServer?.IsListening ?? false;

    /// <summary>
    /// Gets a value indicating whether there are active WebSocket connections.
    /// </summary>
    public bool HasActiveConnections
    {
        get
        {
            if (_httpsServer == null || !_httpsServer.IsListening) return false;
            var service = _httpsServer.WebSocketServices[_path];
            if (service == null) return false;
            return service.Sessions.Count > 0;
        }
    }

    /// <summary>
    /// Starts the WebSocket server on the specified port and subscribes to routing events.
    /// </summary>
    /// <param name="port">The port to listen on.</param>
    public void StartServerAndSetPort(int port)
    {
        if (IsRunning)
        {
            Debug.LogMessage(LogEventLevel.Information, "Routing feedback WebSocket already running on port {port}", this, Port);
            return;
        }

        Debug.LogMessage(LogEventLevel.Information, "Starting Routing Feedback WebSocket on port: {port}", this, port);

        try
        {
            _httpsServer = new HttpServer(port, true);

            var cert = LoadCert(CertPath, _certificatePassword);
            _httpsServer.SslConfiguration.ServerCertificate = cert;
            _httpsServer.SslConfiguration.ClientCertificateRequired = false;
            _httpsServer.SslConfiguration.CheckCertificateRevocation = false;
            _httpsServer.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls12;
            _httpsServer.SslConfiguration.ClientCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            _httpsServer.AddWebSocketService<RoutingFeedbackClient>(_path);
            _httpsServer.OnGet += HandleHttpGet;
            _httpsServer.Log.Level = LogLevel.Warn;
            _httpsServer.Start();

            RoutingFeedbackClient.Owner = this;
            SubscribeToRoutingEvents();

            Debug.LogMessage(LogEventLevel.Information, "Routing Feedback WebSocket ready at {url}", this, Url);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex, "Routing Feedback WebSocket failed to start: {message}", this, ex.Message);
            _httpsServer = null;
        }
    }

    /// <summary>
    /// Stops the WebSocket server and unsubscribes from routing events.
    /// </summary>
    public void StopServer()
    {
        UnsubscribeFromRoutingEvents();

        try
        {
            if (_httpsServer == null || !_httpsServer.IsListening)
                return;

            _httpsServer.Log.Output = (d, s) => { };
            _httpsServer.Stop();
            _httpsServer = null;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex, "Routing Feedback WebSocket failed to stop: {message}", this, ex.Message);
        }
    }

    /// <summary>
    /// Builds and returns the full current routing state snapshot to send to newly connected clients.
    /// </summary>
    internal string GetSnapshotMessage()
    {
        var midpointRoutes = new Dictionary<string, List<MidpointRouteDto>>();
        var sinkRoutes = new Dictionary<string, List<SinkRouteDto>>();

        // Collect midpoint current routes
        var midpointDevices = DeviceManager.AllDevices.OfType<IRoutingMidpointWithFeedback>();
        foreach (var device in midpointDevices)
        {
            if (device.CurrentRoutes == null || device.CurrentRoutes.Count == 0)
                continue;

            midpointRoutes[device.Key] = device.CurrentRoutes
                .Where(r => r.InputPort != null)
                .Select(r => new MidpointRouteDto
                {
                    InputPortKey = r.InputPort.Key,
                    OutputPortKey = r.OutputPort?.Key,
                    SignalType = r.InputPort.Type.ToString()
                })
                .ToList();
        }

        // Collect sink current sources directly from each sink's own current-source bookkeeping
        // (ICurrentSources, part of IRoutingSinkWithFeedback), which is authoritative regardless of
        // whether the route was made via a tie line (ReleaseAndMakeRoute) or a device-specific bulk
        // API (e.g. IHasDynamicMultiviewLayout.ApplyDynamicLayout) that never touches
        // TieLineCollection/RouteDescriptorCollection at all.
        var sinkDevices = DeviceManager.AllDevices.OfType<IRoutingSinkWithFeedback>();
        foreach (var device in sinkDevices)
        {
            if (device.CurrentInputPort == null)
                continue;

            var sourceKey = RoutingGraphHelpers.GetCurrentSourceKey(device);
            if (string.IsNullOrEmpty(sourceKey))
                continue;

            var deviceKey = device.Key;
            var inputPortKey = device.CurrentInputPort.Key;

            if (_tileChildren.TryGetValue(deviceKey, out var tileInfo))
            {
                deviceKey = tileInfo.Parent.Key;
                inputPortKey = RoutingGraphHelpers.QualifyTilePortKey(tileInfo.TileNumber, inputPortKey);
            }

            if (!sinkRoutes.TryGetValue(deviceKey, out var routes))
            {
                routes = new List<SinkRouteDto>();
                sinkRoutes[deviceKey] = routes;
            }

            routes.Add(new SinkRouteDto
            {
                InputPortKey = inputPortKey,
                SourceDeviceKey = sourceKey,
                SignalType = device.CurrentInputPort.Type.ToString()
            });
        }

        var snapshot = new RoutingSnapshotDto
        {
            Type = "snapshot",
            MidpointRoutes = midpointRoutes,
            SinkRoutes = sinkRoutes,
            Layouts = RoutingGraphHelpers.BuildMultiviewLayoutSnapshot()
        };

        return JsonConvert.SerializeObject(snapshot, JsonSettings);
    }

    private void SubscribeToRoutingEvents()
    {
        _tileChildren = RoutingGraphHelpers.BuildTileChildMap();

        var midpointDevices = DeviceManager.AllDevices.OfType<IRoutingMidpointWithFeedback>();
        foreach (var device in midpointDevices)
        {
            device.RouteChanged += HandleMidpointRouteChanged;
        }

        var sinkDevices = DeviceManager.AllDevices.OfType<IRoutingSinkWithFeedback>();
        foreach (var device in sinkDevices)
        {
            device.InputChanged += HandleSinkInputChanged;
        }

        var layoutDevices = DeviceManager.AllDevices.OfType<IRoutingSinkWithLayoutState>();
        foreach (var device in layoutDevices)
        {
            device.LayoutChanged += HandleLayoutChanged;
        }
    }

    private void UnsubscribeFromRoutingEvents()
    {
        var midpointDevices = DeviceManager.AllDevices.OfType<IRoutingMidpointWithFeedback>();
        foreach (var device in midpointDevices)
        {
            device.RouteChanged -= HandleMidpointRouteChanged;
        }

        var sinkDevices = DeviceManager.AllDevices.OfType<IRoutingSinkWithFeedback>();
        foreach (var device in sinkDevices)
        {
            device.InputChanged -= HandleSinkInputChanged;
        }

        var layoutDevices = DeviceManager.AllDevices.OfType<IRoutingSinkWithLayoutState>();
        foreach (var device in layoutDevices)
        {
            device.LayoutChanged -= HandleLayoutChanged;
        }
    }

    private void HandleMidpointRouteChanged(IRoutingMidpointWithFeedback midpoint, RouteSwitchDescriptor newRoute)
    {
        DebounceBroadcast($"midpoint-{midpoint.Key}", () =>
        {
            var routes = midpoint.CurrentRoutes?
                .Where(r => r.InputPort != null)
                .Select(r => new MidpointRouteDto
                {
                    InputPortKey = r.InputPort.Key,
                    OutputPortKey = r.OutputPort?.Key,
                    SignalType = r.InputPort.Type.ToString()
                })
                .ToList() ?? new List<MidpointRouteDto>();

            var msg = new MidpointRouteChangedDto
            {
                Type = "midpointRouteChanged",
                DeviceKey = midpoint.Key,
                Routes = routes
            };

            Broadcast(JsonConvert.SerializeObject(msg, JsonSettings));
        });
    }

    private void HandleLayoutChanged(object sender, MultiviewLayoutStateEventArgs e)
    {
        if (sender is not IKeyed device)
            return;

        DebounceBroadcast($"layout-{device.Key}", () =>
        {
            var msg = new LayoutChangedDto
            {
                Type = "layoutChanged",
                DeviceKey = device.Key,
                Layout = e.CurrentLayout
            };

            Broadcast(JsonConvert.SerializeObject(msg, JsonSettings));
        });
    }

    private void HandleSinkInputChanged(IRoutingSinkWithFeedback sender, RoutingInputPort currentInputPort)
    {
        // Tile-sink children are reported under their IRoutingSinkWithLayouts parent's key, with a
        // qualified port key, so clients see this as an input change on the parent's node rather than
        // on a device that isn't otherwise represented in the graph.
        var deviceKey = sender.Key;
        var inputPortKey = currentInputPort?.Key ?? "";

        if (_tileChildren.TryGetValue(deviceKey, out var tileInfo))
        {
            deviceKey = tileInfo.Parent.Key;
            if (!string.IsNullOrEmpty(inputPortKey))
                inputPortKey = RoutingGraphHelpers.QualifyTilePortKey(tileInfo.TileNumber, inputPortKey);
        }

        DebounceBroadcast($"sink-{sender.Key}", () =>
        {
            // Read the source directly from the sink's own current-source bookkeeping (see
            // GetCurrentSourceKey) rather than tracing a tie line - a route made via a
            // device-specific bulk API (e.g. ApplyDynamicLayout) never creates a tie line at all.
            var sourceDeviceKey = currentInputPort != null ? (RoutingGraphHelpers.GetCurrentSourceKey(sender) ?? "") : "";

            var msg = new SinkInputChangedDto
            {
                Type = "sinkInputChanged",
                DeviceKey = deviceKey,
                InputPortKey = inputPortKey,
                SourceDeviceKey = sourceDeviceKey,
                SignalType = currentInputPort?.Type.ToString() ?? ""
            };

            Broadcast(JsonConvert.SerializeObject(msg, JsonSettings));
        });
    }

    private void DebounceBroadcast(string key, Action action)
    {
        lock (_debounceTimers)
        {
            if (_debounceTimers.TryGetValue(key, out var existingTimer))
            {
                existingTimer.Stop();
                existingTimer.Dispose();
            }

            var timer = new Timer(DEBOUNCE_MS) { AutoReset = false };
            timer.Elapsed += (s, e) =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex, "Error in debounced routing broadcast for {key}: {message}", this, key, ex.Message);
                }
                finally
                {
                    lock (_debounceTimers)
                    {
                        if (_debounceTimers.ContainsKey(key))
                        {
                            _debounceTimers[key]?.Dispose();
                            _debounceTimers.Remove(key);
                        }
                    }
                }
            };
            timer.Start();
            _debounceTimers[key] = timer;
        }
    }

    private void Broadcast(string message)
    {
        if (_httpsServer == null || !_httpsServer.IsListening) return;

        var service = _httpsServer.WebSocketServices[_path];
        if (service == null) return;

        service.Sessions.Broadcast(message);
    }

    private void HandleHttpGet(object sender, HttpRequestEventArgs e)
    {
        var res = e.Response;
        var body = System.Text.Encoding.UTF8.GetBytes(
            "<html><body><h2>Certificate accepted.</h2><p>You can close this tab and return to the application.</p></body></html>");
        res.ContentType = "text/html";
        res.ContentLength64 = body.Length;
        res.Close(body, true);
    }

    private static X509Certificate2 LoadCert(string certPath, string certPassword)
    {
        return new X509Certificate2(certPath, certPassword, X509KeyStorageFlags.EphemeralKeySet);
    }

    // ── DTOs ─────────────────────────────────────────────────────────────────

    private class RoutingSnapshotDto
    {
        public string Type { get; set; }
        public Dictionary<string, List<MidpointRouteDto>> MidpointRoutes { get; set; }
        public Dictionary<string, List<SinkRouteDto>> SinkRoutes { get; set; }
        public Dictionary<string, MultiviewLayoutState> Layouts { get; set; }
    }

    private class LayoutChangedDto
    {
        public string Type { get; set; }
        public string DeviceKey { get; set; }
        public MultiviewLayoutState Layout { get; set; }
    }

    private class MidpointRouteChangedDto
    {
        public string Type { get; set; }
        public string DeviceKey { get; set; }
        public List<MidpointRouteDto> Routes { get; set; }
    }

    private class SinkInputChangedDto
    {
        public string Type { get; set; }
        public string DeviceKey { get; set; }
        public string InputPortKey { get; set; }
        public string SourceDeviceKey { get; set; }
        public string SignalType { get; set; }
    }

    private class MidpointRouteDto
    {
        public string InputPortKey { get; set; }
        public string OutputPortKey { get; set; }
        public string SignalType { get; set; }
    }

    private class SinkRouteDto
    {
        public string InputPortKey { get; set; }
        public string SourceDeviceKey { get; set; }
        public string SignalType { get; set; }
    }
}

/// <summary>
/// WebSocket client behavior for routing feedback connections.
/// Sends a full state snapshot on connect.
/// </summary>
public class RoutingFeedbackClient : WebSocketBehavior
{
    /// <summary>
    /// Static reference to the owning <see cref="RoutingFeedbackWebsocket"/> instance.
    /// Set before the server starts accepting connections.
    /// </summary>
    internal static RoutingFeedbackWebsocket Owner { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RoutingFeedbackClient"/> class.
    /// </summary>
    public RoutingFeedbackClient()
    {
    }

    /// <inheritdoc/>
    protected override void OnOpen()
    {
        base.OnOpen();
        Debug.LogMessage(LogEventLevel.Information, "Routing feedback client connected from: {url}", Owner, Context.WebSocket.Url);

        // Send full state snapshot to the newly connected client
        try
        {
            var snapshot = Owner.GetSnapshotMessage();
            Send(snapshot);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex, "Error sending routing snapshot to client: {message}", Owner, ex.Message);
        }
    }

    /// <inheritdoc/>
    protected override void OnClose(CloseEventArgs e)
    {
        base.OnClose(e);
        Debug.LogMessage(LogEventLevel.Debug, "Routing feedback client disconnected: {code} {reason}", Owner, e.Code, e.Reason);
    }

    /// <inheritdoc/>
    protected override void OnError(WebSocketSharp.ErrorEventArgs e)
    {
        base.OnError(e);
        Debug.LogError(e.Exception, "Routing feedback client error: {message}", Owner, e.Message);
    }
}
