using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro;

using PepperDash.Core;
using Serilog.Events;


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
			Source = source;
			SignalType = signalType;
			Routes = new List<RouteSwitchDescriptor>();
		}

		/// <summary>
		/// Executes all routes described in this collection.  Typically called via
		/// extension method IRoutingInputs.ReleaseAndMakeRoute()
		/// </summary>
		public void ExecuteRoutes()
		{
			foreach (var route in Routes)
			{
				Debug.LogMessage(LogEventLevel.Verbose, "ExecuteRoutes: {0}",null, route.ToString());

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
		/// Releases all routes in this collection. Typically called via
		/// extension method IRoutingInputs.ReleaseAndMakeRoute()
		/// </summary>
		public void ReleaseRoutes()
		{
			foreach (var route in Routes.Where(r => r.SwitchingDevice is IRouting))
			{
				if (route.SwitchingDevice is IRouting switchingDevice)
				{
                    switchingDevice.ExecuteSwitch(null, route.OutputPort.Selector, SignalType);

					// Pull the route from the port.  Whatever is watching the output's in use tracker is
					// responsible for responding appropriately.
					route.OutputPort.InUseTracker.RemoveUser(Destination, "destination-" + SignalType);
					Debug.LogMessage(LogEventLevel.Verbose, "Port {0} releasing. Count={1}",null, route.OutputPort.Key, route.OutputPort.InUseTracker.InUseCountFeedback.UShortValue);
				}
			}
		}

		public override string ToString()
		{
			var routesText = Routes.Select(r => r.ToString()).ToArray();
			return string.Format("Route table from {0} to {1}:\r{2}", Source.Key, Destination.Key, string.Join("\r", routesText));
		}
	}

    /*/// <summary>
    /// Represents an collection of individual route steps between Source and Destination
    /// </summary>
    public class RouteDescriptor<TInputSelector, TOutputSelector>
    {
        public IRoutingInputs<TInputSelector> Destination { get; private set; }
        public IRoutingOutputs<TOutputSelector> Source { get; private set; }
        public eRoutingSignalType SignalType { get; private set; }
        public List<RouteSwitchDescriptor<TInputSelector, TOutputSelector>> Routes { get; private set; }


        public RouteDescriptor(IRoutingOutputs<TOutputSelector> source, IRoutingInputs<TInputSelector> destination, eRoutingSignalType signalType)
        {
            Destination = destination;
            Source = source;
            SignalType = signalType;
            Routes = new List<RouteSwitchDescriptor<TInputSelector, TOutputSelector>>();
        }

        /// <summary>
        /// Executes all routes described in this collection.  Typically called via
        /// extension method IRoutingInputs.ReleaseAndMakeRoute()
        /// </summary>
        public void ExecuteRoutes()
        {
            foreach (var route in Routes)
            {
                Debug.LogMessage(LogEventLevel.Verbose, "ExecuteRoutes: {0}", null, route.ToString());

                if (route.SwitchingDevice is IRoutingSinkWithSwitching<TInputSelector> sink)
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
        /// Releases all routes in this collection. Typically called via
        /// extension method IRoutingInputs.ReleaseAndMakeRoute()
        /// </summary>
        public void ReleaseRoutes()
        {
            foreach (var route in Routes)
            {
                if (route.SwitchingDevice is IRouting<TInputSelector, TOutputSelector>)
                {
                    // Pull the route from the port.  Whatever is watching the output's in use tracker is
                    // responsible for responding appropriately.
                    route.OutputPort.InUseTracker.RemoveUser(Destination, "destination-" + SignalType);
                    Debug.LogMessage(LogEventLevel.Verbose, "Port {0} releasing. Count={1}", null, route.OutputPort.Key, route.OutputPort.InUseTracker.InUseCountFeedback.UShortValue);
                }
            }
        }

        public override string ToString()
        {
            var routesText = Routes.Select(r => r.ToString()).ToArray();
            return string.Format("Route table from {0} to {1}:\r{2}", Source.Key, Destination.Key, string.Join("\r", routesText));
        }
    }*/
}