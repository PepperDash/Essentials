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
    public class GenericVersiportDigitalInputDevice : EssentialsDevice, IDigitalInput
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

        public GenericVersiportDigitalInputDevice(string key, Versiport inputPort, IOPortConfig props):
            base(key)
        {
            InputStateFeedback = new BoolFeedback(InputStateFeedbackFunc);
            InputPort = inputPort;
            InputPort.SetVersiportConfiguration(eVersiportConfiguration.DigitalInput);
            if (props.DisablePullUpResistor)
                InputPort.DisablePullUpResistor = true;
            InputPort.VersiportChange += new VersiportEventHandler(InputPort_VersiportChange);

            Debug.Console(1, this, "Created GenericVersiportDigitalInputDevice on port '{0}'.  DisablePullUpResistor: '{1}'", props.PortNumber, InputPort.DisablePullUpResistor);
        }

        void InputPort_VersiportChange(Versiport port, VersiportEventArgs args)
        {
			Debug.Console(1, this, "Versiport change: {0}", args.Event);

            if(args.Event == eVersiportEvent.DigitalInChange)
                InputStateFeedback.FireUpdate();
        }
    }
}