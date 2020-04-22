using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.Cameras;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    public class MockVC : VideoCodecBase, IRoutingSource, IHasCallHistory, IHasScheduleAwareness, IHasCallFavorites, IHasDirectory, IHasCodecCameras, IHasCameraAutoMode, IHasCodecRoomPresets
    {
        public MockVcPropertiesConfig PropertiesConfig;

        public RoutingInputPort CodecOsdIn { get; private set; }
        public RoutingInputPort HdmiIn1 { get; private set; }
        public RoutingInputPort HdmiIn2 { get; private set; }
        public RoutingOutputPort HdmiOut { get; private set; }

		public CodecCallFavorites CallFavorites { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public MockVC(DeviceConfig config)
            : base(config)
        {
            PropertiesConfig = JsonConvert.DeserializeObject<VideoCodec.MockVcPropertiesConfig>(config.Properties.ToString());

            CodecInfo = new MockCodecInfo();

			// Get favoritesw
            if (PropertiesConfig.Favorites != null)
			{
				CallFavorites = new CodecCallFavorites();
				CallFavorites.Favorites = PropertiesConfig.Favorites;
			}

            DirectoryBrowseHistory = new List<CodecDirectory>();

            // Debug helpers
            MuteFeedback.OutputChange += (o, a) => Debug.Console(1, this, "Mute={0}", _IsMuted);
            PrivacyModeIsOnFeedback.OutputChange += (o, a) => Debug.Console(1, this, "Privacy={0}", _PrivacyModeIsOn);
            SharingSourceFeedback.OutputChange += (o, a) => Debug.Console(1, this, "SharingSource={0}", _SharingSource);   
            VolumeLevelFeedback.OutputChange += (o, a) => Debug.Console(1, this, "Volume={0}", _VolumeLevel);

            CurrentDirectoryResultIsNotDirectoryRoot = new BoolFeedback(() => DirectoryBrowseHistory.Count > 0);

            CurrentDirectoryResultIsNotDirectoryRoot.FireUpdate();

            CodecOsdIn = new RoutingInputPort(RoutingPortNames.CodecOsd, eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, 0, this);
            InputPorts.Add(CodecOsdIn);
            HdmiIn1 = new RoutingInputPort(RoutingPortNames.HdmiIn1, eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, 1, this);
            InputPorts.Add(HdmiIn1);
            HdmiIn2 = new RoutingInputPort(RoutingPortNames.HdmiIn2, eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, 2, this);
            InputPorts.Add(HdmiIn2);
            HdmiOut = new RoutingOutputPort(RoutingPortNames.HdmiOut, eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, null, this);
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

            SetupCameras();

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
            var call = new CodecActiveCallItem() { Name = number, Number = number, Id = number, Status = eCodecCallStatus.Dialing, Direction = eCodecCallDirection.Outgoing, Type = eCodecCallType.Video };
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
				if (_CodecSchedule == null || _CodecSchedule.Meetings.Count == 0
					|| _CodecSchedule.Meetings[_CodecSchedule.Meetings.Count - 1].StartTime < DateTime.Now)
				{
					_CodecSchedule = new CodecScheduleAwareness();
					for (int i = 0; i < 5; i++)
					{
						var m = new Meeting();
						m.StartTime = DateTime.Now.AddMinutes(3).AddHours(i);
						m.EndTime = DateTime.Now.AddHours(i).AddMinutes(30);
						m.Title = "Meeting " + i;
                        m.Calls.Add(new Call() { Number = i + "meeting@fake.com"});
						_CodecSchedule.Meetings.Add(m);
					}
				}
				return _CodecSchedule;
			}
		}
		CodecScheduleAwareness _CodecSchedule;

		#endregion

        #region IHasDirectory Members

        public event EventHandler<DirectoryEventArgs> DirectoryResultReturned;


        public CodecDirectory DirectoryRoot
        {
            get 
            {
                return MockVideoCodecDirectory.DirectoryRoot;
            }
        }

        public CodecDirectory CurrentDirectoryResult
        {
            get
            {
                if (DirectoryBrowseHistory.Count > 0)
                    return DirectoryBrowseHistory[DirectoryBrowseHistory.Count - 1];
                else
                    return DirectoryRoot;
            }
        }

        public CodecPhonebookSyncState PhonebookSyncState
        {
            get 
            {
                var syncState = new CodecPhonebookSyncState(Key + "PhonebookSync");

                syncState.InitialPhonebookFoldersReceived();
                syncState.PhonebookRootEntriesReceived();
                syncState.SetPhonebookHasFolders(true);
                syncState.SetNumberOfContacts(0);  // just need to call this method for the sync to complete

                return syncState;
            }
        }

        public void SearchDirectory(string searchString)
        {
            var searchResults = new CodecDirectory();

            searchResults.ResultsFolderId = "searchResult";

            // Search mock directory for contacts that contain the search string, ignoring case
            List<DirectoryItem> matches = MockVideoCodecDirectory.CompleteDirectory.CurrentDirectoryResults.FindAll(
                s => s is DirectoryContact && s.Name.ToLower().Contains(searchString.ToLower()));

            if (matches != null)
            {
                searchResults.AddContactsToDirectory(matches);

                DirectoryBrowseHistory.Add(searchResults);
            }

            OnDirectoryResultReturned(searchResults);
        }

        public void GetDirectoryFolderContents(string folderId)
        {
            var folderDirectory = new CodecDirectory();

            if (folderId == MockVideoCodecDirectory.eFolderId.UnitedStates.ToString())
                folderDirectory = MockVideoCodecDirectory.UnitedStatesFolderContents;
            else if (folderId == MockVideoCodecDirectory.eFolderId.Canada.ToString())
                folderDirectory = MockVideoCodecDirectory.CanadaFolderContents;
            else if (folderId == MockVideoCodecDirectory.eFolderId.NewYork.ToString())
                folderDirectory = MockVideoCodecDirectory.NewYorkFolderContents;
            else if (folderId == MockVideoCodecDirectory.eFolderId.Boston.ToString())
                folderDirectory = MockVideoCodecDirectory.BostonFolderContents;
            else if (folderId == MockVideoCodecDirectory.eFolderId.SanFrancisco.ToString())
                folderDirectory = MockVideoCodecDirectory.SanFranciscoFolderContents;
            else if (folderId == MockVideoCodecDirectory.eFolderId.Denver.ToString())
                folderDirectory = MockVideoCodecDirectory.DenverFolderContents;
            else if (folderId == MockVideoCodecDirectory.eFolderId.Austin.ToString())
                folderDirectory = MockVideoCodecDirectory.AustinFolderContents;
            else if (folderId == MockVideoCodecDirectory.eFolderId.Calgary.ToString())
                folderDirectory = MockVideoCodecDirectory.CalgaryFolderContents;

            DirectoryBrowseHistory.Add(folderDirectory);

            OnDirectoryResultReturned(folderDirectory);
        }

        public void SetCurrentDirectoryToRoot()
        {
            DirectoryBrowseHistory.Clear();

            OnDirectoryResultReturned(DirectoryRoot);
        }

        public void GetDirectoryParentFolderContents()
        {
            var currentDirectory = new CodecDirectory();

            if (DirectoryBrowseHistory.Count > 0)
            {
                var lastItemIndex = DirectoryBrowseHistory.Count - 1;
                var parentDirectoryContents = DirectoryBrowseHistory[lastItemIndex];

                DirectoryBrowseHistory.Remove(DirectoryBrowseHistory[lastItemIndex]);

                currentDirectory = parentDirectoryContents;

            }
            else
            {
                currentDirectory = DirectoryRoot;
            }

            OnDirectoryResultReturned(currentDirectory);
        }

        public BoolFeedback CurrentDirectoryResultIsNotDirectoryRoot { get; private set; }

        public List<CodecDirectory> DirectoryBrowseHistory { get; private set; }

        public void OnDirectoryResultReturned(CodecDirectory result)
        {
            CurrentDirectoryResultIsNotDirectoryRoot.FireUpdate();

            var handler = DirectoryResultReturned;
            if (handler != null)
            {
                handler(this, new DirectoryEventArgs()
                {
                    Directory = result,
                    DirectoryIsOnRoot = !CurrentDirectoryResultIsNotDirectoryRoot.BoolValue
                });
            }
        }

        #endregion

        void SetupCameras()
        {
            Cameras = new List<CameraBase>();

            var internalCamera = new MockVCCamera(Key + "-camera1", "Near End", this);

            Cameras.Add(internalCamera);

            var farEndCamera = new MockFarEndVCCamera(Key + "-cameraFar", "Far End", this);

            Cameras.Add(farEndCamera);

            SelectedCameraFeedback = new StringFeedback(() => SelectedCamera.Key);

            ControllingFarEndCameraFeedback = new BoolFeedback(() => SelectedCamera is IAmFarEndCamera);

            CameraAutoModeIsOnFeedback = new BoolFeedback(() => _CameraAutoModeIsOn);

            CameraAutoModeIsOnFeedback.FireUpdate();

            DeviceManager.AddDevice(internalCamera);
            DeviceManager.AddDevice(farEndCamera);

            NearEndPresets = new List<CodecRoomPreset>(15); // Fix the capacity to emulate Cisco

            NearEndPresets = PropertiesConfig.Presets;

            FarEndRoomPresets = new List<CodecRoomPreset>(15); // Fix the capacity to emulate Cisco

            // Add the far end presets
            for (int i = 1; i <= FarEndRoomPresets.Capacity; i++)
            {
                var label = string.Format("Far End Preset {0}", i);
                FarEndRoomPresets.Add(new CodecRoomPreset(i, label, true, false));
            }

            SelectedCamera = internalCamera; ; // call the method to select the camera and ensure the feedbacks get updated.
        }

        #region IHasCameras Members

        public event EventHandler<CameraSelectedEventArgs> CameraSelected;

        public List<CameraBase> Cameras { get; private set; }

        private CameraBase _selectedCamera;

        /// <summary>
        /// Returns the selected camera
        /// </summary>
        public CameraBase SelectedCamera
        {
            get
            {
                return _selectedCamera;
            }
            private set
            {
                _selectedCamera = value;
                SelectedCameraFeedback.FireUpdate();
                ControllingFarEndCameraFeedback.FireUpdate();

                var handler = CameraSelected;
                if (handler != null)
                {
                    handler(this, new CameraSelectedEventArgs(SelectedCamera));
                }
            }
        }

        public StringFeedback SelectedCameraFeedback { get; private set; }

        public void SelectCamera(string key)
        {
            var camera = Cameras.FirstOrDefault(c => c.Key.ToLower().IndexOf(key.ToLower()) > -1);
            if (camera != null)
            {
                Debug.Console(2, this, "Selected Camera with key: '{0}'", camera.Key);
                SelectedCamera = camera;
            }
            else
                Debug.Console(2, this, "Unable to select camera with key: '{0}'", key);
        }

        #endregion

        #region IHasFarEndCameraControl Members

        public CameraBase FarEndCamera { get; private set; }
        
        public BoolFeedback ControllingFarEndCameraFeedback { get; private set; }

        #endregion

        #region IHasCameraAutoMode Members

        private bool _CameraAutoModeIsOn;

        public void CameraAutoModeOn()
        {
            _CameraAutoModeIsOn = true;
            CameraAutoModeIsOnFeedback.FireUpdate();
        }

        public void CameraAutoModeOff()
        {
            _CameraAutoModeIsOn = false;
            CameraAutoModeIsOnFeedback.FireUpdate();
        }

        public void CameraAutoModeToggle()
        {
            if(_CameraAutoModeIsOn)
                _CameraAutoModeIsOn = false;
            else
                _CameraAutoModeIsOn = true;

            CameraAutoModeIsOnFeedback.FireUpdate();

        }

        public BoolFeedback CameraAutoModeIsOnFeedback {get; private set;}

        #endregion

        #region IHasCameraPresets Members

        public event EventHandler<EventArgs> CodecRoomPresetsListHasChanged;

        public List<CodecRoomPreset> NearEndPresets { get; private set; }

        public List<CodecRoomPreset> FarEndRoomPresets { get; private set; }

        public void CodecRoomPresetSelect(int preset)
        {
            if (SelectedCamera is IAmFarEndCamera)
            {
                Debug.Console(1, this, "Selecting Far End Preset: {0}", preset);
            }
            else
            {
                Debug.Console(1, this, "Selecting Near End Preset: {0}", preset);
            }
        }

        public void CodecRoomPresetStore(int preset, string description)
        {
            var editPreset = NearEndPresets.FirstOrDefault(p => p.ID.Equals(preset));

            if (editPreset != null)
            {
                editPreset.Defined = true;
                editPreset.Description = description;
            }
            else
                NearEndPresets.Add(new CodecRoomPreset(preset, description, true, true));

            var handler = CodecRoomPresetsListHasChanged;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }

            // Update the config
            SetConfig(Config);
        }

        #endregion

        protected override void CustomSetConfig(DeviceConfig config)
        {
            PropertiesConfig.Presets = NearEndPresets;

            Config.Properties = JToken.FromObject(PropertiesConfig);

            ConfigWriter.UpdateDeviceConfig(config);
        }

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

    public class MockVCFactory : EssentialsDeviceFactory<MockVC>
    {
        public MockVCFactory()
        {
            TypeNames = new List<string>() { "mockvc" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new MockVC Device");
            return new VideoCodec.MockVC(dc);
        }
    }

}