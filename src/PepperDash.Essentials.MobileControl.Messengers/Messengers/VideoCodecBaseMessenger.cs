using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Devices.Common.Cameras;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.VideoCodec;
using PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Provides a messaging bridge for a VideoCodecBase device
    /// </summary>
    public class VideoCodecBaseMessenger : MessengerBase
    {
        /// <summary>
        /// The video codec device being managed by this messenger.
        /// </summary>
        protected VideoCodecBase Codec { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="VideoCodecBaseMessenger"/> class.
        /// </summary>
        /// <param name="key">The unique identifier for the messenger.</param>
        /// <param name="codec">The video codec device to be managed.</param>
        /// <param name="messagePath">The message path for communication.</param>
        /// <exception cref="ArgumentNullException">Thrown when codec is null</exception>
        public VideoCodecBaseMessenger(string key, VideoCodecBase codec, string messagePath)
            : base(key, messagePath, codec)
        {
            Codec = codec ?? throw new ArgumentNullException("codec");
            codec.CallStatusChange += Codec_CallStatusChange;
            codec.IsReadyChange += Codec_IsReadyChange;

            if (codec is IHasDirectory dirCodec)
            {
                dirCodec.DirectoryResultReturned += DirCodec_DirectoryResultReturned;
            }

            if (codec is IHasCallHistory recCodec)
            {
                recCodec.CallHistory.RecentCallsListHasChanged += CallHistory_RecentCallsListHasChanged;
            }

            if (codec is IPasswordPrompt pwPromptCodec)
            {
                pwPromptCodec.PasswordRequired += OnPasswordRequired;
            }
        }

        private void OnPasswordRequired(object sender, PasswordPromptEventArgs args)
        {
            var eventMsg = new PasswordPromptEventMessage
            {
                Message = args.Message,
                LastAttemptWasIncorrect = args.LastAttemptWasIncorrect,
                LoginAttemptFailed = args.LoginAttemptFailed,
                LoginAttemptCancelled = args.LoginAttemptCancelled,
                EventType = "passwordPrompt"
            };

            PostEventMessage(eventMsg);
        }

        private void CallHistory_RecentCallsListHasChanged(object sender, EventArgs e)
        {
            try
            {
                var state = new VideoCodecBaseStateMessage();

                if (!(sender is CodecCallHistory codecCallHistory)) return;
                var recents = codecCallHistory.RecentCalls;

                if (recents != null)
                {
                    state.RecentCalls = recents;

                    PostStatusMessage(state);
                }
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting call history");
            }
        }

        /// <summary>
        ///  Handles the event when a directory result is returned from the codec.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The directory event arguments.</param>
        protected virtual void DirCodec_DirectoryResultReturned(object sender, DirectoryEventArgs e)
        {
            if (Codec is IHasDirectory)
                SendDirectory(e.Directory);
        }

        /// <summary>
        /// Sends the current directory structure to the mobile control interface.
        /// </summary>
        /// <param name="directory">The directory structure to send.</param>
        protected void SendDirectory(CodecDirectory directory)
        {
            try
            {
                var state = new VideoCodecBaseStateMessage();


                if (Codec is IHasDirectory dirCodec)
                {
                    this.LogVerbose("Sending Directory.  Directory Item Count: {directoryItemCount}", directory.CurrentDirectoryResults.Count);

                    //state.CurrentDirectory = PrefixDirectoryFolderItems(directory);
                    state.CurrentDirectory = directory;

                    CrestronInvoke.BeginInvoke((o) => PostStatusMessage(state));
                }
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error sending directory");
            }
        }

        private void Codec_IsReadyChange(object sender, EventArgs e)
        {
            try
            {
                var state = new VideoCodecBaseStateMessage
                {
                    IsReady = true
                };

                PostStatusMessage(state);

                SendFullStatus();
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error sending codec ready status");
            }
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            try
            {
                base.RegisterActions();

                AddAction("/isReady", (id, content) => SendIsReady());

                AddAction("/fullStatus", (id, content) => SendFullStatus(id));

                AddAction("/dial", (id, content) =>
                {
                    var value = content.ToObject<MobileControlSimpleContent<string>>();

                    Codec.Dial(value.Value);
                });

                AddAction("/dialMeeting", (id, content) => Codec.Dial(content.ToObject<Meeting>()));

                AddAction("/endCallById", (id, content) =>
                {
                    var s = content.ToObject<MobileControlSimpleContent<string>>();
                    var call = GetCallWithId(s.Value);
                    if (call != null)
                        Codec.EndCall(call);
                });

                AddAction("/endAllCalls", (id, content) => Codec.EndAllCalls());

                AddAction("/dtmf", (id, content) =>
                {
                    var s = content.ToObject<MobileControlSimpleContent<string>>();
                    Codec.SendDtmf(s.Value);
                });

                AddAction("/rejectById", (id, content) =>
                {
                    var s = content.ToObject<MobileControlSimpleContent<string>>();

                    var call = GetCallWithId(s.Value);
                    if (call != null)
                        Codec.RejectCall(call);
                });

                AddAction("/acceptById", (id, content) =>
                {
                    var s = content.ToObject<MobileControlSimpleContent<string>>();

                    var call = GetCallWithId(s.Value);
                    if (call != null)
                        Codec.AcceptCall(call);
                });

                Codec.SharingContentIsOnFeedback.OutputChange += SharingContentIsOnFeedback_OutputChange;
                Codec.SharingSourceFeedback.OutputChange += SharingSourceFeedback_OutputChange;

                // Directory actions
                if (Codec is IHasDirectory dirCodec)
                {
                    AddAction("/getDirectory", (id, content) => GetDirectoryRoot());

                    AddAction("/directoryById", (id, content) =>
                    {
                        var msg = content.ToObject<MobileControlSimpleContent<string>>();
                        GetDirectory(msg.Value);
                    });

                    AddAction("/directorySearch", (id, content) =>
                    {
                        var msg = content.ToObject<MobileControlSimpleContent<string>>();

                        GetDirectory(msg.Value);
                    });

                    AddAction("/directoryBack", (id, content) => GetPreviousDirectory());

                    dirCodec.PhonebookSyncState.InitialSyncCompleted += PhonebookSyncState_InitialSyncCompleted;
                }

                // History actions
                if (Codec is IHasCallHistory recCodec)
                {
                    AddAction("/getCallHistory", (id, content) => PostCallHistory());
                }
                if (Codec is IHasCodecCameras cameraCodec)
                {
                    this.LogVerbose("Adding IHasCodecCameras Actions");

                    cameraCodec.CameraSelected += CameraCodec_CameraSelected;

                    AddAction("/cameraSelect", (id, content) =>
                    {
                        var msg = content.ToObject<MobileControlSimpleContent<string>>();

                        cameraCodec.SelectCamera(msg.Value);
                    });


                    MapCameraActions();

                    if (Codec is IHasCodecRoomPresets presetsCodec)
                    {
                        this.LogVerbose("Adding IHasCodecRoomPresets Actions");

                        presetsCodec.CodecRoomPresetsListHasChanged += PresetsCodec_CameraPresetsListHasChanged;

                        AddAction("/cameraPreset", (id, content) =>
                        {
                            var msg = content.ToObject<MobileControlSimpleContent<int>>();

                            presetsCodec.CodecRoomPresetSelect(msg.Value);
                        });

                        AddAction("/cameraPresetStore", (id, content) =>
                        {
                            var msg = content.ToObject<CodecRoomPreset>();

                            presetsCodec.CodecRoomPresetStore(msg.ID, msg.Description);
                        });
                    }

                    if (Codec is IHasCameraAutoMode speakerTrackCodec)
                    {
                        this.LogVerbose("Adding IHasCameraAutoMode Actions");

                        speakerTrackCodec.CameraAutoModeIsOnFeedback.OutputChange += CameraAutoModeIsOnFeedback_OutputChange;

                        AddAction("/cameraModeAuto", (id, content) => speakerTrackCodec.CameraAutoModeOn());

                        AddAction("/cameraModeManual", (id, content) => speakerTrackCodec.CameraAutoModeOff());
                    }

                    if (Codec is IHasCameraOff cameraOffCodec)
                    {
                        this.LogVerbose("Adding IHasCameraOff Actions");

                        cameraOffCodec.CameraIsOffFeedback.OutputChange += (CameraIsOffFeedback_OutputChange);

                        AddAction("/cameraModeOff", (id, content) => cameraOffCodec.CameraOff());
                    }
                }



                if (Codec is IHasCodecSelfView selfViewCodec)
                {
                    this.LogVerbose("Adding IHasCodecSelfView Actions");

                    AddAction("/cameraSelfView", (id, content) => selfViewCodec.SelfViewModeToggle());

                    selfViewCodec.SelfviewIsOnFeedback.OutputChange += new EventHandler<FeedbackEventArgs>(SelfviewIsOnFeedback_OutputChange);
                }


                if (Codec is IHasCodecLayouts layoutsCodec)
                {
                    this.LogVerbose("Adding IHasCodecLayouts Actions");

                    AddAction("/cameraRemoteView", (id, content) => layoutsCodec.LocalLayoutToggle());

                    AddAction("/cameraLayout", (id, content) => layoutsCodec.LocalLayoutToggle());
                }

                if (Codec is IPasswordPrompt pwCodec)
                {
                    this.LogVerbose("Adding IPasswordPrompt Actions");

                    AddAction("/password", (id, content) =>
                    {
                        var msg = content.ToObject<MobileControlSimpleContent<string>>();

                        pwCodec.SubmitPassword(msg.Value);
                    });
                }


                if (Codec is IHasFarEndContentStatus farEndContentStatus)
                {
                    farEndContentStatus.ReceivingContent.OutputChange +=
                        (sender, args) => PostReceivingContent(args.BoolValue);
                }

                this.LogVerbose("Adding Privacy & Standby Actions");

                AddAction("/privacyModeOn", (id, content) => Codec.PrivacyModeOn());
                AddAction("/privacyModeOff", (id, content) => Codec.PrivacyModeOff());
                AddAction("/privacyModeToggle", (id, content) => Codec.PrivacyModeToggle());
                AddAction("/sharingStart", (id, content) => Codec.StartSharing());
                AddAction("/sharingStop", (id, content) => Codec.StopSharing());
                AddAction("/standbyOn", (id, content) => Codec.StandbyActivate());
                AddAction("/standbyOff", (id, content) => Codec.StandbyDeactivate());
            }
            catch (Exception e)
            {
                this.LogException(e, "Exception adding paths");
            }
        }

        private void SharingSourceFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            try
            {
                var state = new VideoCodecBaseStateMessage
                {
                    SharingSource = e.StringValue
                };

                PostStatusMessage(state);
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting sharing source");
            }
        }

        private void SharingContentIsOnFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            try
            {
                var state = new VideoCodecBaseStateMessage
                {
                    SharingContentIsOn = e.BoolValue
                };

                PostStatusMessage(state);
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting sharing content");
            }
        }

        private void PhonebookSyncState_InitialSyncCompleted(object sender, EventArgs e)
        {
            try
            {
                var state = new VideoCodecBaseStateMessage
                {
                    InitialPhonebookSyncComplete = true
                };

                PostStatusMessage(state);
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting phonebook sync state");
            }
        }

        private void CameraIsOffFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            PostCameraMode();
        }

        private void SelfviewIsOnFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            PostCameraSelfView();
        }

        private void PresetsCodec_CameraPresetsListHasChanged(object sender, EventArgs e)
        {
            PostCameraPresets();
        }

        private void CameraAutoModeIsOnFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            PostCameraMode();
        }


        private void CameraCodec_CameraSelected(object sender, CameraSelectedEventArgs e)
        {
            try
            {
                MapCameraActions();
                PostSelectedCamera();
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Exception handling camera selected event");
            }
        }

        private void MapCameraActions()
        {
            if (Codec is IHasCameras cameraCodec && cameraCodec.SelectedCamera != null)
            {
                RemoveAction("/cameraUp");
                RemoveAction("/cameraDown");
                RemoveAction("/cameraLeft");
                RemoveAction("/cameraRight");
                RemoveAction("/cameraZoomIn");
                RemoveAction("/cameraZoomOut");
                RemoveAction("/cameraHome");

                if (cameraCodec.SelectedCamera is IHasCameraPtzControl camera)
                {
                    AddAction("/cameraUp", (id, content) => HandleCameraPressAndHold(content, (b) =>
                    {
                        if (b)
                        {
                            camera.TiltUp();
                            return;
                        }

                        camera.TiltStop();
                    }));

                    AddAction("/cameraDown", (id, content) => HandleCameraPressAndHold(content, (b) =>
                    {
                        if (b)
                        {
                            camera.TiltDown();
                            return;
                        }

                        camera.TiltStop();
                    }));

                    AddAction("/cameraLeft", (id, content) => HandleCameraPressAndHold(content, (b) =>
                    {
                        if (b)
                        {
                            camera.PanLeft();
                            return;
                        }

                        camera.PanStop();
                    }));

                    AddAction("/cameraRight", (id, content) => HandleCameraPressAndHold(content, (b) =>
                    {
                        if (b)
                        {
                            camera.PanRight();
                            return;
                        }

                        camera.PanStop();
                    }));

                    AddAction("/cameraZoomIn", (id, content) => HandleCameraPressAndHold(content, (b) =>
                    {
                        if (b)
                        {
                            camera.ZoomIn();
                            return;
                        }

                        camera.ZoomStop();
                    }));

                    AddAction("/cameraZoomOut", (id, content) => HandleCameraPressAndHold(content, (b) =>
                    {
                        if (b)
                        {
                            camera.ZoomOut();
                            return;
                        }

                        camera.ZoomStop();
                    }));
                    AddAction("/cameraHome", (id, content) => camera.PositionHome());


                    RemoveAction("/cameraAutoFocus");
                    RemoveAction("/cameraFocusNear");
                    RemoveAction("/cameraFocusFar");

                    if (cameraCodec is IHasCameraFocusControl focusCamera)
                    {
                        AddAction("/cameraAutoFocus", (id, content) => focusCamera.TriggerAutoFocus());

                        AddAction("/cameraFocusNear", (id, content) => HandleCameraPressAndHold(content, (b) =>
                        {
                            if (b)
                            {
                                focusCamera.FocusNear();
                                return;
                            }

                            focusCamera.FocusStop();
                        }));

                        AddAction("/cameraFocusFar", (id, content) => HandleCameraPressAndHold(content, (b) =>
                        {
                            if (b)
                            {
                                focusCamera.FocusFar();
                                return;
                            }

                            focusCamera.FocusStop();
                        }));
                    }
                }
            }
        }

        private void HandleCameraPressAndHold(JToken content, Action<bool> cameraAction)
        {
            var state = content.ToObject<MobileControlSimpleContent<string>>();

            var timerHandler = PressAndHoldHandler.GetPressAndHoldHandler(state.Value);
            if (timerHandler == null)
            {
                return;
            }

            timerHandler(state.Value, cameraAction);

            cameraAction(state.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase));
        }

        private string GetCameraMode()
        {
            string m = "";

            if (Codec is IHasCameraAutoMode speakerTrackCodec)
            {
                m = speakerTrackCodec.CameraAutoModeIsOnFeedback.BoolValue
                    ? eCameraControlMode.Auto.ToString().ToLower()
                    : eCameraControlMode.Manual.ToString().ToLower();
            }

            if (Codec is IHasCameraOff cameraOffCodec)
            {
                if (cameraOffCodec.CameraIsOffFeedback.BoolValue)
                    m = eCameraControlMode.Off.ToString().ToLower();
            }

            return m;
        }

        private void PostCallHistory()
        {
            try
            {
                var codec = Codec as IHasCallHistory;

                if (codec != null)
                {
                    var status = new VideoCodecBaseStateMessage();

                    var recents = codec.CallHistory.RecentCalls;

                    if (recents != null)
                    {
                        status.RecentCalls = codec.CallHistory.RecentCalls;

                        PostStatusMessage(status);
                    }
                }
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting call history");
            }
        }

        private CodecActiveCallItem GetCallWithId(string id)
        {
            return Codec.ActiveCalls.FirstOrDefault(c => c.Id == id);
        }

        private void GetDirectory(string id)
        {
            if (!(Codec is IHasDirectory dirCodec))
            {
                return;
            }
            dirCodec.GetDirectoryFolderContents(id);
        }

        private void GetDirectoryRoot(string id = null)
        {
            try
            {
                if (!(Codec is IHasDirectory dirCodec))
                {
                    // do something else?
                    return;
                }
                if (!dirCodec.PhonebookSyncState.InitialSyncComplete)
                {
                    var state = new VideoCodecBaseStateMessage
                    {
                        InitialPhonebookSyncComplete = false
                    };

                    PostStatusMessage(state);
                    return;
                }

                dirCodec.SetCurrentDirectoryToRoot();
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error getting directory root");
            }
        }

        private void GetPreviousDirectory()
        {
            if (!(Codec is IHasDirectory dirCodec))
            {
                return;
            }

            dirCodec.GetDirectoryParentFolderContents();
        }

        private void Codec_CallStatusChange(object sender, CodecCallStatusItemChangeEventArgs e)
        {
            SendFullStatus();
        }

        private void SendIsReady()
        {
            try
            {
                var status = new VideoCodecBaseStateMessage();

                var codecType = Codec.GetType();

                status.IsReady = Codec.IsReady;
                status.IsZoomRoom = codecType.GetInterface("IHasZoomRoomLayouts") != null;

                PostStatusMessage(status);
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error sending codec ready status");
            }
        }

        /// <summary>
        /// Gets the current status of the video codec.
        /// </summary>
        /// <returns> The current status of the video codec.</returns>
        protected VideoCodecBaseStateMessage GetStatus()
        {
            try
            {
                var status = new VideoCodecBaseStateMessage();

                if (Codec is IHasCodecCameras camerasCodec)
                {
                    status.Cameras = new CameraStatus
                    {
                        CameraManualIsSupported = true,
                        CameraAutoIsSupported = Codec.SupportsCameraAutoMode,
                        CameraOffIsSupported = Codec.SupportsCameraOff,
                        CameraMode = GetCameraMode(),
                        Cameras = camerasCodec.Cameras,
                        SelectedCamera = GetSelectedCamera(camerasCodec)
                    };
                }

                if (Codec is IHasDirectory directoryCodec)
                {
                    status.HasDirectory = true;
                    status.HasDirectorySearch = true;
                    status.CurrentDirectory = directoryCodec.CurrentDirectoryResult;
                }

                var codecType = Codec.GetType();

                status.CameraSelfViewIsOn = Codec is IHasCodecSelfView && (Codec as IHasCodecSelfView).SelfviewIsOnFeedback.BoolValue;
                status.IsInCall = Codec.IsInCall;
                status.PrivacyModeIsOn = Codec.PrivacyModeIsOnFeedback.BoolValue;
                status.SharingContentIsOn = Codec.SharingContentIsOnFeedback.BoolValue;
                status.SharingSource = Codec.SharingSourceFeedback.StringValue;
                status.StandbyIsOn = Codec.StandbyIsOnFeedback.BoolValue;
                status.Calls = Codec.ActiveCalls;
                status.Info = Codec.CodecInfo;
                status.ShowSelfViewByDefault = Codec.ShowSelfViewByDefault;
                status.SupportsAdHocMeeting = Codec is IHasStartMeeting;
                status.HasRecents = Codec is IHasCallHistory;
                status.HasCameras = Codec is IHasCameras;
                status.Presets = GetCurrentPresets();
                status.IsZoomRoom = codecType.GetInterface("IHasZoomRoomLayouts") != null;
                status.ReceivingContent = Codec is IHasFarEndContentStatus && (Codec as IHasFarEndContentStatus).ReceivingContent.BoolValue;

                if (Codec is IHasMeetingInfo meetingInfoCodec)
                {
                    status.MeetingInfo = meetingInfoCodec.MeetingInfo;
                }

                return status;
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error getting codec status");
                return null;
            }
        }

        /// <summary>
        /// Sends the full status of the codec.
        /// </summary>
        /// <param name="id">The unique identifier for the status message.</param>
        protected virtual void SendFullStatus(string id = null)
        {
            if (!Codec.IsReady)
            {
                return;
            }

            Task.Run(() => PostStatusMessage(GetStatus(), id));
        }

        private void PostReceivingContent(bool receivingContent)
        {
            try
            {
                var state = new VideoCodecBaseStateMessage
                {
                    ReceivingContent = receivingContent
                };

                PostStatusMessage(state);
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting receiving content");
            }
        }

        private void PostCameraSelfView()
        {
            try
            {
                var status = new VideoCodecBaseStateMessage
                {
                    CameraSelfViewIsOn = Codec is IHasCodecSelfView
                                         && (Codec as IHasCodecSelfView).SelfviewIsOnFeedback.BoolValue
                };

                PostStatusMessage(status);
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting camera self view");
            }
        }

        private void PostCameraMode()
        {
            try
            {
                var status = new VideoCodecBaseStateMessage
                {
                    CameraMode = GetCameraMode()
                };

                PostStatusMessage(status);
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting camera mode");
            }
        }

        private void PostSelectedCamera()
        {
            try
            {
                var camerasCodec = Codec as IHasCodecCameras;

                var status = new VideoCodecBaseStateMessage
                {
                    Cameras = new CameraStatus() { SelectedCamera = GetSelectedCamera(camerasCodec) },
                    Presets = GetCurrentPresets()
                };

                PostStatusMessage(status);
            }
            catch (Exception e)
            {
                this.LogError(e, "Error posting selected camera");
            }
        }

        private void PostCameraPresets()
        {
            try
            {
                var status = new VideoCodecBaseStateMessage
                {
                    Presets = GetCurrentPresets()
                };

                PostStatusMessage(status);
            }
            catch (Exception e)
            {
                this.LogError(e, "Error posting camera presets");
            }
        }

        private Camera GetSelectedCamera(IHasCodecCameras camerasCodec)
        {
            var camera = new Camera();

            if (camerasCodec.SelectedCameraFeedback != null)
                camera.Key = camerasCodec.SelectedCameraFeedback.StringValue;
            if (camerasCodec.SelectedCamera != null)
            {
                camera.Name = camerasCodec.SelectedCamera.Name;

                camera.Capabilities = new CameraCapabilities()
                {
                    CanPan = camerasCodec.SelectedCamera.CanPan,
                    CanTilt = camerasCodec.SelectedCamera.CanTilt,
                    CanZoom = camerasCodec.SelectedCamera.CanZoom,
                    CanFocus = camerasCodec.SelectedCamera.CanFocus,
                };
            }

            if (camerasCodec.ControllingFarEndCameraFeedback != null)
                camera.IsFarEnd = camerasCodec.ControllingFarEndCameraFeedback.BoolValue;


            return camera;
        }

        private List<CodecRoomPreset> GetCurrentPresets()
        {
            var presetsCodec = Codec as IHasCodecRoomPresets;

            List<CodecRoomPreset> currentPresets = null;

            if (presetsCodec != null && Codec is IHasFarEndCameraControl &&
                (Codec as IHasFarEndCameraControl).ControllingFarEndCameraFeedback.BoolValue)
                currentPresets = presetsCodec.FarEndRoomPresets;
            else if (presetsCodec != null) currentPresets = presetsCodec.NearEndPresets;

            return currentPresets;
        }
    }

    /// <summary>
    /// A class that represents the state data to be sent to the user app
    /// </summary>
    public class VideoCodecBaseStateMessage : DeviceStateMessageBase
    {

        /// <summary>
        /// The list of active calls on the codec.
        /// </summary>
        [JsonProperty("calls", NullValueHandling = NullValueHandling.Ignore)]
        public List<CodecActiveCallItem> Calls { get; set; }

        /// <summary>
        /// The current mode of the camera.
        /// </summary>
        [JsonProperty("cameraMode", NullValueHandling = NullValueHandling.Ignore)]
        public string CameraMode { get; set; }

        /// <summary>
        /// Indicates whether the camera self-view is enabled.
        /// </summary>
        [JsonProperty("cameraSelfView", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CameraSelfViewIsOn { get; set; }

        /// <summary>
        /// Gets the current status of the cameras.
        /// </summary>
        [JsonProperty("cameras", NullValueHandling = NullValueHandling.Ignore)]
        public CameraStatus Cameras { get; set; }

        /// <summary>
        /// Indicates whether the camera supports auto mode.
        /// </summary>
        [JsonProperty("cameraSupportsAutoMode", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CameraSupportsAutoMode { get; set; }

        /// <summary>
        /// Indicates whether the camera supports off mode.
        /// </summary>
        [JsonProperty("cameraSupportsOffMode", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CameraSupportsOffMode { get; set; }

        /// <summary>
        /// The current dial string for the codec.
        /// </summary>
        [JsonProperty("currentDialString", NullValueHandling = NullValueHandling.Ignore)]
        public string CurrentDialString { get; set; }

        /// <summary>
        /// Gets the current directory for the codec.
        /// </summary>
        [JsonProperty("currentDirectory", NullValueHandling = NullValueHandling.Ignore)]
        public CodecDirectory CurrentDirectory { get; set; }

        /// <summary>
        /// Gets the selected folder name in the directory.
        /// </summary>
        [JsonProperty("directorySelectedFolderName", NullValueHandling = NullValueHandling.Ignore)]
        public string DirectorySelectedFolderName { get; set; }

        /// <summary>
        /// Indicates whether the codec has active camera streams.
        /// </summary>
        [JsonProperty("hasCameras", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasCameras { get; set; }

        /// <summary>
        /// Indicates whether the codec has a directory.
        /// </summary>
        [JsonProperty("hasDirectory", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasDirectory { get; set; }

        /// <summary>
        /// Indicates whether the codec supports directory search functionality.
        /// </summary>
        [JsonProperty("hasDirectorySearch", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasDirectorySearch { get; set; }

        /// <summary>
        /// Indicates whether the codec has presets.
        /// </summary>
        [JsonProperty("hasPresets", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasPresets { get; set; }

        /// <summary>
        /// Indicates whether the codec has recent calls.
        /// </summary>
        [JsonProperty("hasRecents", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasRecents { get; set; }

        /// <summary>
        /// Indicates whether the initial phonebook sync is complete.
        /// </summary>
        [JsonProperty("initialPhonebookSyncComplete", NullValueHandling = NullValueHandling.Ignore)]
        public bool? InitialPhonebookSyncComplete { get; set; }

        /// <summary>
        /// Gets the information about the video codec.
        /// </summary>
        [JsonProperty("info", NullValueHandling = NullValueHandling.Ignore)]
        public VideoCodecInfo Info { get; set; }

        /// <summary>
        /// Indicates whether the codec is currently in a call.
        /// </summary>
        [JsonProperty("isInCall", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsInCall { get; set; }

        /// <summary>
        /// Indicates whether the codec is ready.
        /// </summary>
        [JsonProperty("isReady", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsReady { get; set; }

        /// <summary>
        /// Indicates whether the codec is a Zoom Room.
        /// </summary>
        [JsonProperty("isZoomRoom", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsZoomRoom { get; set; }

        /// <summary>
        /// Gets the meeting information for the codec, if available.
        /// </summary>
        [JsonProperty("meetingInfo", NullValueHandling = NullValueHandling.Ignore)]
        public MeetingInfo MeetingInfo { get; set; }

        /// <summary>
        /// Gets the list of presets for the codec.
        /// </summary>
        [JsonProperty("presets", NullValueHandling = NullValueHandling.Ignore)]
        public List<CodecRoomPreset> Presets { get; set; }

        /// <summary>
        /// Indicates whether the privacy mode is currently enabled.
        /// </summary>
        [JsonProperty("privacyModeIsOn", NullValueHandling = NullValueHandling.Ignore)]
        public bool? PrivacyModeIsOn { get; set; }

        /// <summary>
        /// Indicates whether the codec is currently receiving content.
        /// </summary>
        [JsonProperty("receivingContent", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ReceivingContent { get; set; }

        /// <summary>
        /// Gets the list of recent calls for the codec, if available.
        /// </summary>
        [JsonProperty("recentCalls", NullValueHandling = NullValueHandling.Ignore)]
        public List<CodecCallHistory.CallHistoryEntry> RecentCalls { get; set; }

        /// <summary>
        /// Indicates whether the codec is currently sharing content.
        /// </summary>
        [JsonProperty("sharingContentIsOn", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SharingContentIsOn { get; set; }

        /// <summary>
        /// Gets the source of the shared content, if available.
        /// </summary>
        [JsonProperty("sharingSource", NullValueHandling = NullValueHandling.Ignore)]
        public string SharingSource { get; set; }

        /// <summary>
        /// Indicates whether the cameras should be shown when not in a call.
        /// </summary>
        [JsonProperty("showCamerasWhenNotInCall", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowCamerasWhenNotInCall { get; set; }

        /// <summary>
        /// Indicates whether the self-view is shown by default.
        /// </summary>
        [JsonProperty("showSelfViewByDefault", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowSelfViewByDefault { get; set; }

        /// <summary>
        /// Indicates whether the codec is currently in standby mode.
        /// </summary>
        [JsonProperty("standbyIsOn", NullValueHandling = NullValueHandling.Ignore)]
        public bool? StandbyIsOn { get; set; }

        /// <summary>
        /// Indicates whether the codec supports ad-hoc meetings.
        /// </summary>
        [JsonProperty("supportsAdHocMeeting", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SupportsAdHocMeeting { get; set; }
    }

    /// <summary>
    /// Represents the status of the camera.
    /// </summary>
    public class CameraStatus
    {
        /// <summary>
        /// Indicates whether the camera manual control is supported.
        /// </summary>
        [JsonProperty("cameraManualSupported", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CameraManualIsSupported { get; set; }

        /// <summary>
        /// Indicates whether the camera auto control is supported.
        /// </summary>
        [JsonProperty("cameraAutoSupported", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CameraAutoIsSupported { get; set; }

        /// <summary>
        /// Indicates whether the camera off control is supported.
        /// </summary>
        [JsonProperty("cameraOffSupported", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CameraOffIsSupported { get; set; }

        /// <summary>
        /// Indicates the current mode of the camera.
        /// </summary>
        [JsonProperty("cameraMode", NullValueHandling = NullValueHandling.Ignore)]
        public string CameraMode { get; set; }

        /// <summary>
        /// Represents the list of cameras available.
        /// </summary>
        [JsonProperty("cameraList", NullValueHandling = NullValueHandling.Ignore)]
        public List<CameraBase> Cameras { get; set; }

        /// <summary>
        /// Represents the currently selected camera.
        /// </summary>
        [JsonProperty("selectedCamera", NullValueHandling = NullValueHandling.Ignore)]
        public Camera SelectedCamera { get; set; }
    }

    /// <summary>
    /// Represents a camera in the video codec system.
    /// </summary>
    public class Camera
    {
        /// <summary>
        /// The unique identifier for the camera.
        /// </summary>
        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public string Key { get; set; }

        /// <summary>
        /// The name of the camera.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        /// <summary>
        /// Indicates whether the camera is a far-end camera.
        /// </summary>
        [JsonProperty("isFarEnd", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsFarEnd { get; set; }

        /// <summary>
        /// Represents the capabilities of the camera.
        /// </summary>
        [JsonProperty("capabilities", NullValueHandling = NullValueHandling.Ignore)]
        public CameraCapabilities Capabilities { get; set; }
    }

    /// <summary>
    /// Represents the capabilities of the camera.
    /// </summary>
    public class CameraCapabilities
    {
        /// <summary>
        /// Indicates whether the camera can pan.
        /// </summary>
        [JsonProperty("canPan", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CanPan { get; set; }

        /// <summary>
        /// Indicates whether the camera can tilt.
        /// </summary>
        [JsonProperty("canTilt", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CanTilt { get; set; }

        /// <summary>
        /// Indicates whether the camera can zoom.
        /// </summary>
        [JsonProperty("canZoom", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CanZoom { get; set; }

        /// <summary>
        /// Indicates whether the camera can focus.
        /// </summary>
        [JsonProperty("canFocus", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CanFocus { get; set; }

    }

    /// <summary>
    /// Represents a video codec event message.
    /// </summary>
    public class VideoCodecBaseEventMessage : DeviceEventMessageBase
    {

    }

    /// <summary>
    /// Represents a password prompt event message.
    /// </summary>
    public class PasswordPromptEventMessage : VideoCodecBaseEventMessage
    {
        /// <summary>
        /// The message to display in the password prompt.
        /// </summary>
        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        /// <summary>
        /// Indicates whether the last password attempt was incorrect.
        /// </summary>
        [JsonProperty("lastAttemptWasIncorrect", NullValueHandling = NullValueHandling.Ignore)]
        public bool LastAttemptWasIncorrect { get; set; }

        /// <summary>
        /// Indicates whether the login attempt failed.
        /// </summary>
        [JsonProperty("loginAttemptFailed", NullValueHandling = NullValueHandling.Ignore)]
        public bool LoginAttemptFailed { get; set; }

        /// <summary>
        /// Indicates whether the login attempt was cancelled.
        /// </summary>
        [JsonProperty("loginAttemptCancelled", NullValueHandling = NullValueHandling.Ignore)]
        public bool LoginAttemptCancelled { get; set; }
    }
}