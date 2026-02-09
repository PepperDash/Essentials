using System;
using System.Collections.Generic;
using System.Globalization;
using Crestron.SimplSharpPro;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using Serilog.Events;

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

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="key">device key</param>
		/// <param name="name">device name</param>
		/// <param name="processor">control system processor</param>
		/// <param name="buttons">dictionary of keypad buttons</param>
		public Mpc3TouchpanelController(string key, string name, CrestronControlSystem processor, Dictionary<string, KeypadButton> buttons)
			: base(key, name)
		{
			_touchpanel = processor.ControllerTouchScreenSlotDevice as MPC3Basic;
			if (_touchpanel == null)
			{
				Debug.LogMessage(LogEventLevel.Debug, this, "Failed to construct MPC3 Touchpanel Controller with key {0}, check configuration", key);
				return;
			}

			if (_touchpanel.Registerable)
			{
				var registrationResponse = _touchpanel.Register();
				Debug.LogMessage(LogEventLevel.Information, this, "touchpanel registration response: {0}", registrationResponse);
			}

			_touchpanel.BaseEvent += Touchpanel_BaseEvent;
			_touchpanel.ButtonStateChange += Touchpanel_ButtonStateChange;
			_touchpanel.PanelStateChange += Touchpanel_PanelStateChange;

			_buttons = buttons;
			if (_buttons == null)
			{
				Debug.LogMessage(LogEventLevel.Debug, this,
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

				ListButtons();
			});
		}

		/// <summary>
		/// Enables/disables buttons based on event type configuration 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="config"></param>
  /// <summary>
  /// InitializeButton method
  /// </summary>
		public void InitializeButton(string key, KeypadButton config)
		{
			if (config == null)
			{
				Debug.LogMessage(LogEventLevel.Debug, this, "Button '{0}' config is null, unable to initialize", key);
				return;
			}

            TryParseInt(key, out int buttonNumber);

            var buttonEventTypes = config.EventTypes;
			BoolOutputSig enabledFb = null;
			BoolOutputSig disabledFb = null;

			switch (key)
			{
				case ("power"):
					{
						if (buttonEventTypes == null || buttonEventTypes.Keys == null)
							_touchpanel.DisablePowerButton();
						else
							_touchpanel.EnablePowerButton();


						enabledFb = _touchpanel.PowerButtonEnabledFeedBack;
						disabledFb = _touchpanel.PowerButtonDisabledFeedBack;

						break;
					}
				//case ("volumeup"):
				//    {
				//        break;
				//    }
				//case ("volumedown"):
				//    {
				//        break;
				//    }
				//case ("volumefeedback"):
				//    {
				//        break;
				//    }
				case ("mute"):
					{
						if (buttonEventTypes == null || buttonEventTypes.Keys == null)
							_touchpanel.DisableMuteButton();
						else
							_touchpanel.EnableMuteButton();


						enabledFb = _touchpanel.MuteButtonEnabledFeedBack;
						disabledFb = _touchpanel.MuteButtonDisabledFeedBack;

						break;
					}
				default:
					{
						if (buttonNumber == 0 || buttonNumber > 9)
							break;

						if (buttonEventTypes == null || buttonEventTypes.Keys == null)
							_touchpanel.DisableNumericalButton((uint)buttonNumber);
						else
							_touchpanel.EnableNumericalButton((uint)buttonNumber);


						if (_touchpanel.NumericalButtonEnabledFeedBack != null)
							enabledFb = _touchpanel.NumericalButtonEnabledFeedBack[(uint)buttonNumber];

						if (_touchpanel.NumericalButtonDisabledFeedBack != null)
							disabledFb = _touchpanel.NumericalButtonDisabledFeedBack[(uint)buttonNumber];

						break;
					}
			}

			Debug.LogMessage(LogEventLevel.Information, this, "InitializeButton: key-'{0}' enabledFb-'{1}', disabledFb-'{2}'",
				key, enabledFb ?? (object)"null", disabledFb ?? (object)"null");
		}

		/// <summary>
		/// Links button feedback if configured
		/// </summary>
		/// <param name="key"></param>
		/// <param name="config"></param>
  /// <summary>
  /// InitializeButtonFeedback method
  /// </summary>
		public void InitializeButtonFeedback(string key, KeypadButton config)
		{
			//Debug.LogMessage(LogEventLevel.Debug, this, "Initializing button '{0}' feedback...", key);

			if (config == null)
			{
				Debug.LogMessage(LogEventLevel.Debug, this, "Button '{0}' config is null, skipping.", key);
				return;
			}

            TryParseInt(key, out int buttonNumber);

            // Link up the button feedbacks to the specified device feedback
            var buttonFeedback = config.Feedback;
			if (buttonFeedback == null || string.IsNullOrEmpty(buttonFeedback.DeviceKey))
			{
				Debug.LogMessage(LogEventLevel.Debug, this, "Button '{0}' feedback not configured, skipping.",
					key);
				return;
			}

			Feedback deviceFeedback;

			try
			{
                if (!(DeviceManager.GetDeviceForKey(buttonFeedback.DeviceKey) is Device device))
                {
                    Debug.LogMessage(LogEventLevel.Debug, this, "Button '{0}' feedback deviceKey '{1}' not found.",
                        key, buttonFeedback.DeviceKey);
                    return;
                }

                deviceFeedback = device.GetFeedbackProperty(buttonFeedback.FeedbackName);
				if (deviceFeedback == null)
				{
					Debug.LogMessage(LogEventLevel.Debug, this, "Button '{0}' feedbackName property '{1}' not found.",
						key, buttonFeedback.FeedbackName);
					return;
				}

				// TODO [ ] verify if this can replace the current method
				//Debug.LogMessage(LogEventLevel.Information, this, "deviceFeedback.GetType().Name: '{0}'", deviceFeedback.GetType().Name);
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
			}
			catch (Exception ex)
			{
				Debug.LogMessage(LogEventLevel.Debug, this, "InitializeButtonFeedback (button '{1}', deviceKey '{2}') Exception Message: {0}",
					ex.Message, key, buttonFeedback.DeviceKey);
				Debug.LogMessage(LogEventLevel.Verbose, this, "InitializeButtonFeedback (button '{1}', deviceKey '{2}') Exception StackTrace: {0}",
					ex.StackTrace, key, buttonFeedback.DeviceKey);
				if (ex.InnerException != null) Debug.LogMessage(LogEventLevel.Verbose, this, "InitializeButtonFeedback (button '{1}', deviceKey '{2}') InnerException: {0}",
					ex.InnerException, key, buttonFeedback.DeviceKey);

				return;
			}

			var boolFeedback = deviceFeedback as BoolFeedback;

            switch (key)
            {
                case ("power"):
                    {
                        boolFeedback?.LinkCrestronFeedback(_touchpanel.FeedbackPower);
                        break;
                    }
                case ("volumeup"):
                case ("volumedown"):
                case ("volumefeedback"):
                    {
                        if (deviceFeedback is IntFeedback intFeedback)
                        {
                            var volumeFeedback = intFeedback;
                            volumeFeedback.LinkInputSig(_touchpanel.VolumeBargraph);
                        }
                        break;
                    }
                case ("mute"):
                    {
                        boolFeedback?.LinkCrestronFeedback(_touchpanel.FeedbackMute);
                        break;
                    }
                default:
                    {
                        boolFeedback?.LinkCrestronFeedback(_touchpanel.Feedbacks[(uint)buttonNumber]);
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

		private void Touchpanel_BaseEvent(GenericBase device, BaseEventArgs args)
		{
			Debug.LogMessage(LogEventLevel.Debug, this, "BaseEvent: eventId-'{0}', index-'{1}'", args.EventId, args.Index);
		}

		private void Touchpanel_ButtonStateChange(GenericBase device, Crestron.SimplSharpPro.DeviceSupport.ButtonEventArgs args)
		{
			Debug.LogMessage(LogEventLevel.Debug, this, "ButtonStateChange: buttonNumber-'{0}' buttonName-'{1}', buttonState-'{2}'", args.Button.Number, args.Button.Name, args.NewButtonState);
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

		private void Touchpanel_PanelStateChange(GenericBase device, BaseEventArgs args)
		{
			Debug.LogMessage(LogEventLevel.Debug, this, "PanelStateChange: eventId-'{0}', index-'{1}'", args.EventId, args.Index);
		}

		/// <summary>
		/// Runs the function associated with this button/type. One of the following strings:
		/// Pressed, Released, Tapped, DoubleTapped, Held, HeldReleased    
		/// </summary>
		/// <param name="buttonKey"></param>
		/// <param name="type"></param>
  /// <summary>
  /// Press method
  /// </summary>
		public void Press(string buttonKey, string type)
		{
			this.LogVerbose("Press: buttonKey-'{buttonKey}', type-'{type}'", buttonKey, type);

			// TODO: In future, consider modifying this to generate actions at device activation time
			//       to prevent the need to dynamically call the method via reflection on each button press
			if (!_buttons.ContainsKey(buttonKey)) return;

			var button = _buttons[buttonKey];
			if (!button.EventTypes.ContainsKey(type)) return;

			foreach (var eventType in button.EventTypes[type]) DeviceJsonApi.DoDeviceAction(eventType);
		}


  /// <summary>
  /// ListButtons method
  /// </summary>
		public void ListButtons()
		{
			this.LogVerbose("MPC3 Controller {0} - Available Buttons", Key);

			foreach (var button in _buttons)
			{
				this.LogVerbose("Key: {key}", button.Key);
			}
		}
	}

 /// <summary>
 /// Represents a KeypadButton
 /// </summary>
	public class KeypadButton
	{
		/// <summary>
		/// Gets or sets the EventTypes
		/// </summary>
		[JsonProperty("eventTypes")]
		public Dictionary<string, DeviceActionWrapper[]> EventTypes { get; set; }

		/// <summary>
		/// Gets or sets the Feedback
		/// </summary>
		[JsonProperty("feedback")]
		public KeypadButtonFeedback Feedback { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public KeypadButton()
		{
			EventTypes = new Dictionary<string, DeviceActionWrapper[]>();
			Feedback = new KeypadButtonFeedback();
		}
	}

	/// <summary>
	/// Represents a KeypadButtonFeedback
	/// </summary>
	public class KeypadButtonFeedback
	{
		/// <summary>
		/// Gets or sets the DeviceKey
		/// </summary>
		[JsonProperty("deviceKey")]
		public string DeviceKey { get; set; }

		/// <summary>
		/// Gets or sets the FeedbackName
		/// </summary>
		[JsonProperty("feedbackName")]
		public string FeedbackName { get; set; }
	}
}