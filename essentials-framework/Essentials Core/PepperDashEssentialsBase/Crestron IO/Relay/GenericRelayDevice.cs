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
    /// Represents a generic device controlled by relays
    /// </summary>
    public class GenericRelayDevice : Device, ISwitchedOutput
    {
        public Relay RelayOutput { get; private set; }

        public BoolFeedback OutputIsOnFeedback { get; private set; }

        public GenericRelayDevice(string key, Relay relay):
            base(key)
        {
            OutputIsOnFeedback = new BoolFeedback(new Func<bool>(() => RelayOutput.State));

            RelayOutput = relay;
            RelayOutput.Register();

            RelayOutput.StateChange += new RelayEventHandler(RelayOutput_StateChange);
        }

        void RelayOutput_StateChange(Relay relay, RelayEventArgs args)
        {
            OutputIsOnFeedback.FireUpdate();
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

        #region ISwitchedOutput Members

        void ISwitchedOutput.On()
        {
            CloseRelay();
        }

        void ISwitchedOutput.Off()
        {
            OpenRelay();
        }

        #endregion
    }
}