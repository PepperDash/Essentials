

using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.Presets;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Enumeration of eCameraCapabilities values
    /// </summary>
    public enum eCameraCapabilities
    {
        /// <summary>
        /// No camera capabilities
        /// </summary>
        None = 0,
        /// <summary>
        /// Camera supports pan movement
        /// </summary>
        Pan = 1,
        /// <summary>
        /// Camera supports tilt movement
        /// </summary>
        Tilt = 2,
        /// <summary>
        /// Camera supports zoom functionality
        /// </summary>
        Zoom = 4,
        /// <summary>
        /// Camera supports focus adjustment
        /// </summary>
        Focus = 8
    }

    /// <summary>
    /// Abstract base class for camera devices that provides common camera functionality and capabilities
    /// </summary>
    public abstract class CameraBase : ReconfigurableDevice, IRoutingOutputs
    {
        /// <summary>
        /// Gets or sets the ControlMode
        /// </summary>
        [JsonProperty("controlMode", NullValueHandling = NullValueHandling.Ignore)]
        public eCameraControlMode ControlMode { get; protected set; }

        #region IRoutingOutputs Members

        /// <summary>
        /// Gets or sets the OutputPorts
        /// </summary>
        [JsonIgnore]
        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; protected set; }

        #endregion

        /// <summary>
        /// Gets a value indicating whether this camera supports pan movement
        /// </summary>
        [JsonProperty("canPan", NullValueHandling = NullValueHandling.Ignore)]
        public bool CanPan
        {
            get
            {
                return (Capabilities & eCameraCapabilities.Pan) == eCameraCapabilities.Pan;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this camera supports tilt movement
        /// </summary>
        [JsonProperty("canTilt", NullValueHandling = NullValueHandling.Ignore)]
        public bool CanTilt
        {
            get
            {
                return (Capabilities & eCameraCapabilities.Tilt) == eCameraCapabilities.Tilt;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this camera supports zoom functionality
        /// </summary>
        [JsonProperty("canZoom", NullValueHandling = NullValueHandling.Ignore)]
        public bool CanZoom
        {
            get
            {
                return (Capabilities & eCameraCapabilities.Zoom) == eCameraCapabilities.Zoom;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this camera supports focus adjustment
        /// </summary>
        [JsonProperty("canFocus", NullValueHandling = NullValueHandling.Ignore)]
        public bool CanFocus
        {
            get
            {
                return (Capabilities & eCameraCapabilities.Focus) == eCameraCapabilities.Focus;
            }
        }

        /// <summary>
        /// Gets or sets a bitmasked value to indicate the movement capabilities of this camera
        /// </summary>
        protected eCameraCapabilities Capabilities { get; set; }

        /// <summary>
        /// Initializes a new instance of the CameraBase class with the specified device configuration
        /// </summary>
        /// <param name="config">The device configuration</param>
        protected CameraBase(DeviceConfig config) : base(config)
        {
            OutputPorts = new RoutingPortCollection<RoutingOutputPort>();

            ControlMode = eCameraControlMode.Manual;

        }

        /// <summary>
        /// Initializes a new instance of the CameraBase class with the specified key and name
        /// </summary>
        /// <param name="key">The unique key for this camera device</param>
        /// <param name="name">The friendly name for this camera device</param>
        protected CameraBase(string key, string name) :
            this(new DeviceConfig { Name = name, Key = key })
        {

        }

        /// <summary>
        /// Links the camera device to the API bridge for control and feedback
        /// </summary>
        /// <param name="cameraDevice">The camera device to link</param>
        /// <param name="trilist">The trilist for communication</param>
        /// <param name="joinStart">The starting join number for the camera controls</param>
        /// <param name="joinMapKey">The join map key for custom join mappings</param>
        /// <param name="bridge">The EiscApiAdvanced bridge for advanced join mapping</param>
        protected void LinkCameraToApi(CameraBase cameraDevice, BasicTriList trilist, uint joinStart, string joinMapKey,
            EiscApiAdvanced bridge)
        {
            CameraControllerJoinMap joinMap = new CameraControllerJoinMap(joinStart);

            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }
            else
            {
                Debug.LogMessage(LogEventLevel.Information, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
            }

            var customJoins = JoinMapHelper.TryGetJoinMapAdvancedForDevice(joinMapKey);

            if (customJoins != null)
            {
                joinMap.SetCustomJoinData(customJoins);
            }

            Debug.LogMessage(LogEventLevel.Debug, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
            Debug.LogMessage(LogEventLevel.Information, "Linking to Bridge Type {0}", cameraDevice.GetType().Name.ToString());

            var commMonitor = cameraDevice as ICommunicationMonitor;
            commMonitor.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(
                trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);

            var ptzCamera = cameraDevice as IHasCameraPtzControl;

            if (ptzCamera != null)
            {
                trilist.SetBoolSigAction(joinMap.PanLeft.JoinNumber, (b) =>
                {
                    if (b)
                    {
                        ptzCamera.PanLeft();
                    }
                    else
                    {
                        ptzCamera.PanStop();
                    }
                });
                trilist.SetBoolSigAction(joinMap.PanRight.JoinNumber, (b) =>
                {
                    if (b)
                    {
                        ptzCamera.PanRight();
                    }
                    else
                    {
                        ptzCamera.PanStop();
                    }
                });

                trilist.SetBoolSigAction(joinMap.TiltUp.JoinNumber, (b) =>
                {
                    if (b)
                    {
                        ptzCamera.TiltUp();
                    }
                    else
                    {
                        ptzCamera.TiltStop();
                    }
                });
                trilist.SetBoolSigAction(joinMap.TiltDown.JoinNumber, (b) =>
                {
                    if (b)
                    {
                        ptzCamera.TiltDown();
                    }
                    else
                    {
                        ptzCamera.TiltStop();
                    }
                });

                trilist.SetBoolSigAction(joinMap.ZoomIn.JoinNumber, (b) =>
                {
                    if (b)
                    {
                        ptzCamera.ZoomIn();
                    }
                    else
                    {
                        ptzCamera.ZoomStop();
                    }
                });

                trilist.SetBoolSigAction(joinMap.ZoomOut.JoinNumber, (b) =>
                {
                    if (b)
                    {
                        ptzCamera.ZoomOut();
                    }
                    else
                    {
                        ptzCamera.ZoomStop();
                    }
                });
            }

            var powerCamera = cameraDevice as IHasPowerControl;
            if (powerCamera != null)
            {
                trilist.SetSigTrueAction(joinMap.PowerOn.JoinNumber, () => powerCamera.PowerOn());
                trilist.SetSigTrueAction(joinMap.PowerOff.JoinNumber, () => powerCamera.PowerOff());

                var powerFbCamera = powerCamera as IHasPowerControlWithFeedback;
                if (powerFbCamera != null)
                {
                    powerFbCamera.PowerIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PowerOn.JoinNumber]);
                    powerFbCamera.PowerIsOnFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.PowerOff.JoinNumber]);
                }
            }

            if (cameraDevice is ICommunicationMonitor)
            {
                var monitoredCamera = cameraDevice as ICommunicationMonitor;
                monitoredCamera.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(
                    trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);
            }

            if (cameraDevice is IHasCameraPresets)
            {
                // Set the preset lables when they change
                var presetsCamera = cameraDevice as IHasCameraPresets;
                presetsCamera.PresetsListHasChanged += new EventHandler<EventArgs>((o, a) =>
                {
                    SendCameraPresetNamesToApi(presetsCamera, joinMap, trilist);
                });

                SendCameraPresetNamesToApi(presetsCamera, joinMap, trilist);

                for (int i = 0; i < joinMap.PresetRecallStart.JoinSpan; i++)
                {
                    int tempNum = i;

                    trilist.SetSigTrueAction((ushort)(joinMap.PresetRecallStart.JoinNumber + tempNum), () =>
                    {
                        presetsCamera.PresetSelect(tempNum);
                    });
                    trilist.SetSigTrueAction((ushort)(joinMap.PresetSaveStart.JoinNumber + tempNum), () =>
                    {
                        var label = trilist.GetString((ushort)(joinMap.PresetLabelStart.JoinNumber + tempNum));

                        presetsCamera.PresetStore(tempNum, label);
                    });
                }
                trilist.OnlineStatusChange += (sender, args) =>
                {
                    if (!args.DeviceOnLine)
                    { return; }

                    SendCameraPresetNamesToApi(presetsCamera, joinMap, trilist);
                };

            }
        }
        private void SendCameraPresetNamesToApi(IHasCameraPresets presetsCamera, CameraControllerJoinMap joinMap, BasicTriList trilist)
        {
            for (int i = 1; i <= joinMap.NumberOfPresets.JoinNumber; i++)
            {
                int tempNum = i - 1;

                string label = "";

                var preset = presetsCamera.Presets.FirstOrDefault(p => p.ID.Equals(i));

                if (preset != null)
                    label = preset.Description;

                trilist.SetString((ushort)(joinMap.PresetLabelStart.JoinNumber + tempNum), label);
            }
        }
    }


    /// <summary>
    /// Represents a CameraPreset
    /// </summary>
    public class CameraPreset : PresetBase
    {
        /// <summary>
        /// Initializes a new instance of the CameraPreset class
        /// </summary>
        /// <param name="id">The preset ID</param>
        /// <param name="description">The preset description</param>
        /// <param name="isDefined">Whether the preset is defined</param>
        /// <param name="isDefinable">Whether the preset can be defined</param>
        public CameraPreset(int id, string description, bool isDefined, bool isDefinable)
            : base(id, description, isDefined, isDefinable)
        {

        }
    }


    /// <summary>
    /// Represents a CameraPropertiesConfig
    /// </summary>
    public class CameraPropertiesConfig
    {
        /// <summary>
        /// Gets or sets the CommunicationMonitorProperties
        /// </summary>
        public CommunicationMonitorConfig CommunicationMonitorProperties { get; set; }

        /// <summary>
        /// Gets or sets the Control
        /// </summary>
        public ControlPropertiesConfig Control { get; set; }

        /// <summary>
        /// Gets or sets the SupportsAutoMode
        /// </summary>
        [JsonProperty("supportsAutoMode")]
        public bool SupportsAutoMode { get; set; }

        /// <summary>
        /// Gets or sets the SupportsOffMode
        /// </summary>
        [JsonProperty("supportsOffMode")]
        public bool SupportsOffMode { get; set; }

        /// <summary>
        /// Gets or sets the Presets
        /// </summary>
        [JsonProperty("presets")]
        public List<CameraPreset> Presets { get; set; }
    }
}