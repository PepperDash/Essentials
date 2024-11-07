﻿using PepperDash.Core;
using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.Devices
{
	public interface IOnline
	{
		BoolFeedback IsOnline { get; }
	}

	///// <summary>
	///// ** WANT THIS AND ALL ITS FRIENDS TO GO AWAY **
	///// Defines a class that has a list of CueAction objects, typically
	///// for linking functions to user interfaces or API calls
	///// </summary>
	//public interface IHasCueActionList
	//{
	//    List<CueActionPair> CueActionList { get; }
	//}


	//public interface IHasComPortsHardware
	//{
	//    IComPorts ComPortsDevice { get; }
	//}

	/// <summary>
	/// Describes a device that can have a video sync providing device attached to it
	/// </summary>
	public interface IAttachVideoStatus : IKeyed
	{
		// Extension methods will depend on this
	}

	/// <summary>
	/// For display classes that can provide usage data
	/// </summary>
	public interface IDisplayUsage
	{
		IntFeedback LampHours { get; }
	}

	public interface IMakeModel : IKeyed
	{
		string DeviceMake { get; }
		string DeviceModel { get; }
	}
}