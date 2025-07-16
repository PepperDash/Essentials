using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Devices.Common.Cameras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Messenger for devices that implement the IHasCameras interface.
    /// </summary>
    public class IHasCamerasMessenger : MessengerBase
    {
        /// <summary>
        /// Device being bridged that implements IHasCameras interface.
        /// </summary>
        public IHasCameras CameraController { get; private set; }

        /// <summary>
        /// Messenger for devices that implement IHasCameras interface.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cameraController"></param>
        /// <param name="messagePath"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public IHasCamerasMessenger(string key, IHasCameras cameraController, string messagePath)
            : base(key, messagePath, cameraController)
        {
            CameraController = cameraController ?? throw new ArgumentNullException("cameraController");
            CameraController.CameraSelected += CameraController_CameraSelected;
        }

        private void CameraController_CameraSelected(object sender, CameraSelectedEventArgs e)
        {
            PostStatusMessage(new IHasCamerasStateMessage
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

            AddAction("/fullStatus", (id, context) =>
            {
                SendFullStatus();
            });

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

        private void SendFullStatus()
        {
            var state = new IHasCamerasStateMessage
            {
                CameraList = CameraController.Cameras,
                SelectedCamera = CameraController.SelectedCamera
            };

            PostStatusMessage(state);
        }


    }

    /// <summary>
    /// State message for devices that implement the IHasCameras interface.
    /// </summary>
    public class IHasCamerasStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// List of cameras available in the device.
        /// </summary>
        [JsonProperty("cameraList", NullValueHandling = NullValueHandling.Ignore)]
        public List<CameraBase> CameraList { get; set; }

        /// <summary>
        /// The currently selected camera on the device.
        /// </summary>
        [JsonProperty("selectedCamera", NullValueHandling = NullValueHandling.Ignore)]
        public CameraBase SelectedCamera { get; set; }

    }
}
