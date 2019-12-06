using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Cameras;

namespace PepperDash.Essentials.AppServer.Messengers
{
    public class CameraBaseMessenger : MessengerBase
    {
        /// <summary>
        /// Device being bridged
        /// </summary>
        public CameraBase Camera { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key"></param>
        /// <param name="camera"></param>
        /// <param name="messagePath"></param>
        public CameraBaseMessenger(string key, CameraBase camera, string messagePath)
            : base(key, messagePath)
        {
            if (camera == null)
                throw new ArgumentNullException("camera");

            Camera = camera;

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
            appServerController.AddAction(MessagePath + "/fullStatus", new Action(SendCameraFullMessageObject));

            var ptzCamera = Camera as IHasCameraPtzControl;

            if (ptzCamera != null)
            {
                
                //  Need to evaluate how to pass through these P&H actions.  Need a method that takes a bool maybe?
                AppServerController.AddAction(MessagePath + "/cameraUp", new PressAndHoldAction((b) =>
                    {
                        if (b)
                            ptzCamera.TiltUp();
                        else
                            ptzCamera.TiltStop();
                    }));
                AppServerController.AddAction(MessagePath + "/cameraDown", new PressAndHoldAction((b) =>
                {
                    if (b)
                        ptzCamera.TiltDown();
                    else
                        ptzCamera.TiltStop();
                }));
                AppServerController.AddAction(MessagePath + "/cameraLeft", new PressAndHoldAction((b) =>
                {
                    if (b)
                        ptzCamera.PanLeft();
                    else
                        ptzCamera.PanStop();
                }));
                AppServerController.AddAction(MessagePath + "/cameraRight", new PressAndHoldAction((b) =>
                {
                    if (b)
                        ptzCamera.PanRight();
                    else
                        ptzCamera.PanStop();
                }));
                AppServerController.AddAction(MessagePath + "/cameraZoomIn", new PressAndHoldAction((b) =>
                {
                    if (b)
                        ptzCamera.ZoomIn();
                    else
                        ptzCamera.ZoomStop();
                }));
                AppServerController.AddAction(MessagePath + "/cameraZoomOut", new PressAndHoldAction((b) =>
                {
                    if (b)
                        ptzCamera.ZoomOut();
                    else
                        ptzCamera.ZoomStop();
                }));
            }

            if (Camera is IHasCameraAutoMode)
            {
                appServerController.AddAction(MessagePath + "/cameraModeAuto", new Action((Camera as IHasCameraAutoMode).CameraAutoModeOn));
                appServerController.AddAction(MessagePath + "/cameraModeManual", new Action((Camera as IHasCameraAutoMode).CameraAutoModeOff));
            }

            if (Camera is IPower)
            {
                appServerController.AddAction(MessagePath + "/cameraModeOff", new Action((Camera as IPower).PowerOff));
            }

            var presetsCamera = Camera as IHasCameraPresets;

            if (presetsCamera != null)
            {
                for(int i = 1; i <= 6; i++)
                {
                    var preset = i;
                    appServerController.AddAction(MessagePath + "/cameraPreset" + i, new Action<int>((p) => presetsCamera.PresetSelect(preset)));
                }
            }
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
        /// Computes the current camera mode
        /// </summary>
        /// <returns></returns>
        string GetCameraMode()
        {
            string m;
            if (Camera is IHasCameraAutoMode && (Camera as IHasCameraAutoMode).CameraAutoModeIsOnFeedback.BoolValue)
                m = eCameraControlMode.Auto.ToString().ToLower();
            else if (Camera is IPower && !(Camera as IPower).PowerIsOnFeedback.BoolValue)
                m = eCameraControlMode.Off.ToString().ToLower();
            else
                m = eCameraControlMode.Manual.ToString().ToLower();
            return m;
        }
    }
}