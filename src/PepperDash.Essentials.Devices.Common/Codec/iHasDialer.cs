using System;

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

        bool IsInCall { get; }
    }

}