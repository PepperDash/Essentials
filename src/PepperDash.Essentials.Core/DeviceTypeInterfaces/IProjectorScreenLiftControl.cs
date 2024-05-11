using Crestron.SimplSharpPro.DeviceSupport;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
	/// <summary>
	/// Defines a class that has warm up and cool down
	/// </summary>
	public interface IProjectorScreenLiftControl
	{
		void Raise();
		void Lower();
		BoolFeedback IsInUpPosition { get; }
		bool InUpPosition { get; }
		event EventHandler<EventArgs> PositionChanged;
		string DisplayDeviceKey { get; }
		eScreenLiftControlType Type { get; } // screen/lift
	}

	public enum eScreenLiftControlType
	{
		lift,
		screen
	}
}