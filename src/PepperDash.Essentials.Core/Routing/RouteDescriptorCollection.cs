using System.Collections.Generic;
using System.Linq;
using PepperDash.Core;

namespace PepperDash.Essentials.Core
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
        static RouteDescriptorCollection _DefaultCollection;

        List<RouteDescriptor> RouteDescriptors = new List<RouteDescriptor>();

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
                Debug.Console(1, descriptor.Destination, 
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
            return RouteDescriptors.FirstOrDefault(rd => rd.Destination == destination);
        }

        /// <summary>
        /// Returns the RouteDescriptor for a given destination AND removes it from collection.
        /// Returns null if no route with the provided destination exists.
        /// </summary>
        public RouteDescriptor RemoveRouteDescriptor(IRoutingInputs destination)
        {
            var descr = GetRouteDescriptorForDestination(destination);
            if (descr != null)
                RouteDescriptors.Remove(descr);
            return descr;
        }
    }
}