using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Helper class for various Sig events
	/// </summary>
	public class SigHelper
	{
		/// <summary>
		/// Runs action when Sig is pressed
		/// </summary>
		/// <param name="sig">signal pressed</param>
		/// <param name="act">action to run</param>
		public static void Pressed(Sig sig, Action act) { if (sig.BoolValue) act(); }

		/// <summary>
		/// Runs action when Sig is released
		/// </summary>
		public static void Released(Sig sig, Action act) { if (!sig.BoolValue) act(); }

  /// <summary>
  /// SetBoolOutAction method
  /// </summary>
		public static void SetBoolOutAction(BoolOutputSig sig, Action<bool> a)
		{
			if (sig != null)
				sig.UserObject = a;
		}

		/// <summary>
		/// Safely clears action of non-null sig.
		/// </summary>
		public static void ClearBoolOutAction(BoolOutputSig sig)
		{
			if (sig != null)
				sig.UserObject = null;
		}

		/// <summary>
		/// Does a timed ramp, where the time is scaled proportional to the 
		/// remaining range to cover
		/// </summary>
		/// <param name="sig">Ushort sig to scale</param>
		/// <param name="newLevel">Level to go to</param>
		/// <param name="time">In ms (not hundredths like Crestron Sig ramp function)</param>
  /// <summary>
  /// RampTimeScaled method
  /// </summary>
		public static void RampTimeScaled(Sig sig, ushort newLevel, uint time)
		{
			ushort level = sig.UShortValue;
			int diff = Math.Abs(level - newLevel);
			uint scaledTime = (uint)(diff * time / 65535);
			Ramp(sig, newLevel, scaledTime);
		}

		/// <summary>
		/// Ramps signal
		/// </summary>
		/// <param name="sig"></param>
		/// <param name="level"></param>
		/// <param name="time">In ms (not hundredths like Crestron Sig ramp function)</param>
  /// <summary>
  /// Ramp method
  /// </summary>
		public static void Ramp(Sig sig, ushort level, uint time)
		{
			sig.CreateRamp(level, time / 10);
		}
	}
}