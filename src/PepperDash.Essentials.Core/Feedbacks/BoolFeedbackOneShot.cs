using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
namespace PepperDash.Essentials.Core
{
 /// <summary>
 /// Represents a BoolFeedbackPulse
 /// </summary>
	public class BoolFeedbackPulse
	{
  /// <summary>
  /// Gets or sets the TimeoutMs
  /// </summary>
		public uint TimeoutMs { get; set; }

  /// <summary>
  /// Gets or sets the CanRetrigger
  /// </summary>
		public bool CanRetrigger { get; set; }

  /// <summary>
  /// Gets or sets the Feedback
  /// </summary>
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
		/// Start method
		/// </summary>
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

  /// <summary>
  /// Cancel method
  /// </summary>
		public void Cancel()
		{
			if(Timer != null)
				Timer.Reset(0);
		}
	}
}