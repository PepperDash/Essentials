using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Cameras;
using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Messenger for devices implementing <see cref="IHasCodecCameras"/>, including
    /// sub-interface support for <see cref="IHasCodecRoomPresets"/>,
    /// <see cref="IHasCameraAutoMode"/>, and <see cref="IHasCameraOff"/>.
    /// </summary>
    public class IHasCodecCamerasMessenger : MessengerBase
    {
        private readonly VideoCodecBase _codec;
        private readonly IHasCodecCameras _cameraCodec;

        /// <summary>
        /// Initializes a new instance of the <see cref="IHasCodecCamerasMessenger"/> class.
        /// </summary>
        public IHasCodecCamerasMessenger(string key, string messagePath, VideoCodecBase codec)
            : base(key, messagePath, codec)
        {
            _codec = codec ?? throw new ArgumentNullException(nameof(codec));
            _cameraCodec = codec as IHasCodecCameras ?? throw new ArgumentException("codec must implement IHasCodecCameras", nameof(codec));
    
            _cameraCodec.CameraSelected += CameraCodec_CameraSelected;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            this.LogVerbose("Adding IHasCodecCameras Actions");

            _cameraCodec.CameraSelected += CameraCodec_CameraSelected;

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));
            AddAction("/codecCamerasStatus", (id, content) => SendFullStatus(id));

            AddAction("/cameraSelect", (id, content) =>
            {
                var msg = content.ToObject<MobileControlSimpleContent<string>>();
                _cameraCodec.SelectCamera(msg.Value);
            });

            MapCameraActions();

            if (_codec is IHasCodecRoomPresets presetsCodec)
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

            if (_codec is IHasCameraAutoMode speakerTrackCodec)
            {
                this.LogVerbose("Adding IHasCameraAutoMode Actions");

                speakerTrackCodec.CameraAutoModeIsOnFeedback.OutputChange += CameraAutoModeIsOnFeedback_OutputChange;

                AddAction("/cameraModeAuto", (id, content) => speakerTrackCodec.CameraAutoModeOn());
                AddAction("/cameraModeManual", (id, content) => speakerTrackCodec.CameraAutoModeOff());
            }

            if (_codec is IHasCameraOff cameraOffCodec)
            {
                this.LogVerbose("Adding IHasCameraOff Actions");

                cameraOffCodec.CameraIsOffFeedback.OutputChange += CameraIsOffFeedback_OutputChange;

                AddAction("/cameraModeOff", (id, content) => cameraOffCodec.CameraOff());
            }
        }

        private void CameraCodec_CameraSelected(object sender, CameraSelectedEventArgs<IHasCameraControls> e)
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
            if (_cameraCodec.SelectedCamera == null) return;

            RemoveAction("/cameraUp");
            RemoveAction("/cameraDown");
            RemoveAction("/cameraLeft");
            RemoveAction("/cameraRight");
            RemoveAction("/cameraZoomIn");
            RemoveAction("/cameraZoomOut");
            RemoveAction("/cameraHome");

            if (_cameraCodec.SelectedCamera is IHasCameraPtzControl camera)
            {
                AddAction("/cameraUp", (id, content) => HandleCameraPressAndHold(content, b =>
                {
                    if (b) camera.TiltUp(); else camera.TiltStop();
                }));

                AddAction("/cameraDown", (id, content) => HandleCameraPressAndHold(content, b =>
                {
                    if (b) camera.TiltDown(); else camera.TiltStop();
                }));

                AddAction("/cameraLeft", (id, content) => HandleCameraPressAndHold(content, b =>
                {
                    if (b) camera.PanLeft(); else camera.PanStop();
                }));

                AddAction("/cameraRight", (id, content) => HandleCameraPressAndHold(content, b =>
                {
                    if (b) camera.PanRight(); else camera.PanStop();
                }));

                AddAction("/cameraZoomIn", (id, content) => HandleCameraPressAndHold(content, b =>
                {
                    if (b) camera.ZoomIn(); else camera.ZoomStop();
                }));

                AddAction("/cameraZoomOut", (id, content) => HandleCameraPressAndHold(content, b =>
                {
                    if (b) camera.ZoomOut(); else camera.ZoomStop();
                }));

                AddAction("/cameraHome", (id, content) => camera.PositionHome());

                RemoveAction("/cameraAutoFocus");
                RemoveAction("/cameraFocusNear");
                RemoveAction("/cameraFocusFar");

                if (_cameraCodec.SelectedCamera is IHasCameraFocusControl focusCamera)
                {
                    AddAction("/cameraAutoFocus", (id, content) => focusCamera.TriggerAutoFocus());

                    AddAction("/cameraFocusNear", (id, content) => HandleCameraPressAndHold(content, b =>
                    {
                        if (b) focusCamera.FocusNear(); else focusCamera.FocusStop();
                    }));

                    AddAction("/cameraFocusFar", (id, content) => HandleCameraPressAndHold(content, b =>
                    {
                        if (b) focusCamera.FocusFar(); else focusCamera.FocusStop();
                    }));
                }
            }
        }

        private void HandleCameraPressAndHold(JToken content, Action<bool> cameraAction)
        {
            var state = content.ToObject<MobileControlSimpleContent<string>>();
            var timerHandler = PressAndHoldHandler.GetPressAndHoldHandler(state.Value);
            if (timerHandler == null) return;
            timerHandler(state.Value, cameraAction);
            cameraAction(state.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase));
        }

        private void CameraAutoModeIsOnFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            PostCameraMode();
        }

        private void CameraIsOffFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            PostCameraMode();
        }

        private void PresetsCodec_CameraPresetsListHasChanged(object sender, EventArgs e)
        {
            PostCameraPresets();
        }

        private string GetCameraMode()
        {
            string m = "";

            if (_codec is IHasCameraAutoMode speakerTrackCodec)
            {
                m = speakerTrackCodec.CameraAutoModeIsOnFeedback.BoolValue
                    ? eCameraControlMode.Auto.ToString().ToLower()
                    : eCameraControlMode.Manual.ToString().ToLower();
            }

            if (_codec is IHasCameraOff cameraOffCodec && cameraOffCodec.CameraIsOffFeedback.BoolValue)
            {
                m = eCameraControlMode.Off.ToString().ToLower();
            }

            return m;
        }

        private Camera GetSelectedCamera()
        {
            var camera = new Camera();

            if (_cameraCodec.SelectedCameraFeedback != null)
                camera.Key = _cameraCodec.SelectedCameraFeedback.StringValue;

            if (_cameraCodec.SelectedCamera != null)
            {
                camera.Name = _cameraCodec.SelectedCamera.Name;

                if (_cameraCodec.SelectedCamera is IHasCameraPtzControl ptz)
                {
                    camera.Capabilities = new CameraCapabilities
                    {
                        CanPan = ptz is IHasCameraPanControl,
                        CanTilt = ptz is IHasCameraTiltControl,
                        CanZoom = ptz is IHasCameraZoomControl,
                        CanFocus = ptz is IHasCameraFocusControl,
                    };
                }
            }

            if (_cameraCodec.ControllingFarEndCameraFeedback != null)
                camera.IsFarEnd = _cameraCodec.ControllingFarEndCameraFeedback.BoolValue;

            return camera;
        }

        private List<CodecRoomPreset> GetCurrentPresets()
        {
            if (!(_codec is IHasCodecRoomPresets presetsCodec)) return null;

            if (_codec is IHasFarEndCameraControl farEnd && farEnd.ControllingFarEndCameraFeedback.BoolValue)
                return presetsCodec.FarEndRoomPresets;

            return presetsCodec.NearEndPresets;
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

        private void PostCameraMode()
        {
            try
            {
                PostStatusMessage(new IHasCodecCamerasStateMessage
                {
                    CameraMode = GetCameraMode()
                });
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
                PostStatusMessage(new IHasCodecCamerasStateMessage
                {
                    Cameras = new CameraStatus { SelectedCamera = GetSelectedCamera() },
                    Presets = GetCurrentPresets()
                });
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting selected camera");
            }
        }

        private void PostCameraPresets()
        {
            try
            {
                PostStatusMessage(new IHasCodecCamerasStateMessage
                {
                    Presets = GetCurrentPresets()
                });
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting camera presets");
            }
        }

        private void SendFullStatus(string id = null)
        {
            try
            {
                PostStatusMessage(new IHasCodecCamerasStateMessage
                {
                    CameraMode = GetCameraMode(),
                    Cameras = new CameraStatus
                    {
                        CameraManualIsSupported = true,
                        CameraAutoIsSupported = _codec.SupportsCameraAutoMode,
                        CameraOffIsSupported = _codec.SupportsCameraOff,
                        CameraMode = GetCameraMode(),
                        Cameras = _cameraCodec.Cameras,
                        SelectedCamera = GetSelectedCamera(_cameraCodec)
                    },
                    Presets = GetCurrentPresets()
                }, id);
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error sending full camera status");
            }
        }
    }

    public class IHasCodecCamerasStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("cameraMode", NullValueHandling = NullValueHandling.Ignore)]
        public string CameraMode { get; set; }

        [JsonProperty("cameras", NullValueHandling = NullValueHandling.Ignore)]
        public CameraStatus Cameras { get; set; }

        [JsonProperty("presets", NullValueHandling = NullValueHandling.Ignore)]
        public List<CodecRoomPreset> Presets { get; set; }

        [JsonProperty("cameraSupportsAutoMode", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CameraSupportsAutoMode { get; set; }

        [JsonProperty("cameraSupportsOffMode", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CameraSupportsOffMode { get; set; }
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
