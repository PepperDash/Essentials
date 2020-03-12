using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Touchpanels
{
    /// <summary>
    /// A wrapper class for the touchpanel portion of an MPC3 class process to allow for configurable
    /// behavior of the keybad buttons
    /// </summary>
    public class Mpc3TouchpanelController : Device
    {
        MPC3Basic _Touchpanel;

        Dictionary<uint, KeypadButton> _Buttons;

        public Mpc3TouchpanelController(string key, string name, CrestronControlSystem processor, Dictionary<uint, KeypadButton> buttons)
            : base(key, name)
        {
            _Touchpanel = processor.ControllerTouchScreenSlotDevice as MPC3Basic;
            _Buttons = buttons;

            _Touchpanel.ButtonStateChange += new Crestron.SimplSharpPro.DeviceSupport.ButtonEventHandler(_Touchpanel_ButtonStateChange);


            AddPostActivationAction(() =>
                {
                    // Link up the button feedbacks to the specified BoolFeedbacks
                    foreach (var button in _Buttons)
                    {
                        var feedbackConfig = button.Value.Feedback;
                        var device = DeviceManager.GetDeviceForKey(feedbackConfig.DeviceKey) as Device;
                        if (device != null)
                        {
                            var feedback = device.GetFeedbackProperty(feedbackConfig.BoolFeedbackName) as BoolFeedback;
                            if (feedback != null)
                            {
                                // Link to the Crestron Feedback corresponding to the button number
                                feedback.LinkCrestronFeedback(_Touchpanel.Feedbacks[button.Key]);
                            }
                            else
                            {
                                Debug.Console(1, this, "Unable to get BoolFeedback with name: {0} from device: {1}", feedbackConfig.BoolFeedbackName, device.Key);
                            }
                        }
                        else
                        {
                            Debug.Console(1, this, "Unable to get device with key: {0}", feedbackConfig.DeviceKey);
                        }
                    }
                });
        }

        void _Touchpanel_ButtonStateChange(GenericBase device, Crestron.SimplSharpPro.DeviceSupport.ButtonEventArgs args)
        {
            Debug.Console(1, this, "Button {0} ({1}), {2}", args.Button.Number, args.Button.Name, args.NewButtonState);
            if (_Buttons.ContainsKey(args.Button.Number))
            {
                var type = args.NewButtonState.ToString();
                Press(args.Button.Number, type);
            }
        }

        /// <summary>
        /// Runs the function associated with this button/type. One of the following strings:
        /// Pressed, Released, Tapped, DoubleTapped, Held, HeldReleased    
        /// </summary>
        /// <param name="number"></param>
        /// <param name="type"></param>
        public void Press(uint number, string type)
        {
            // TODO: In future, consider modifying this to generate actions at device activation time
            //       to prevent the need to dynamically call the method via reflection on each button press
            if (!_Buttons.ContainsKey(number)) { return; }
            var but = _Buttons[number];
            if (but.EventTypes.ContainsKey(type))
            {
                foreach (var a in but.EventTypes[type]) { DeviceJsonApi.DoDeviceAction(a); }
            }
        }


    }

    /// <summary>
    /// Represents the configuration of a keybad buggon
    /// </summary>
    public class KeypadButton
    {
        public Dictionary<string, DeviceActionWrapper[]> EventTypes { get; set; }
        public KeypadButtonFeedback Feedback { get; set; }

        public KeypadButton()
        {
            EventTypes = new Dictionary<string, DeviceActionWrapper[]>();
            Feedback = new KeypadButtonFeedback();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class KeypadButtonFeedback
    {
        public string DeviceKey { get; set; }
        public string BoolFeedbackName { get; set; }
    }
}