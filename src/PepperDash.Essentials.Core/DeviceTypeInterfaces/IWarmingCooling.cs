using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core;

	/// <summary>
	/// Defines a class that has warm up and cool down
	/// </summary>
	public interface IWarmingCooling
	{
		BoolFeedback IsWarmingUpFeedback { get; }
		BoolFeedback IsCoolingDownFeedback { get; }
	}