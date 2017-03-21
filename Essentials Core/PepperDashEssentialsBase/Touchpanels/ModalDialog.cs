using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core
{
	public class ModalDialog
	{
		/// <summary>
		/// Bool press 3991
		/// </summary>
		public const uint Button1Join = 3991;
		/// <summary>
		/// Bool press 3992
		/// </summary>
		public const uint Button2Join = 3992;
		/// <summary>
		///For visibility of single button.  Bool feedback 3994
		/// </summary>
		public const uint OneButtonVisibleJoin = 3994;
		/// <summary>
		/// For visibility of two buttons. Bool feedback 3995.
		/// </summary>
		public const uint TwoButtonVisibleJoin = 3995;
		/// <summary>
		/// Shows the timer guage if in use. Bool feedback 3996
		/// </summary>
		public const uint TimerVisibleJoin = 3996;
		/// <summary>
		/// Shows the modal subpage. Boolean feeback join 3999
		/// </summary>
		public const uint ModalVisibleJoin = 3999;

		/// <summary>
		/// The seconds value of the countdown timer. Ushort join 3991
		/// </summary>
		public const uint TimerSecondsJoin = 3991;
		/// <summary>
		/// The full ushort value of the countdown timer for a gauge. Ushort join 3992
		/// </summary>
		public const uint TimerGaugeJoin = 3992;

		/// <summary>
		/// Text on button one. String join 3991
		/// </summary>
		public const uint Button1TextJoin = 3991;
		/// <summary>
		/// Text on button two. String join 3992
		/// </summary>
		public const uint Button2TextJoin = 3992;
		/// <summary>
		/// Message text. String join 3994
		/// </summary>
		public const uint MessageTextJoin = 3994;
		/// <summary>
		/// Title text. String join 3995
		/// </summary>
		public const uint TitleTextJoin = 3995;
		/// <summary>
		/// Icon name. String join 3996
		/// </summary>
		public const uint IconNameJoin = 3996;

		/// <summary>
		/// Returns true when modal is showing
		/// </summary>
		public bool ModalIsVisible 
		{ 
			get { return TriList.BooleanInput[ModalVisibleJoin].BoolValue; } 
		}


		BasicTriList TriList;

		Action<uint> ModalCompleteAction;
		CTimer Timer;

		static object CompleteActionLock = new object();

		/// <summary>
		/// Creates a new modal to be shown on provided TriList
		/// </summary>
		/// <param name="triList"></param>
		public ModalDialog(BasicTriList triList)
		{
			TriList = triList;
			// Attach actions to buttons
			triList.SetSigFalseAction(Button1Join, () => OnModalComplete(1, true));
			triList.SetSigFalseAction(Button2Join, () => OnModalComplete(2, true));
		}

		/// <summary>
		/// Shows the dialog
		/// </summary>
		/// <param name="numberOfButtons">Number of buttons to show. 0, 1, 2</param>
		/// <param name="timeMs">The amount of time to show the dialog. Use 0 for no timeout.</param>
		/// <param name="decreasingGauge">If the progress bar gauge needs to count down instead of up</param>
		/// <param name="completeAction">The action to run when the dialog is dismissed. Parameter will be 1 or 2 if button pressed, or 0 if dialog times out</param>
		/// <returns>True when modal is created.</returns>
		public bool PresentModalTimerDialog(uint numberOfButtons, string title, string iconName, 
			string message, string button1Text,
			string button2Text, uint timeMs, bool decreasingGauge, Action<uint> completeAction)
		{
			//Debug.Console(0, "Present dialog");
			// Don't reset dialog if visible now
			if (!ModalIsVisible)
			{
				ModalCompleteAction = completeAction;
				TriList.StringInput[TitleTextJoin].StringValue = title;
				if (string.IsNullOrEmpty(iconName)) iconName = "Blank";
				TriList.StringInput[IconNameJoin].StringValue = iconName;
				TriList.StringInput[MessageTextJoin].StringValue = message;
				if (numberOfButtons == 0)
				{
					// Show no buttons
					TriList.BooleanInput[OneButtonVisibleJoin].BoolValue = false;
					TriList.BooleanInput[TwoButtonVisibleJoin].BoolValue = false;
				}
				else if (numberOfButtons == 1)
				{
					// Show one button
					TriList.BooleanInput[OneButtonVisibleJoin].BoolValue = true;
					TriList.BooleanInput[TwoButtonVisibleJoin].BoolValue = false;
					TriList.StringInput[Button1TextJoin].StringValue = button1Text;
				}
				else if (numberOfButtons == 2)
				{
					// Show two
					TriList.BooleanInput[OneButtonVisibleJoin].BoolValue = false;
					TriList.BooleanInput[TwoButtonVisibleJoin].BoolValue = true;
					TriList.StringInput[Button1TextJoin].StringValue = button1Text;
					TriList.StringInput[Button2TextJoin].StringValue = button2Text;
				}
				// Show/hide timer
				TriList.BooleanInput[TimerVisibleJoin].BoolValue = timeMs > 0;

				//Reveal and activate
				TriList.BooleanInput[ModalVisibleJoin].BoolValue = true;

				// Start ramp timers if visible
				if (timeMs > 0)
				{
					TriList.UShortInput[TimerSecondsJoin].UShortValue = (ushort)(timeMs / 1000); // Seconds display
					TriList.UShortInput[TimerSecondsJoin].CreateRamp(0, (uint)(timeMs / 10));
					if (decreasingGauge)
					{
						// Gauge
						TriList.UShortInput[TimerGaugeJoin].UShortValue = ushort.MaxValue; 
						// Text
						TriList.UShortInput[TimerGaugeJoin].CreateRamp(0, (uint)(timeMs / 10));
					}
					else
					{
						TriList.UShortInput[TimerGaugeJoin].UShortValue = 0; // Gauge
						TriList.UShortInput[TimerGaugeJoin].
							CreateRamp(ushort.MaxValue, (uint)(timeMs / 10));
					}
					Timer = new CTimer(o => OnModalComplete(0, false), timeMs);
				}

				// Start a timer and fire action with no button on timeout.
				return true;
			}

			// Dialog is busy
			//Debug.Console(2, "Modal is already visible");
			return false;
		}

		public void CancelDialog()
		{
			if (ModalIsVisible)
			{
				TriList.UShortInput[TimerSecondsJoin].StopRamp();
				TriList.UShortInput[TimerGaugeJoin].StopRamp();
				if (Timer != null) Timer.Stop();
				TriList.BooleanInput[ModalVisibleJoin].BoolValue = false;
			}
		}

		// When the modal is cleared or times out, clean up the various bits
		void OnModalComplete(uint buttonNum, bool cancelled)
		{
			//Debug.Console(2, "OnModalComplete {0}, {1}", buttonNum, cancelled);
			TriList.BooleanInput[ModalVisibleJoin].BoolValue = false;
			if (cancelled)
			{
				TriList.UShortInput[TimerSecondsJoin].StopRamp();
				TriList.UShortInput[TimerGaugeJoin].StopRamp();
				Timer.Stop();
			}
			if (ModalCompleteAction != null)
			{
				//Debug.Console(2, "Modal complete action");
				ModalCompleteAction(buttonNum);
			}
		}
	}

}