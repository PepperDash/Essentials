using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// A class that wraps a BoolFeedback with logic that extends it's true state for
	/// a time period after the value goes false.
	/// </summary>
	public class BoolFeedbackPulseExtender
	{
		/// <summary>
		/// Gets or sets the TimeoutMs
		/// </summary>
		public uint TimeoutMs { get; set; }

		/// <summary>
		/// Gets the Feedback
		/// </summary>
		public BoolFeedback Feedback { get; private set; }
		CTimer Timer;

		/// <summary>
		/// When set to true, will cause Feedback to go high, and cancel the timer.
		/// When false, will start the timer, and after timeout, will go low and 
		/// feedback will go low.
		/// </summary>
		public bool BoolValue
		{
			get { return _BoolValue; }
			set
			{
				if (value)
				{			// if Timer is running and the value goes high, cancel it.
					if (Timer != null)
					{
						Timer.Stop();
						Timer = null;
					}
					// if it's already true, don't fire again
					if (_BoolValue == true)
						return;
					_BoolValue = true;
					Feedback.FireUpdate();
				}
				else
				{
					if (Timer == null)
						Timer = new CTimer(o => ClearFeedback(), TimeoutMs);
				}
			}
		}
		bool _BoolValue;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="timeoutMs">The time which the true state will be extended after set to false</param>
		public BoolFeedbackPulseExtender(uint timeoutMs)
		{
			TimeoutMs = timeoutMs;
			Feedback = new BoolFeedback(() => this.BoolValue);
		}

		/// <summary>
		/// Forces the feedback to false regardless of timeout
		/// </summary>
		public void ClearNow()
		{
			if (Timer != null)
				Timer.Stop();
			ClearFeedback();
		}

		void ClearFeedback()
		{
			_BoolValue = false;
			Feedback.FireUpdate();
			Timer = null;
		}
	}
}