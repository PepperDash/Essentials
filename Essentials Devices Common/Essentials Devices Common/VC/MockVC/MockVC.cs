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
            MuteFeedback.OutputChange += (o, a) => Debug.Console(1, this, "Mute={0}", _IsMuted);
            VolumeLevelFeedback.OutputChange += (o, a) => Debug.Console(1, this, "Volume={0}", _VolumeLevel);
            InCallFeedback.OutputChange += (o, a) => Debug.Console(1, this, "InCall={0}", _InCall);
            IncomingCallFeedback.OutputChange += (o, a) => Debug.Console(1, this, "IncomingCall={0}", _IncomingCall);
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


        protected override Func<bool> PrivacyModeFeedbackFunc
        {
            get { return () => _PrivacyModeIsOn; }
        }
        bool _PrivacyModeIsOn;

        protected override Func<int> VolumeLevelFeedbackFunc
        {
            get { return () => _VolumeLevel; }
        }
        int _VolumeLevel;

        protected override Func<bool> MuteFeedbackFunc
        {
            get { return () => _IsMuted; }
        }
        bool _IsMuted;

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
        /// Makes horrible tones go out on the wire!
        /// </summary>
        /// <param name="s"></param>
        public override void SendDtmf(string s)
        {
            Debug.Console(1, this, "SendDTMF: {0}", s);
        }

        public override void StartSharing()
        {
            
        }

        public override void StopSharing()
        {
            
        }

        /// <summary>
        /// Called by routing to make it happen
        /// </summary>
        /// <param name="selector"></param>
        public override void ExecuteSwitch(object selector)
        {
            Debug.Console(1, this, "ExecuteSwitch");

        }

        public override void MuteOff()
        {
            _IsMuted = false;
            MuteFeedback.FireUpdate();
        }

        public override void MuteOn()
        {
            _IsMuted = true;
            MuteFeedback.FireUpdate();
        }

        public override void MuteToggle()
        {
            _IsMuted = !_IsMuted;
            MuteFeedback.FireUpdate();
        }
        
        public override void SetVolume(ushort level)
        {
            _VolumeLevel = level;
            VolumeLevelFeedback.FireUpdate();
        }

        public override void VolumeDown(bool pressRelease)
        {
           
        }

        public override void  VolumeUp(bool pressRelease)
        {
 	        
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