

using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Devices.Common.Cameras;
using PepperDash.Essentials.Devices.Common.Codec;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    /// <summary>
    /// Represents a MockVC
    /// </summary>
    public class MockVC : VideoCodecBase, IRoutingSource, IHasCallHistory, IHasScheduleAwareness, IHasCallFavorites, IHasDirectory, IHasCodecCameras, IHasCameraAutoMode, IHasCodecRoomPresets
    {
        /// <summary>
        /// Gets or sets the PropertiesConfig
        /// </summary>
        public MockVcPropertiesConfig PropertiesConfig;

        /// <summary>
        /// Gets or sets the CodecOsdIn
        /// </summary>
        public RoutingInputPort CodecOsdIn { get; private set; }
        /// <summary>
        /// Gets or sets the HdmiIn1
        /// </summary>
        public RoutingInputPort HdmiIn1 { get; private set; }
        /// <summary>
        /// Gets or sets the HdmiIn2
        /// </summary>
        public RoutingInputPort HdmiIn2 { get; private set; }
        /// <summary>
        /// Gets or sets the HdmiOut
        /// </summary>
        public RoutingOutputPort HdmiOut { get; private set; }

        /// <summary>
        /// Gets or sets the CallFavorites
        /// </summary>
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
            MuteFeedback.OutputChange += (o, a) => Debug.LogMessage(LogEventLevel.Debug, this, "Mute={0}", _IsMuted);
            PrivacyModeIsOnFeedback.OutputChange += (o, a) => Debug.LogMessage(LogEventLevel.Debug, this, "Privacy={0}", _PrivacyModeIsOn);
            SharingSourceFeedback.OutputChange += (o, a) => Debug.LogMessage(LogEventLevel.Debug, this, "SharingSource={0}", _SharingSource);
            VolumeLevelFeedback.OutputChange += (o, a) => Debug.LogMessage(LogEventLevel.Debug, this, "Volume={0}", _VolumeLevel);

            CurrentDirectoryResultIsNotDirectoryRoot = new BoolFeedback("currentDirectoryResultIsNotDirectoryRoot", () => DirectoryBrowseHistory.Count > 0);

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

            CreateOsdSource();

            SetIsReady();
        }

        /// <inheritdoc />
        protected override Func<bool> MuteFeedbackFunc
        {
            get { return () => _IsMuted; }
        }
        bool _IsMuted;

        /// <inheritdoc />
        protected override Func<bool> PrivacyModeIsOnFeedbackFunc
        {
            get { return () => _PrivacyModeIsOn; }
        }
        bool _PrivacyModeIsOn;

        /// <inheritdoc />
        protected override Func<string> SharingSourceFeedbackFunc
        {
            get { return () => _SharingSource; }
        }
        string _SharingSource;

        /// <inheritdoc />
        protected override Func<bool> SharingContentIsOnFeedbackFunc
        {
            get { return () => _SharingIsOn; }
        }
        bool _SharingIsOn;

        /// <inheritdoc />
        protected override Func<int> VolumeLevelFeedbackFunc
        {
            get { return () => _VolumeLevel; }
        }
        int _VolumeLevel;

        /// <inheritdoc />
        protected override Func<bool> StandbyIsOnFeedbackFunc
        {
            get { return () => _StandbyIsOn; }
        }
        bool _StandbyIsOn;

        /// <summary>
        /// Creates the fake OSD source, and connects it's AudioVideo output to the CodecOsdIn input
        /// to enable routing 
        /// </summary>
        private void CreateOsdSource()
        {
            OsdSource = new DummyRoutingInputsDevice(Key + "[osd]");
            DeviceManager.AddDevice(OsdSource);
            var tl = new TieLine(OsdSource.AudioVideoOutputPort, CodecOsdIn);
            TieLineCollection.Default.Add(tl);

            //foreach(var input in Status.Video.
        }

        /// <inheritdoc />
        public override void Dial(string number)
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Dial: {0}", number);
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

        /// <inheritdoc />
        public override void Dial(Meeting meeting)
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Dial Meeting: {0}", meeting.Id);
            var call = new CodecActiveCallItem() { Name = meeting.Title, Number = meeting.Id, Id = meeting.Id, Status = eCodecCallStatus.Dialing, Direction = eCodecCallDirection.Outgoing, Type = eCodecCallType.Video };
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

        /// <inheritdoc />
        public override void EndCall(CodecActiveCallItem call)
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "EndCall");
            ActiveCalls.Remove(call);
            SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Disconnected, call);
            //ActiveCallCountFeedback.FireUpdate();
        }


        /// <inheritdoc />
        public override void EndAllCalls()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "EndAllCalls");
            for (int i = ActiveCalls.Count - 1; i >= 0; i--)
            {
                var call = ActiveCalls[i];
                ActiveCalls.Remove(call);
                SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Disconnected, call);
            }
            //ActiveCallCountFeedback.FireUpdate();
        }


        /// <inheritdoc />
        public override void AcceptCall(CodecActiveCallItem call)
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "AcceptCall");
            SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Connecting, call);
            new CTimer(o => SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Connected, call), 1000);
            // should already be in active list
        }

        /// <inheritdoc />
        public override void RejectCall(CodecActiveCallItem call)
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "RejectCall");
            ActiveCalls.Remove(call);
            SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Disconnected, call);
            //ActiveCallCountFeedback.FireUpdate();
        }

        /// <inheritdoc />
        public override void SendDtmf(string s)
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "SendDTMF: {0}", s);
        }

        /// <inheritdoc />
        public override void StartSharing()
        {
            _SharingIsOn = true;
            SharingContentIsOnFeedback.FireUpdate();
        }

        /// <inheritdoc />
        public override void StopSharing()
        {
            _SharingIsOn = false;
            SharingContentIsOnFeedback.FireUpdate();
        }

        /// <inheritdoc />
        public override void StandbyActivate()
        {
            _StandbyIsOn = true;
        }

        /// <inheritdoc />
        public override void StandbyDeactivate()
        {
            _StandbyIsOn = false;
        }

        /// <inheritdoc />          
        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void ExecuteSwitch(object selector)
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "ExecuteSwitch: {0}", selector);
            _SharingSource = selector.ToString();
        }

        /// <inheritdoc />
        public override void MuteOff()
        {
            _IsMuted = false;
            MuteFeedback.FireUpdate();
        }

        /// <inheritdoc />
        public override void MuteOn()
        {
            _IsMuted = true;
            MuteFeedback.FireUpdate();
        }

        /// <inheritdoc />
        public override void MuteToggle()
        {
            _IsMuted = !_IsMuted;
            MuteFeedback.FireUpdate();
        }

        /// <inheritdoc />
        public override void SetVolume(ushort level)
        {
            _VolumeLevel = level;
            VolumeLevelFeedback.FireUpdate();
        }

        /// <inheritdoc />
        public override void VolumeDown(bool pressRelease)
        {
        }


        /// <inheritdoc />
        public override void VolumeUp(bool pressRelease)
        {
        }

        /// <inheritdoc />
        public override void PrivacyModeOn()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "PrivacyMuteOn");
            if (_PrivacyModeIsOn)
                return;
            _PrivacyModeIsOn = true;
            PrivacyModeIsOnFeedback.FireUpdate();
        }


        /// <inheritdoc />
        public override void PrivacyModeOff()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "PrivacyMuteOff");
            if (!_PrivacyModeIsOn)
                return;
            _PrivacyModeIsOn = false;
            PrivacyModeIsOnFeedback.FireUpdate();
        }


        /// <inheritdoc />
        public override void PrivacyModeToggle()
        {
            _PrivacyModeIsOn = !_PrivacyModeIsOn;
            Debug.LogMessage(LogEventLevel.Debug, this, "PrivacyMuteToggle: {0}", _PrivacyModeIsOn);
            PrivacyModeIsOnFeedback.FireUpdate();
        }

        //********************************************************
        // SIMULATION METHODS

        /// <summary>
        /// TestIncomingVideoCall method
        /// </summary>
        /// <param name="url"></param>        
        public void TestIncomingVideoCall(string url)
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "TestIncomingVideoCall from {0}", url);
            var call = new CodecActiveCallItem() { Name = url, Id = url, Number = url, Type = eCodecCallType.Video, Direction = eCodecCallDirection.Incoming };
            ActiveCalls.Add(call);
            SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Ringing, call);

            //OnCallStatusChange(eCodecCallStatus.Unknown, eCodecCallStatus.Ringing, call);

        }

        /// <summary>
        /// TestIncomingAudioCall method
        /// </summary>
        /// <param name="url"></param>        
        public void TestIncomingAudioCall(string url)
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "TestIncomingAudioCall from {0}", url);
            var call = new CodecActiveCallItem() { Name = url, Id = url, Number = url, Type = eCodecCallType.Audio, Direction = eCodecCallDirection.Incoming };
            ActiveCalls.Add(call);
            SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus.Ringing, call);

            //OnCallStatusChange(eCodecCallStatus.Unknown, eCodecCallStatus.Ringing, call);
        }

        /// <summary>
        /// TestFarEndHangup method
        /// </summary>
        public void TestFarEndHangup()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "TestFarEndHangup");

        }


        #region IHasCallHistory Members

        /// <summary>
        /// CallHistory property
        /// </summary>
        public CodecCallHistory CallHistory { get; private set; }

        /// <summary>
        /// RemoveCallHistoryEntry method
        /// </summary>
        public void RemoveCallHistoryEntry(CodecCallHistory.CallHistoryEntry entry)
        {

        }

        #endregion

        #region IHasScheduleAwareness Members

        /// <summary>
        /// GetSchedule method
        /// </summary>
        public void GetSchedule()
        {

        }

        /// <summary>
        /// CodecSchedule property
        /// </summary>
        public CodecScheduleAwareness CodecSchedule
        {
            get
            {
                // if the last meeting has past, generate a new list
                if (_CodecSchedule == null || _CodecSchedule.Meetings.Count == 0
                    || _CodecSchedule.Meetings[_CodecSchedule.Meetings.Count - 1].StartTime < DateTime.Now)
                {
                    _CodecSchedule = new CodecScheduleAwareness(1000);
                    for (int i = 0; i < 5; i++)
                    {
                        var m = new Meeting();
                        m.MinutesBeforeMeeting = 5;
                        m.Id = i.ToString();
                        m.Organizer = "Employee " + 1;
                        m.StartTime = DateTime.Now.AddMinutes(5).AddHours(i);
                        m.EndTime = DateTime.Now.AddHours(i).AddMinutes(50);
                        m.Title = "Meeting " + i;
                        m.Calls.Add(new Call() { Number = i + "meeting@fake.com" });
                        _CodecSchedule.Meetings.Add(m);
                    }
                }
                return _CodecSchedule;
            }
        }
        CodecScheduleAwareness _CodecSchedule;

        #endregion

        #region IHasDirectory Members

        /// <summary>
        /// DirectoryResultReturned event. Fired when the directory result changes
        /// </summary>
        public event EventHandler<DirectoryEventArgs> DirectoryResultReturned;

        /// <summary>
        /// DirectoryRoot property. The root of the directory
        /// </summary>
        public CodecDirectory DirectoryRoot
        {
            get
            {
                return MockVideoCodecDirectory.DirectoryRoot;
            }
        }

        /// <summary>
        /// CurrentDirectoryResult property. The current directory result
        /// </summary>
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

        /// <summary>
        /// PhonebookSyncState property. The current state of the phonebook synchronization
        /// </summary>
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

        /// <summary>
        /// Search the directory for contacts that contain the search string
        /// </summary>
        /// <param name="searchString">The search string to use</param>
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

        /// <summary>
        /// Get the contents of the specified folder
        /// </summary>
        /// <param name="folderId">The ID of the folder to get the contents of</param>
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

        /// <summary>
        /// Set the current directory to the root
        /// </summary>
        public void SetCurrentDirectoryToRoot()
        {
            DirectoryBrowseHistory.Clear();

            OnDirectoryResultReturned(DirectoryRoot);
        }

        /// <summary>
        /// Get the contents of the parent folder
        /// </summary>
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

        /// <summary>
        /// Gets or sets the CurrentDirectoryResultIsNotDirectoryRoot
        /// </summary>
        public BoolFeedback CurrentDirectoryResultIsNotDirectoryRoot { get; private set; }

        /// <summary>
        /// Gets or sets the DirectoryBrowseHistory
        /// </summary>
        public List<CodecDirectory> DirectoryBrowseHistory { get; private set; }

        /// <summary>
        /// OnDirectoryResultReturned method
        /// </summary>
        public void OnDirectoryResultReturned(CodecDirectory result)
        {
            CurrentDirectoryResultIsNotDirectoryRoot.FireUpdate();
            DirectoryResultReturned?.Invoke(this, new DirectoryEventArgs()
            {
                Directory = result,
                DirectoryIsOnRoot = !CurrentDirectoryResultIsNotDirectoryRoot.BoolValue
            });
        }

        #endregion

        void SetupCameras()
        {
            SupportsCameraAutoMode = true;

            SupportsCameraOff = false;

            Cameras = new List<CameraBase>();

            var internalCamera = new MockVCCamera(Key + "-camera1", "Near End", this);

            Cameras.Add(internalCamera);

            var farEndCamera = new MockFarEndVCCamera(Key + "-cameraFar", "Far End", this);

            Cameras.Add(farEndCamera);

            SelectedCameraFeedback = new StringFeedback("selectedCamera", () => SelectedCamera.Key);

            ControllingFarEndCameraFeedback = new BoolFeedback("controllingFarEndCamera", () => SelectedCamera is IAmFarEndCamera);

            CameraAutoModeIsOnFeedback = new BoolFeedback("cameraAutoModeIsOn", () => _CameraAutoModeIsOn);

            SupportsCameraAutoMode = true;

            CameraAutoModeIsOnFeedback.FireUpdate();

            DeviceManager.AddDevice(internalCamera);
            DeviceManager.AddDevice(farEndCamera);

            NearEndPresets = new List<CodecRoomPreset>(15); // Fix the capacity to emulate Cisco

            if (PropertiesConfig.Presets != null && PropertiesConfig.Presets.Count > 0)
            {
                NearEndPresets = PropertiesConfig.Presets;
            }
            else
            {
                for (int i = 1; i <= NearEndPresets.Capacity; i++)
                {
                    var label = string.Format("Near End Preset {0}", i);
                    NearEndPresets.Add(new CodecRoomPreset(i, label, true, false));
                }
            }

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

        /// <summary>
        /// CameraSelected event. Fired when a camera is selected
        /// </summary>
        public event EventHandler<CameraSelectedEventArgs> CameraSelected;

        /// <summary>
        /// Gets the list of cameras associated with this codec
        /// </summary>
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
                CameraSelected?.Invoke(this, new CameraSelectedEventArgs(SelectedCamera));
            }
        }

        /// <summary>
        /// Gets or sets the SelectedCameraFeedback
        /// </summary>
        public StringFeedback SelectedCameraFeedback { get; private set; }

        /// <summary>
        /// SelectCamera method
        /// </summary>
        public void SelectCamera(string key)
        {
            var camera = Cameras.FirstOrDefault(c => c.Key.ToLower().IndexOf(key.ToLower()) > -1);
            if (camera != null)
            {
                Debug.LogMessage(LogEventLevel.Verbose, this, "Selected Camera with key: '{0}'", camera.Key);
                SelectedCamera = camera;
            }
            else
                Debug.LogMessage(LogEventLevel.Verbose, this, "Unable to select camera with key: '{0}'", key);
        }

        #endregion

        #region IHasFarEndCameraControl Members

        /// <summary>
        /// Gets or sets the FarEndCamera
        /// </summary>
        public CameraBase FarEndCamera { get; private set; }

        /// <summary>
        /// Gets or sets the ControllingFarEndCameraFeedback
        /// </summary>
        public BoolFeedback ControllingFarEndCameraFeedback { get; private set; }

        #endregion

        #region IHasCameraAutoMode Members

        private bool _CameraAutoModeIsOn;

        /// <summary>
        /// CameraAutoModeOn method
        /// </summary>
        public void CameraAutoModeOn()
        {
            _CameraAutoModeIsOn = true;
            CameraAutoModeIsOnFeedback.FireUpdate();
        }

        /// <summary>
        /// CameraAutoModeOff method
        /// </summary>
        public void CameraAutoModeOff()
        {
            _CameraAutoModeIsOn = false;
            CameraAutoModeIsOnFeedback.FireUpdate();
        }

        /// <summary>
        /// CameraAutoModeToggle method
        /// </summary>
        public void CameraAutoModeToggle()
        {
            if (_CameraAutoModeIsOn)
                _CameraAutoModeIsOn = false;
            else
                _CameraAutoModeIsOn = true;

            CameraAutoModeIsOnFeedback.FireUpdate();

        }

        /// <summary>
        /// Gets or sets the CameraAutoModeIsOnFeedback
        /// </summary>
        public BoolFeedback CameraAutoModeIsOnFeedback { get; private set; }

        #endregion

        #region IHasCameraPresets Members

        /// <summary>
        /// CodecRoomPresetsListHasChanged event. Fired when the presets list changes
        /// </summary>
        public event EventHandler<EventArgs> CodecRoomPresetsListHasChanged;

        /// <summary>
        /// Gets or sets the NearEndPresets
        /// </summary>
        public List<CodecRoomPreset> NearEndPresets { get; private set; }

        /// <summary>
        /// Gets or sets the FarEndRoomPresets
        /// </summary>
        public List<CodecRoomPreset> FarEndRoomPresets { get; private set; }

        /// <summary>
        /// CodecRoomPresetSelect method
        /// </summary>
        public void CodecRoomPresetSelect(int preset)
        {
            if (SelectedCamera is IAmFarEndCamera)
            {
                Debug.LogMessage(LogEventLevel.Debug, this, "Selecting Far End Preset: {0}", preset);
            }
            else
            {
                Debug.LogMessage(LogEventLevel.Debug, this, "Selecting Near End Preset: {0}", preset);
            }
        }

        /// <summary>
        /// CodecRoomPresetStore method
        /// </summary>
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

            CodecRoomPresetsListHasChanged?.Invoke(this, new EventArgs());

            // Update the config
            SetConfig(Config);
        }

        /// <summary>
        /// SelectFarEndPreset method
        /// </summary>
        public void SelectFarEndPreset(int i)
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Selecting Far End Preset: {0}", i);
        }

        #endregion

        /// <inheritdoc />
        protected override void CustomSetConfig(DeviceConfig config)
        {
            PropertiesConfig.Presets = NearEndPresets;

            Config.Properties = JToken.FromObject(PropertiesConfig);

            ConfigWriter.UpdateDeviceConfig(config);
        }

    }

    /// <summary>
    /// Represents a MockCodecInfo
    /// </summary>
    public class MockCodecInfo : VideoCodecInfo
    {

        /// <inheritdoc />
        public override bool MultiSiteOptionIsEnabled
        {
            get { return true; }
        }

        /// <inheritdoc />
        public override string E164Alias
        {
            get { return "someE164alias"; }
        }

        /// <inheritdoc />
        public override string H323Id
        {
            get { return "someH323Id"; }
        }

        /// <inheritdoc />
        public override string IpAddress
        {
            get { return "xxx.xxx.xxx.xxx"; }
        }

        /// <inheritdoc />
        public override string SipPhoneNumber
        {
            get { return "333-444-5555"; }
        }

        /// <inheritdoc />
        public override string SipUri
        {
            get { return "mock@someurl.com"; }
        }

        /// <inheritdoc />
        public override bool AutoAnswerEnabled
        {
            get { return _AutoAnswerEnabled; }
        }
        bool _AutoAnswerEnabled;

        /// <summary>
        /// SetAutoAnswer method
        /// </summary>
        public void SetAutoAnswer(bool value)
        {
            _AutoAnswerEnabled = value;
        }
    }

    /// <summary>
    /// Represents a MockVCFactory
    /// </summary>
    public class MockVCFactory : EssentialsDeviceFactory<MockVC>
    {
        /// <inheritdoc />
        public MockVCFactory()
        {
            TypeNames = new List<string>() { "mockvc" };
        }

        /// <inheritdoc />
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new MockVC Device");
            return new VideoCodec.MockVC(dc);
        }
    }

}