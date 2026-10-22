using System;
using System.Collections.Generic;
using System.Text;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Web.RequestHandlers;

/// <summary>
/// Reads the query and writes the response for the routes that let clients poll a message stream over
/// HTTP instead of a WebSocket.
/// </summary>
/// <remarks>
/// Query: <c>since</c> is the last message ID the client received; leave it out to start a session.
/// <c>wait</c> is how many milliseconds to hold the request when nothing is new (default 0).
/// Response: <c>{ "next": 42, "missed": false, "messages": ["...", "..."] }</c>. Each message is the
/// same text the WebSocket would have sent.
/// </remarks>
internal static class PolledMessagesResponse
{
    /// <summary>
    /// Returns the <c>since</c> cursor, or null when the client is starting a session.
    /// </summary>
    public static long? GetSince(HttpCwsContext context)
    {
        var value = context.Request.QueryString["since"];

        return long.TryParse(value, out var since) && since >= 0 ? since : (long?)null;
    }

    /// <summary>
    /// Returns how long the client asked the server to hold the request.
    /// </summary>
    public static TimeSpan GetWait(HttpCwsContext context)
    {
        var value = context.Request.QueryString["wait"];

        return int.TryParse(value, out var ms) && ms > 0 ? TimeSpan.FromMilliseconds(ms) : TimeSpan.Zero;
    }

    /// <summary>
    /// Writes a 200 response with the messages.
    /// </summary>
    public static void Write(HttpCwsContext context, long next, bool missed, IReadOnlyList<string> messages)
    {
        var body = JsonConvert.SerializeObject(new { next, missed, messages });

        context.Response.StatusCode = 200;
        context.Response.StatusDescription = "OK";
        context.Response.ContentType = "application/json";
        context.Response.ContentEncoding = Encoding.UTF8;
        context.Response.AppendHeader("Cache-Control", "no-store");
        context.Response.Write(body, false);
        context.Response.End();
    }

    /// <summary>
    /// Writes a 500 response. Never logs, since a log message would itself land in the debug stream.
    /// </summary>
    public static void WriteError(HttpCwsContext context, string error)
    {
        try
        {
            context.Response.StatusCode = 500;
            context.Response.StatusDescription = "Internal Server Error";
            context.Response.ContentType = "application/json";
            context.Response.Write(JsonConvert.SerializeObject(new { error }), false);
            context.Response.End();
        }
        catch
        {
            // Response may already have been sent; nothing more to do.
        }
    }
}
