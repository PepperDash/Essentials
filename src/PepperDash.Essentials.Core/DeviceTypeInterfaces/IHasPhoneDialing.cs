using System;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Defines the contract for IHasPhoneDialing
    /// </summary>
    public interface IHasPhoneDialing
    {
        BoolFeedback PhoneOffHookFeedback { get; }
        StringFeedback CallerIdNameFeedback { get; }
        StringFeedback CallerIdNumberFeedback { get; }
        void DialPhoneCall(string number);
        void EndPhoneCall();
        void SendDtmfToPhone(string digit);
    }
}