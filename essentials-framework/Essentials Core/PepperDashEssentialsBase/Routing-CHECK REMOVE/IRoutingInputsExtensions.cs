using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;

using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// 
	/// </summary>
	public static class IRoutingInputsExtensions
	{
		/// <summary>
		/// Gets any existing route for a destination, clears it, and then 
		/// </summary>
		public static void ReleaseAndMakeRoute(this IRoutingInputs destination, IRoutingOutputs source, eRoutingSignalType signalType)
		{
			var sw = new Stopwatch();
			sw.Start();

			destination.ReleaseRoute();

			if (source == null) return;
			var newRoute = destination.GetRouteToSource(source, signalType);
			if (newRoute == null) return;
			RouteDescriptorCollection.DefaultCollection.AddRouteDescriptor(newRoute);
			Debug.Console(2, destination, "Executing new route");
			newRoute.ExecuteRoutes();
			sw.Stop();
			Debug.Console(2, destination, "Route took {0} ms", sw.ElapsedMilliseconds);
		}

		/// <summary>
		/// Will release the existing route on the destination
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="signalType"></param>
		public static void ReleaseRoute(this IRoutingInputs destination)
		{
			var current = RouteDescriptorCollection.DefaultCollection.RemoveRouteDescriptor(destination);
			if (current != null)
			{
				Debug.Console(2, destination, "Releasing current route: {0}", current.Source.Key);
				current.ReleaseRoutes();
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="device"></param>
		/// <param name="targetSource"></param>
		/// <param name="signalType"></param>
		/// <returns></returns>
		public static RouteDescriptor GetRouteToSource(this IRoutingInputs destination, IRoutingOutputs source, eRoutingSignalType signalType)
		{
			var routeTable = new RouteDescriptor (source, destination, signalType);

			Debug.Console(0, destination, "Attempting to build source route from {0}***", source.Key);
			if (!destination.GetRouteToSource(source, null, null, signalType, 0, routeTable))
				routeTable = null;

			Debug.Console(0, destination, "Route{0} discovered ***", routeTable == null ? " NOT" : "");
			return routeTable;
		}

		/// <summary>
		/// The recursive part of this.  Will stop on each device, search its inputs for the 
		/// desired source and if not found, invoke this function for the each input port
		/// hoping to find the source.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="source"></param>
		/// <param name="onSuccessOutputPort"></param>
		/// <param name="alreadyCheckedDevices"></param>
		/// <param name="signalType"></param>
		/// <param name="cycle"></param>
		/// <param name="routeTable"></param>
		/// <returns>true if source is hit</returns>
		static bool GetRouteToSource(this IRoutingInputs destination, IRoutingOutputs source,
			RoutingOutputPort onSuccessOutputPort, List<IRoutingInputsOutputs> alreadyCheckedDevices, 
				eRoutingSignalType signalType, int cycle, RouteDescriptor routeTable)
		{
			cycle++;
			Debug.Console(0, destination, "SelectInput-cycle {1}. Finding {2} route back to {0}", source.Key, cycle, signalType);
			var destDevInputTies = TieLineCollection.Default.Where(t =>
				t.DestinationPort.ParentDevice == destination && (t.Type == signalType || t.Type == eRoutingSignalType.AudioVideo));

			// find a direct tie
			var directTie = destDevInputTies.FirstOrDefault(
				t => !(t.SourcePort.ParentDevice is IRoutingInputsOutputs) 
					&& t.DestinationPort.ParentDevice == destination 
					&& t.SourcePort.ParentDevice == source);
			RoutingInputPort inputPort = null;
			if (directTie != null) // Found a tie directly to the source
			{
				Debug.Console(0, destination, "Found direct tie to {0}**", source.Key);
				inputPort = directTie.DestinationPort;
			}
			else // no direct-connect.  Walk back devices.
			{
				Debug.Console(0, destination, "is not directly connected to {0}. Walking down tie lines", source.Key);

				// No direct tie? Run back out on the inputs' attached devices... 
				// Only the ones that are routing devices
				var attachedMidpoints = destDevInputTies.Where(t => t.SourcePort.ParentDevice is IRoutingInputsOutputs);
				foreach (var inputTieToTry in attachedMidpoints)
				{
					Debug.Console(0, destination, "Trying to find route on {0}", inputTieToTry.SourcePort.ParentDevice.Key);
					var upstreamDeviceOutputPort = inputTieToTry.SourcePort;
					var upstreamRoutingDevice = upstreamDeviceOutputPort.ParentDevice as IRoutingInputsOutputs;
					// Check if this previous device has already been walked
					if (!(alreadyCheckedDevices != null && alreadyCheckedDevices.Contains(upstreamRoutingDevice)))
					{
						// haven't seen this device yet.  Do it.  Pass the output port to the next
						// level to enable switching on success
						var upstreamRoutingSuccess = upstreamRoutingDevice.GetRouteToSource(source, upstreamDeviceOutputPort, 
							alreadyCheckedDevices, signalType, cycle, routeTable);
						if (upstreamRoutingSuccess)
						{
							Debug.Console(0, destination, "Upstream device route found");
							inputPort = inputTieToTry.DestinationPort;
							break; // Stop looping the inputs in this cycle
						}
					}
				}
			}

			// we have a route on corresponding inputPort. *** Do the route ***
			if (inputPort != null) 
			{
				Debug.Console(0, destination, "adding route:");
				if (onSuccessOutputPort == null)
				{
					// it's a sink device
					routeTable.Routes.Add(new RouteSwitchDescriptor(inputPort));
				}
				else if (destination is IRouting)
				{
					routeTable.Routes.Add(new RouteSwitchDescriptor (onSuccessOutputPort, inputPort));
				}
				else // device is merely IRoutingInputOutputs
					Debug.Console(0, destination, "    No routing. Passthrough device");
				Debug.Console(0, destination, "Exiting cycle {0}", cycle);
				return true;
			}
	
			if(alreadyCheckedDevices == null)
				alreadyCheckedDevices = new List<IRoutingInputsOutputs>();
			alreadyCheckedDevices.Add(destination as IRoutingInputsOutputs);

			Debug.Console(0, destination, "No route found to {0}", source.Key);
			return false;
		}
	}





	// MOVE MOVE MOVE MOVE MOVE MOVE MOVE  MOVE MOVE MOVE MOVE MOVE MOVE MOVE  MOVE MOVE MOVE MOVE MOVE MOVE MOVE 


	/// <summary>
	/// A collection of routes - typically the global DefaultCollection is used
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

		public RouteDescriptor GetRouteDescriptorForDestination(IRoutingInputs destination)
		{
			return RouteDescriptors.FirstOrDefault(rd => rd.Destination == destination);
		}

		/// <summary>
		/// Returns the RouteDescriptor for a given destination and removes it from collection.
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

		public void ExecuteRoutes()
		{
			foreach (var route in Routes)
			{
				Debug.Console(2, route.ToString());
				if (route.SwitchingDevice is IRoutingSinkWithSwitching)
					(route.SwitchingDevice as IRoutingSinkWithSwitching).ExecuteSwitch(route.InputPort.Selector);
				else if (route.SwitchingDevice is IRouting)
				{
					(route.SwitchingDevice as IRouting).ExecuteSwitch(route.InputPort.Selector, route.OutputPort.Selector, SignalType);
					route.OutputPort.InUseTracker.AddUser(Destination, "destination");
				}
			}
		}

		public void ReleaseRoutes()
		{
			foreach (var route in Routes)
			{
				if (route.SwitchingDevice is IRouting)
				{
					// Pull the route from the port.  Whatever is watching the output's in use tracker is
					// responsible for responding appropriately.
					route.OutputPort.InUseTracker.RemoveUser(Destination, "destination");
				}
			}
		}

		public override string ToString()
		{
			var routesText = Routes.Select(r => r.ToString()).ToArray();
			return string.Format("Route table from {0} to {1}:\r{2}", Source.Key, Destination.Key, string.Join("\r", routesText));
		}
	}

	/// <summary>
	/// Represents an individual link for a route
	/// </summary>
	public class RouteSwitchDescriptor
	{
		public IRoutingInputs SwitchingDevice { get { return InputPort.ParentDevice; } }
		public RoutingOutputPort OutputPort { get; set; }
		public RoutingInputPort InputPort { get; set; }

		public RouteSwitchDescriptor(RoutingInputPort inputPort)
		{
			InputPort = inputPort;
		}

		public RouteSwitchDescriptor(RoutingOutputPort outputPort, RoutingInputPort inputPort)
		{
			InputPort = inputPort;
			OutputPort = outputPort;
		}

		public override string ToString()
		{
			if(OutputPort == null) // IRoutingSink
				return string.Format("{0} switches to input '{1}'", SwitchingDevice.Key, InputPort.Selector);

			return string.Format("{0} switches output '{1}' to input '{2}'", SwitchingDevice.Key, OutputPort.Selector, InputPort.Selector);
		}
	}
}