using System;
using PepperDash.Essentials.Devices.Common.VideoCodec.Cisco;

namespace PepperDash.Essentials.Devices.Common.Codec;

/// <summary>
/// Defines the contract for devices that support external source switching functionality
/// </summary>
public interface IHasExternalSourceSwitching
{
	/// <summary>
	/// Gets a value indicating whether the external source list is enabled
	/// </summary>
	bool ExternalSourceListEnabled { get; }

	/// <summary>
	/// Gets the port name for external source input switching
	/// </summary>
	string ExternalSourceInputPort { get; }


	/// <summary>
	/// Adds an external source to the list of available sources for switching
	/// </summary>
	/// <param name="connectorId"></param>
	/// <param name="key"></param>
	/// <param name="name"></param>
	/// <param name="type"></param>
	void AddExternalSource(string connectorId, string key, string name, eExternalSourceType type);

	/// <summary>
	/// Sets the state of the specified external source
	/// </summary>
	/// <param name="key">The unique key of the external source</param>
	/// <param name="mode">The mode to set for the source</param>
	void SetExternalSourceState(string key, eExternalSourceMode mode);

	/// <summary>
	/// Clears all external sources from the list
	/// </summary>
	void ClearExternalSources();

	/// <summary>
	/// Sets the selected external source for switching
	/// </summary>
	/// <param name="key"></param>
	void SetSelectedSource(string key);

	/// <summary>
	/// Defines an action to run when an external source is selected for switching. 
	/// The action takes the key and name of the selected source as parameters.
	/// </summary>
	Action<string, string> RunRouteAction { set; }
}
