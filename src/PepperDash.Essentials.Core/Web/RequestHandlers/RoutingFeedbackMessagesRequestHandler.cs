using System;
using Crestron.SimplSharp.WebScripting;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Web.RequestHandlers;

/// <summary>
/// Delivers routing feedback over plain HTTP, for clients that can't reach the routing feedback
/// WebSocket port, such as a browser on the far side of a Virtual Control server's firewall.
/// </summary>
/// <remarks>
/// <c>GET routingFeedbackSession/messages</c> starts a session and returns the routing snapshot as its
/// only message, plus the cursor to poll from.
/// <c>GET routingFeedbackSession/messages?since=&lt;id&gt;&amp;wait=&lt;ms&gt;</c> returns the changes
/// after <c>since</c>. When the response says <c>missed</c>, the client should start again without
/// <c>since</c> to get a fresh snapshot. <c>POST routingFeedbackSession</c> ends the session, as it does
/// for the WebSocket. See <see cref="PolledMessagesResponse"/> for the format.
/// </remarks>
public class RoutingFeedbackMessagesRequestHandler : WebApiBaseRequestHandler
{
    /// <summary>
    /// Constructor
    /// </summary>
    public RoutingFeedbackMessagesRequestHandler()
        : base(true)
    {
    }

    /// <summary>
    /// Starts a polling session, or returns the routing changes after the client's cursor.
    /// </summary>
    /// <param name="context"></param>
    protected override void HandleGet(HttpCwsContext context)
    {
        var feedback = RoutingFeedbackSessionRequestHandler.Instance;

        try
        {
            var since = PolledMessagesResponse.GetSince(context);

            if (since == null)
            {
                var snapshot = feedback.StartPolling(out var cursor);

                Debug.LogMessage(LogEventLevel.Debug, "Routing feedback session started for an HTTP polling client", feedback);

                PolledMessagesResponse.Write(context, cursor, false, new[] { snapshot });
                return;
            }

            var result = feedback.PollBuffer.Poll(since.Value, PolledMessagesResponse.GetWait(context));

            PolledMessagesResponse.Write(context, result.Next, result.Missed, result.Messages);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex, "Error handling routing feedback poll: {message}", feedback, ex.Message);
            PolledMessagesResponse.WriteError(context, ex.Message);
        }
    }
}
