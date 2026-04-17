using System.Collections.Generic;
using System.Linq;
using PepperDash.Core;
using Serilog.Events;


namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// A collection of RouteDescriptors - typically the static DefaultCollection is used
    /// </summary>
    public class RouteDescriptorCollection
    {
        /// <summary>
        /// Gets the default collection of RouteDescriptors.
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
                 (t.InputPort != null && descriptor.InputPort != null && t.InputPort.Key == descriptor.InputPort.Key)) &&
                ((t.OutputPort == null && descriptor.OutputPort == null) ||
                    (t.OutputPort != null && descriptor.OutputPort != null && t.OutputPort.Key == descriptor.OutputPort.Key)));

            if (existingRoute != null)
            {
                Debug.LogInformation(descriptor.Destination,
                    "Route from {source}:{outputPort} to {destination}:{inputPort} ({signalType}) already exists in this collection",
                    descriptor?.Source?.Key,
                    descriptor?.OutputPort?.Key ?? "auto",
                    descriptor?.Destination?.Key,
                    descriptor?.InputPort?.Key ?? "auto",
                    descriptor?.SignalType
                    );
                return;
            }
            Debug.LogVerbose("Adding route descriptor: {source}:{outputPort} -> {destination}:{inputPort} ({signalType})",
                descriptor?.Source?.Key,
                descriptor?.OutputPort?.Key ?? "auto",
                descriptor?.Destination?.Key,
                descriptor?.InputPort?.Key ?? "auto",
                descriptor?.SignalType);
            RouteDescriptors.Add(descriptor);
        }

        /// <summary>
        /// Gets the RouteDescriptor for a destination. Returns null if no RouteDescriptor for a destination exists.
        /// </summary>
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
        /// Removes a RouteDescriptor from the collection based on the specified destination and input port key.
        /// </summary>
        /// <param name="destination">The destination for which the route descriptor is to be removed.</param>
        /// <param name="inputPortKey">The key of the input port associated with the route descriptor. If empty, the method will attempt to remove a descriptor based solely on the destination.</param>
        /// <returns>The removed RouteDescriptor object if a matching descriptor was found; otherwise, null.</returns>
        public RouteDescriptor RemoveRouteDescriptor(IRoutingInputs destination, string inputPortKey = "")
        {
            Debug.LogMessage(LogEventLevel.Information, "Removing route descriptor for '{destination}':'{inputPortKey}'", destination.Key ?? null, string.IsNullOrEmpty(inputPortKey) ? "auto" : inputPortKey);

            var descr = string.IsNullOrEmpty(inputPortKey)
                ? GetRouteDescriptorForDestination(destination)
                : GetRouteDescriptorForDestinationAndInputPort(destination, inputPortKey);
            if (descr != null)
                RouteDescriptors.Remove(descr);

            Debug.LogMessage(LogEventLevel.Information, "Found route descriptor {routeDescriptor}", destination, descr);

            return descr;
        }
    }
}