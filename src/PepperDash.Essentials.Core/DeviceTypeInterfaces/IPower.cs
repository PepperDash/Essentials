using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
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
}