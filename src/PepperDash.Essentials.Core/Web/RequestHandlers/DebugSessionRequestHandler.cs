using Crestron.SimplSharp;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;
using Serilog.Events;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core.Web.RequestHandlers;

/// <summary>
/// Represents a DebugSessionRequestHandler
/// </summary>
public class DebugSessionRequestHandler : WebApiBaseRequestHandler
{
    private CTimer _portForwardTimeoutTimer;
    private readonly object _timerLock = new object();

    /// <summary>
    /// Constructor
    /// </summary>
    public DebugSessionRequestHandler()
        : base(true)
    {
    }
    /// <summary>
    /// Gets details for a debug session
    /// </summary>
    /// <param name="context"></param>
    protected override void HandleGet(Crestron.SimplSharp.WebScripting.HttpCwsContext context)
    {
        var routeData = context.Request.RouteData;
        if (routeData == null)
        {
            context.Response.StatusCode = 400;
            context.Response.StatusDescription = "Bad Request";
            context.Response.End();

            return;
        }

        try
        {
            var port = 0;

            if (!Debug.WebsocketSink.IsRunning)
            {
                Debug.LogMessage(LogEventLevel.Information, "Starting WS Server");
                // Generate a random port within a specified range
                port = new Random().Next(65435, 65535);
                // Start the WS Server
                Debug.WebsocketSink.StartServerAndSetPort(port);
                Debug.SetWebSocketMinimumDebugLevel(Serilog.Events.LogEventLevel.Verbose);
            }
            else
            {
                port = Debug.WebsocketSink.Port;
            }

            // Returns null on processors with no control subnet, rather than "Invalid Value"
            var csIp = ProcessorEthernetInfo.GetCsLanIpAddress();

            if (csIp == null)
            {
                Debug.LogMessage(LogEventLevel.Debug, "This processor does not have a CS LAN adapter; skipping port forwarding");
            }
            else if (port > 0)
            {
                // Always ensure port forwarding is active — it may have been removed by timeout
                try
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
                        Debug.LogMessage(LogEventLevel.Information, "Port {0} forwarded to CS LAN for debug websocket", port);
                        StartPortForwardTimeout(port, csIp);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogMessage(LogEventLevel.Warning, "Error automatically forwarding debug websocket port to CS LAN: {0}", ex.Message);
                }
            }

            if (!Debug.WebsocketSink.IsRunning)
            {
                context.Response.StatusCode = 500;
                context.Response.StatusDescription = "Internal Server Error";
                context.Response.Write(JsonConvert.SerializeObject(new { error = "Failed to start WebSocket debug server. Check logs for details." }), false);
                context.Response.End();
                return;
            }

            // The client can be on either side of the processor, so return every network the websocket is
            // reachable on and let it choose with ?network=lan|cslan|current. Without an explicit choice
            // we default to the side the browser is already on.
            var requestHost = SessionUrlHelper.GetRequestHost(context);

            var networks = SessionUrlHelper.BuildEndpointOptions(
                requestHost,
                ProcessorEthernetInfo.GetLanIpAddress(),
                csIp,
                Debug.WebsocketSink.GetUrlForHost);

            if (networks.Count == 0)
            {
                Debug.LogMessage(LogEventLevel.Error, "Unable to determine a reachable address for the debug websocket");

                context.Response.StatusCode = 500;
                context.Response.StatusDescription = "Internal Server Error";
                context.Response.Write(JsonConvert.SerializeObject(new { error = "Unable to determine a reachable address for the debug websocket. Check logs for details." }), false);
                context.Response.End();
                return;
            }

            var selected = SessionUrlHelper.SelectEndpoint(
                networks, SessionUrlHelper.GetRequestedNetwork(context), requestHost);

            var fallback = networks.FirstOrDefault(n => n != selected);

            // port and path let a client that already knows the processor's address build the URL itself
            // — the debug app was served from the processor, so window.location.hostname is by definition
            // an address that reaches it.
            var data = new
            {
                url = selected.Url,
                fallbackUrl = fallback?.Url,
                selectedNetwork = selected.Id,
                networks,
                port = Debug.WebsocketSink.Port,
                path = Debug.WebsocketSink.ServicePath
            };

            Debug.LogMessage(LogEventLevel.Information, "Debug Session URL ({0}): {1}", selected.Id, selected.Url);
            Debug.LogMessage(LogEventLevel.Information, "Fallback Debug Session URL: {0}", fallback?.Url ?? "<none>");

            // Return the port number with the full url of the WS Server
            var res = JsonConvert.SerializeObject(data);

            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            context.Response.Write(res, false);
            context.Response.End();
        }
        catch (Exception e)
        {
            Debug.LogMessage(LogEventLevel.Error, "Error handling debug session request: {0}", e);

            try
            {
                context.Response.StatusCode = 500;
                context.Response.StatusDescription = "Internal Server Error";
                context.Response.End();
            }
            catch
            {
                // Response may already have been sent; nothing more to do.
            }
        }
    }

    /// <summary>
    /// Stops a debug session
    /// </summary>
    /// <param name="context"></param>
    protected override void HandlePost(HttpCwsContext context)
    {
        CancelPortForwardTimeout();

        var port = Debug.WebsocketSink.Port;

        Task.Run(() => Debug.WebsocketSink.StopServer());

        // Remove port forwarding if CS LAN exists — null when this processor has no control subnet
        var csIp = ProcessorEthernetInfo.GetCsLanIpAddress();

        if (csIp != null)
        {
            try
            {
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
            catch (Exception ex)
            {
                Debug.LogMessage(LogEventLevel.Warning, "Error removing port forwarding: {0}", ex.Message);
            }
        }

        context.Response.StatusCode = 200;
        context.Response.StatusDescription = "OK";
        context.Response.End();

        Debug.LogMessage(LogEventLevel.Information, "Websocket Debug Session Stopped");
    }

    private void StartPortForwardTimeout(int port, string csIp)
    {
        lock (_timerLock)
        {
            _portForwardTimeoutTimer?.Dispose();
            _portForwardTimeoutTimer = new CTimer(_ =>
            {
                if (Debug.WebsocketSink.HasActiveConnections)
                {
                    Debug.LogMessage(LogEventLevel.Debug, "Debug websocket has active connections; keeping port forward");
                    StartPortForwardTimeout(port, csIp);
                    return;
                }

                Debug.LogMessage(LogEventLevel.Information, "No debug websocket connection within timeout; removing port forward for port {0}", port);

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
            }, 120000);
        }
    }

    /// <summary>
    /// Cancels the port forward timeout timer if a session is being explicitly stopped.
    /// </summary>
    private void CancelPortForwardTimeout()
    {
        lock (_timerLock)
        {
            _portForwardTimeoutTimer?.Dispose();
            _portForwardTimeoutTimer = null;
        }
    }
}
