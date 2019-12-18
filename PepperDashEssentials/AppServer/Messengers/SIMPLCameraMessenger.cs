using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.EthernetCommunication;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Bridges;
using PepperDash.Essentials.Devices.Common.Cameras;

namespace PepperDash.Essentials.AppServer.Messengers
{
    public class SIMPLCameraMessenger : MessengerBase
    {
        BasicTriList EISC;

        CameraControllerJoinMap JoinMap;

        //public class BoolJoin
        //{
        //    /// <summary>
        //    /// 1
        //    /// </summary>
        //    public const uint CameraControlUp = 1;
        //    /// <summary>
        //    /// 2
        //    /// </summary>
        //    public const uint CameraControlDown = 2;
        //    /// <summary>
        //    /// 3
        //    /// </summary>
        //    public const uint CameraControlLeft = 3;
        //    /// <summary>
        //    /// 4
        //    /// </summary>
        //    public const uint CameraControlRight = 4;
        //    /// <summary>
        //    /// 5
        //    /// </summary>
        //    public const uint CameraControlZoomIn = 5;
        //    /// <summary>
        //    /// 6
        //    /// </summary>
        //    public const uint CameraControlZoomOut = 6;
        //    /// <summary>
        //    /// 10
        //    /// </summary>
        //    public const uint CameraHasPresets = 10;
        //    /// <summary>
        //    /// 11 - 20
        //    /// </summary>
        //    public const uint CameraPresetStart = 10;

        //    /// <summary>
        //    /// 21
        //    /// </summary>
        //    public const uint CameraModeAuto = 21;
        //    /// <summary>
        //    /// 22
        //    /// </summary>
        //    public const uint CameraModeManual = 22;
        //    /// <summary>
        //    /// 23
        //    /// </summary>
        //    public const uint CameraModeOff = 23;
        //    /// <summary>
        //    /// 24
        //    /// </summary>
        //    public const uint CameraSupportsModeAuto = 24;
        //    /// <summary>
        //    /// 25
        //    /// </summary>
        //    public const uint CameraSupportsModeOff = 25;
        //}

        //public class UshortJoin
        //{
        //    /// <summary>
        //    /// 10
        //    /// </summary>
        //    public const uint CameraPresetCount = 10;
        //}

        //public class StringJoin
        //{
        //    /// <summary>
        //    /// 11-20
        //    /// </summary>
        //    public const uint CameraPresetNameStart = 10;
        //}

        public SIMPLCameraMessenger(string key, BasicTriList eisc, string messagePath, uint joinStart)
            : base(key, messagePath)
        {
            EISC = eisc;
            //JoinStart = joinStart - 1;
            JoinMap = new CameraControllerJoinMap();

            JoinMap.OffsetJoinNumbers(joinStart);


            EISC.SetUShortSigAction(JoinMap.GetJoinForKey(CameraControllerJoinMap.NumberOfPresets), (u) => SendCameraFullMessageObject());

            EISC.SetBoolSigAction(JoinMap.GetJoinForKey(CameraControllerJoinMap.CameraModeAuto), (b) => PostCameraMode());
            EISC.SetBoolSigAction(JoinMap.GetJoinForKey(CameraControllerJoinMap.CameraModeManual), (b) => PostCameraMode());
            EISC.SetBoolSigAction(JoinMap.GetJoinForKey(CameraControllerJoinMap.CameraModeOff), (b) => PostCameraMode());
        }


        protected override void CustomRegisterWithAppServer(MobileControlSystemController appServerController)
        {
            var asc = appServerController;

            asc.AddAction(MessagePath + "/fullStatus", new Action(SendCameraFullMessageObject));

            // Add press and holds using helper action
            Action<string, uint> addPHAction = (s, u) =>
                asc.AddAction(MessagePath + s, new PressAndHoldAction(b => EISC.SetBool(u, b)));
            addPHAction("/cameraUp", JoinMap.GetJoinForKey(CameraControllerJoinMap.TiltUp));
            addPHAction("/cameraDown", JoinMap.GetJoinForKey(CameraControllerJoinMap.TiltDown));
            addPHAction("/cameraLeft", JoinMap.GetJoinForKey(CameraControllerJoinMap.PanLeft));
            addPHAction("/cameraRight", JoinMap.GetJoinForKey(CameraControllerJoinMap.PanRight));
            addPHAction("/cameraZoomIn", JoinMap.GetJoinForKey(CameraControllerJoinMap.ZoomIn));
            addPHAction("/cameraZoomOut", JoinMap.GetJoinForKey(CameraControllerJoinMap.ZoomOut));

            Action<string, uint> addAction = (s, u) =>
                asc.AddAction(MessagePath + s, new Action(() => EISC.PulseBool(u, 100)));

            addAction("/cameraModeAuto", JoinMap.GetJoinForKey(CameraControllerJoinMap.CameraModeAuto));
            addAction("/cameraModeManual", JoinMap.GetJoinForKey(CameraControllerJoinMap.CameraModeManual));
            addAction("/cameraModeOff", JoinMap.GetJoinForKey(CameraControllerJoinMap.CameraModeOff));

            var presetStart = JoinMap.GetJoinForKey(CameraControllerJoinMap.PresetRecallStart);
            var presetEnd = JoinMap.GetJoinForKey(CameraControllerJoinMap.PresetRecallStart) + JoinMap.GetJoinSpanForKey(CameraControllerJoinMap.PresetRecallStart);

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

            EISC.SetUShortSigAction(JoinMap.GetJoinForKey(CameraControllerJoinMap.NumberOfPresets), null);

            EISC.SetBoolSigAction(JoinMap.GetJoinForKey(CameraControllerJoinMap.CameraModeAuto), null);
            EISC.SetBoolSigAction(JoinMap.GetJoinForKey(CameraControllerJoinMap.CameraModeManual), null);
            EISC.SetBoolSigAction(JoinMap.GetJoinForKey(CameraControllerJoinMap.CameraModeOff), null);
        }

        /// <summary>
        /// Helper method to update the full status of the camera
        /// </summary>
        void SendCameraFullMessageObject()
        {
            var presetList = new List<CameraPreset>();

            // Build a list of camera presets based on the names and count 
            if (EISC.GetBool(JoinMap.GetJoinForKey(CameraControllerJoinMap.SupportsPresets)))
            {
                var presetStart = JoinMap.GetJoinForKey(CameraControllerJoinMap.PresetLabelStart);
                var presetEnd = JoinMap.GetJoinForKey(CameraControllerJoinMap.PresetLabelStart) + JoinMap.GetJoinForKey(CameraControllerJoinMap.NumberOfPresets);

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
                hasPresets = EISC.GetBool(JoinMap.GetJoinForKey(CameraControllerJoinMap.SupportsPresets)),
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
            if (EISC.GetBool(JoinMap.GetJoinForKey(CameraControllerJoinMap.CameraModeAuto))) m = eCameraControlMode.Auto.ToString().ToLower();
            else if (EISC.GetBool(JoinMap.GetJoinForKey(CameraControllerJoinMap.CameraModeManual))) m = eCameraControlMode.Manual.ToString().ToLower();
            else m = eCameraControlMode.Off.ToString().ToLower();
            return m;
        }
    }
}