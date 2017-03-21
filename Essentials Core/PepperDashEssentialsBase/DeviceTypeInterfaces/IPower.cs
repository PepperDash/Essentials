using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// 
	/// </summary>
	public interface IPower
	{
		void PowerOn();
		void PowerOff();
		void PowerToggle();
		BoolFeedback PowerIsOnFeedback { get; }
	}

	/// <summary>
	/// 
	/// </summary>
	public static class IPowerExtensions
	{
		public static void LinkButtons(this IPower dev, BasicTriList triList)
		{
			triList.SetSigFalseAction(101, dev.PowerOn);
			triList.SetSigFalseAction(102, dev.PowerOff);
			triList.SetSigFalseAction(103, dev.PowerToggle);
			dev.PowerIsOnFeedback.LinkInputSig(triList.BooleanInput[101]);
		}

		public static void UnlinkButtons(this IPower dev, BasicTriList triList)
		{
			triList.ClearBoolSigAction(101);
			triList.ClearBoolSigAction(102);
			triList.ClearBoolSigAction(103);
			dev.PowerIsOnFeedback.UnlinkInputSig(triList.BooleanInput[101]);
		}
	}
}