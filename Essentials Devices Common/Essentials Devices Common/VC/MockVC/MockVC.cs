using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    public class MockVC : VideoCodecBase
    {
        public MockVC(string key, string name)
            : base(key, name)
        {

        }

        protected override Func<bool> InCallFeedbackFunc
        {
            get { return () => _InCall; }
        }
        bool _InCall;

        protected override Func<bool> IncomingCallFeedbackFunc
        {
            get { return () => _IncomingCall; }
        }
        bool _IncomingCall;

        protected override Func<bool> TransmitMuteFeedbackFunc
        {
            get { return () => _TransmitMute; }
        }
        bool _TransmitMute;

        protected override Func<bool> ReceiveMuteFeedbackFunc
        {
            get { return () => _ReceiveMute; }
        }
        bool _ReceiveMute;

        protected override Func<bool> PrivacyModeFeedbackFunc
        {
            get { return () => _PrivacyModeIsOn; }
        }
        bool _PrivacyModeIsOn;

        /// <summary>
        /// Dials, yo!
        /// </summary>
        public override void Dial(string s)
        {
            
            _InCall = true;
            InCallFeedback.FireUpdate();
        }

        /// <summary>
        /// Makes horrible tones go out on the wire!
        /// </summary>
        /// <param name="s"></param>
        public override void SendDtmf(string s)
        {
            
        }

        public override void EndCall()
        {
            _InCall = false;
            InCallFeedback.FireUpdate();
        }

        /// <summary>
        /// For a call from the test methods below
        /// </summary>
        public override void AcceptCall()
        {
            
        }

        /// <summary>
        /// For a call from the test methods below
        /// </summary>
        public override void RejectCall()
        {
            
        }

        public override void StartSharing()
        {
            
        }

        public override void StopSharing()
        {

        }

        public override void ExecuteSwitch(object selector)
        {
            
        }

        public override void ReceiveMuteOff()
        {
            if (!_ReceiveMute)
                return;
            _ReceiveMute = false;
            ReceiveMuteIsOnFeedback.FireUpdate();
        }

        public override void ReceiveMuteOn()
        {
            if (_ReceiveMute)
                return;
            ReceiveMuteIsOnFeedback.FireUpdate();
        }

        public override void ReceiveMuteToggle()
        {
            _ReceiveMute = !_ReceiveMute;
            ReceiveMuteIsOnFeedback.FireUpdate();
        }

        public override void SetReceiveVolume(ushort level)
        {
            
        }

        public override void TransmitMuteOff()
        {
            if (!_TransmitMute)
                return;
            _TransmitMute = false;
            TransmitMuteIsOnFeedback.FireUpdate();
        }

        public override void TransmitMuteOn()
        {
            if (_TransmitMute)
                return;
            TransmitMuteIsOnFeedback.FireUpdate();
        }

        public override void TransmitMuteToggle()
        {
            _TransmitMute = !_TransmitMute;
            TransmitMuteIsOnFeedback.FireUpdate();
        }

        public override void SetTransmitVolume(ushort level)
        {
            
        }

        public override void PrivacyModeOn()
        {
            if (_PrivacyModeIsOn)
                return;
            _PrivacyModeIsOn = true;
            PrivacyModeIsOnFeedback.FireUpdate();
            
        }

        public override void PrivacyModeOff()
        {
            if (!_PrivacyModeIsOn)
                return;
            _PrivacyModeIsOn = false;
            PrivacyModeIsOnFeedback.FireUpdate();
        }

        public override void PrivacyModeToggle()
        {
            _PrivacyModeIsOn = !_PrivacyModeIsOn;
            PrivacyModeIsOnFeedback.FireUpdate();
        }

        //********************************************************
        // SIMULATION METHODS

        public void TestIncomingCall(string url)
        {
            _IncomingCall = true;
            IncomingCallFeedback.FireUpdate();
        }

        public void TestFarEndHangup()
        {

        }



    }
}