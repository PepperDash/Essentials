using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;


namespace PepperDash.Essentials.Core
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
}