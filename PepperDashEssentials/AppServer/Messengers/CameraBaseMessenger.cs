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
            SendCameraFullMessageObject();
        }

        protected override void CustomRegisterWithAppServer(MobileControlSystemController appServerController)
        {
            appServerController.AddAction(MessagePath + "/fullStatus", new Action(SendCameraFullMessageObject));

            var ptzCamera = Camera as IHasCameraPtzControl;

            if (ptzCamera != null)
            {
                //  Need to evaluate how to pass through these P&H actions.  Need a method that takes a bool maybe?
                AppServerController.AddAction(MessagePath + "/cameraUp", new PressAndHoldAction(ptzCamera.TiltUp));

            }

        }

        /// <summary>
        /// Helper method to update the full status of the camera
        /// </summary>
        void SendCameraFullMessageObject()
        {
            var presetsCamera = Camera as IHasCameraPresets;

            var presets = new List<CameraPreset>();

            if (presetsCamera != null)
                presets = presetsCamera.Presets;

            var info = new
            {
                cameraMode = GetCameraMode(),
                hasPresets = Camera as IHasCameraPresets,
                presets = presets
            };
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