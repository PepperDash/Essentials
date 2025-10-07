using Newtonsoft.Json;
using PepperDash.Essentials.Devices.Common.Cameras;
using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Messenger for devices that implement the IHasCameras interface.
    /// </summary>
    public class IHasCamerasWithControlMessenger : MessengerBase
    {
        /// <summary>
        /// Device being bridged that implements IHasCameras interface.
        /// </summary>
        public IHasCamerasWithControls CameraController { get; private set; }

        /// <summary>
        /// Messenger for devices that implement IHasCameras interface.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cameraController"></param>
        /// <param name="messagePath"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public IHasCamerasWithControlMessenger(string key, string messagePath, IHasCamerasWithControls cameraController)
            : base(key, messagePath, cameraController)
        {
            CameraController = cameraController ?? throw new ArgumentNullException("cameraController");
            CameraController.CameraSelected += CameraController_CameraSelected;
        }

        private void CameraController_CameraSelected(object sender, CameraSelectedEventArgs<IHasCameraControls> e)
        {
            PostStatusMessage(new IHasCamerasWithControlsStateMessage
            {
                SelectedCamera = e.SelectedCamera
            });
        }

        /// <summary>
        /// Registers the actions for this messenger.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, context) => SendFullStatus(id));

            AddAction("/cameraListStatus", (id, content) => SendFullStatus(id));

            AddAction("/selectCamera", (id, content) =>
            {
                var cameraKey = content?.ToObject<string>();

                if (!string.IsNullOrEmpty(cameraKey))
                {
                    CameraController.SelectCamera(cameraKey);
                }
                else
                {
                    throw new ArgumentException("Content must be a string representing the camera key");
                }
            });
        }

        private void SendFullStatus(string clientId)
        {
            var state = new IHasCamerasWithControlsStateMessage
            {
                CameraList = CameraController.Cameras,
                SelectedCamera = CameraController.SelectedCamera
            };

            PostStatusMessage(state, clientId);
        }


    }

    /// <summary>
    /// State message for devices that implement the IHasCameras interface.
    /// </summary>
    public class IHasCamerasWithControlsStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// List of cameras available in the device.
        /// </summary>
        [JsonProperty("cameraList", NullValueHandling = NullValueHandling.Ignore)]
        public List<IHasCameraControls> CameraList { get; set; }

        /// <summary>
        /// The currently selected camera on the device.
        /// </summary>
        [JsonProperty("selectedCamera", NullValueHandling = NullValueHandling.Ignore)]
        public IHasCameraControls SelectedCamera { get; set; }

    }
}
