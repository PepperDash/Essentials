using System;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    /// <summary>
    /// Requirements for a device that has dialing capabilities
    /// </summary>
    public interface IHasDialer
    {
        // Add requirements for Dialer functionality

        /// <summary>
        /// Event that is raised when call status changes
        /// </summary>
        event EventHandler<CodecCallStatusItemChangeEventArgs> CallStatusChange;

        /// <summary>
        /// Dials the specified number
        /// </summary>
        /// <param name="number">The number to dial</param>
        void Dial(string number);

        /// <summary>
        /// Ends the specified active call
        /// </summary>
        /// <param name="activeCall">The active call to end</param>
        void EndCall(CodecActiveCallItem activeCall);

        /// <summary>
        /// Ends all active calls
        /// </summary>
        void EndAllCalls();

        /// <summary>
        /// Accepts the specified incoming call
        /// </summary>
        /// <param name="item">The call item to accept</param>
        void AcceptCall(CodecActiveCallItem item);

        /// <summary>
        /// Rejects the specified incoming call
        /// </summary>
        /// <param name="item">The call item to reject</param>
        void RejectCall(CodecActiveCallItem item);

        /// <summary>
        /// Sends DTMF digits during a call
        /// </summary>
        /// <param name="digit">The DTMF digit(s) to send</param>
        void SendDtmf(string digit);

        /// <summary>
        /// Gets a value indicating whether the device is currently in a call
        /// </summary>
        bool IsInCall { get; }
    }

}