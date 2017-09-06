using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Cisco_One_Button_To_Push;
using Cisco_SX80_Corporate_Phone_Book;

namespace PepperDash.Essentials.Devices.VideoCodec
{
    public class CiscoCodec : VideoCodecBase
    {
        private CiscoOneButtonToPush Codec;

        private Corporate_Phone_Book PhoneBook;

        public CiscoCodec(string key, string name)
            : base(key, name)
        {
            Codec = new CiscoOneButtonToPush();

            PhoneBook = new Corporate_Phone_Book();

            Codec.Initialize();

            Codec.GetMeetings();           
        }

        public override void ExecuteSwitch(object selector)
        {
            throw new NotImplementedException();
        }

        protected override Func<bool> InCallFeedbackFunc
        {
            get { throw new NotImplementedException(); }
        }

        protected override Func<bool> TransmitMuteFeedbackFunc
        {
            get { throw new NotImplementedException(); }
        }

        protected override Func<bool> ReceiveMuteFeedbackFunc
        {
            get { throw new NotImplementedException(); }
        }

        protected override Func<bool> PrivacyModeFeedbackFunc
        {
            get { throw new NotImplementedException(); }
        }

        public override void Dial()
        {
            throw new NotImplementedException();
        }

        public override void EndCall()
        {
            throw new NotImplementedException();
        }

        public override void ReceiveMuteOff()
        {
            throw new NotImplementedException();
        }

        public override void ReceiveMuteOn()
        {
            throw new NotImplementedException();
        }

        public override void ReceiveMuteToggle()
        {
            throw new NotImplementedException();
        }

        public override void SetReceiveVolume(ushort level)
        {
            throw new NotImplementedException();
        }

        public override void TransmitMuteOff()
        {
            throw new NotImplementedException();
        }

        public override void TransmitMuteOn()
        {
            throw new NotImplementedException();
        }

        public override void TransmitMuteToggle()
        {
            throw new NotImplementedException();
        }

        public override void SetTransmitVolume(ushort level)
        {
            throw new NotImplementedException();
        }

        public override void PrivacyModeOn()
        {
            throw new NotImplementedException();
        }

        public override void PrivacyModeOff()
        {
            throw new NotImplementedException();
        }

        public override void PrivacyModeToggle()
        {
            throw new NotImplementedException();
        }
    }
}