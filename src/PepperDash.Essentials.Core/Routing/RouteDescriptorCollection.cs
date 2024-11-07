using System.Collections.Generic;
using System.Linq;
using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Routing
{
    /// <summary>
    /// A collection of RouteDescriptors - typically the static DefaultCollection is used
    /// </summary>
    public class RouteDescriptorCollection
    {
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
                    "Route to [{0}] already exists in global routes table", descriptor.Source.Key);
                return;
            }
            RouteDescriptors.Add(descriptor);
        }

        /// <summary>
        /// Gets the RouteDescriptor for a destination
        /// </summary>
        /// <returns>null if no RouteDescriptor for a destination exists</returns>
        public RouteDescriptor GetRouteDescriptorForDestination(IRoutingInputs destination)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Getting route descriptor", destination);

            return RouteDescriptors.FirstOrDefault(rd => rd.Destination == destination);
        }

        public RouteDescriptor GetRouteDescriptorForDestinationAndInputPort(IRoutingInputs destination, string inputPortKey)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Getting route descriptor for {inputPortKey}", destination, string.IsNullOrEmpty(inputPortKey) ? "auto" : inputPortKey);
            return RouteDescriptors.FirstOrDefault(rd => rd.Destination == destination && rd.InputPort != null && rd.InputPort.Key == inputPortKey);
        }

        /// <summary>
        /// Returns the RouteDescriptor for a given destination AND removes it from collection.
        /// Returns null if no route with the provided destination exists.
        /// </summary>
        public RouteDescriptor RemoveRouteDescriptor(IRoutingInputs destination, string inputPortKey = "")
        {
            Debug.LogMessage(LogEventLevel.Debug, "Removing route descriptor for {inputPortKey}", destination, string.IsNullOrEmpty(inputPortKey) ? "auto" : inputPortKey);

            var descr = string.IsNullOrEmpty(inputPortKey) 
                ? GetRouteDescriptorForDestination(destination)
                : GetRouteDescriptorForDestinationAndInputPort(destination, inputPortKey);
            if (descr != null)
                RouteDescriptors.Remove(descr);

            Debug.LogMessage(LogEventLevel.Debug, "Found route descriptor {routeDescriptor}", destination, descr);

            return descr;
        }
    }

    /*/// <summary>
    /// A collection of RouteDescriptors - typically the static DefaultCollection is used
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