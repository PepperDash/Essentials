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
            commMonitor.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);

            var ptzCamera = cameraDevice as IHasCameraPtzControl;

            if (ptzCamera != null)
            {
                trilist.SetBoolSigAction(joinMap.Left, (b) =>
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
                trilist.SetBoolSigAction(joinMap.Right, (b) =>
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

                trilist.SetBoolSigAction(joinMap.Up, (b) =>
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
                trilist.SetBoolSigAction(joinMap.Down, (b) =>
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

                trilist.SetBoolSigAction(joinMap.ZoomIn, (b) =>
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

                trilist.SetBoolSigAction(joinMap.ZoomOut, (b) =>
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

            if (cameraDevice.GetType().Name.ToString().ToLower() == "cameravisca")
            {
                var viscaCamera = cameraDevice as PepperDash.Essentials.Devices.Common.Cameras.CameraVisca;
                trilist.SetSigTrueAction(joinMap.PowerOn, () => viscaCamera.PowerOn());
                trilist.SetSigTrueAction(joinMap.PowerOff, () => viscaCamera.PowerOff());

                viscaCamera.PowerIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PowerOn]);
                viscaCamera.PowerIsOnFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.PowerOff]);

                viscaCamera.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);
                for (int i = 0; i < joinMap.NumberOfPresets; i++)
                {
                    int tempNum = i;
                    trilist.SetSigTrueAction((ushort)(joinMap.PresetRecallOffset + tempNum), () =>
                        {
                            viscaCamera.RecallPreset(tempNum);
                        });
                    trilist.SetSigTrueAction((ushort)(joinMap.PresetSaveOffset + tempNum), () =>
                    {
                        viscaCamera.SavePreset(tempNum);
                    });
                }
            }
        }
    }

}