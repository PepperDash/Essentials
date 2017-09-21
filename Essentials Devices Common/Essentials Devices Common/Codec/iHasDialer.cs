using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    /// <summary>
    /// Requirements for a device that has dialing capabilities
    /// </summary>
    public interface IHasDialer
    {
        // Add requirements for Dialer functionality

        void Dial(string number);
        void EndCall(CodecActiveCallItem activeCall);
        void EndAllCalls();
        void AcceptCall(CodecActiveCallItem item);
        void RejectCall(CodecActiveCallItem item);
        void SendDtmf(string digit);

        BoolFeedback IncomingCallFeedback { get; }
    }
}