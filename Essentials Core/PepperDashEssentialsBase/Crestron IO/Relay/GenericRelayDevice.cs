using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace PepperDash.Essentials.Core.Crestron_IO
{
    /// <summary>
    /// Represents a generic device controlled by relays
    /// </summary>
    public class GenericRelayDevice
    {
        public Relay RelayOutput { get; private set; }

        public BoolFeedback RelayStateFeedback { get; private set; }

        Func<bool> RelayStateFeedbackFunc
        {
            get
            {
                return () => RelayOutput.State;
            }
        }

        public GenericRelayDevice(Relay relay)
        {
            RelayStateFeedback = new BoolFeedback(RelayStateFeedbackFunc);

            if (relay.AvailableForUse)
                RelayOutput = relay;

            RelayOutput.StateChange += new RelayEventHandler(RelayOutput_StateChange);
        }

        void RelayOutput_StateChange(Relay relay, RelayEventArgs args)
        {
            RelayStateFeedback.FireUpdate();
        }

        public void OpenRelay()
        {
            RelayOutput.State = false;
        }

        public void CloseRelay()
        {
            RelayOutput.State = true;
        }

        public void ToggleRelayState()
        {
            if (RelayOutput.State == true)
                OpenRelay();
            else
                CloseRelay();
        }
    }
}