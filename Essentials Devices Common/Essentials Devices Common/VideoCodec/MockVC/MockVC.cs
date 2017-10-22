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
    public class MockVC : VideoCodecBase, IRoutingSource, IHasCallHistory, IHasScheduleAwareness, IHasCallFavorites
    {
        public RoutingInputPort CodecOsdIn { get; private set; }
        public RoutingInputPort HdmiIn1 { get; private set; }
        public RoutingInputPort HdmiIn2 { get; private set; }
        public RoutingOutputPort HdmiOut { get; private set; }

		public CodecCallFavorites CallFavorites { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public MockVC(string key, string name, MockVcPropertiesConfig props)
            : base(key, name)
        {
            CodecInfo = new MockCodecInfo();

			// Get favoritesw
			if (props.Favorites != null)
			{
				CallFavorites = new CodecCallFavorites();
				CallFavorites.Favorites = props.Favorites;
			}

            // Debug helpers
            MuteFeedback.OutputChange += (o, a) => Debug.Console(1, this, "Mute={0}", _IsMuted);
            PrivacyModeIsOnFeedback.OutputChange += (o, a) => Debug.Console(1, this, "Privacy={0}", _PrivacyModeIsOn);
            SharingSourceFeedback.OutputChange += (o, a) => Debug.Console(1, this, "SharingSource={0}", _SharingSource);   
            VolumeLevelFeedback.OutputChange += (o, a) => Debug.Console(1, this, "Volume={0}", _VolumeLevel);

            CodecOsdIn = new RoutingInputPort(RoutingPortNames.CodecOsd, eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, 0, this);
            InputPorts.Add(CodecOsdIn);
            HdmiIn1 = new RoutingInputPort(RoutingPortNames.HdmiIn1, eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, 1, this);
            InputPorts.Add(HdmiIn1);
            HdmiIn2 = new RoutingInputPort(RoutingPortNames.HdmiIn2, eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, 2, this);
            InputPorts.Add(HdmiIn2);
            HdmiOut = new RoutingOutputPort(RoutingPortNames.HdmiOut, eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, null, this);
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

        protected override Func<bool> SharingContentIsOnFeedbackFunc
        {
            get { return () => _SharingIsOn; }
        }
        bool _SharingIsOn;

        protected override Func<int> VolumeLevelFeedbackFunc
        {
            get { return () => _VolumeLevel; }
        }
        int _VolumeLevel;

        protected override Func<bool> StandbyIsOnFeedbackFunc
        {
            get { return () => _StandbyIsOn; }
        }
        bool _StandbyIsOn;


        /// <summary>
        /// Dials, yo!
        /// </summary>
        public override void Dial(string number)
        {
            Debug.Console(1, this, "Dial: {0}", number);
            var call = new CodecActiveCallItem() { Name = number, Number = number, Id = number, Status = eCodecCallStatus.Dialing };
            ActiveCalls.Add(call);
            OnCallStatusChange(call);
            //ActiveCallCountFeedback.FireUpdate();
            // Simulate 2-second ring, then connecting, then connected
            new CTimer(o =>
            {
                call.Type = eCodecCallType.Video;
                SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Connecting, call);
                new CTimer(oo => SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Connected, call), 1000);
            }, 2000);
        }

        public override void Dial(Meeting meeting)
        {
            throw new NotImplementedException();
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
            _SharingIsOn = true;
            SharingContentIsOnFeedback.FireUpdate();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void StopSharing()
        {
            _SharingIsOn = false;
            SharingContentIsOnFeedback.FireUpdate();
        }

        public override void StandbyActivate()
        {
            _StandbyIsOn = true;
        }

        public override void StandbyDeactivate()
        {
            _StandbyIsOn = false;
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

            //OnCallStatusChange(eCodecCallStatus.Unknown, eCodecCallStatus.Ringing, call);
                
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

            //OnCallStatusChange(eCodecCallStatus.Unknown, eCodecCallStatus.Ringing, call);
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

        public void GetSchedule()
        {

        }

		public CodecScheduleAwareness CodecSchedule
		{
			get {
				// if the last meeting has past, generate a new list
				if (_CodecSchedule == null
					|| _CodecSchedule.Meetings[_CodecSchedule.Meetings.Count - 1].StartTime < DateTime.Now)
				{
					_CodecSchedule = new CodecScheduleAwareness();
					for (int i = 0; i < 5; i++)
					{
						var m = new Meeting();
						m.StartTime = DateTime.Now.AddMinutes(3).AddHours(i);
						m.EndTime = DateTime.Now.AddHours(i).AddMinutes(30);
						m.Title = "Meeting " + i;
						m.Calls[0].Number = i + "meeting@fake.com";
						_CodecSchedule.Meetings.Add(m);
					}
				}
				return _CodecSchedule;
			}
		}
		CodecScheduleAwareness _CodecSchedule;

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