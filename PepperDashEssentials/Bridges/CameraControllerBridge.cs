using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common;
using PepperDash.Essentials.Devices.Common.Cameras;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Bridges
{
    public static class CameraControllerApiExtensions
    {
        public static void LinkToApi(this PepperDash.Essentials.Devices.Common.Cameras.CameraBase cameraDevice, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            CameraControllerJoinMap joinMap = new CameraControllerJoinMap();

            var joinMapSerialized = JoinMapHelper.GetJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<CameraControllerJoinMap>(joinMapSerialized);

            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
            Debug.Console(0, "Linking to Bridge Type {0}", cameraDevice.GetType().Name.ToString());

            var commMonitor = cameraDevice as ICommunicationMonitor;
            commMonitor.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.GetJoinForKey(CameraControllerJoinMap.IsOnline)]);

            var ptzCamera = cameraDevice as IHasCameraPtzControl;

            if (ptzCamera != null)
            {
                trilist.SetBoolSigAction(joinMap.GetJoinForKey(CameraControllerJoinMap.PanLeft), (b) =>
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
                trilist.SetBoolSigAction(joinMap.GetJoinForKey(CameraControllerJoinMap.PanRight), (b) =>
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

                trilist.SetBoolSigAction(joinMap.GetJoinForKey(CameraControllerJoinMap.TiltUp), (b) =>
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
                trilist.SetBoolSigAction(joinMap.GetJoinForKey(CameraControllerJoinMap.TiltDown), (b) =>
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

                trilist.SetBoolSigAction(joinMap.GetJoinForKey(CameraControllerJoinMap.ZoomIn), (b) =>
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

                trilist.SetBoolSigAction(joinMap.GetJoinForKey(CameraControllerJoinMap.ZoomOut), (b) =>
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
                trilist.SetSigTrueAction(joinMap.GetJoinForKey(CameraControllerJoinMap.PowerOn), () => powerCamera.PowerOn());
                trilist.SetSigTrueAction(joinMap.GetJoinForKey(CameraControllerJoinMap.PowerOff), () => powerCamera.PowerOff());
             
                powerCamera.PowerIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.GetJoinForKey(CameraControllerJoinMap.PowerOn)]);
                powerCamera.PowerIsOnFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.GetJoinForKey(CameraControllerJoinMap.PowerOff)]);
            }

            if (cameraDevice is ICommunicationMonitor)
            {
                var monitoredCamera = cameraDevice as ICommunicationMonitor;
                monitoredCamera.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.GetJoinForKey(CameraControllerJoinMap.IsOnline)]);
            }

            if (cameraDevice is IHasCameraPresets)
            {
                // Set the preset lables when they change
                var presetsCamera = cameraDevice as IHasCameraPresets;
                presetsCamera.PresetsListHasChanged += new EventHandler<EventArgs>((o, a) =>
                {
                    for (int i = 1; i <= joinMap.GetJoinForKey(CameraControllerJoinMap.NumberOfPresets); i++)
                    {
                        int tempNum = i - 1;

                        string label = "" ;

                        var preset = presetsCamera.Presets.FirstOrDefault(p => p.ID.Equals(i));

                        if (preset != null)
                            label = preset.Description;

                        trilist.SetString((ushort)(joinMap.GetJoinForKey(CameraControllerJoinMap.PresetLabelStart) + tempNum), label);
                    }
                });
                
                for (int i = 0; i < joinMap.GetJoinForKey(CameraControllerJoinMap.NumberOfPresets); i++)
                {
                    int tempNum = i;

                    trilist.SetSigTrueAction((ushort)(joinMap.GetJoinForKey(CameraControllerJoinMap.PresetRecallStart) + tempNum), () =>
                        {
                            presetsCamera.PresetSelect(tempNum);
                        });
                    trilist.SetSigTrueAction((ushort)(joinMap.GetJoinForKey(CameraControllerJoinMap.PresetSaveStart) + tempNum), () =>
                        {
                            var label = trilist.GetString(joinMap.GetJoinForKey(CameraControllerJoinMap.PresetLabelStart + tempNum));

                            presetsCamera.PresetStore(tempNum, label);
                        });
                }
            }
        }
    }

}