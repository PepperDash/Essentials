using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Presets;
using PepperDash.Essentials.Devices.Common.Codec;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.Cameras
{
    public enum eCameraCapabilities
    {
        None = 0,
        Pan = 1,
        Tilt = 2, 
        Zoom = 4,
        Focus = 8
    }

    public abstract class CameraBase : EssentialsDevice, IRoutingOutputs
	{
        public eCameraControlMode ControlMode { get; protected set; }

        #region IRoutingOutputs Members

        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; protected set; }

        #endregion

        public bool CanPan
        {
            get
            {
                return (Capabilities & eCameraCapabilities.Pan) == eCameraCapabilities.Pan;
            }
        }

        public bool CanTilt
        {
            get
            {
                return (Capabilities & eCameraCapabilities.Tilt) == eCameraCapabilities.Tilt;
            }
        }

        public bool CanZoom
        {
            get
            {
                return (Capabilities & eCameraCapabilities.Zoom) == eCameraCapabilities.Zoom;
            }
        }

        public bool CanFocus
        {
            get
            {
                return (Capabilities & eCameraCapabilities.Focus) == eCameraCapabilities.Focus;
            }
        }

        // A bitmasked value to indicate the movement capabilites of this camera
        protected eCameraCapabilities Capabilities { get; set; }

        protected CameraBase(string key, string name) :
			base(key, name) 
        {
            OutputPorts = new RoutingPortCollection<RoutingOutputPort>();

            ControlMode = eCameraControlMode.Manual;
        }

        protected void LinkCameraToApi(CameraBase cameraDevice, BasicTriList trilist, uint joinStart, string joinMapKey,
            EiscApiAdvanced bridge)
        {
            CameraControllerJoinMap joinMap = new CameraControllerJoinMap(joinStart);

            // Adds the join map to the bridge
            bridge.AddJoinMap(cameraDevice.Key, joinMap);

            var customJoins = JoinMapHelper.TryGetJoinMapAdvancedForDevice(joinMapKey);

            if (customJoins != null)
            {
                joinMap.SetCustomJoinData(customJoins);
            }

            Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
            Debug.Console(0, "Linking to Bridge Type {0}", cameraDevice.GetType().Name.ToString());

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

            if (cameraDevice is IPower)
            {
                var powerCamera = cameraDevice as IPower;
                trilist.SetSigTrueAction(joinMap.PowerOn.JoinNumber, () => powerCamera.PowerOn());
                trilist.SetSigTrueAction(joinMap.PowerOff.JoinNumber, () => powerCamera.PowerOff());

                powerCamera.PowerIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PowerOn.JoinNumber]);
                powerCamera.PowerIsOnFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.PowerOff.JoinNumber]);
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
                    for (int i = 1; i <= joinMap.NumberOfPresets.JoinNumber; i++)
                    {
                        int tempNum = i - 1;

                        string label = "";

                        var preset = presetsCamera.Presets.FirstOrDefault(p => p.ID.Equals(i));

                        if (preset != null)
                            label = preset.Description;

                        trilist.SetString((ushort) (joinMap.PresetLabelStart.JoinNumber + tempNum), label);
                    }
                });

                for (int i = 0; i < joinMap.NumberOfPresets.JoinNumber; i++)
                {
                    int tempNum = i;

                    trilist.SetSigTrueAction((ushort) (joinMap.PresetRecallStart.JoinNumber + tempNum), () =>
                    {
                        presetsCamera.PresetSelect(tempNum);
                    });
                    trilist.SetSigTrueAction((ushort) (joinMap.PresetSaveStart.JoinNumber + tempNum), () =>
                    {
                        var label = trilist.GetString((ushort) (joinMap.PresetLabelStart.JoinNumber + tempNum));

                        presetsCamera.PresetStore(tempNum, label);
                    });
                }
            }
        }
	}

    public class CameraPreset : PresetBase
    {
        public CameraPreset(int id, string description, bool isDefined, bool isDefinable)
            : base(id, description, isDefined, isDefinable)
        {

        }
    }


	public class CameraPropertiesConfig
	{
		public CommunicationMonitorConfig CommunicationMonitorProperties { get; set; }

		public ControlPropertiesConfig Control { get; set; }

        [JsonProperty("supportsAutoMode")]
        public bool SupportsAutoMode { get; set; }

        [JsonProperty("supportsOffMode")]
        public bool SupportsOffMode { get; set; }

        [JsonProperty("presets")]
        public List<CameraPreset> Presets { get; set; }
	}
}