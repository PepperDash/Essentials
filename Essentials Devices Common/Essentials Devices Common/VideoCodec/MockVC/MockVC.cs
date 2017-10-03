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
    public class MockVC : VideoCodecBase, IRoutingSource, IHasCallHistory, IHasScheduleAwareness
    {
        public RoutingInputPort CodecOsdIn { get; private set; }
        public RoutingInputPort HdmiIn1 { get; private set; }
        public RoutingInputPort HdmiIn2 { get; private set; }
        public RoutingOutputPort HdmiOut { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public MockVC(string key, string name)
            : base(key, name)
        {
            CodecInfo = new MockCodecInfo();

            // Debug helpers
            IncomingCallFeedback.OutputChange += (o, a) => Debug.Console(1, this, "IncomingCall={0}", _IncomingCall);
            MuteFeedback.OutputChange += (o, a) => Debug.Console(1, this, "Mute={0}", _IsMuted);
            PrivacyModeIsOnFeedback.OutputChange += (o, a) => Debug.Console(1, this, "Privacy={0}", _PrivacyModeIsOn);
            SharingSourceFeedback.OutputChange += (o, a) => Debug.Console(1, this, "SharingSource={0}", _SharingSource);   
            VolumeLevelFeedback.OutputChange += (o, a) => Debug.Console(1, this, "Volume={0}", _VolumeLevel);

            CodecOsdIn = new RoutingInputPort(RoutingPortNames.CodecOsd, eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, 0, this);
            HdmiIn1 = new RoutingInputPort(RoutingPortNames.HdmiIn1, eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, 1, this);
            HdmiIn2 = new RoutingInputPort(RoutingPortNames.HdmiIn2, eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, 2, this);
            HdmiOut = new RoutingOutputPort(RoutingPortNames.HdmiOut, eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, null, this);

            InputPorts.Add(CodecOsdIn);
            InputPorts.Add(HdmiIn1);
            InputPorts.Add(HdmiIn2);
            OutputPorts.Add(HdmiOut);

            CallHistory = new CodecCallHistory();
            for (int i = 0; i < 10; i++)
            {
                var call = new CodecCallHistory.CallHistoryEntry();
                call.Name = "Call " + i;
                call.Number = i + "@call.com";
                CallHistory.RecentCalls.Add(call);
            }
            // eventually fire history event here

            SetIsReady();
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
            var call = new CodecActiveCallItem() { Name = s, Number = s, Id = s, Status = eCodecCallStatus.Dialing };
            ActiveCalls.Add(call);
            OnCallStatusChange(eCodecCallStatus.Unknown, call.Status, call);
            //ActiveCallCountFeedback.FireUpdate();
            // Simulate 2-second ring, then connecting, then connected
            new CTimer(o =>
            {
                call.Type = eCodecCallType.Video;
                SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Connecting, call);
                new CTimer(oo => SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Connected, call), 1000);
            }, 2000);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void EndCall(CodecActiveCallItem call)
        {
            Debug.Console(1, this, "EndCall");
            ActiveCalls.Remove(call);
            SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Disconnected, call);
            //ActiveCallCountFeedback.FireUpdate();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void EndAllCalls()
        {
            Debug.Console(1, this, "EndAllCalls");
            for(int i = ActiveCalls.Count - 1; i >= 0; i--)
            {
                var call = ActiveCalls[i];
                ActiveCalls.Remove(call);
                SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Disconnected, call);
            }
            //ActiveCallCountFeedback.FireUpdate();
        }

        /// <summary>
        /// For a call from the test methods below
        /// </summary>
        public override void AcceptCall(CodecActiveCallItem call)
        {
            Debug.Console(1, this, "AcceptCall");
            SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Connecting, call);
            new CTimer(o => SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Connected, call), 1000);
            // should already be in active list
        }

        /// <summary>
        /// For a call from the test methods below
        /// </summary>
        public override void RejectCall(CodecActiveCallItem call)
        {
            Debug.Console(1, this, "RejectCall");
            ActiveCalls.Remove(call);
            SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Disconnected, call);
            //ActiveCallCountFeedback.FireUpdate();
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
            Debug.Console(1, this, "ExecuteSwitch: {0}", selector);
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
        public void TestIncomingVideoCall(string url)
        {
            Debug.Console(1, this, "TestIncomingVideoCall from {0}", url);
            var call = new CodecActiveCallItem() { Name = url, Id = url, Number = url, Type= eCodecCallType.Video, Direction = eCodecCallDirection.Incoming };
            ActiveCalls.Add(call);
            SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Ringing, call);
            _IncomingCall = true;
            IncomingCallFeedback.FireUpdate();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        public void TestIncomingAudioCall(string url)
        {
            Debug.Console(1, this, "TestIncomingAudioCall from {0}", url);
            var call = new CodecActiveCallItem() { Name = url, Id = url, Number = url, Type = eCodecCallType.Audio, Direction = eCodecCallDirection.Incoming };
            ActiveCalls.Add(call);
            SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Ringing, call);
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

        #region IHasCallHistory Members

        public CodecCallHistory CallHistory { get; private set; }

        public void RemoveCallHistoryEntry(CodecCallHistory.CallHistoryEntry entry)
        {
            
        }

        #endregion

		#region IHasScheduleAwareness Members

		public CodecScheduleAwareness CodecSchedule
		{
			get {
				var sch = new CodecScheduleAwareness();
				for(int i = 0; i < 5; i++)
				{
					var m = new Meeting();
					m.StartTime = DateTime.Now.AddHours(1 + i);
					m.EndTime = DateTime.Now.AddHours(1 + i).AddMinutes(30);
					m.Title = "Meeting " + i;
					m.ConferenceNumberToDial = i + "meeting@fake.com";
					sch.Meetings.Add(m);
				}
				return sch;
			}
		}

		#endregion
	}

    /// <summary>
    /// Implementation for the mock VC
    /// </summary>
    public class MockCodecInfo : VideoCodecInfo
    {

        public override bool MultiSiteOptionIsEnabled
        {
            get { return true; }
        }

        public override string IpAddress
        {
            get { return "xx.xx.xx.xx"; }
        }

        public override string PhoneNumber
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