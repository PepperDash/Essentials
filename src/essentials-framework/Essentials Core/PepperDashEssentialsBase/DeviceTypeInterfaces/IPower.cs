using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.Fusion;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;


namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Defines the ability to power a device on and off
	/// </summary>
    [Obsolete("Will be replaced by IHasPowerControlWithFeedback")]
	public interface IPower
	{
		void PowerOn();
		void PowerOff();
		void PowerToggle();
        BoolFeedback PowerIsOnFeedback { get; }
	}

    /// <summary>
    /// Adds feedback for current power state
    /// </summary>
    public interface IHasPowerControlWithFeedback : IHasPowerControl
    {
        BoolFeedback PowerIsOnFeedback { get; }
    }

    /// <summary>
    /// Defines the ability to power a device on and off
    /// </summary>
    public interface IHasPowerControl
    {
        void PowerOn();
        void PowerOff();
        void PowerToggle();
    }

	/// <summary>
	/// 
	/// </summary>
	public static class IHasPowerControlExtensions
	{
        public static void LinkButtons(this IHasPowerControl dev, BasicTriList triList)
		{
			triList.SetSigFalseAction(101, dev.PowerOn);
			triList.SetSigFalseAction(102, dev.PowerOff);
			triList.SetSigFalseAction(103, dev.PowerToggle);

            var fbdev = dev as IHasPowerControlWithFeedback;
            if (fbdev != null)
            {
                fbdev.PowerIsOnFeedback.LinkInputSig(triList.BooleanInput[101]);
            }
		}

        public static void UnlinkButtons(this IHasPowerControl dev, BasicTriList triList)
		{
			triList.ClearBoolSigAction(101);
			triList.ClearBoolSigAction(102);
			triList.ClearBoolSigAction(103);

            var fbdev = dev as IHasPowerControlWithFeedback;
            if (fbdev != null)
            {
                fbdev.PowerIsOnFeedback.UnlinkInputSig(triList.BooleanInput[101]);
            }
		}
	}
}