using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

using PepperDash.Core;

namespace PepperDash.Essentials.Core.Touchpanels
{
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
        }

        void _Touchpanel_ButtonStateChange(GenericBase device, Crestron.SimplSharpPro.DeviceSupport.ButtonEventArgs args)
        {
            Debug.Console(1, this, "Button {0}, {1}", args.Button.Number, args.NewButtonState);
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
            if (!_Buttons.ContainsKey(number)) { return; }
            var but = _Buttons[number];
            if (but.EventTypes.ContainsKey(type))
            {
                foreach (var a in but.EventTypes[type]) { DeviceJsonApi.DoDeviceAction(a); }
            }
        }


    }

    /// <summary>
    /// 
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
        public string Type { get; set; }
        public string LinkToKey { get; set; }
    }
}