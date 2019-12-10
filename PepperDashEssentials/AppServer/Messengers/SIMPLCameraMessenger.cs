using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.EthernetCommunication;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Cameras;

namespace PepperDash.Essentials.AppServer.Messengers
{
    public class SIMPLCameraMessenger : MessengerBase
    {
        BasicTriList EISC;

        uint JoinStart;

        public class BoolJoin
        {
            /// <summary>
            /// 1
            /// </summary>
            public const uint CameraControlUp = 1;
            /// <summary>
            /// 2
            /// </summary>
            public const uint CameraControlDown = 2;
            /// <summary>
            /// 3
            /// </summary>
            public const uint CameraControlLeft = 3;
            /// <summary>
            /// 4
            /// </summary>
            public const uint CameraControlRight = 4;
            /// <summary>
            /// 5
            /// </summary>
            public const uint CameraControlZoomIn = 5;
            /// <summary>
            /// 6
            /// </summary>
            public const uint CameraControlZoomOut = 6;
            /// <summary>
            /// 10
            /// </summary>
            public const uint CameraHasPresets = 10;
            /// <summary>
            /// 11 - 20
            /// </summary>
            public const uint CameraPresetStart = 10;

            /// <summary>
            /// 21
            /// </summary>
            public const uint CameraModeAuto = 21;
            /// <summary>
            /// 22
            /// </summary>
            public const uint CameraModeManual = 22;
            /// <summary>
            /// 23
            /// </summary>
            public const uint CameraModeOff = 23;
            /// <summary>
            /// 24
            /// </summary>
            public const uint CameraSupportsModeAuto = 24;
            /// <summary>
            /// 25
            /// </summary>
            public const uint CameraSupportsModeOff = 25;
        }

        public class UshortJoin
        {
            /// <summary>
            /// 10
            /// </summary>
            public const uint CameraPresetCount = 10;
        }

        public class StringJoin
        {
            /// <summary>
            /// 11-20
            /// </summary>
            public const uint CameraPresetNameStart = 10;
        }

        public SIMPLCameraMessenger(string key, BasicTriList eisc, string messagePath, uint joinStart)
            : base(key, messagePath)
        {
            EISC = eisc;
            JoinStart = joinStart - 1;

            EISC.SetUShortSigAction(UshortJoin.CameraPresetCount + JoinStart, (u) => SendCameraFullMessageObject());

            EISC.SetBoolSigAction(BoolJoin.CameraModeAuto, (b) => PostCameraMode());
            EISC.SetBoolSigAction(BoolJoin.CameraModeManual, (b) => PostCameraMode());
            EISC.SetBoolSigAction(BoolJoin.CameraModeOff, (b) => PostCameraMode());
        }


        protected override void CustomRegisterWithAppServer(MobileControlSystemController appServerController)
        {
            var asc = appServerController;


            // Add press and holds using helper action
            Action<string, uint> addPHAction = (s, u) =>
                AppServerController.AddAction(MessagePath + s, new PressAndHoldAction(b => EISC.SetBool(u, b)));
            addPHAction("/cameraUp", BoolJoin.CameraControlUp + JoinStart);
            addPHAction("/cameraDown", BoolJoin.CameraControlDown + JoinStart);
            addPHAction("/cameraLeft", BoolJoin.CameraControlLeft + JoinStart);
            addPHAction("/cameraRight", BoolJoin.CameraControlRight + JoinStart);
            addPHAction("/cameraZoomIn", BoolJoin.CameraControlZoomIn + JoinStart);
            addPHAction("/cameraZoomOut", BoolJoin.CameraControlZoomOut + JoinStart);

            Action<string, uint> addAction = (s, u) =>
                AppServerController.AddAction(MessagePath + s, new Action(() => EISC.PulseBool(u, 100)));

            addAction("/cameraModeAuto", BoolJoin.CameraModeAuto);
            addAction("/cameraModeManual", BoolJoin.CameraModeManual);
            addAction("/cameraModeOff", BoolJoin.CameraModeOff);

            // camera presets
            for (uint i = 1; i <= 6; i++)
            {
                addAction("/cameraPreset" + (i), BoolJoin.CameraPresetStart + i + JoinStart);
            }

            asc.AddAction(MessagePath + "/fullStatus", new Action(SendCameraFullMessageObject));


        }

        /// <summary>
        /// Helper method to update the full status of the camera
        /// </summary>
        void SendCameraFullMessageObject()
        {
            var presetList = new List<CameraPreset>();

            // Build a list of camera presets based on the names and count 
            if (EISC.GetBool(JoinStart + BoolJoin.CameraHasPresets))
            {
                for (uint i = 1; i <= EISC.GetUshort(UshortJoin.CameraPresetCount); i++)
                {
                    var presetName = EISC.GetString(StringJoin.CameraPresetNameStart + i + JoinStart);
                    var preset = new CameraPreset((int)i, presetName, string.IsNullOrEmpty(presetName), true);
                    presetList.Add(preset);
                }
            }

            PostStatusMessage(new
            {
                cameraMode = GetCameraMode(),
                hasPresets = EISC.GetBool(BoolJoin.CameraHasPresets),
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
            if (EISC.GetBool(BoolJoin.CameraModeAuto)) m = eCameraControlMode.Auto.ToString().ToLower();
            else if (EISC.GetBool(BoolJoin.CameraModeManual)) m = eCameraControlMode.Manual.ToString().ToLower();
            else m = eCameraControlMode.Off.ToString().ToLower();
            return m;
        }
    }
}