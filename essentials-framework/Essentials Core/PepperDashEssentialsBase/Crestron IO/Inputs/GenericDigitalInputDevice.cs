using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

using PepperDash.Core;

namespace PepperDash.Essentials.Core.CrestronIO
{
    public class GenericDigitalInputDevice : Device, IDigitalInput
    {
        public DigitalInput InputPort { get; private set; }

        public BoolFeedback InputStateFeedback { get; private set; }

        Func<bool> InputStateFeedbackFunc
        {
            get
            {
                return () => InputPort.State;
            }
        }

        public GenericDigitalInputDevice(string key, DigitalInput inputPort):
            base(key)
        {
            InputStateFeedback = new BoolFeedback(InputStateFeedbackFunc);

            InputPort = inputPort;

            InputPort.StateChange += new DigitalInputEventHandler(InputPort_StateChange);

        }

        void InputPort_StateChange(DigitalInput digitalInput, DigitalInputEventArgs args)
        {
            InputStateFeedback.FireUpdate();
        }

    }
}