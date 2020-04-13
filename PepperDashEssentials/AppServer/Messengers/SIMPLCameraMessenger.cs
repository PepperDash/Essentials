using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.EthernetCommunication;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Devices.Common.Cameras;

namespace PepperDash.Essentials.AppServer.Messengers
{
    public class SIMPLCameraMessenger : MessengerBase
    {
        BasicTriList EISC;

        CameraControllerJoinMap JoinMap;


        public SIMPLCameraMessenger(string key, BasicTriList eisc, string messagePath, uint joinStart)
            : base(key, messagePath)
        {
            EISC = eisc;

            JoinMap = new CameraControllerJoinMap(joinStart);

            EISC.SetUShortSigAction(JoinMap.NumberOfPresets.JoinNumber, (u) => SendCameraFullMessageObject());

            EISC.SetBoolSigAction(JoinMap.CameraModeAuto.JoinNumber, (b) => PostCameraMode());
            EISC.SetBoolSigAction(JoinMap.CameraModeManual.JoinNumber, (b) => PostCameraMode());
            EISC.SetBoolSigAction(JoinMap.CameraModeOff.JoinNumber, (b) => PostCameraMode());
        }


        protected override void CustomRegisterWithAppServer(MobileControlSystemController appServerController)
        {
            var asc = appServerController;

            asc.AddAction(MessagePath + "/fullStatus", new Action(SendCameraFullMessageObject));

            // Add press and holds using helper action
            Action<string, uint> addPHAction = (s, u) =>
                asc.AddAction(MessagePath + s, new PressAndHoldAction(b => EISC.SetBool(u, b)));
            addPHAction("/cameraUp", JoinMap.TiltUp.JoinNumber);
            addPHAction("/cameraDown", JoinMap.TiltDown.JoinNumber);
            addPHAction("/cameraLeft", JoinMap.PanLeft.JoinNumber);
            addPHAction("/cameraRight", JoinMap.PanRight.JoinNumber);
            addPHAction("/cameraZoomIn", JoinMap.ZoomIn.JoinNumber);
            addPHAction("/cameraZoomOut", JoinMap.ZoomOut.JoinNumber);

            Action<string, uint> addAction = (s, u) =>
                asc.AddAction(MessagePath + s, new Action(() => EISC.PulseBool(u, 100)));

            addAction("/cameraModeAuto", JoinMap.CameraModeAuto.JoinNumber);
            addAction("/cameraModeManual", JoinMap.CameraModeManual.JoinNumber);
            addAction("/cameraModeOff", JoinMap.CameraModeOff.JoinNumber);

            var presetStart = JoinMap.PresetRecallStart.JoinNumber;
            var presetEnd = JoinMap.PresetRecallStart.JoinNumber + JoinMap.PresetRecallStart.JoinSpan;

            int presetId = 1;
            // camera presets
            for (uint i = presetStart; i <= presetEnd; i++)
            {
                addAction("/cameraPreset" + (presetId), i);
                presetId++;
            }
        }

        public void CustomUnregsiterWithAppServer(MobileControlSystemController appServerController)
        {
            appServerController.RemoveAction(MessagePath + "/fullStatus");

            appServerController.RemoveAction(MessagePath + "/cameraUp");
            appServerController.RemoveAction(MessagePath + "/cameraDown");
            appServerController.RemoveAction(MessagePath + "/cameraLeft");
            appServerController.RemoveAction(MessagePath + "/cameraRight");
            appServerController.RemoveAction(MessagePath + "/cameraZoomIn");
            appServerController.RemoveAction(MessagePath + "/cameraZoomOut");
            appServerController.RemoveAction(MessagePath + "/cameraModeAuto");
            appServerController.RemoveAction(MessagePath + "/cameraModeManual");
            appServerController.RemoveAction(MessagePath + "/cameraModeOff");

            EISC.SetUShortSigAction(JoinMap.NumberOfPresets.JoinNumber, null);

            EISC.SetBoolSigAction(JoinMap.CameraModeAuto.JoinNumber, null);
            EISC.SetBoolSigAction(JoinMap.CameraModeManual.JoinNumber, null);
            EISC.SetBoolSigAction(JoinMap.CameraModeOff.JoinNumber, null);
        }

        /// <summary>
        /// Helper method to update the full status of the camera
        /// </summary>
        void SendCameraFullMessageObject()
        {
            var presetList = new List<CameraPreset>();

            // Build a list of camera presets based on the names and count 
            if (EISC.GetBool(JoinMap.SupportsPresets.JoinNumber))
            {
                var presetStart = JoinMap.PresetLabelStart.JoinNumber;
                var presetEnd = JoinMap.PresetLabelStart.JoinNumber + JoinMap.NumberOfPresets.JoinNumber;

                var presetId = 1;
                for (uint i = presetStart; i < presetEnd; i++)
                {
                    var presetName = EISC.GetString(i);
                    var preset = new CameraPreset(presetId, presetName, string.IsNullOrEmpty(presetName), true);
                    presetList.Add(preset);
                    presetId++;
                }
            }

            PostStatusMessage(new
            {
                cameraMode = GetCameraMode(),
                hasPresets = EISC.GetBool(JoinMap.SupportsPresets.JoinNumber),
                presets = presetList
            });
        }

        /// <summary>
        /// 
        /// </summary>
        void PostCameraMode()
        {
            PostStatusMessage(new
            {
                cameraMode = GetCameraMode()
            });
        }

        /// <summary>
        /// Computes the current camera mode
        /// </summary>
        /// <returns></returns>
        string GetCameraMode()
        {
            string m;
            if (EISC.GetBool(JoinMap.CameraModeAuto.JoinNumber)) m = eCameraControlMode.Auto.ToString().ToLower();
            else if (EISC.GetBool(JoinMap.CameraModeManual.JoinNumber)) m = eCameraControlMode.Manual.ToString().ToLower();
            else m = eCameraControlMode.Off.ToString().ToLower();
            return m;
        }
    }
}