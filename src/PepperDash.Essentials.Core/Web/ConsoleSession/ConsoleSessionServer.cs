using System;
using System.Security.Authentication;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.EthernetCommunication;
using PepperDash.Core;
using Serilog.Events;
using WebSocketSharp.Server;

namespace PepperDash.Essentials.Core.Web.ConsoleSession
{
    /// <summary>
    /// Secure WebSocket server that bridges browser sessions to the processor console over a
    /// loopback SSH connection. Each WebSocket client gets its own SSH session.
    /// Separate from <see cref="DebugWebsocketSink"/>, which broadcasts log events to every service on its server.
    /// </summary>
    public static class ConsoleSessionServer
    {
        private const string ServicePath = "/console/";

        // Same range DebugSessionRequestHandler uses. Ports below it were refused from the LAN on a CP4N,
        // while the debug server's port in this range was reachable.
        private const int MinPort = 65435;
        private const int MaxPort = 65535;
        private const int MaxStartAttempts = 5;

        private static readonly object _lock = new object();
        private static HttpServer _server;

        // CS-LAN port forward for the server's port, held for as long as the server runs. On a processor with a
        // control subnet, LAN clients were refused on program ports unless forwarded (DebugSessionRequestHandler
        // forwards the same way, but drops the forward after 30 s, which can expire before a certificate is accepted).
        private static int _forwardedPort;
        private static string _forwardedCsIp;

        static ConsoleSessionServer()
        {
            CrestronEnvironment.ProgramStatusEventHandler += type =>
            {
                if (type == eProgramStatusEventType.Stopping)
                {
                    Stop();
                }
            };
        }

        /// <summary>
        /// True when the server is listening
        /// </summary>
        public static bool IsRunning
        {
            get { lock (_lock) { return _server?.IsListening ?? false; } }
        }

        /// <summary>
        /// WSS URL for a browser that reached the web API at <paramref name="host"/>, or empty when not running
        /// </summary>
        /// <param name="host">
        /// Hostname or IP the browser used for the HTTP request. Reusing it keeps the WebSocket on an address
        /// the browser can already reach (LAN vs CS-LAN). Falls back to the primary adapter's IP when empty.
        /// </param>
        public static string GetUrl(string host)
        {
            lock (_lock)
            {
                if (_server == null) return "";

                if (string.IsNullOrEmpty(host))
                {
                    host = CrestronEthernetHelper.GetEthernetParameter(
                        CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0);
                }

                return $"wss://{host}:{_server.Port}{ServicePath}";
            }
        }

        /// <summary>
        /// Starts the server on a random port if it is not already running
        /// </summary>
        public static void Start()
        {
            lock (_lock)
            {
                if (_server != null && _server.IsListening) return;

                var random = new Random();
                Exception lastError = null;

                for (var attempt = 0; attempt < MaxStartAttempts; attempt++)
                {
                    var port = random.Next(MinPort, MaxPort);

                    // The debug log server shares this range; a collision fails Start, so skip its port up front
                    if (port == Debug.WebsocketSink.Port) continue;

                    try
                    {
                        _server = StartOnPort(port);
                        Debug.LogMessage(LogEventLevel.Information, "Console session server started on port {port}", port);
                        AddPortForward(port);
                        return;
                    }
                    catch (Exception ex)
                    {
                        lastError = ex;
                        Debug.LogMessage(LogEventLevel.Warning, "Console session server could not start on port {port}: {message}", port, ex.Message);
                    }
                }

                throw new InvalidOperationException("Console session server could not find a free port", lastError);
            }
        }

        private static HttpServer StartOnPort(int port)
        {
            var server = new HttpServer(port, true);

            server.SslConfiguration.ServerCertificate = DebugWebsocketSink.LoadServerCertificate();
            server.SslConfiguration.ClientCertificateRequired = false;
            server.SslConfiguration.CheckCertificateRevocation = false;
            server.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls12;

            // Anything this server logs lands on the console we are streaming, so keep it quiet
            server.Log.Level = WebSocketSharp.LogLevel.Error;
            server.Log.Output = (data, s) => Debug.LogMessage(LogEventLevel.Warning, "Console session server: {message}", data.Message);

            server.AddWebSocketService<ConsoleSessionClient>(ServicePath);
            server.Start();

            return server;
        }

        /// <summary>
        /// Stops the server, closing every WebSocket client and its SSH session
        /// </summary>
        public static void Stop()
        {
            lock (_lock)
            {
                if (_server == null) return;

                RemovePortForward();

                try
                {
                    _server.Stop();
                }
                catch (Exception ex)
                {
                    Debug.LogMessage(ex, "Exception stopping console session server");
                }

                _server = null;
            }

            Debug.LogMessage(LogEventLevel.Information, "Console session server stopped");
        }

        // Caller holds _lock
        private static void AddPortForward(int port)
        {
            try
            {
                var csAdapterId = CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(
                    EthernetAdapterType.EthernetCSAdapter);
                var csIp = CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, csAdapterId);

                var result = CrestronEthernetHelper.AddPortForwarding(
                    (ushort)port, (ushort)port, csIp,
                    CrestronEthernetHelper.ePortMapTransport.TCP);

                if (result != CrestronEthernetHelper.PortForwardingUserPatRetCodes.NoErr)
                {
                    Debug.LogMessage(LogEventLevel.Warning, "Error adding port forwarding for console session server: {0}", result);
                    return;
                }

                _forwardedPort = port;
                _forwardedCsIp = csIp;

                Debug.LogMessage(LogEventLevel.Information, "Port {0} forwarded to CS LAN for console session server", port);
            }
            catch (ArgumentException)
            {
                Debug.LogMessage(LogEventLevel.Debug, "This processor does not have a CS LAN adapter; skipping port forwarding");
            }
            catch (Exception ex)
            {
                Debug.LogMessage(LogEventLevel.Warning, "Error forwarding console session server port to CS LAN: {0}", ex.Message);
            }
        }

        // Caller holds _lock
        private static void RemovePortForward()
        {
            if (_forwardedPort <= 0) return;

            try
            {
                var result = CrestronEthernetHelper.RemovePortForwarding(
                    (ushort)_forwardedPort, (ushort)_forwardedPort, _forwardedCsIp,
                    CrestronEthernetHelper.ePortMapTransport.TCP);

                if (result != CrestronEthernetHelper.PortForwardingUserPatRetCodes.NoErr)
                {
                    Debug.LogMessage(LogEventLevel.Warning, "Error removing port forwarding for console session server: {0}", result);
                }
                else
                {
                    Debug.LogMessage(LogEventLevel.Information, "Port forwarding for port {0} removed", _forwardedPort);
                }
            }
            catch (Exception ex)
            {
                Debug.LogMessage(LogEventLevel.Warning, "Error removing console session server port forwarding: {0}", ex.Message);
            }

            _forwardedPort = 0;
            _forwardedCsIp = null;
        }
    }
}
