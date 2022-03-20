using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharp;
using PepperDash.Essentials.Devices.Common.Codec;
using Crestron.SimplSharpPro.CrestronThread;

namespace PepperDash.Essentials.Devices.Common.DSP
{
	public class QscDspDialer : DspDialerBase, IHasDialer
	{
		public QscDialerConfig Tags;
		public bool IsInCall { get; private set; }
		public QscDsp Parent { get; private set; }
		public string DialString { get; private set; }
		public bool OffHook { get; private set; }
		public bool AutoAnswerState { get; private set; }
		public bool DoNotDisturbState { get; private set; }

		public BoolFeedback OffHookFeedback;
		public BoolFeedback AutoAnswerFeedback;
		public BoolFeedback DoNotDisturbFeedback;
		public StringFeedback DialStringFeedback;

		// Add requirements for Dialer functionality

		public QscDspDialer(QscDialerConfig Config, QscDsp parent)
		{
			Tags = Config;
			Parent = parent;
			DialStringFeedback = new StringFeedback(() => { return DialString; });
			OffHookFeedback = new BoolFeedback(() => { return OffHook; });
			AutoAnswerFeedback = new BoolFeedback(() => { return AutoAnswerState; });
			DoNotDisturbFeedback = new BoolFeedback(() => { return DoNotDisturbState; });
		}

		public event EventHandler<CodecCallStatusItemChangeEventArgs> CallStatusChange;
	
		public void Subscribe()
        {
			try
			{
				// Do subscriptions and blah blah
				// This would be better using reflection JTA 2018-08-28
				PropertyInfo[] properties = Tags.GetType().GetCType().GetProperties();
				//GetPropertyValues(Tags);

				Debug.Console(2, "QscDspDialer Subscribe");
				foreach (var prop in properties)
				{
					//var val = prop.GetValue(obj, null);

					
					Debug.Console(2, "Property {0}, {1}, {2}\n", prop.GetType().Name, prop.Name, prop.PropertyType.FullName);
					if (prop.Name.Contains("Tag") && !prop.Name.Contains("keypad"))
					{
						var propValue = prop.GetValue(Tags, null) as string;
						Debug.Console(2, "Property {0}, {1}, {2}\n", prop.GetType().Name, prop.Name, propValue);
						SendSubscriptionCommand(propValue, "1");
					}


				}
			}
			catch (Exception e)
			{
				
					Debug.Console(2, "QscDspDialer Subscription Error: '{0}'\n", e);
			}
			 
 
			// SendSubscriptionCommand(, "1");
            // SendSubscriptionCommand(config. , "mute", 500);
        }
		public void ParseSubscriptionMessage(string customName, string value)
		{

			// Check for valid subscription response
			Debug.Console(1, "QscDialerTag {0} Response: '{1}'", customName, value);
			if (customName == Tags.dialStringTag)
			{
				Debug.Console(2, "QscDialerTag DialStringChanged ", value);
				this.DialString = value;
				this.DialStringFeedback.FireUpdate();
			}
			else if (customName == Tags.doNotDisturbTag)
			{
				if (value == "on")
				{
					this.DoNotDisturbState = true;
				}
				else if (value == "off")
				{
					this.DoNotDisturbState = false;
				}
				DoNotDisturbFeedback.FireUpdate();
			}
			else if (customName == Tags.callStatusTag)
			{
				if (value == "Dialing")
				{
					this.OffHook = true;
				}
				else if (value == "Disconnected")
				{
					this.OffHook = false;
					if (Tags.ClearOnHangup)
					{
						this.SendKeypad(eKeypadKeys.Clear);
					}
				}
				this.OffHookFeedback.FireUpdate();
			}
			else if (customName == Tags.autoAnswerTag)
			{
				if (value == "on")
				{
					this.AutoAnswerState = true;
				}
				else if (value == "off")
				{
					this.AutoAnswerState = false;
				}
				AutoAnswerFeedback.FireUpdate();
			}
			else if (customName == Tags.hookStatusTag)
			{
				if (value == "true")
				{
					this.OffHook = true;
				}
				else if (value == "false")
				{
					this.OffHook = false;
				}
				this.OffHookFeedback.FireUpdate();
			}


		}

		public void DoNotDisturbToggle()
		{
			int dndStateInt = !DoNotDisturbState ? 1 : 0;
			Parent.SendLine(string.Format("csv {0} {1}", Tags.doNotDisturbTag, dndStateInt));
		}
		public void DoNotDisturbOn()
		{
			Parent.SendLine(string.Format("csv {0} 1", Tags.doNotDisturbTag));
		}
		public void DoNotDisturbOff()
		{
			Parent.SendLine(string.Format("csv {0} 0", Tags.doNotDisturbTag));
		}
		public void AutoAnswerToggle()
		{
			int autoAnswerStateInt = !AutoAnswerState ? 1 : 0;
			Parent.SendLine(string.Format("csv {0} {1}", Tags.autoAnswerTag, autoAnswerStateInt));
		}
		public void AutoAnswerOn()
		{
			Parent.SendLine(string.Format("csv {0} 1", Tags.autoAnswerTag));
		}
		public void AutoAnswerOff()
		{
			Parent.SendLine(string.Format("csv {0} 0", Tags.autoAnswerTag));
		}

		private void PollKeypad()
		{
			Thread.Sleep(50);
			Parent.SendLine(string.Format("cg {0}", Tags.dialStringTag));
		}

		public void SendKeypad(eKeypadKeys button)
		{
			string keypadTag = null;
			switch (button)
			{
				case eKeypadKeys.Num0: keypadTag = Tags.keypad0Tag; break;
				case eKeypadKeys.Num1: keypadTag = Tags.keypad1Tag; break;
				case eKeypadKeys.Num2: keypadTag = Tags.keypad2Tag; break;
				case eKeypadKeys.Num3: keypadTag = Tags.keypad3Tag; break;
				case eKeypadKeys.Num4: keypadTag = Tags.keypad4Tag; break;
				case eKeypadKeys.Num5: keypadTag = Tags.keypad5Tag; break;
				case eKeypadKeys.Num6: keypadTag = Tags.keypad6Tag; break;
				case eKeypadKeys.Num7: keypadTag = Tags.keypad7Tag; break;
				case eKeypadKeys.Num8: keypadTag = Tags.keypad8Tag; break;
				case eKeypadKeys.Num9: keypadTag = Tags.keypad9Tag; break;
				case eKeypadKeys.Pound: keypadTag = Tags.keypadPoundTag; break;
				case eKeypadKeys.Star: keypadTag = Tags.keypadStarTag; break;
				case eKeypadKeys.Backspace: keypadTag = Tags.keypadBackspaceTag; break;
				case eKeypadKeys.Clear: keypadTag = Tags.keypadClearTag; break;
			}
			if (keypadTag != null)
			{
				var cmdToSend = string.Format("ct {0}", keypadTag);
				Parent.SendLine(cmdToSend);
				PollKeypad();
			}
		}
		public void SendSubscriptionCommand(string instanceTag, string changeGroup)
        {
            // Subscription string format: InstanceTag subscribe attributeCode Index1 customName responseRate
            // Ex: "RoomLevel subscribe level 1 MyRoomLevel 500"
			
			var cmd = string.Format("cga {0} {1}", changeGroup, instanceTag);

            Parent.SendLine(cmd);
        }
		public void Dial()
		{
			if (!this.OffHook)
			{
				Parent.SendLine(string.Format("ct {0}", Tags.connectTag));
			}
			else
			{
				Parent.SendLine(string.Format("ct {0}", Tags.disconnectTag));
			}
			Thread.Sleep(50);
			Parent.SendLine(string.Format("cg {0}", Tags.callStatusTag));
		}
		public void Dial(string number)
		{
		}
		public void EndCall(CodecActiveCallItem activeCall)
		{
		}
		public void EndAllCalls()
		{
		}
		public void AcceptCall(CodecActiveCallItem item)
		{
		}

		public void RejectCall(CodecActiveCallItem item)
		{

		}

		public void SendDtmf(string digit)
		{

		}

		public enum eKeypadKeys
		{
			Num1,
			Num2,
			Num3,
			Num4,
			Num5,
			Num6,
			Num7,
			Num8,
			Num9,
			Num0,
			Star,
			Pound,
			Clear,
			Backspace
		}

	}
}