using PepperDash.Core;
using Serilog.Events;
using System.Collections.Generic;
using System.Linq;


namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// A collection of RouteDescriptors - typically the static DefaultCollection is used
    /// </summary>
    public class RouteDescriptorCollection
    {
        /// <summary>
        /// DefaultCollection static property
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

            if (RouteDescriptors.Any(t => t.Destination == descriptor.Destination)
                && RouteDescriptors.Any(t => t.Destination == descriptor.Destination && t.InputPort != null && descriptor.InputPort != null && t.InputPort.Key == descriptor.InputPort.Key))
            {
                Debug.LogMessage(LogEventLevel.Debug, descriptor.Destination,
                    "Route to [{0}] already exists in global routes table", descriptor?.Source?.Key);
                return;
            }
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
        /// Gets the RouteDescriptor for a destination and input port key. Returns null if no matching RouteDescriptor exists.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="inputPortKey"></param>
        /// <returns></returns>
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

    /*/// <summary>
    /// A collection of RouteDescriptors - typically the static DefaultCollection is used
    /// </summary>
    /// <summary>
    /// Represents a RouteDescriptorCollection
    /// </summary>
    public class RouteDescriptorCollection<TInputSelector, TOutputSelector>
	{
		public static RouteDescriptorCollection<TInputSelector, TOutputSelector> DefaultCollection
		{
			get
			{
				if (_DefaultCollection == null)
					_DefaultCollection = new RouteDescriptorCollection<TInputSelector, TOutputSelector>();
				return _DefaultCollection;
			}
		}
		private static RouteDescriptorCollection<TInputSelector, TOutputSelector> _DefaultCollection;

		private readonly List<RouteDescriptor> RouteDescriptors = new List<RouteDescriptor>();

		/// <summary>
		/// Adds a RouteDescriptor to the list.  If an existing RouteDescriptor for the
		/// destination exists already, it will not be added - in order to preserve
		/// proper route releasing.
		/// </summary>
		/// <param name="descriptor"></param>
  /// <summary>
  /// AddRouteDescriptor method
  /// </summary>
		public void AddRouteDescriptor(RouteDescriptor descriptor)
		{
			if (RouteDescriptors.Any(t => t.Destination == descriptor.Destination))
			{
				Debug.LogMessage(LogEventLevel.Debug, descriptor.Destination, 
					"Route to [{0}] already exists in global routes table", descriptor.Source.Key);
				return;
			}
			RouteDescriptors.Add(descriptor);
		}

		/// <summary>
		/// Gets the RouteDescriptor for a destination
		/// </summary>
		/// <returns>null if no RouteDescriptor for a destination exists</returns>
  /// <summary>
  /// GetRouteDescriptorForDestination method
  /// </summary>
		public RouteDescriptor GetRouteDescriptorForDestination(IRoutingInputs<TInputSelector> destination)
		{
			return RouteDescriptors.FirstOrDefault(rd => rd.Destination == destination);
		}

		/// <summary>
		/// Returns the RouteDescriptor for a given destination AND removes it from collection.
		/// Returns null if no route with the provided destination exists.
		/// </summary>
		public RouteDescriptor RemoveRouteDescriptor(IRoutingInputs<TInputSelector> destination)
		{
			var descr = GetRouteDescriptorForDestination(destination);
			if (descr != null)
				RouteDescriptors.Remove(descr);
			return descr;
		}
	}*/
}