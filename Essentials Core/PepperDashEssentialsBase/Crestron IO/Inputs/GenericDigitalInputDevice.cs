using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace PepperDash.Essentials.Core.Crestron_IO
{
    public class GenericDigitalInputDevice : IDigitalInput
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

        public GenericDigitalInputDevice(DigitalInput inputPort)
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