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

        event EventHandler<CodecCallStatusItemChangeEventArgs> CallStatusChange;

        void Dial(string number);
        void EndCall(CodecActiveCallItem activeCall);
        void EndAllCalls();
        void AcceptCall(CodecActiveCallItem item);
        void RejectCall(CodecActiveCallItem item);
        void SendDtmf(string digit);

        //BoolFeedback IncomingCallFeedback { get; }
    }


    /// <summary>
    /// 
    /// </summary>
    public class CodecCallStatusItemChangeEventArgs : EventArgs
    {
        public CodecActiveCallItem CallItem { get; private set; }

        //public eCodecCallStatus PreviousStatus { get; private set; }

        //public eCodecCallStatus NewStatus { get; private set; }

        public CodecCallStatusItemChangeEventArgs(/*eCodecCallStatus previousStatus,
             eCodecCallStatus newStatus,*/ CodecActiveCallItem item)
        {
            //PreviousStatus = previousStatus;
            //NewStatus = newStatus;
            CallItem = item;
        }
    }
}