namespace PepperDash.Essentials.Devices.Common.AudioCodec
{
    public class MockAudioCodecInfo : AudioCodecInfo
    {
        string _phoneNumber;

        public override string PhoneNumber
        {
            get
            {
                return _phoneNumber;
            }
            set
            {
                _phoneNumber = value;
            }
        }
    }
}