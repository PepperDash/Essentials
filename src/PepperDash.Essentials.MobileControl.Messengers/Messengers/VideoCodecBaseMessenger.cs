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
        /// 
        /// </summary>
        protected VideoCodecBase Codec { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="codec"></param>
        /// <param name="messagePath"></param>
        public VideoCodecBaseMessenger(string key, VideoCodecBase codec, string messagePath)
            : base(key, messagePath, codec)
        {
            Codec = codec ?? throw new ArgumentNullException("codec");
            codec.CallStatusChange += Codec_CallStatusChange;
            codec.IsReadyChange += Codec_IsReadyChange;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Called from base's RegisterWithAppServer method
        /// </summary>
        protected override void RegisterActions()
        {
            try
            {
                base.RegisterActions();

                AddAction("/isReady", (id, content) => SendIsReady());

                AddAction("/fullStatus", (id, content) => SendFullStatus(id));
                AddAction("/codecStatus", (id, content) => SendFullStatus(id));

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

        /// <summary>
        /// Handler for codec changes
        /// </summary>
        private void Codec_CallStatusChange(object sender, CodecCallStatusItemChangeEventArgs e)
        {
            SendFullStatus();
        }

        /// <summary>
        /// 
        /// </summary>
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
        /// Helper method to build call status for vtc
        /// </summary>
        /// <returns></returns>
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
                status.HasCameras = Codec is IHasCamerasWithControls;
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
        /// Sends the full status of the codec, including active calls, camera status, and directory info if applicable
        /// </summary>
        /// <param name="id"></param>
        protected virtual void SendFullStatus(string id = null)
        {
            if (!Codec.IsReady)
            {
                return;
            }

            Task.Run(() => PostStatusMessage(GetStatus(), id));
        }

        /// <summary>
        /// Helper to grab a call with string ID
        /// </summary>
        private CodecActiveCallItem GetCallWithId(string id)
        {
            return Codec.ActiveCalls.FirstOrDefault(c => c.Id == id);
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

        private Camera GetSelectedCamera(IHasCodecCameras camerasCodec)
        {
            var camera = new Camera();

            if (camerasCodec.SelectedCameraFeedback != null)
                camera.Key = camerasCodec.SelectedCameraFeedback.StringValue;
            if (camerasCodec.SelectedCamera != null)
            {
                camera.Name = camerasCodec.SelectedCamera.Name;

                if(camerasCodec.SelectedCamera is IHasCameraPtzControl cameraControls)
                {
                    camera.Capabilities = new CameraCapabilities()
                    {
                        CanPan = cameraControls is IHasCameraPanControl,
                        CanTilt = cameraControls is IHasCameraTiltControl,
                        CanZoom = cameraControls is IHasCameraZoomControl,
                        CanFocus = cameraControls is IHasCameraFocusControl,
                    };
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
    /// Represents a VideoCodecBaseStateMessage
    /// </summary>
    public class VideoCodecBaseStateMessage : DeviceStateMessageBase
    {

        [JsonProperty("calls", NullValueHandling = NullValueHandling.Ignore)]
        public List<CodecActiveCallItem> Calls { get; set; }

        [JsonProperty("cameraMode", NullValueHandling = NullValueHandling.Ignore)]
        public string CameraMode { get; set; }

        [JsonProperty("cameraSelfView", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CameraSelfViewIsOn { get; set; }

        [JsonProperty("cameras", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the Cameras
        /// </summary>
        public CameraStatus Cameras { get; set; }

        [JsonProperty("cameraSupportsAutoMode", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CameraSupportsAutoMode { get; set; }

        [JsonProperty("cameraSupportsOffMode", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CameraSupportsOffMode { get; set; }


        /// <summary>
        /// Gets or sets the CurrentDialString
        /// </summary>
        [JsonProperty("currentDialString", NullValueHandling = NullValueHandling.Ignore)]
        public string CurrentDialString { get; set; }



        /// <summary>
        /// Gets or sets the DirectorySelectedFolderName
        /// </summary>
        [JsonProperty("directorySelectedFolderName", NullValueHandling = NullValueHandling.Ignore)]
        public string DirectorySelectedFolderName { get; set; }

        [JsonProperty("hasCameras", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasCameras { get; set; }



        [JsonProperty("hasPresets", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasPresets { get; set; }

        [JsonProperty("hasRecents", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasRecents { get; set; }

        [JsonProperty("initialPhonebookSyncComplete", NullValueHandling = NullValueHandling.Ignore)]
        public bool? InitialPhonebookSyncComplete { get; set; }


        /// <summary>
        /// Gets or sets the Info
        /// </summary>
        [JsonProperty("info", NullValueHandling = NullValueHandling.Ignore)]
        public VideoCodecInfo Info { get; set; }

        [JsonProperty("isInCall", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsInCall { get; set; }

        [JsonProperty("isReady", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsReady { get; set; }

        [JsonProperty("isZoomRoom", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsZoomRoom { get; set; }


        /// <summary>
        /// Gets or sets the MeetingInfo
        /// </summary>
        [JsonProperty("meetingInfo", NullValueHandling = NullValueHandling.Ignore)]
        public MeetingInfo MeetingInfo { get; set; }


        /// <summary>
        /// Gets or sets the Presets
        /// </summary>
        [JsonProperty("presets", NullValueHandling = NullValueHandling.Ignore)]
        public List<CodecRoomPreset> Presets { get; set; }

        [JsonProperty("privacyModeIsOn", NullValueHandling = NullValueHandling.Ignore)]
        public bool? PrivacyModeIsOn { get; set; }

        [JsonProperty("receivingContent", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ReceivingContent { get; set; }

        [JsonProperty("recentCalls", NullValueHandling = NullValueHandling.Ignore)]
        public List<CodecCallHistory.CallHistoryEntry> RecentCalls { get; set; }

        [JsonProperty("sharingContentIsOn", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SharingContentIsOn { get; set; }


        /// <summary>
        /// Gets or sets the SharingSource
        /// </summary>
        [JsonProperty("sharingSource", NullValueHandling = NullValueHandling.Ignore)]
        public string SharingSource { get; set; }

        [JsonProperty("showCamerasWhenNotInCall", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowCamerasWhenNotInCall { get; set; }

        [JsonProperty("showSelfViewByDefault", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowSelfViewByDefault { get; set; }

        [JsonProperty("standbyIsOn", NullValueHandling = NullValueHandling.Ignore)]
        public bool? StandbyIsOn { get; set; }

        [JsonProperty("supportsAdHocMeeting", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SupportsAdHocMeeting { get; set; }
    }

    /// <summary>
    /// Represents a CameraStatus
    /// </summary>
    public class CameraStatus
    {
        [JsonProperty("cameraManualSupported", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CameraManualIsSupported { get; set; }

        [JsonProperty("cameraAutoSupported", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CameraAutoIsSupported { get; set; }

        [JsonProperty("cameraOffSupported", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CameraOffIsSupported { get; set; }


        /// <summary>
        /// Gets or sets the CameraMode
        /// </summary>
        [JsonProperty("cameraMode", NullValueHandling = NullValueHandling.Ignore)]
        public string CameraMode { get; set; }


        /// <summary>
        /// Gets or sets the Cameras
        /// </summary>
        [JsonProperty("cameraList", NullValueHandling = NullValueHandling.Ignore)]
        public List<IHasCameraControls> Cameras { get; set; }


        /// <summary>
        /// Gets or sets the SelectedCamera
        /// </summary>
        [JsonProperty("selectedCamera", NullValueHandling = NullValueHandling.Ignore)]
        public Camera SelectedCamera { get; set; }
    }

    /// <summary>
    /// Represents a Camera
    /// </summary>
    public class Camera
    {

        /// <summary>
        /// Gets or sets the Key
        /// </summary>
        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public string Key { get; set; }


        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("isFarEnd", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsFarEnd { get; set; }


        /// <summary>
        /// Gets or sets the Capabilities
        /// </summary>
        [JsonProperty("capabilities", NullValueHandling = NullValueHandling.Ignore)]
        public CameraCapabilities Capabilities { get; set; }
    }

    /// <summary>
    /// Represents a CameraCapabilities
    /// </summary>
    public class CameraCapabilities
    {
        [JsonProperty("canPan", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CanPan { get; set; }

        [JsonProperty("canTilt", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CanTilt { get; set; }

        [JsonProperty("canZoom", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CanZoom { get; set; }

        [JsonProperty("canFocus", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CanFocus { get; set; }

    }
}