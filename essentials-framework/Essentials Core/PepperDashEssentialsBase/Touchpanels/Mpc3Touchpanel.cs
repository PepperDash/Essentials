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

        Dictionary<string, KeypadButton> _Buttons;

        public Mpc3TouchpanelController(string key, string name, CrestronControlSystem processor, Dictionary<string, KeypadButton> buttons)
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
                            var bKey = button.Key.ToLower();

                            var feedback = device.GetFeedbackProperty(feedbackConfig.BoolFeedbackName);

                            var bFeedback = feedback as BoolFeedback;
                            var iFeedback = feedback as IntFeedback;
                            if (bFeedback != null)
                            {

                                if (bKey == "power")
                                {
                                    bFeedback.LinkCrestronFeedback(_Touchpanel.FeedbackPower);
                                    continue;
                                }
                                else if (bKey == "mute")
                                {
                                    bFeedback.LinkCrestronFeedback(_Touchpanel.FeedbackMute);
                                    continue;
                                }

                                // Link to the Crestron Feedback corresponding to the button number
                                bFeedback.LinkCrestronFeedback(_Touchpanel.Feedbacks[UInt16.Parse(button.Key)]);
                            }
                            else if (iFeedback != null)
                            {
                                if (bKey == "volumefeedback")
                                {
                                    var volFeedback = feedback as IntFeedback;
                                    // TODO: Figure out how to subsribe to a volume IntFeedback and link it to the voluem
                                    volFeedback.LinkInputSig(_Touchpanel.VolumeBargraph);
                                }
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
            var type = args.NewButtonState.ToString();

            if (_Buttons.ContainsKey(args.Button.Number.ToString()))
            {
                Press(args.Button.Number.ToString(), type);
            }
            else if(_Buttons.ContainsKey(args.Button.Name.ToString()))
            {
                Press(args.Button.Name.ToString(), type);
            }
        }

        /// <summary>
        /// Runs the function associated with this button/type. One of the following strings:
        /// Pressed, Released, Tapped, DoubleTapped, Held, HeldReleased    
        /// </summary>
        /// <param name="number"></param>
        /// <param name="type"></param>
        public void Press(string number, string type)
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