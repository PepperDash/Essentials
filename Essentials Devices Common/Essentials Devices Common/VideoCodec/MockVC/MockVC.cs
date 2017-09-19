using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    public class MockVC : VideoCodecBase, IRoutingOutputs
    {
        public MockVC(string key, string name)
            : base(key, name)
        {
            // Debug helpers
            ActiveCallCountFeedback.OutputChange += (o, a) => Debug.Console(1, this, "InCall={0}", ActiveCallCountFeedback.IntValue);
            IncomingCallFeedback.OutputChange += (o, a) => Debug.Console(1, this, "IncomingCall={0}", _IncomingCall);
            MuteFeedback.OutputChange += (o, a) => Debug.Console(1, this, "Mute={0}", _IsMuted);
            PrivacyModeIsOnFeedback.OutputChange += (o, a) => Debug.Console(1, this, "Privacy={0}", _PrivacyModeIsOn);
            SharingSourceFeedback.OutputChange += (o, a) => Debug.Console(1, this, "SharingSource={0}", _SharingSource);   
            VolumeLevelFeedback.OutputChange += (o, a) => Debug.Console(1, this, "Volume={0}", _VolumeLevel);
       }

        protected override Func<int> ActiveCallCountFeedbackFunc
        {
            get { return () => ActiveCalls.Count; }
        }

        protected override Func<bool> IncomingCallFeedbackFunc
        {
            get { return () => _IncomingCall; }
        }
        bool _IncomingCall;

        protected override Func<bool> MuteFeedbackFunc
        {
            get { return () => _IsMuted; }
        }
        bool _IsMuted;

        protected override Func<bool> PrivacyModeIsOnFeedbackFunc
        {
            get { return () => _PrivacyModeIsOn; }
        }
        bool _PrivacyModeIsOn;
        
        protected override Func<string> SharingSourceFeedbackFunc
        {
            get { return () => _SharingSource; }
        }
        string _SharingSource;

        protected override Func<int> VolumeLevelFeedbackFunc
        {
            get { return () => _VolumeLevel; }
        }
        int _VolumeLevel;

        /// <summary>
        /// Dials, yo!
        /// </summary>
        public override void Dial(string s)
        {
            Debug.Console(1, this, "Dial: {0}", s);
            var item = new CodecActiveCallItem() { Name = s, Number = s, Id = s, Status = eCodecCallStatus.Dialing };
            ActiveCalls.Add(item);
            OnCallStatusChange(eCodecCallStatus.Unknown, item.Status, item);
            ActiveCallCountFeedback.FireUpdate();
            // Simulate 2-second ring
            new CTimer(o =>
            {
                var prevStatus = item.Status;
                item.Status = eCodecCallStatus.Connected;
                item.Type = eCodecCallType.Video;
                OnCallStatusChange(prevStatus, item.Status, item);
            }, 2000);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void EndCall(CodecActiveCallItem activeCall)
        {
            Debug.Console(1, this, "EndCall");
            ActiveCalls.Remove(activeCall);
            var prevStatus = activeCall.Status;
            activeCall.Status = eCodecCallStatus.Disconnected;
            OnCallStatusChange(prevStatus, activeCall.Status, activeCall);
            ActiveCallCountFeedback.FireUpdate();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void EndAllCalls()
        {
            Debug.Console(1, this, "EndAllCalls");
            ActiveCalls.Clear();
            ActiveCallCountFeedback.FireUpdate();
        }

        /// <summary>
        /// For a call from the test methods below
        /// </summary>
        public override void AcceptCall(CodecActiveCallItem item)
        {
            Debug.Console(1, this, "AcceptCall");
            
        }

        /// <summary>
        /// For a call from the test methods below
        /// </summary>
        public override void RejectCall(CodecActiveCallItem item)
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

        /// <summary>
        /// 
        /// </summary>
        public override void StartSharing()
        {
        }

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        public override void MuteOff()
        {
            _IsMuted = false;
            MuteFeedback.FireUpdate();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void MuteOn()
        {
            _IsMuted = true;
            MuteFeedback.FireUpdate();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void MuteToggle()
        {
            _IsMuted = !_IsMuted;
            MuteFeedback.FireUpdate();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        public override void SetVolume(ushort level)
        {
            _VolumeLevel = level;
            VolumeLevelFeedback.FireUpdate();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pressRelease"></param>
        public override void VolumeDown(bool pressRelease)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pressRelease"></param>
        public override void VolumeUp(bool pressRelease)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public override void PrivacyModeOn()
        {
            Debug.Console(1, this, "PrivacyMuteOn");
            if (_PrivacyModeIsOn)
                return;
            _PrivacyModeIsOn = true;
            PrivacyModeIsOnFeedback.FireUpdate();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void PrivacyModeOff()
        {
            Debug.Console(1, this, "PrivacyMuteOff");
            if (!_PrivacyModeIsOn)
                return;
            _PrivacyModeIsOn = false;
            PrivacyModeIsOnFeedback.FireUpdate();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void PrivacyModeToggle()
        {
            _PrivacyModeIsOn = !_PrivacyModeIsOn;
             Debug.Console(1, this, "PrivacyMuteToggle: {0}", _PrivacyModeIsOn);
           PrivacyModeIsOnFeedback.FireUpdate();
        }

        //********************************************************
        // SIMULATION METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        public void TestIncomingCall(string url)
        {
            Debug.Console(1, this, "TestIncomingCall from {0}", url);
            var item = new CodecActiveCallItem() { Name = url, Id = url, Number = url, Status = eCodecCallStatus.Incoming, Type= eCodecCallType.Unknown };
            ActiveCalls.Add(item);
            _IncomingCall = true;
            IncomingCallFeedback.FireUpdate();
            
        }

        /// <summary>
        /// 
        /// </summary>
        public void TestFarEndHangup()
        {
            Debug.Console(1, this, "TestFarEndHangup");

        }

        public void ListCalls()
        {
            var sb = new StringBuilder();
            foreach (var c in ActiveCalls)
                sb.AppendFormat("{0} {1} -- {2}\r", c.Id, c.Number, c.Name);
            Debug.Console(1, "{0}", sb.ToString());
        }

        #region IRoutingOutputs Members

        public RoutingPortCollection<RoutingOutputPort> OutputPorts
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}