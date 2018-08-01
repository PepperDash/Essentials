using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Room.Cotija
{
	/// <summary>
	/// Represents a room whose configuration is derived from runtime data,
	/// perhaps from another program, and that the data may not be fully
	/// available at startup.
	/// </summary>
	public interface IDelayedConfiguration
	{
		event EventHandler<EventArgs> ConfigurationIsReady;
	}
}

namespace PepperDash.Essentials
{
	/// <summary>
	/// For rooms with a single presentation source, change event
	/// </summary>
	public interface IHasCurrentSourceInfoChange
	{
		event SourceInfoChangeHandler CurrentSingleSourceChange;
	}


	/// <summary>
	/// For rooms with routing
	/// </summary>
	public interface IRunRouteAction
	{
		void RunRouteAction(string routeKey);

		void RunRouteAction(string routeKey, Action successCallback);
	}

	/// <summary>
	/// For rooms that default presentation only routing
	/// </summary>
	public interface IRunDefaultPresentRoute
	{
		void RunDefaultPresentRoute();
	}

	/// <summary>
	/// For rooms that have default presentation and calling routes
	/// </summary>
	public interface IRunDefaultCallRoute : IRunDefaultPresentRoute
	{
		void RunDefaultCallRoute();
	}
}