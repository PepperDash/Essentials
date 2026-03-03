using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Represents a ModalDialog
	/// </summary>
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
		/// 3993
		/// </summary>
		public const uint CancelButtonJoin = 3993;
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
		/// Visibility join to show "X" button 3997
		/// </summary>
		public const uint CancelVisibleJoin = 3997;
		/// <summary>
		/// Shows the modal subpage. Boolean feeback join 3999
		/// </summary>
		public const uint ModalVisibleJoin = 3999;

		///// <summary>
		///// The seconds value of the countdown timer. Ushort join 3991
		///// </summary>
		//public const uint TimerSecondsJoin = 3991;
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

		/// <summary>
		/// 
		/// </summary>
		public bool CanCancel { get; private set; }


		BasicTriList TriList;

		Action<uint> ModalCompleteAction;

		static object CompleteActionLock = new object();

		/// <summary>
		/// Creates a new modal to be shown on provided TriList
		/// </summary>
		/// <param name="triList"></param>
		public ModalDialog(BasicTriList triList)
		{
			TriList = triList;
			// Attach actions to buttons

			triList.SetSigFalseAction(Button1Join, () => OnModalComplete(1));
			triList.SetSigFalseAction(Button2Join, () => OnModalComplete(2));
			triList.SetSigFalseAction(CancelButtonJoin, () => { if (CanCancel) CancelDialog(); });
			CanCancel = true;
		}

		/// <summary>
		/// Shows the dialog
		/// </summary>
		/// <param name="numberOfButtons">Number of buttons to show. 0, 1, 2</param>
		/// <param name="title">Title text</param>
		/// <param name="iconName">Icon name</param>
		/// <param name="message">Message text</param>
		/// <param name="button1Text">Button 1 text</param>
		/// <param name="button2Text">Button 2 text</param>
		/// <param name="showGauge">True to show the gauge</param>
		/// <param name="showCancel">True to show the cancel "X" button</param>
		/// <param name="completeAction">The action to run when the dialog is dismissed. Parameter will be 1 or 2 if button pressed, or 0 if dialog times out</param>
		/// <returns>True when modal is created.</returns>
		public bool PresentModalDialog(uint numberOfButtons, string title, string iconName,
			string message, string button1Text,
			string button2Text, bool showGauge, bool showCancel, Action<uint> completeAction)
		{
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
				// Show/hide guage
				TriList.BooleanInput[TimerVisibleJoin].BoolValue = showGauge;

				CanCancel = showCancel;
				TriList.BooleanInput[CancelVisibleJoin].BoolValue = showCancel;

				//Reveal and activate
				TriList.BooleanInput[ModalVisibleJoin].BoolValue = true;

				WakePanel();

				return true;
			}

			return false;
		}

		/// <summary>
		/// WakePanel method
		/// </summary>
		public void WakePanel()
		{
			try
			{
				var panel = TriList as TswFt5Button;

				if (panel != null && panel.ExtenderSystemReservedSigs.BacklightOffFeedback.BoolValue)
					panel.ExtenderSystemReservedSigs.BacklightOn();
			}
			catch
			{
				Debug.LogMessage(LogEventLevel.Debug, "Error Waking Panel.  Maybe testing with Xpanel?");
			}
		}

		/// <summary>
		/// CancelDialog method
		/// </summary>
		public void CancelDialog()
		{
			OnModalComplete(0);
		}

		/// <summary>
		/// Hides dialog. Fires no action
		/// </summary>
		public void HideDialog()
		{
			TriList.BooleanInput[ModalVisibleJoin].BoolValue = false;
		}

		// When the modal is cleared or times out, clean up the various bits
		void OnModalComplete(uint buttonNum)
		{
			TriList.BooleanInput[ModalVisibleJoin].BoolValue = false;

			var action = ModalCompleteAction;
			if (action != null)
				action(buttonNum);
		}
	}

}