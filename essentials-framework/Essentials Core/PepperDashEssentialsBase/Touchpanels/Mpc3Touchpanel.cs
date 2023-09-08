using System.Collections.Generic;
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
				Debug.Console(1, this, "Failed to construct {0}, check configuration", key);
		        return;
	        }
			
			_touchpanel.ButtonStateChange += _touchpanel_ButtonStateChange;
			_buttons = buttons;

	        AddPostActivationAction(() =>
	        {
		        // Link up the button feedbacks to the specified BoolFeedbacks
		        foreach (var button in _buttons)
		        {
					var buttonKey = button.Key.ToLower();
					var buttonConfig = button.Value;
					if (buttonConfig == null)
			        {
						Debug.Console(1, this, "Unable to get button config for {0}-{1}", Key, button.Key);
						continue;
			        }

					int buttonNumber;
					if (TryParseInt(buttonKey, out buttonNumber))
					{
						Debug.Console(0, this, "buttonFeedback: tryIntParse successful, buttonNumber = {0}", buttonNumber);
						_touchpanel.EnableNumericalButton((uint)buttonNumber);						
					}
					else
					{
						Debug.Console(0, this, "buttonFeedback: tryIntParse failed, buttonKey = {0}", buttonKey);
					}

			        //var buttonEventTypes = buttonConfig.EventTypes;

			        var buttonFeedback = buttonConfig.Feedback;
			        if (buttonFeedback == null)
			        {
				        Debug.Console(1, this, "Button '{0}' feedback not configured, feedback will not be implemented", buttonKey);
				        continue;
			        }
			        
					var device = DeviceManager.GetDeviceForKey(buttonFeedback.DeviceKey) as Device;
			        if (device == null)
			        {
				        Debug.Console(1, this, "Unable to get device with key {0}, feedback will not be implemented",
					        buttonFeedback.DeviceKey);
						continue;				       
			        }

					var deviceFeedback = device.GetFeedbackProperty(buttonFeedback.FeedbackName);
					Debug.Console(0, this, "deviceFeedback.GetType().Name: {0}", deviceFeedback.GetType().Name);
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
					//}

					var boolFeedback = deviceFeedback as BoolFeedback;
					var intFeedback = deviceFeedback as IntFeedback;

					switch (buttonKey)
					{
						case ("power"):
							{
								if (boolFeedback != null) boolFeedback.LinkCrestronFeedback(_touchpanel.FeedbackPower);
								break;
							}
						case ("volumeup"):
							{
								break;
							}
						case ("volumedown"):
							{
								break;
							}
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
	        });
        }

	    public bool TryParseInt(string str, out int result)
	    {
		    result = 0;

		    foreach (var c in str)
		    {
			    if(c < '0' || c > '9')
					return false;

			    result = result*10 + (c - '0');
		    }

		    return true;
	    }

        void _touchpanel_ButtonStateChange(GenericBase device, Crestron.SimplSharpPro.DeviceSupport.ButtonEventArgs args)
        {
            Debug.Console(1, this, "Button {0} ({1}), {2}", args.Button.Number, args.Button.Name, args.NewButtonState);
            var type = args.NewButtonState.ToString();

            if (_buttons.ContainsKey(args.Button.Number.ToString(CultureInfo.InvariantCulture)))
            {
                Press(args.Button.Number.ToString(CultureInfo.InvariantCulture), type);
            }
            else if(_buttons.ContainsKey(args.Button.Name.ToString()))
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