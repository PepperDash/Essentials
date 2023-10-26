using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    /// <summary>
    /// Implementation for the mock VC
    /// </summary>
    public class MockCodecInfo : VideoCodecInfo
    {

        public override bool MultiSiteOptionIsEnabled
        {
            get { return true; }
        }

        public override string E164Alias
        {
            get { return "someE164alias"; }
        }

        public override string H323Id
        {
            get { return "someH323Id"; }
        }

        public override string IpAddress
        {
            get { return "xxx.xxx.xxx.xxx"; }
        }

        public override string SipPhoneNumber
        {
            get { return "333-444-5555"; }
        }

        public override string SipUri
        {
            get { return "mock@someurl.com"; }
        }

        public override bool AutoAnswerEnabled
        {
            get { return _AutoAnswerEnabled; }
        }
        bool _AutoAnswerEnabled;

        public void SetAutoAnswer(bool value)
        {
            _AutoAnswerEnabled = value;
        }
    }
}