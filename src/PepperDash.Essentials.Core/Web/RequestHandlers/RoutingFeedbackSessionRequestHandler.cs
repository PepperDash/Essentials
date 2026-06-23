using System;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Web.RequestHandlers;

/// <summary>
/// Handles HTTP requests to start and retrieve the routing feedback WebSocket session URL.
/// GET returns the current URL (starting the server if necessary).
/// POST stops the server.
/// </summary>
public class RoutingFeedbackSessionRequestHandler : WebApiBaseRequestHandler
{
    private static readonly RoutingFeedbackWebsocket _instance = new RoutingFeedbackWebsocket();
    private CTimer _portForwardTimeoutTimer;
    private readonly object _timerLock = new object();

    /// <summary>
    /// Gets the singleton RoutingFeedbackWebsocket instance.
    /// </summary>
    public static RoutingFeedbackWebsocket Instance => _instance;

    /// <summary>
    /// Constructor
    /// </summary>
    public RoutingFeedbackSessionRequestHandler()
        : base(true)
    {
    }

    /// <summary>
    /// Handles GET requests: starts the routing feedback WebSocket if not running and returns the URL.
    /// </summary>
    protected override void HandleGet(HttpCwsContext context)
    {
        try
        {
            var ip = CrestronEthernetHelper.GetEthernetParameter(
                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0);

            var port = 0;
            string csIp = null;

            if (!_instance.IsRunning)
            {
                Debug.LogMessage(LogEventLevel.Information, "Starting Routing Feedback WS Server");
                port = new Random().Next(65335, 65434);
                _instance.StartServerAndSetPort(port);
            }

            // Attempt to get the CS LAN IP and forward the port
            try
            {
                var csAdapterId = CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(
                    EthernetAdapterType.EthernetCSAdapter);
                csIp = CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, csAdapterId);

                if (port > 0)
                {
                    var result = CrestronEthernetHelper.AddPortForwarding(
                        (ushort)port, (ushort)port, csIp,
                        CrestronEthernetHelper.ePortMapTransport.TCP);

                    if (result != CrestronEthernetHelper.PortForwardingUserPatRetCodes.NoErr)
                    {
                        Debug.LogMessage(LogEventLevel.Warning, "Error forwarding port {0} to CS LAN: {1}", port, result);
                    }
                    else
                    {
                        Debug.LogMessage(LogEventLevel.Information, "Port {0} forwarded to CS LAN for routing feedback websocket", port);
                        StartPortForwardTimeout(port, csIp);
                    }
                }
            }
            catch (ArgumentException)
            {
                Debug.LogMessage(LogEventLevel.Debug, "This processor does not have a CS LAN adapter; skipping port forwarding");
            }
            catch (Exception ex)
            {
                Debug.LogMessage(LogEventLevel.Warning, "Error automatically forwarding routing feedback websocket port to CS LAN: {0}", ex.Message);
            }

            if (!_instance.IsRunning)
            {
                context.Response.StatusCode = 500;
                context.Response.StatusDescription = "Internal Server Error";
                context.Response.ContentType = "application/json";
                context.Response.ContentEncoding = Encoding.UTF8;
                context.Response.Write(
                    JsonConvert.SerializeObject(new { error = "Failed to start routing feedback WebSocket server." }),
                    false);
                context.Response.End();
                return;
            }

            var url = _instance.Url;

            var data = new
            {
                url,
                fallbackUrl = csIp != null ? url.Replace(csIp, ip) : null
            };

            Debug.LogMessage(LogEventLevel.Information, "Routing Feedback Session URL: {0}", url);
            Debug.LogMessage(LogEventLevel.Information, "Fallback Routing Feedback Session URL: {0}", data.fallbackUrl);

            var json = JsonConvert.SerializeObject(data);

            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.Write(json, false);
            context.Response.End();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex, "Error handling routing feedback session GET: {message}", ex.Message);
            context.Response.StatusCode = 500;
            context.Response.StatusDescription = "Internal Server Error";
            context.Response.End();
        }
    }

    /// <summary>
    /// Handles POST requests: stops the routing feedback WebSocket server.
    /// </summary>
    protected override void HandlePost(HttpCwsContext context)
    {
        try
        {
            CancelPortForwardTimeout();

            var port = _instance.Port;
            _instance.StopServer();

            // Remove port forwarding if CS LAN exists
            try
            {
                var csAdapterId = CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(
                    EthernetAdapterType.EthernetCSAdapter);
                var csIp = CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, csAdapterId);

                var result = CrestronEthernetHelper.RemovePortForwarding(
                    (ushort)port, (ushort)port, csIp,
                    CrestronEthernetHelper.ePortMapTransport.TCP);

                if (result != CrestronEthernetHelper.PortForwardingUserPatRetCodes.NoErr)
                {
                    Debug.LogMessage(LogEventLevel.Warning, "Error removing port forwarding for port {0}: {1}", port, result);
                }
                else
                {
                    Debug.LogMessage(LogEventLevel.Information, "Port forwarding for port {0} removed", port);
                }
            }
            catch (ArgumentException)
            {
                // No CS LAN adapter
            }
            catch (Exception ex)
            {
                Debug.LogMessage(LogEventLevel.Warning, "Error removing port forwarding: {0}", ex.Message);
            }

            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            context.Response.End();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex, "Error handling routing feedback session POST: {message}", ex.Message);
            context.Response.StatusCode = 500;
            context.Response.StatusDescription = "Internal Server Error";
            context.Response.End();
        }
    }

    private void StartPortForwardTimeout(int port, string csIp)
    {
        lock (_timerLock)
        {
            _portForwardTimeoutTimer?.Dispose();
            _portForwardTimeoutTimer = new CTimer(_ =>
            {
                if (_instance.HasActiveConnections)
                {
                    Debug.LogMessage(LogEventLevel.Debug, "Routing feedback websocket has active connections; keeping port forward");
                    return;
                }

                Debug.LogMessage(LogEventLevel.Information, "No routing feedback websocket connection within 30 seconds; removing port forward for port {0}", port);

                try
                {
                    var result = CrestronEthernetHelper.RemovePortForwarding(
                        (ushort)port, (ushort)port, csIp,
                        CrestronEthernetHelper.ePortMapTransport.TCP);

                    if (result != CrestronEthernetHelper.PortForwardingUserPatRetCodes.NoErr)
                    {
                        Debug.LogMessage(LogEventLevel.Warning, "Error removing port forwarding on timeout: {0}", result);
                    }
                    else
                    {
                        Debug.LogMessage(LogEventLevel.Information, "Port forwarding for port {0} removed due to timeout", port);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogMessage(LogEventLevel.Warning, "Error removing port forwarding on timeout: {0}", ex.Message);
                }
            }, 30000);
        }
    }

    private void CancelPortForwardTimeout()
    {
        lock (_timerLock)
        {
            _portForwardTimeoutTimer?.Dispose();
            _portForwardTimeoutTimer = null;
        }
    }
}
