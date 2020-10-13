using PepperDash.Essentials.Core;

namespace PepperDash_Essentials_Core.DeviceTypeInterfaces
{
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