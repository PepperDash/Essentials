using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

using PepperDash.Core;

namespace PepperDash.Essentials.Core.CrestronIO
{
    /// <summary>
    /// Represents a generic digital input deviced tied to a versiport
    /// </summary>
    public class GenericVersiportDigitalInputDevice : Device, IDigitalInput
    {
        public Versiport InputPort { get; private set; }

        public BoolFeedback InputStateFeedback { get; private set; }

        Func<bool> InputStateFeedbackFunc
        {
            get
            {
                return () => InputPort.DigitalIn;
            }
        }

        public GenericVersiportDigitalInputDevice(string key, Versiport inputPort):
            base(key)
        {
            InputStateFeedback = new BoolFeedback(InputStateFeedbackFunc);
            InputPort = inputPort;
            InputPort.SetVersiportConfiguration(eVersiportConfiguration.DigitalInput);		
            InputPort.VersiportChange += new VersiportEventHandler(InputPort_VersiportChange);
        }

        void InputPort_VersiportChange(Versiport port, VersiportEventArgs args)
        {
			Debug.Console(1, this, "Versiport change: {0}", args.Event);
            InputStateFeedback.FireUpdate();
        }
    }
}