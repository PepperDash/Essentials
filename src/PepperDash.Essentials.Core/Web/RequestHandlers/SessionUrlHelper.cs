using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp.WebScripting;
using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Web.RequestHandlers;

/// <summary>
/// Shared helpers for the session handlers that hand a websocket URL back to a browser.
/// </summary>
internal static class SessionUrlHelper
{
    /// <summary>Query string key a client uses to choose which network to connect over.</summary>
    public const string NetworkQueryStringKey = "network";

    /// <summary>Network id for the processor's LAN adapter.</summary>
    public const string LanNetworkId = "lan";

    /// <summary>Network id for the processor's control subnet adapter.</summary>
    public const string ControlSubnetNetworkId = "cslan";

    /// <summary>Network id for the address the client reached CWS on, when it is neither adapter's IP.</summary>
    public const string CurrentNetworkId = "current";

    /// <summary>
    /// Gets the host the client used to reach CWS, without scheme or port. Returns null when it cannot
    /// be determined or is a loopback address, neither of which is useful to hand back to the browser.
    /// </summary>
    public static string GetRequestHost(HttpCwsContext context)
    {
        try
        {
            var host = context.Request.Headers["Host"];

            if (string.IsNullOrWhiteSpace(host))
                host = context.Request.Url?.Host;

            if (string.IsNullOrWhiteSpace(host)) return null;

            host = host.Trim();

            // Strip the port. IPv6 literals are bracketed, so only look for a colon after the bracket.
            var closingBracket = host.LastIndexOf(']');
            if (closingBracket >= 0)
            {
                host = host.Substring(0, closingBracket + 1);
            }
            else
            {
                var colon = host.IndexOf(':');
                if (colon >= 0) host = host.Substring(0, colon);
            }

            if (host.Length == 0) return null;

            if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                || host.Equals("127.0.0.1", StringComparison.Ordinal)
                || host.Equals("[::1]", StringComparison.Ordinal))
            {
                return null;
            }

            return host;
        }
        catch (Exception ex)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Unable to read request host: {0}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Gets the network the client explicitly asked for via the <c>network</c> query string, or null
    /// when it did not ask for one.
    /// </summary>
    public static string GetRequestedNetwork(HttpCwsContext context)
    {
        try
        {
            var requested = context.Request.QueryString?[NetworkQueryStringKey];

            return string.IsNullOrWhiteSpace(requested) ? null : requested.Trim();
        }
        catch (Exception ex)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Unable to read requested network: {0}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Builds the list of networks the websocket can be reached on, in the order a picker should show
    /// them. Addresses that are unreadable, duplicated or that produce no URL are dropped.
    /// </summary>
    /// <param name="requestHost">The host the client used to reach CWS, from <see cref="GetRequestHost"/>.</param>
    /// <param name="lanIp">The processor's LAN address, or null.</param>
    /// <param name="csIp">The processor's control subnet address, or null when it has no control subnet.</param>
    /// <param name="urlForHost">Builds the websocket URL for a host; returns an empty string when unusable.</param>
    public static List<WebsocketEndpointOption> BuildEndpointOptions(
        string requestHost, string lanIp, string csIp, Func<string, string> urlForHost)
    {
        var options = new List<WebsocketEndpointOption>();

        void Add(string id, string label, string host)
        {
            if (string.IsNullOrWhiteSpace(host)) return;

            var trimmed = host.Trim();

            if (options.Any(o => o.Host.Equals(trimmed, StringComparison.OrdinalIgnoreCase))) return;

            var url = urlForHost(trimmed);

            if (string.IsNullOrEmpty(url)) return;

            options.Add(new WebsocketEndpointOption { Id = id, Label = label, Host = trimmed, Url = url });
        }

        Add(LanNetworkId, "LAN", lanIp);
        Add(ControlSubnetNetworkId, "Control Subnet", csIp);

        // The client reached CWS on an address that is neither adapter's IP — a host name, or through a
        // NAT. Offer it too, since it is the one address known to be routable from that browser.
        Add(CurrentNetworkId, "Current connection", requestHost);

        return options;
    }

    /// <summary>
    /// Picks the endpoint to hand back: the network the client explicitly asked for, otherwise the side
    /// the client is already on, otherwise the first option.
    /// </summary>
    /// <param name="options">Options from <see cref="BuildEndpointOptions"/>. Must not be empty.</param>
    /// <param name="requestedNetwork">The network id the client asked for, or null.</param>
    /// <param name="requestHost">The host the client used to reach CWS, or null.</param>
    public static WebsocketEndpointOption SelectEndpoint(
        List<WebsocketEndpointOption> options, string requestedNetwork, string requestHost)
    {
        if (options == null || options.Count == 0) return null;

        if (requestedNetwork != null)
        {
            var requested = options.FirstOrDefault(o => o.Id.Equals(requestedNetwork, StringComparison.OrdinalIgnoreCase));

            if (requested != null) return requested;

            Debug.LogMessage(LogEventLevel.Warning,
                "Network '{0}' was requested for the websocket session but is not available on this processor; selecting automatically",
                requestedNetwork);
        }

        // Default to the side the client is already on.
        if (!string.IsNullOrWhiteSpace(requestHost))
        {
            var match = options.FirstOrDefault(o => o.Host.Equals(requestHost, StringComparison.OrdinalIgnoreCase))
                        ?? options.FirstOrDefault(o => o.Id == CurrentNetworkId);

            if (match != null) return match;
        }

        return options[0];
    }
}
