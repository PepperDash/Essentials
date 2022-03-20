using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials.Devices.Common.Cameras
{
    public class MockVCCamera : CameraBase, IHasCameraPtzControl, IHasCameraFocusControl, IBridgeAdvanced
    {
        protected VideoCodecBase ParentCodec { get; private set; }


        public MockVCCamera(string key, string name, VideoCodecBase codec)
            : base(key, name)
        {
            Capabilities = eCameraCapabilities.Pan | eCameraCapabilities.Tilt | eCameraCapabilities.Zoom | eCameraCapabilities.Focus;

            ParentCodec = codec;
        }

        #region IHasCameraPtzControl Members

        public void PositionHome()
        {
            Debug.Console(1, this, "Resetting to home position");
        }

        #endregion

        #region IHasCameraPanControl Members

        public void PanLeft()
        {
            Debug.Console(1, this, "Panning Left");
        }

        public void PanRight()
        {
            Debug.Console(1, this, "Panning Right");
        }

        public void PanStop()
        {
            Debug.Console(1, this, "Stopping Pan");
        }

        #endregion

        #region IHasCameraTiltControl Members

        public void TiltDown()
        {
            Debug.Console(1, this, "Tilting Down");
        }

        public void TiltUp()
        {
            Debug.Console(1, this, "Tilting Up");
        }

        public void TiltStop()
        {
            Debug.Console(1, this, "Stopping Tilt");
        }

        #endregion

        #region IHasCameraZoomControl Members

        public void ZoomIn()
        {
            Debug.Console(1, this, "Zooming In");
        }

        public void ZoomOut()
        {
            Debug.Console(1, this, "Zooming Out");
        }

        public void ZoomStop()
        {
            Debug.Console(1, this, "Stopping Zoom");
        }

        #endregion

        #region IHasCameraFocusControl Members

        public void FocusNear()
        {
            Debug.Console(1, this, "Focusing Near");
        }

        public void FocusFar()
        {
            Debug.Console(1, this, "Focusing Far");
        }

        public void FocusStop()
        {
            Debug.Console(1, this, "Stopping Focus");
        }

        public void TriggerAutoFocus()
        {
            Debug.Console(1, this, "AutoFocus Triggered");
        }

        #endregion

        public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            LinkCameraToApi(this, trilist, joinStart, joinMapKey, bridge);
        }
    }

    public class MockFarEndVCCamera : CameraBase, IHasCameraPtzControl, IAmFarEndCamera, IBridgeAdvanced
    {
        protected VideoCodecBase ParentCodec { get; private set; }


        public MockFarEndVCCamera(string key, string name, VideoCodecBase codec)
            : base(key, name)
        {
            Capabilities = eCameraCapabilities.Pan | eCameraCapabilities.Tilt | eCameraCapabilities.Zoom;

            ParentCodec = codec;
        }

        #region IHasCameraPtzControl Members

        public void PositionHome()
        {
            Debug.Console(1, this, "Resetting to home position");
        }

        #endregion

        #region IHasCameraPanControl Members

        public void PanLeft()
        {
            Debug.Console(1, this, "Panning Left");
        }

        public void PanRight()
        {
            Debug.Console(1, this, "Panning Right");
        }

        public void PanStop()
        {
            Debug.Console(1, this, "Stopping Pan");
        }

        #endregion

        #region IHasCameraTiltControl Members

        public void TiltDown()
        {
            Debug.Console(1, this, "Tilting Down");
        }

        public void TiltUp()
        {
            Debug.Console(1, this, "Tilting Up");
        }

        public void TiltStop()
        {
            Debug.Console(1, this, "Stopping Tilt");
        }

        #endregion

        #region IHasCameraZoomControl Members

        public void ZoomIn()
        {
            Debug.Console(1, this, "Zooming In");
        }

        public void ZoomOut()
        {
            Debug.Console(1, this, "Zooming Out");
        }

        public void ZoomStop()
        {
            Debug.Console(1, this, "Stopping Zoom");
        }

        #endregion

        public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            LinkCameraToApi(this, trilist, joinStart, joinMapKey, bridge);
        }
    }
}