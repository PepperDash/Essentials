using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro;

using PepperDash.Core;
using Serilog.Events;


namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Represents a collection of individual route steps between a Source and a Destination device for a specific signal type.
    /// </summary>
    public class RouteDescriptor
    {
        /// <summary>
        /// The destination device (sink or midpoint) for the route.
        /// </summary>
        public IRoutingInputs Destination { get; private set; }

        /// <summary>
        /// Gets or sets the InputPort
        /// </summary>
        public RoutingInputPort InputPort { get; private set; }

        /// <summary>
        /// Gets or sets the Source
        /// </summary>
        public IRoutingOutputs Source { get; private set; }

        /// <summary>
        /// Gets or sets the SignalType
        /// </summary>
        public eRoutingSignalType SignalType { get; private set; }

        /// <summary>
        /// Gets or sets the Routes
        /// </summary>
        public List<RouteSwitchDescriptor> Routes { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RouteDescriptor"/> class for a route without a specific destination input port.
        /// </summary>
        /// <param name="source">The source device.</param>
        /// <param name="destination">The destination device.</param>
        /// <param name="signalType">The type of signal being routed.</param>
        public RouteDescriptor(IRoutingOutputs source, IRoutingInputs destination, eRoutingSignalType signalType) : this(source, destination, null, signalType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RouteDescriptor"/> class for a route with a specific destination input port.
        /// </summary>
        /// <param name="source">The source device.</param>
        /// <param name="destination">The destination device.</param>
        /// <param name="inputPort">The destination input port (optional).</param>
        /// <param name="signalType">The signal type for this route.</param>
        public RouteDescriptor(IRoutingOutputs source, IRoutingInputs destination, RoutingInputPort inputPort, eRoutingSignalType signalType)
        {
            Destination = destination;
            InputPort = inputPort;
            Source = source;
            SignalType = signalType;
            Routes = new List<RouteSwitchDescriptor>();
        }

        /// <summary>
        /// ExecuteRoutes method
        /// </summary>
        public void ExecuteRoutes()
        {
            foreach (var route in Routes)
            {
                Debug.LogMessage(LogEventLevel.Verbose, "ExecuteRoutes: {0}", null, route.ToString());

                if (route.SwitchingDevice is IRoutingSinkWithSwitching sink)
                {
                    sink.ExecuteSwitch(route.InputPort.Selector);
                    continue;
                }

                if (route.SwitchingDevice is IRouting switchingDevice)
                {
                    switchingDevice.ExecuteSwitch(route.InputPort.Selector, route.OutputPort.Selector, SignalType);

                    route.OutputPort.InUseTracker.AddUser(Destination, "destination-" + SignalType);

                    Debug.LogMessage(LogEventLevel.Verbose, "Output port {0} routing. Count={1}", null, route.OutputPort.Key, route.OutputPort.InUseTracker.InUseCountFeedback.UShortValue);
                }
            }
        }

        /// <summary>
        /// Releases the usage tracking for the route and optionally clears the route on the switching devices.
        /// </summary>
        /// <param name="clearRoute">If true, attempts to clear the route on the switching devices (e.g., set input to null/0).</param>


        public void ReleaseRoutes(bool clearRoute = false)
        {
            foreach (var route in Routes.Where(r => r.SwitchingDevice is IRouting))
            {
                if (route.SwitchingDevice is IRouting switchingDevice)
                {
                    if (clearRoute)
                    {
                        try
                        {
                            switchingDevice.ExecuteSwitch(null, route.OutputPort.Selector, SignalType);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("Error executing switch: {exception}", e.Message);
                        }
                    }

                    if (route.OutputPort == null)
                    {
                        continue;
                    }

                    if (route.OutputPort.InUseTracker != null)
                    {
                        route.OutputPort.InUseTracker.RemoveUser(Destination, "destination-" + SignalType);
                        Debug.LogMessage(LogEventLevel.Verbose, "Port {0} releasing. Count={1}", null, route.OutputPort.Key, route.OutputPort.InUseTracker.InUseCountFeedback.UShortValue);
                    }
                    else
                    {
                        Debug.LogMessage(LogEventLevel.Error, "InUseTracker is null for OutputPort {0}", null, route.OutputPort.Key);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a string representation of the route descriptor, including source, destination, and individual route steps.
        /// </summary>
        /// <returns>A string describing the route.</returns>
        public override string ToString()
        {
            var routesText = Routes.Select(r => r.ToString()).ToArray();
            return $"Route table from {Source.Key} to {Destination.Key} for {SignalType}:\r\n    {string.Join("\r\n    ", routesText)}";
        }
    }

}