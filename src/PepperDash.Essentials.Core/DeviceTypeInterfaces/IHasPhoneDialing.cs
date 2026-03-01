using System;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Defines the contract for IHasPhoneDialing
    /// </summary>
    public interface IHasPhoneDialing
    {
        /// <summary>
        /// Feedback that indicates whether the phone is off-hook
        /// </summary>
        BoolFeedback PhoneOffHookFeedback { get; }

        /// <summary>
        /// Feedback that provides the caller ID name
        /// </summary>
        StringFeedback CallerIdNameFeedback { get; }

        /// <summary>
        /// Feedback that provides the caller ID number
        /// </summary>
        StringFeedback CallerIdNumberFeedback { get; }

        /// <summary>
        /// Dials a phone call to the specified number
        /// </summary>
        /// <param name="number">the number to dial</param>
        void DialPhoneCall(string number);

        /// <summary>
        /// Ends the current phone call
        /// </summary>
        void EndPhoneCall();

        /// <summary>
        /// Sends a DTMF digit to the phone
        /// </summary>
        /// <param name="digit">the DTMF digit to send</param>
        void SendDtmfToPhone(string digit);
    }
}