using System;
using PepperDash.Core;
using Serilog.Events;
using System.Collections.Generic;
using System.Linq;


namespace PepperDash.Essentials.Core;

/// <summary>
/// A collection of RouteDescriptors - typically the static DefaultCollection is used
/// </summary>
public class RouteDescriptorCollection
{
    /// <summary>
    /// The static default collection of RouteDescriptors.  This is typically used for global routing management across the system, but additional collections could be used for specific purposes if desired.
    /// </summary>
    public static RouteDescriptorCollection DefaultCollection
    {
        get
        {
            if (_DefaultCollection == null)
                _DefaultCollection = new RouteDescriptorCollection();
            return _DefaultCollection;
        }
    }
    private static RouteDescriptorCollection _DefaultCollection;

    private readonly List<RouteDescriptor> RouteDescriptors = new List<RouteDescriptor>();

    /// <summary>
    /// Event raised when the collection of RouteDescriptors changes (add/remove).  This is useful for updating routing status in the UI, for example.
    /// </summary>
    public event EventHandler RouteDescriptorCollectionChanged;

    /// <summary>
    /// Gets an enumerable collection of all RouteDescriptors in this collection.
    /// </summary>
    public IEnumerable<RouteDescriptor> Descriptors => RouteDescriptors.AsReadOnly();


    /// <summary>
    /// Adds a RouteDescriptor to the list.  If an existing RouteDescriptor for the
    /// destination exists already, it will not be added - in order to preserve
    /// proper route releasing.
    /// </summary>
    /// <param name="descriptor"></param>
    public void AddRouteDescriptor(RouteDescriptor descriptor)
    {
        if (descriptor == null)
        {
            return;
        }

        // Check if a route already exists with the same source, destination, input port, AND signal type
        var existingRoute = RouteDescriptors.FirstOrDefault(t =>
            t.Source == descriptor.Source &&
            t.Destination == descriptor.Destination &&
            t.SignalType == descriptor.SignalType &&
            ((t.InputPort == null && descriptor.InputPort == null) ||
             (t.InputPort != null && descriptor.InputPort != null && t.InputPort.Key == descriptor.InputPort.Key)));

        if (existingRoute != null)
        {
            Debug.LogMessage(LogEventLevel.Information, descriptor.Destination,
                "Route from {0} to {1}:{2} ({3}) already exists in this collection",
                descriptor?.Source?.Key,
                descriptor?.Destination?.Key,
                descriptor?.InputPort?.Key ?? "auto",
                descriptor?.SignalType);
            return;
        }
        Debug.LogMessage(LogEventLevel.Verbose, "Adding route descriptor: {0} -> {1}:{2} ({3})",
            descriptor?.Source?.Key,
            descriptor?.Destination?.Key,
            descriptor?.InputPort?.Key ?? "auto",
            descriptor?.SignalType);
        RouteDescriptors.Add(descriptor);

        RouteDescriptorCollectionChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Gets the RouteDescriptor for a destination
    /// </summary>
    /// <returns>null if no RouteDescriptor for a destination exists</returns>
    public RouteDescriptor GetRouteDescriptorForDestination(IRoutingInputs destination)
    {
        Debug.LogMessage(LogEventLevel.Information, "Getting route descriptor for '{destination}'", destination?.Key ?? null);

        return RouteDescriptors.FirstOrDefault(rd => rd.Destination == destination);
    }

    /// <summary>
    /// Gets the route descriptor for a specific destination and input port
    /// </summary>
    /// <param name="destination">The destination device</param>
    /// <param name="inputPortKey">The input port key</param>
    /// <returns>The matching RouteDescriptor or null if not found</returns>
    public RouteDescriptor GetRouteDescriptorForDestinationAndInputPort(IRoutingInputs destination, string inputPortKey)
    {
        Debug.LogMessage(LogEventLevel.Information, "Getting route descriptor for '{destination}':'{inputPortKey}'", destination?.Key ?? null, string.IsNullOrEmpty(inputPortKey) ? "auto" : inputPortKey);
        return RouteDescriptors.FirstOrDefault(rd => rd.Destination == destination && rd.InputPort != null && rd.InputPort.Key == inputPortKey);
    }

    /// <summary>
    /// Returns the RouteDescriptor for a given destination AND removes it from collection.
    /// Returns null if no route with the provided destination exists.
    /// </summary>
    /// <remarks>
    /// Removes only the first match. An AudioVideo route is stored as two descriptors (one Audio,
    /// one Video), so to release everything routed to a destination use
    /// <see cref="RemoveRouteDescriptors"/> instead.
    /// </remarks>
    /// <param name="destination">The destination device</param>
    /// <param name="inputPortKey">The input port key (optional)</param>
    /// <returns>The matching RouteDescriptor or null if not found</returns>
    public RouteDescriptor RemoveRouteDescriptor(IRoutingInputs destination, string inputPortKey = "")
    {
        Debug.LogMessage(LogEventLevel.Information, "Removing route descriptor for '{destination}':'{inputPortKey}'", destination.Key ?? null, string.IsNullOrEmpty(inputPortKey) ? "auto" : inputPortKey);

        var descr = string.IsNullOrEmpty(inputPortKey)
            ? GetRouteDescriptorForDestination(destination)
            : GetRouteDescriptorForDestinationAndInputPort(destination, inputPortKey);
        if (descr != null)
        {
            RouteDescriptors.Remove(descr);
            RouteDescriptorCollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        Debug.LogMessage(LogEventLevel.Information, "Found route descriptor {routeDescriptor}", destination, descr);

        return descr;
    }

    /// <summary>
    /// Removes and returns every RouteDescriptor for a destination, optionally limited to one input
    /// port. An AudioVideo route is stored as separate Audio and Video descriptors, so "the route to
    /// this destination" can be more than one; releasing only one of them orphans the other, along
    /// with its output ports' in-use registrations.
    /// </summary>
    /// <param name="destination">The destination device</param>
    /// <param name="inputPortKey">The input port key (optional). When empty, every descriptor for the destination is removed.</param>
    /// <returns>The removed descriptors, oldest first. Empty if none matched.</returns>
    public List<RouteDescriptor> RemoveRouteDescriptors(IRoutingInputs destination, string inputPortKey = "")
    {
        var removed = RouteDescriptors
            .Where(rd => rd.Destination == destination &&
                (string.IsNullOrEmpty(inputPortKey) || (rd.InputPort != null && rd.InputPort.Key == inputPortKey)))
            .ToList();

        foreach (var descriptor in removed)
        {
            RouteDescriptors.Remove(descriptor);
        }

        if (removed.Count > 0)
        {
            RouteDescriptorCollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        Debug.LogMessage(LogEventLevel.Information, "Removed {count} route descriptor(s) for '{destination}':'{inputPortKey}'",
            removed.Count, destination?.Key, string.IsNullOrEmpty(inputPortKey) ? "auto" : inputPortKey);

        return removed;
    }

    /// <summary>
    /// Records <paramref name="replacement"/> as the route for its destination, input port and signal
    /// type, removing whatever was recorded there before - but only once there is something to put in
    /// its place. Used to resync the collection with routing feedback without ever leaving a
    /// destination with no descriptor, which would leave a later release nothing to tear down.
    /// </summary>
    /// <remarks>
    /// When the existing descriptor already names the same source, nothing changes: that descriptor is
    /// the one that was executed, and it holds the output ports' in-use registrations.
    /// </remarks>
    /// <param name="replacement">The descriptor to record. Null is ignored.</param>
    /// <returns>True if the collection changed.</returns>
    public bool ReplaceRouteDescriptor(RouteDescriptor replacement)
    {
        if (replacement == null)
        {
            return false;
        }

        var existing = RouteDescriptors
            .Where(rd => rd.Destination == replacement.Destination &&
                rd.SignalType == replacement.SignalType &&
                rd.InputPort?.Key == replacement.InputPort?.Key)
            .ToList();

        if (existing.Count == 1 && existing[0].Source == replacement.Source)
        {
            return false;
        }

        foreach (var descriptor in existing)
        {
            RouteDescriptors.Remove(descriptor);
        }

        RouteDescriptors.Add(replacement);
        RouteDescriptorCollectionChanged?.Invoke(this, EventArgs.Empty);

        Debug.LogMessage(LogEventLevel.Debug, "Replaced {count} route descriptor(s) for '{destination}':'{inputPortKey}' ({signalType}) with route from {source}",
            existing.Count, replacement.Destination?.Key, replacement.InputPort?.Key ?? "auto", replacement.SignalType, replacement.Source?.Key);

        return true;
    }
}
