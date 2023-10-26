using System.Collections.Generic;
using System.Linq;
using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Represents an collection of individual route steps between Source and Destination
    /// </summary>
    public class RouteDescriptor
    {
        public IRoutingInputs Destination { get; private set; }
        public IRoutingOutputs Source { get; private set; }
        public eRoutingSignalType SignalType { get; private set; }
        public List<RouteSwitchDescriptor> Routes { get; private set; }


        public RouteDescriptor(IRoutingOutputs source, IRoutingInputs destination, eRoutingSignalType signalType)
        {
            Destination = destination;
            Source      = source;
            SignalType  = signalType;
            Routes      = new List<RouteSwitchDescriptor>();
        }

        /// <summary>
        /// Executes all routes described in this collection.  Typically called via
        /// extension method IRoutingInputs.ReleaseAndMakeRoute()
        /// </summary>
        public void ExecuteRoutes()
        {
            foreach (var route in Routes)
            {
                Debug.Console(2, "ExecuteRoutes: {0}", route.ToString());
                if (route.SwitchingDevice is IRoutingSink)
                {
                    var device = route.SwitchingDevice as IRoutingSinkWithSwitching;
                    if (device == null)
                        continue;

                    device.ExecuteSwitch(route.InputPort.Selector);
                }
                else if (route.SwitchingDevice is IRouting)
                {
                    (route.SwitchingDevice as IRouting).ExecuteSwitch(route.InputPort.Selector, route.OutputPort.Selector, SignalType);
                    route.OutputPort.InUseTracker.AddUser(Destination, "destination-" + SignalType);
                    Debug.Console(2, "Output port {0} routing. Count={1}", route.OutputPort.Key, route.OutputPort.InUseTracker.InUseCountFeedback.UShortValue);
                }
            }
        }

        /// <summary>
        /// Releases all routes in this collection. Typically called via
        /// extension method IRoutingInputs.ReleaseAndMakeRoute()
        /// </summary>
        public void ReleaseRoutes()
        {
            foreach (var route in Routes)
            {
                if (route.SwitchingDevice is IRouting)
                {
                    // Pull the route from the port.  Whatever is watching the output's in use tracker is
                    // responsible for responding appropriately.
                    route.OutputPort.InUseTracker.RemoveUser(Destination, "destination-" + SignalType);
                    Debug.Console(2, "Port {0} releasing. Count={1}", route.OutputPort.Key, route.OutputPort.InUseTracker.InUseCountFeedback.UShortValue);
                }
            }
        }

        public override string ToString()
        {
            var routesText = Routes.Select(r => r.ToString()).ToArray();
            return string.Format("Route table from {0} to {1}:\r{2}", Source.Key, Destination.Key, string.Join("\r", routesText));
        }
    }
}