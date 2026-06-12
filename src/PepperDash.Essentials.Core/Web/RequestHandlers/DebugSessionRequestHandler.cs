using Crestron.SimplSharp;
using Crestron.SimplSharp.WebScripting;
using Crestron.SimplSharpPro.EthernetCommunication;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
    /// <summary>
    /// Represents a DebugSessionRequestHandler
    /// </summary>
    public class DebugSessionRequestHandler : WebApiBaseRequestHandler
    {    
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
                var ip = CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0);

                var port = 0;
                string csIp = null;

                if (!Debug.WebsocketSink.IsRunning)
                {
                    Debug.LogMessage(LogEventLevel.Information, "Starting WS Server");
                    // Generate a random port within a specified range
                    port = new Random().Next(65435, 65535);
                    // Start the WS Server
                    Debug.WebsocketSink.StartServerAndSetPort(port);
                    Debug.SetWebSocketMinimumDebugLevel(Serilog.Events.LogEventLevel.Verbose);

                    // Attempt to forward the port to the CS LAN
                    try
                    {
                        var csAdapterId = CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(
                            EthernetAdapterType.EthernetCSAdapter);
                        csIp = CrestronEthernetHelper.GetEthernetParameter(
                            CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, csAdapterId);

                        var result = CrestronEthernetHelper.AddPortForwarding(
                            (ushort)port, (ushort)port, csIp,
                            CrestronEthernetHelper.ePortMapTransport.TCP);

                        if (result != CrestronEthernetHelper.PortForwardingUserPatRetCodes.NoErr)
                        {
                            Debug.LogMessage(LogEventLevel.Warning, "Error adding port forwarding for debug websocket: {0}", result);
                        }
                        else
                        {
                            Debug.LogMessage(LogEventLevel.Information, "Port {0} forwarded to CS LAN for debug websocket", port);
                        }
                    }
                    catch (ArgumentException)
                    {
                        Debug.LogMessage(LogEventLevel.Debug, "This processor does not have a CS LAN adapter; skipping port forwarding");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogMessage(LogEventLevel.Warning, "Error automatically forwarding debug websocket port to CS LAN: {0}", ex.Message);
                    }
                }

                var url = Debug.WebsocketSink.Url;

                object data = new
                {
                    url = Debug.WebsocketSink.Url,
                    csLanUrl = csIp != null ? url.Replace(ip, csIp) : null
                };

                Debug.LogMessage(LogEventLevel.Information, "Debug Session URL: {0}", url);

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
                Debug.LogMessage(LogEventLevel.Information, "Error: {0}", e);
            }
        }

        /// <summary>
        /// Stops a debug session
        /// </summary>
        /// <param name="context"></param>
        protected override void HandlePost(HttpCwsContext context)
        {
            var port = Debug.WebsocketSink.Port;

            Debug.WebsocketSink.StopServer();

            // Remove the port forwarding entry
            try
            {
                var csAdapterId = CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(
                    EthernetAdapterType.EthernetCSAdapter);
                var csIp = CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, csAdapterId);

                if (port <= 0)
                {
                    Debug.LogMessage(LogEventLevel.Debug, "Debug websocket port is not set; skipping port forwarding removal");
                }
                else
                {
                    var result = CrestronEthernetHelper.RemovePortForwarding(
                        (ushort)port, (ushort)port, csIp,
                        CrestronEthernetHelper.ePortMapTransport.TCP);

                    if (result != CrestronEthernetHelper.PortForwardingUserPatRetCodes.NoErr)
                    {
                        Debug.LogMessage(LogEventLevel.Warning, "Error removing port forwarding for debug websocket: {0}", result);
                    }
                    else
                    {
                        Debug.LogMessage(LogEventLevel.Information, "Port forwarding for port {0} removed", port);
                    }
                }
            }
            catch (ArgumentException)
            {
                Debug.LogMessage(LogEventLevel.Debug, "This processor does not have a CS LAN adapter; skipping port forwarding removal");
            }
            catch (Exception ex)
            {
                Debug.LogMessage(LogEventLevel.Warning, "Error removing debug websocket port forwarding: {0}", ex.Message);
            }

            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            context.Response.End();

            Debug.LogMessage(LogEventLevel.Information, "Websocket Debug Session Stopped");
        }

    }
}
