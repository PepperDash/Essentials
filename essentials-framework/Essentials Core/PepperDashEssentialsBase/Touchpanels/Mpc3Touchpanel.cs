﻿using System.Collections.Generic;
using System.Globalization;
using Crestron.SimplSharpPro;
using Newtonsoft.Json;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.Touchpanels
{
	/// <summary>
	/// A wrapper class for the touchpanel portion of an MPC3 class process to allow for configurable
	/// behavior of the keybad buttons
	/// </summary>
	public class Mpc3TouchpanelController : Device
	{
		readonly MPC3Basic _touchpanel;

		readonly Dictionary<string, KeypadButton> _buttons;

		public Mpc3TouchpanelController(string key, string name, CrestronControlSystem processor, Dictionary<string, KeypadButton> buttons)
			: base(key, name)
		{
			_touchpanel = processor.ControllerTouchScreenSlotDevice as MPC3Basic;
			if (_touchpanel == null)
			{
				Debug.Console(1, this, "Failed to construct MPC3 Touchpanel Controller with key {0}, check configuration", key);
				return;
			}

			_touchpanel.ButtonStateChange += _touchpanel_ButtonStateChange;
			_buttons = buttons;
			if (_buttons == null)
			{
				Debug.Console(1, this,
					"Button properties are null, failed to setup MPC3 Touch Controller, check configuration");
				return;
			}

			AddPostActivationAction(() =>
			{
				foreach (var button in _buttons)
				{
					var buttonKey = button.Key.ToLower();
					var buttonConfig = button.Value;

					InitializeButton(buttonKey, buttonConfig);
					InitializeButtonFeedback(buttonKey, buttonConfig);
				}
			});
		}

		/// <summary>
		/// Enables/disables buttons based on event type configuration 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="config"></param>
		public void InitializeButton(string key, KeypadButton config)
		{
			if (config == null)
			{
				Debug.Console(1, this, "Button '{0}' config is null, unable to initialize", key);
				return;
			}

			int buttonNumber;
			TryParseInt(key, out buttonNumber);

			var buttonEventTypes = config.EventTypes;

			switch (key)
			{
				case ("power"):
					{
						if (buttonEventTypes == null)
							_touchpanel.DisablePowerButton();
						else
							_touchpanel.EnablePowerButton();

						break;
					}
				case ("volumeup"):
					{
						if (buttonEventTypes == null)
							_touchpanel.DisableVolumeUpButton();

						break;
					}
				case ("volumedown"):
					{
						if (buttonEventTypes == null)
							_touchpanel.DisableVolumeDownButton();

						break;
					}
				case ("volumefeedback"):
					{

						break;
					}
				case ("mute"):
					{
						if (buttonEventTypes == null)
							_touchpanel.DisableMuteButton();
						else
							_touchpanel.EnableMuteButton();

						break;
					}
				default:
					{
						if (buttonNumber == 0)
							break;

						if (buttonEventTypes == null)
							_touchpanel.DisableNumericalButton((uint)buttonNumber);
						else
							_touchpanel.EnableNumericalButton((uint)buttonNumber);

						break;
					}
			}

			Debug.Console(1, this, "Button '{0}' {1}", key, buttonEventTypes == null
				? "is disabled, verify eventTypes are configured."
				: "is enabled");
		}

		/// <summary>
		/// Links button feedback if configured
		/// </summary>
		/// <param name="key"></param>
		/// <param name="config"></param>
		public void InitializeButtonFeedback(string key, KeypadButton config)
		{
			if (config == null)
			{
				Debug.Console(1, this, "Button '{0}' config is null, unable to initialize feedback", key);
				return;
			}

			int buttonNumber;
			TryParseInt(key, out buttonNumber);

			// Link up the button feedbacks to the specified device feedback
			var buttonFeedback = config.Feedback;
			if (buttonFeedback == null)
			{
				Debug.Console(1, this, "Button '{0}' feedback not configured and will not be implemented. Verify feedback is configured if required.", key);
				return;
			}

			var device = DeviceManager.GetDeviceForKey(buttonFeedback.DeviceKey) as Device;
			if (device == null)
			{
				Debug.Console(1, this, "Button '{0}' feedback device with key '{0}' not found, feedback will not be implemented.  Verify feedback deviceKey is properly configured.",
					buttonFeedback.DeviceKey);
				return;
			}

			// TODO [ ] verify if this can replace the current method
			var deviceFeedback = device.GetFeedbackProperty(buttonFeedback.FeedbackName);
			Debug.Console(0, this, "deviceFeedback.GetType().Name: '{0}'", deviceFeedback.GetType().Name);
			//switch (feedback.GetType().Name.ToLower())
			//{
			//    case("boolfeedback"):
			//    {
			//        break;
			//    }
			//    case("intfeedback"):
			//    {
			//        break;
			//    }
			//    case("stringfeedback"):
			//    {
			//        break;
			//    }
			//}

			var boolFeedback = deviceFeedback as BoolFeedback;
			var intFeedback = deviceFeedback as IntFeedback;

			switch (key)
			{
				case ("power"):
					{
						if (boolFeedback != null) boolFeedback.LinkCrestronFeedback(_touchpanel.FeedbackPower);
						break;
					}
				case ("volumeup"):
				case ("volumedown"):
				case ("volumefeedback"):
					{
						if (intFeedback != null)
						{
							var volumeFeedback = intFeedback;
							volumeFeedback.LinkInputSig(_touchpanel.VolumeBargraph);
						}
						break;
					}
				case ("mute"):
					{
						if (boolFeedback != null) boolFeedback.LinkCrestronFeedback(_touchpanel.FeedbackMute);
						break;
					}
				default:
					{
						if (boolFeedback != null) boolFeedback.LinkCrestronFeedback(_touchpanel.Feedbacks[(uint)buttonNumber]);
						break;
					}
			}
		}

		/// <summary>
		/// Try parse int helper method
		/// </summary>
		/// <param name="str"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public bool TryParseInt(string str, out int result)
		{
			try
			{
				result = int.Parse(str);
				return true;
			}
			catch
			{
				result = 0;
				return false;
			}
		}

		private void _touchpanel_ButtonStateChange(GenericBase device, Crestron.SimplSharpPro.DeviceSupport.ButtonEventArgs args)
		{
			Debug.Console(1, this, "Button {0} ({1}), {2}", args.Button.Number, args.Button.Name, args.NewButtonState);
			var type = args.NewButtonState.ToString();

			if (_buttons.ContainsKey(args.Button.Number.ToString(CultureInfo.InvariantCulture)))
			{
				Press(args.Button.Number.ToString(CultureInfo.InvariantCulture), type);
			}
			else if (_buttons.ContainsKey(args.Button.Name.ToString()))
			{
				Press(args.Button.Name.ToString(), type);
			}
		}

		/// <summary>
		/// Runs the function associated with this button/type. One of the following strings:
		/// Pressed, Released, Tapped, DoubleTapped, Held, HeldReleased    
		/// </summary>
		/// <param name="buttonKey"></param>
		/// <param name="type"></param>
		public void Press(string buttonKey, string type)
		{
			// TODO: In future, consider modifying this to generate actions at device activation time
			//       to prevent the need to dynamically call the method via reflection on each button press
			if (!_buttons.ContainsKey(buttonKey)) return;

			var button = _buttons[buttonKey];
			if (!button.EventTypes.ContainsKey(type)) return;

			foreach (var eventType in button.EventTypes[type]) DeviceJsonApi.DoDeviceAction(eventType);
		}
	}

	/// <summary>
	/// Represents the configuration of a keypad button
	/// </summary>
	public class KeypadButton
	{
		[JsonProperty("eventTypes")]
		public Dictionary<string, DeviceActionWrapper[]> EventTypes { get; set; }

		[JsonProperty("feedback")]
		public KeypadButtonFeedback Feedback { get; set; }

		public KeypadButton()
		{
			EventTypes = new Dictionary<string, DeviceActionWrapper[]>();
			Feedback = new KeypadButtonFeedback();
		}
	}

	/// <summary>
	/// Represents the configuration of a keypad button feedback
	/// </summary>
	public class KeypadButtonFeedback
	{
		[JsonProperty("deviceKey")]
		public string DeviceKey { get; set; }

		[JsonProperty("feedbackName")]
		public string FeedbackName { get; set; }
	}
}