using System;
using Crestron.SimplSharp.WebScripting;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Web.RequestHandlers;

/// <summary>
/// Delivers debug console messages over plain HTTP, for clients that can't reach the debug WebSocket
/// port, such as a browser on the far side of a Virtual Control server's firewall.
/// </summary>
/// <remarks>
/// <c>GET debugSession/messages</c> starts a session and returns the cursor to poll from.
/// <c>GET debugSession/messages?since=&lt;id&gt;&amp;wait=&lt;ms&gt;</c> returns the log messages after
/// <c>since</c>, holding the request up to <c>wait</c> ms until one arrives. <c>POST debugSession</c> ends
/// the session, as it does for the WebSocket. See <see cref="PolledMessagesResponse"/> for the format.
/// </remarks>
public class DebugSessionMessagesRequestHandler : WebApiBaseRequestHandler
{
    /// <summary>
    /// Constructor
    /// </summary>
    public DebugSessionMessagesRequestHandler()
        : base(true)
    {
    }

    /// <summary>
    /// Starts a polling session, or returns the messages after the client's cursor.
    /// </summary>
    /// <param name="context"></param>
    protected override void HandleGet(HttpCwsContext context)
    {
        // Don't log per request here: each log message would become a message for this same stream
        try
        {
            var buffer = Debug.WebsocketSink?.PollBuffer;

            if (buffer == null)
            {
                PolledMessagesResponse.WriteError(context, "Debug messages are not available on this system.");
                return;
            }

            var since = PolledMessagesResponse.GetSince(context);

            if (since == null)
            {
                var starting = !buffer.IsActive;
                var cursor = buffer.Activate();

                if (starting)
                {
                    // As starting a WebSocket session does, so the console can filter every level itself
                    Debug.SetWebSocketMinimumDebugLevel(LogEventLevel.Verbose);
                    Debug.LogMessage(LogEventLevel.Information, "Debug session started for an HTTP polling client");
                }

                PolledMessagesResponse.Write(context, cursor, false, Array.Empty<string>());
                return;
            }

            var result = buffer.Poll(since.Value, PolledMessagesResponse.GetWait(context));

            PolledMessagesResponse.Write(context, result.Next, result.Missed, result.Messages);
        }
        catch (Exception ex)
        {
            PolledMessagesResponse.WriteError(context, ex.Message);
        }
    }
}
