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
            ActiveCallCountFeedback.OutputChange += (o, a) => Debug.Console(1, this, "InCall={0}", _ActiveCallCount);
            IncomingCallFeedback.OutputChange += (o, a) => Debug.Console(1, this, "IncomingCall={0}", _IncomingCall);
            //ReceiveLevelFeedback.OutputChange += (o, a) => Debug.Console(1, this, "ReceiveLevel={0}", _ReceiveLevel);
            //ReceiveMuteIsOnFeedback.OutputChange += (o, a) => Debug.Console(1, this, "ReceiveMute={0}", _ReceiveMute);
            //TransmitLevelFeedback.OutputChange += (o, a) => Debug.Console(1, this, "TransmitLevel={0}", _TransmitLevel);
            //TransmitMuteIsOnFeedback.OutputChange += (o, a) => Debug.Console(1, this, "TransmitMute={0}", _TransmitMute);
            SharingSourceFeedback.OutputChange += (o, a) => Debug.Console(1, this, "SharingSource={0}", _SharingSource);   
        }

        protected override Func<int> ActiveCallCountFeedbackFunc
        {
            get { return () => _ActiveCallCount; }
        }
        int _ActiveCallCount;

        protected override Func<bool> IncomingCallFeedbackFunc
        {
            get { return () => _IncomingCall; }
        }
        bool _IncomingCall;

        //protected override Func<int> TransmitLevelFeedbackFunc
        //{
        //    get { return () => _TransmitLevel; }
        //}
        //int _TransmitLevel;

        //protected override Func<bool> TransmitMuteFeedbackFunc
        //{
        //    get { return () => _TransmitMute; }
        //}
        //bool _TransmitMute;

        //protected override Func<int> ReceiveLevelFeedbackFunc
        //{
        //    get { return () => _ReceiveLevel; }
        //}
        //int _ReceiveLevel;

        //protected override Func<bool> ReceiveMuteFeedbackFunc
        //{
        //    get { return () => _ReceiveMute; }
        //}
        //bool _ReceiveMute;

        protected override Func<bool> PrivacyModeIsOnFeedbackFunc
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

        protected override Func<string> SharingSourceFeedbackFunc
        {
            get { return () => _SharingSource; }
        }
        string _SharingSource;

        /// <summary>
        /// Dials, yo!
        /// </summary>
        public override void Dial(string s)
        {
            Debug.Console(1, this, "Dial: {0}", s);

            //_InCall = true;
            //IsInCall.FireUpdate();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void EndCall(CodecActiveCallItem activeCall)
        {
            Debug.Console(1, this, "EndCall");
            //_InCall = false;
            //IsInCall.FireUpdate();
        }

        public override void EndAllCalls()
        {
            
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
            _SharingSource = selector.ToString();

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

        public override void VolumeUp(bool pressRelease)
        {
        }


        ///// <summary>
        ///// 
        ///// </summary>
        //public override void ReceiveMuteOff()
        //{
        //    Debug.Console(1, this, "ReceiveMuteOff");

        //    if (!_ReceiveMute)
        //        return;
        //    _ReceiveMute = false;
        //    ReceiveMuteIsOnFeedback.FireUpdate();
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        //public override void ReceiveMuteOn()
        //{
        //    Debug.Console(1, this, "ReceiveMuteOn");
        //    if (_ReceiveMute)
        //        return;
        //    ReceiveMuteIsOnFeedback.FireUpdate();
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        //public override void ReceiveMuteToggle()
        //{
        //    Debug.Console(1, this, "ReceiveMuteToggle");

        //    _ReceiveMute = !_ReceiveMute;
        //    ReceiveMuteIsOnFeedback.FireUpdate();
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="level"></param>
        //public override void SetReceiveVolume(ushort level)
        //{
        //    Debug.Console(1, this, "SetReceiveVolume: {0}", level);

        //}

        ///// <summary>
        ///// 
        ///// </summary>
        //public override void TransmitMuteOff()
        //{
        //    Debug.Console(1, this, "TransmitMuteOff");

        //    if (!_TransmitMute)
        //        return;
        //    _TransmitMute = false;
        //    TransmitMuteIsOnFeedback.FireUpdate();
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        //public override void TransmitMuteOn()
        //{
        //    Debug.Console(1, this, "TransmitMuteOn");
        //    if (_TransmitMute)
        //        return;
        //    TransmitMuteIsOnFeedback.FireUpdate();
        //}

        //public override void TransmitMuteToggle()
        //{
           
        //}

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