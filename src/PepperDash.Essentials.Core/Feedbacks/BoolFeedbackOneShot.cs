using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
namespace PepperDash.Essentials.Core
{
	public class BoolFeedbackPulse
	{
		public uint TimeoutMs { get; set; }

		/// <summary>
		/// Defaults to false
		/// </summary>
		public bool CanRetrigger { get; set; }

		public BoolFeedback Feedback { get; private set; }
		CTimer Timer;

		bool _BoolValue;

		/// <summary>
		/// Creates a non-retriggering one shot
		/// </summary>
		public BoolFeedbackPulse(uint timeoutMs)
			: this(timeoutMs, false)
		{
		}

		/// <summary>
		/// Create a retriggerable one shot by setting canRetrigger true
		/// </summary>
		public BoolFeedbackPulse(uint timeoutMs, bool canRetrigger)
		{
			TimeoutMs = timeoutMs;
			CanRetrigger = canRetrigger;
			Feedback = new BoolFeedback(() => _BoolValue);
		}

		/// <summary>
		/// Starts the 
		/// </summary>
		/// <param name="timeout"></param>
		public void Start()
		{
			if (Timer == null)
			{
				_BoolValue = true;
				Feedback.FireUpdate();
				Timer = new CTimer(o =>
					{
						_BoolValue = false;
						Feedback.FireUpdate();
						Timer = null;
					}, TimeoutMs);
			}
			// Timer is running, if retrigger is set, reset it.
			else if (CanRetrigger)
				Timer.Reset(TimeoutMs);
		}

		public void Cancel()
		{
			if(Timer != null)
				Timer.Reset(0);
		}
	}
}