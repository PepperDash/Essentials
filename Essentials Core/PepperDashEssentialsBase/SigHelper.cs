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
		/// <param name="sig"></param>
		public static void Pressed(Sig sig, Action act) { if (sig.BoolValue) act(); }

		/// <summary>
		/// Runs action when Sig is released
		/// </summary>
		public static void Released(Sig sig, Action act) { if (!sig.BoolValue) act(); }

		/// <summary>
		/// Safely sets an action to non-null sig
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
		public static void Ramp(Sig sig, ushort level, uint time)
		{
			sig.CreateRamp(level, time / 10);
		}
	}

	/// <summary>
	/// Attaches to UShortInputSig and does incremental ramping of the signal 
	/// </summary>
	public class UshortSigIncrementer
	{
		UShortInputSig TheSig;
		public ushort ChangeAmount { get; set; }
		public int MaxValue { get; set; }
		public int MinValue { get; set; }
		public uint RepeatDelay { get; set; }
		public uint RepeatTime { get; set; }
		bool SignedMode;
		CTimer Timer;

		public UshortSigIncrementer(UShortInputSig sig, ushort changeAmount, int minValue, int maxValue, uint repeatDelay, uint repeatTime)
		{
			TheSig = sig;
			ChangeAmount = changeAmount;
			MaxValue = maxValue;
			MinValue = minValue;
			if (MinValue < 0 || MaxValue < 0) SignedMode = true;
			RepeatDelay = repeatDelay;
			RepeatTime = repeatTime;
			if (SignedMode && (MinValue < -32768 || MaxValue > 32767))
				Debug.Console(1, "UshortSigIncrementer has signed values that exceed range of -32768, 32767");
		}

		public void StartUp()
		{
			if (Timer != null) return;
			Go(ChangeAmount);
		}

		public void StartDown()
		{
			if (Timer != null) return;
			Go(-ChangeAmount);
		}

		void Go(int change)
		{
			int level;
			if (SignedMode) level = TheSig.ShortValue;
			else level = TheSig.UShortValue;

			// Fire once then pause
			int newLevel = level + change;
			bool atLimit = CheckLevel(newLevel, out newLevel);
			SetValue((ushort)newLevel);


			if (atLimit) // Don't go past end
				Stop();
			else if (Timer == null) // Only enter the timer if it's not already running
				Timer = new CTimer(o => { Go(change); }, null, RepeatDelay, RepeatTime);
		}

		bool CheckLevel(int levelIn, out int levelOut)
		{
			bool IsAtLimit = false;
			if (levelIn > MaxValue)
			{
				levelOut = MaxValue;
				IsAtLimit = true;
			}
			else if (levelIn < MinValue)
			{
				levelOut = MinValue;
				IsAtLimit = true;
			}
			else
				levelOut = levelIn;
			return IsAtLimit;
		}

		public void Stop()
		{
			if (Timer != null) 
				Timer.Stop();
			Timer = null;
		}

		void SetValue(ushort value)
		{
			//CrestronConsole.PrintLine("Increment level:{0} / {1}", value, (short)value);
			TheSig.UShortValue = value;
		}
	}
}