using System;
using PepperDash.Essentials.Devices.Common.VideoCodec.Cisco;

namespace PepperDash.Essentials.Devices.Common.Codec
{
	/// <summary>
	/// Defines the contract for IHasExternalSourceSwitching
	/// </summary>
	public interface IHasExternalSourceSwitching
	{
		/// <summary>
		/// Gets a value indicating whether the external source list is enabled
		/// </summary>
		bool ExternalSourceListEnabled { get; }

		/// <summary>
		/// Gets the external source input port identifier
		/// </summary>
		string ExternalSourceInputPort { get; }

		/// <summary>
		/// Adds an external source to the available sources
		/// </summary>
		/// <param name="connectorId">The connector identifier</param>
		/// <param name="key">The unique key for the source</param>
		/// <param name="name">The display name for the source</param>
		/// <param name="type">The type of external source</param>
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
		/// Sets the selected source by its key
		/// </summary>
		/// <param name="key">The unique key of the source to select</param>
		void SetSelectedSource(string key);

		/// <summary>
		/// Sets the action to run when routing between sources
		/// </summary>
		Action<string, string> RunRouteAction { set; }
	}

}