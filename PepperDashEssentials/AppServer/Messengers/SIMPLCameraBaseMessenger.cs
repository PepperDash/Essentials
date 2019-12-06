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
    public class SIMPLCameraBaseMessenger : MessengerBase
    {
        BasicTriList EISC;

        CameraBase Camera;

        uint JoinStart;

        /// <summary>
        /// 811
        /// </summary>
        const uint BCameraControlUp = 1;
        /// <summary>
        /// 812
        /// </summary>
        const uint BCameraControlDown = 2;
        /// <summary>
        /// 813
        /// </summary>
        const uint BCameraControlLeft = 3;
        /// <summary>
        /// 814
        /// </summary>
        const uint BCameraControlRight = 4;
        /// <summary>
        /// 815
        /// </summary>
        const uint BCameraControlZoomIn = 5;
        /// <summary>
        /// 816
        /// </summary>
        const uint BCameraControlZoomOut = 6;
        /// <summary>
        /// 821 - 826
        /// </summary>
        const uint BCameraPresetStart = 11;

        /// <summary>
        /// 831
        /// </summary>
        const uint BCameraModeAuto = 21;
        /// <summary>
        /// 832
        /// </summary>
        const uint BCameraModeManual = 22;
        /// <summary>
        /// 833
        /// </summary>
        const uint BCameraModeOff = 23;


        public SIMPLCameraBaseMessenger(string key, CameraBase camera, BasicTriList eisc, string messagePath, uint joinStart)
            : base(key, messagePath)
        {
            if (camera == null)
                throw new ArgumentNullException("camera");

            EISC = eisc;
            Camera = camera;
            JoinStart = joinStart;

            var presetsCamera = Camera as IHasCameraPresets;

            if (presetsCamera != null)
            {
                presetsCamera.PresetsListHasChanged += new EventHandler<EventArgs>(presetsCamera_PresetsListHasChanged);
            }
        }

        void presetsCamera_PresetsListHasChanged(object sender, EventArgs e)
        {
            var presetsCamera = Camera as IHasCameraPresets;

            var presetList = new List<CameraPreset>();

            if (presetsCamera != null)
                presetList = presetsCamera.Presets;

            PostStatusMessage(new
            {
                presets = presetList
            });
        }

        protected override void CustomRegisterWithAppServer(MobileControlSystemController appServerController)
        {
            var asc = appServerController;


            // Add press and holds using helper action
            Action<string, uint> addPHAction = (s, u) =>
                AppServerController.AddAction(MessagePath + s, new PressAndHoldAction(b => EISC.SetBool(u, b)));
            addPHAction("/cameraUp", BCameraControlUp + JoinStart);
            addPHAction("/cameraDown", BCameraControlDown + JoinStart);
            addPHAction("/cameraLeft", BCameraControlLeft + JoinStart);
            addPHAction("/cameraRight", BCameraControlRight + JoinStart);
            addPHAction("/cameraZoomIn", BCameraControlZoomIn + JoinStart);
            addPHAction("/cameraZoomOut", BCameraControlZoomOut + JoinStart);

            Action<string, uint> addAction = (s, u) =>
                AppServerController.AddAction(MessagePath + s, new Action(() => EISC.PulseBool(u, 100)));

            addAction("/cameraModeAuto", BCameraModeAuto);
            addAction("/cameraModeManual", BCameraModeManual);
            addAction("/cameraModeOff", BCameraModeOff);

            // camera presets
            for (uint i = 0; i < 6; i++)
            {
                addAction("/cameraPreset" + (i + 1), BCameraPresetStart + i);
            }

            asc.AddAction(MessagePath + "/fullStatus", new Action(SendCameraFullMessageObject));


        }

        /// <summary>
        /// Helper method to update the full status of the camera
        /// </summary>
        void SendCameraFullMessageObject()
        {
            var presetsCamera = Camera as IHasCameraPresets;

            var presetList = new List<CameraPreset>();

            if (presetsCamera != null)
                presetList = presetsCamera.Presets;

            PostStatusMessage(new
            {
                cameraMode = GetCameraMode(),
                hasPresets = Camera is IHasCameraPresets,
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
            if (EISC.GetBool(BCameraModeAuto)) m = eCameraControlMode.Auto.ToString().ToLower();
            else if (EISC.GetBool(BCameraModeManual)) m = eCameraControlMode.Manual.ToString().ToLower();
            else m = eCameraControlMode.Off.ToString().ToLower();
            return m;
        }
    }
}