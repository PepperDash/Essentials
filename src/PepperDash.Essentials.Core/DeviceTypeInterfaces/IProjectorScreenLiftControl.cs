using Crestron.SimplSharpPro.DeviceSupport;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
	/// <summary>
	/// Defines the contract for IProjectorScreenLiftControl
	/// </summary>
	public interface IProjectorScreenLiftControl
	{
		/// <summary>
		/// Raises the screen/lift
		/// </summary>
		void Raise();

		/// <summary>
		/// Lowers the screen/lift
		/// </summary>
		void Lower();

		/// <summary>
		/// Stops the screen/lift
		/// </summary>
		BoolFeedback IsInUpPosition { get; }

		/// <summary>
		/// Gets whether the screen/lift is in the up position
		/// </summary>
		bool InUpPosition { get; }

		/// <summary>
		/// Gets whether the screen/lift is in the down position
		/// </summary>
		event EventHandler<EventArgs> PositionChanged;

		/// <summary>
		/// The device key of the display associated with this screen/lift
		/// </summary>
		string DisplayDeviceKey { get; }

		/// <summary>
		/// The type of device
		/// </summary>
		eScreenLiftControlType Type { get; } // screen/lift
	}

	/// <summary>
	/// Enumeration of eScreenLiftControlType values
	/// </summary>
	public enum eScreenLiftControlType
	{
		/// <summary>
		/// Lift type device
		/// </summary>
		lift,

		/// <summary>
		/// Screen type device
		/// </summary>
		screen
	}
}