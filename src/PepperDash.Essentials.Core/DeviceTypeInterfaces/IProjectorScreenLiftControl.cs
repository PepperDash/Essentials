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
		void Raise();
		void Lower();
		BoolFeedback IsInUpPosition { get; }
		bool InUpPosition { get; }
		event EventHandler<EventArgs> PositionChanged;
		string DisplayDeviceKey { get; }
		eScreenLiftControlType Type { get; } // screen/lift
	}

 /// <summary>
 /// Enumeration of eScreenLiftControlType values
 /// </summary>
	public enum eScreenLiftControlType
	{
		lift,
		screen
	}
}