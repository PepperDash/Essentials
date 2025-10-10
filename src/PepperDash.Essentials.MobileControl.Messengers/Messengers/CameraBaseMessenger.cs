using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Cameras;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Messenger for a CameraBase device
    /// </summary>
    public class CameraBaseMessenger<T> : MessengerBase where T : IKeyed
    {
        /// <summary>
        /// Gets or sets the Camera
        /// </summary>
        public T Camera { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key"></param>
        /// <param name="camera"></param>
        /// <param name="messagePath"></param>
        public CameraBaseMessenger(string key, T camera, string messagePath)
            : base(key, messagePath, camera as IKeyName)
        {
            if (camera == null)
                throw new ArgumentNullException(nameof(camera));

            Camera = camera;


            if (Camera is IHasCameraPresets presetsCamera)
            {
                presetsCamera.PresetsListHasChanged += PresetsCamera_PresetsListHasChanged;
            }
        }

        private void PresetsCamera_PresetsListHasChanged(object sender, EventArgs e)
        {
            var presetList = new List<CameraPreset>();

            if (Camera is IHasCameraPresets presetsCamera)
                presetList = presetsCamera.Presets;

            PostStatusMessage(JToken.FromObject(new
            {
                presets = presetList
            })
            );
        }

        /// <summary>
        /// Registers the actions for this messenger.  This is called by the base class
        /// </summary>
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendCameraFullMessageObject(id));

            AddAction("/cameraStatus", (id, content) => SendCameraFullMessageObject(id));


            if (Camera is IHasCameraPtzControl ptzCamera)
            {
                //  Need to evaluate how to pass through these P&H actions.  Need a method that takes a bool maybe?
                AddAction("/cameraUp", (id, content) => HandleCameraPressAndHold(content, (b) =>
                {
                    if (b)
                    {
                        ptzCamera.TiltUp();
                        return;
                    }

                    ptzCamera.TiltStop();
                }));
                AddAction("/cameraDown", (id, content) => HandleCameraPressAndHold(content, (b) =>
                {
                    if (b)
                    {
                        ptzCamera.TiltDown();
                        return;
                    }

                    ptzCamera.TiltStop();
                }));
                AddAction("/cameraLeft", (id, content) => HandleCameraPressAndHold(content, (b) =>
                {
                    if (b)
                    {
                        ptzCamera.PanLeft();
                        return;
                    }

                    ptzCamera.PanStop();
                }));
                AddAction("/cameraRight", (id, content) => HandleCameraPressAndHold(content, (b) =>
                {
                    if (b)
                    {
                        ptzCamera.PanRight();
                        return;
                    }

                    ptzCamera.PanStop();
                }));
                AddAction("/cameraZoomIn", (id, content) => HandleCameraPressAndHold(content, (b) =>
                {
                    if (b)
                    {
                        ptzCamera.ZoomIn();
                        return;
                    }

                    ptzCamera.ZoomStop();
                }));
                AddAction("/cameraZoomOut", (id, content) => HandleCameraPressAndHold(content, (b) =>
                {
                    if (b)
                    {
                        ptzCamera.ZoomOut();
                        return;
                    }

                    ptzCamera.ZoomStop();
                }));
            }

            if (Camera is IHasCameraAutoMode)
            {
                AddAction("/cameraModeAuto", (id, content) => (Camera as IHasCameraAutoMode).CameraAutoModeOn());

                AddAction("/cameraModeManual", (id, content) => (Camera as IHasCameraAutoMode).CameraAutoModeOff());

            }

            if (Camera is IHasPowerControl)
            {
                AddAction("/cameraModeOff", (id, content) => (Camera as IHasPowerControl).PowerOff());
                AddAction("/cameraModeManual", (id, content) => (Camera as IHasPowerControl).PowerOn());
            }


            if (Camera is IHasCameraPresets presetsCamera)
            {
                AddAction("/recallPreset", (id, content) =>
                {
                    var msg = content.ToObject<MobileControlSimpleContent<int>>();

                    presetsCamera.PresetSelect(msg.Value);
                });

                AddAction("/storePreset", (id, content) =>
                {
                    var msg = content.ToObject<MobileControlSimpleContent<int>>();

                    presetsCamera.PresetStore(msg.Value, string.Empty);
                });
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

            timerHandler(Camera.Key, cameraAction);

        }

        /// <summary>
        /// Helper method to update the full status of the camera
        /// </summary>
        private void SendCameraFullMessageObject(string id = null)
        {
            var presetList = new List<CameraPreset>();
            CameraCapabilities capabilities = null;

            if (Camera is IHasCameraPresets presetsCamera)
                presetList = presetsCamera.Presets;

            if (Camera is ICameraCapabilities cameraCapabilities)
                capabilities = new CameraCapabilities
                {
                    CanPan = cameraCapabilities.CanPan,
                    CanTilt = cameraCapabilities.CanTilt,
                    CanZoom = cameraCapabilities.CanZoom,
                    CanFocus = cameraCapabilities.CanFocus

                };

            if (Camera is CameraBase cameraBase)
                capabilities = new CameraCapabilities
                {
                    CanPan = cameraBase.CanPan,
                    CanTilt = cameraBase.CanTilt,
                    CanZoom = cameraBase.CanZoom,
                    CanFocus = cameraBase.CanFocus
                    
                };

            var message = new CameraStateMessage
            {
                CameraManualSupported = Camera is IHasCameraControls,
                CameraAutoSupported = Camera is IHasCameraAutoMode,
                CameraOffSupported = Camera is IHasCameraOff,
                CameraMode = (eCameraControlMode)Enum.Parse(typeof(eCameraControlMode), GetCameraMode(), true),
                HasPresets = Camera is IHasCameraPresets,
                Presets = presetList,
                Capabilities = capabilities,
                IsFarEnd = Camera is IAmFarEndCamera
            };

            PostStatusMessage(message, id
            );
        }

        /// <summary>
        /// Computes the current camera mode
        /// </summary>
        /// <returns></returns>
        private string GetCameraMode()
        {
            string m;
            if (Camera is IHasCameraAutoMode && (Camera as IHasCameraAutoMode).CameraAutoModeIsOnFeedback.BoolValue)
                m = eCameraControlMode.Auto.ToString().ToLower();
            else if (Camera is IHasPowerControlWithFeedback && !(Camera as IHasPowerControlWithFeedback).PowerIsOnFeedback.BoolValue)
                m = eCameraControlMode.Off.ToString().ToLower();
            else
                m = eCameraControlMode.Manual.ToString().ToLower();
            return m;
        }
    }

    /// <summary>
    /// State message for a camera device
    /// </summary>
    public class CameraStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Indicates whether the camera supports manual control
        /// </summary>
        [JsonProperty("cameraManualSupported", NullValueHandling = NullValueHandling.Ignore)]
        public bool CameraManualSupported { get; set; }

        /// <summary>
        /// Indicates whether the camera supports auto control
        /// </summary>
        [JsonProperty("cameraAutoSupported", NullValueHandling = NullValueHandling.Ignore)]
        public bool CameraAutoSupported { get; set; }

        /// <summary>
        /// Indicates whether the camera supports off control
        /// </summary>
        [JsonProperty("cameraOffSupported", NullValueHandling = NullValueHandling.Ignore)]
        public bool CameraOffSupported { get; set; }

        /// <summary>
        /// Indicates the current camera control mode
        /// </summary>
        [JsonProperty("cameraMode", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public eCameraControlMode CameraMode { get; set; }

        /// <summary>
        /// Indicates whether the camera has presets
        /// </summary>
        [JsonProperty("hasPresets", NullValueHandling = NullValueHandling.Ignore)]
        public bool HasPresets { get; set; }

        /// <summary>
        /// List of presets if the camera supports them
        /// </summary>
        [JsonProperty("presets", NullValueHandling = NullValueHandling.Ignore)]
        public List<CameraPreset> Presets { get; set; }

        /// <summary>
        /// Indicates the capabilities of the camera
        /// </summary>
        [JsonProperty("capabilities", NullValueHandling = NullValueHandling.Ignore)]
        public CameraCapabilities Capabilities { get; set; }

        /// <summary>
        /// Indicates whether the camera is a far end camera
        /// </summary>
        [JsonProperty("isFarEnd", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsFarEnd { get; set; }
    }
}