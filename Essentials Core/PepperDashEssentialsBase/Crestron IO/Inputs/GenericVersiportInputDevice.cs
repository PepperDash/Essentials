using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace PepperDash.Essentials.Core.Crestron_IO
{
    /// <summary>
    /// Represents a generic digital input deviced tied to a versiport
    /// </summary>
    public class GenericVersiportInputDevice : IDigitalInput
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

        public GenericVersiportInputDevice(Versiport inputPort)
        {
            InputStateFeedback = new BoolFeedback(InputStateFeedbackFunc);

            InputPort = inputPort;

            InputPort.SetVersiportConfiguration(eVersiportConfiguration.DigitalInput);

            InputPort.VersiportChange += new VersiportEventHandler(InputPort_VersiportChange);

        }

        void InputPort_VersiportChange(Versiport port, VersiportEventArgs args)
        {
            InputStateFeedback.FireUpdate();
        }
    }

    public class GenericVersiportInputDeviceConfigProperties
    {

    }

}