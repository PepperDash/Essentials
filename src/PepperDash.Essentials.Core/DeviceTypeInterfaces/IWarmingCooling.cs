using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Defines a class that has warm up and cool down
	/// </summary>
	public interface IWarmingCooling
	{
		/// <summary>
		/// Feedback indicating whether the device is warming up
		/// </summary>
		BoolFeedback IsWarmingUpFeedback { get; }

		/// <summary>
		/// Feedback indicating whether the device is cooling down
		/// </summary>
		BoolFeedback IsCoolingDownFeedback { get; }
	}
}