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
            Debug.Console(1, this, "Dial: {0}", s);

            _InCall = true;
            InCallFeedback.FireUpdate();
        }

        /// <summary>
        /// Makes horrible tones go out on the wire!
        /// </summary>
        /// <param name="s"></param>
        public void SendDTMF(string s)
        {
            Debug.Console(1, this, "SendDTMF: {0}", s);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void EndCall()
        {
            Debug.Console(1, this, "EndCall");
            _InCall = false;
            InCallFeedback.FireUpdate();
        }

        /// <summary>
        /// For a call from the test methods below
        /// </summary>
        public override void AcceptCall()
        {
            Debug.Console(1, this, "AcceptCall");
        }

        /// <summary>
        /// For a call from the test methods below
        /// </summary>
        public override void RejectCall()
        {
            Debug.Console(1, this, "RejectCall");
        }


        /// <summary>
        /// Called by routing to make it happen
        /// </summary>
        /// <param name="selector"></param>
        public override void ExecuteSwitch(object selector)
        {
            Debug.Console(1, this, "ExecuteSwitch");

        }

        /// <summary>
        /// 
        /// </summary>
        public override void ReceiveMuteOff()
        {
            Debug.Console(1, this, "ReceiveMuteOff");

            if (!_ReceiveMute)
                return;
            _ReceiveMute = false;
            ReceiveMuteIsOnFeedback.FireUpdate();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void ReceiveMuteOn()
        {
            Debug.Console(1, this, "ReceiveMuteOn");
            if (_ReceiveMute)
                return;
            ReceiveMuteIsOnFeedback.FireUpdate();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void ReceiveMuteToggle()
        {
            Debug.Console(1, this, "ReceiveMuteToggle");

            _ReceiveMute = !_ReceiveMute;
            ReceiveMuteIsOnFeedback.FireUpdate();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        public override void SetReceiveVolume(ushort level)
        {
            Debug.Console(1, this, "SetReceiveVolume: {0}", level);

        }

        /// <summary>
        /// 
        /// </summary>
        public override void TransmitMuteOff()
        {
            Debug.Console(1, this, "TransmitMuteOff");

            if (!_TransmitMute)
                return;
            _TransmitMute = false;
            TransmitMuteIsOnFeedback.FireUpdate();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void TransmitMuteOn()
        {
            Debug.Console(1, this, "TransmitMuteOn");
            if (_TransmitMute)
                return;
            TransmitMuteIsOnFeedback.FireUpdate();
        }

        public override void TransmitMuteToggle()
        {
            _TransmitMute = !_TransmitMute;
            Debug.Console(1, this, "TransmitMuteToggle: {0}", _TransmitMute);
            TransmitMuteIsOnFeedback.FireUpdate();
        }

        public override void SetTransmitVolume(ushort level)
        {
            Debug.Console(1, this, "SetTransmitVolume: {0}", level);
        }

        public override void PrivacyModeOn()
        {
            Debug.Console(1, this, "PrivacyMuteOn");
            if (_PrivacyModeIsOn)
                return;
            _PrivacyModeIsOn = true;
            PrivacyModeIsOnFeedback.FireUpdate();
            
        }

        public override void PrivacyModeOff()
        {
            Debug.Console(1, this, "PrivacyMuteOff");
            if (!_PrivacyModeIsOn)
                return;
            _PrivacyModeIsOn = false;
            PrivacyModeIsOnFeedback.FireUpdate();
        }

        public override void PrivacyModeToggle()
        {
            _PrivacyModeIsOn = !_PrivacyModeIsOn;
             Debug.Console(1, this, "PrivacyMuteToggle: {0}", _PrivacyModeIsOn);
           PrivacyModeIsOnFeedback.FireUpdate();
        }

        //********************************************************
        // SIMULATION METHODS

        public void TestIncomingCall(string url)
        {
            Debug.Console(1, this, "TestIncomingCall");

            _IncomingCall = true;
            IncomingCallFeedback.FireUpdate();
        }

        public void TestFarEndHangup()
        {
            Debug.Console(1, this, "TestFarEndHangup");

        }
    }
}