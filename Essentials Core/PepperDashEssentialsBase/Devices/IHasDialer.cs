using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Requirements for a device that has dialing capabilities
    /// </summary>
    public interface IHasDialer
    {
        // Add requirements for Dialer functionality

        void Dial(string number);
        void EndCall();
        void AcceptCall();
        void RejectCall();

        BoolFeedback InCallFeedback { get; }
        BoolFeedback IncomingCallFeedback { get; }

    }
}