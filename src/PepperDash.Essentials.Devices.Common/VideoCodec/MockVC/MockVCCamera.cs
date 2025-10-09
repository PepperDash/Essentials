using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Devices.Common.VideoCodec;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Represents a MockVCCamera
    /// </summary>
    public class MockVCCamera : CameraBase, IHasCameraPtzControl, IHasCameraFocusControl, IBridgeAdvanced
    {
        /// <summary>
        /// Gets the parent video codec
        /// </summary>
        protected VideoCodecBase ParentCodec { get; private set; }

        /// <summary>
        /// Initializes a new instance of the MockVCCamera class
        /// </summary>
        /// <param name="key">The device key</param>
        /// <param name="name">The device name</param>
        /// <param name="codec">The parent video codec</param>
        public MockVCCamera(string key, string name, VideoCodecBase codec)
            : base(key, name)
        {
            Capabilities = eCameraCapabilities.Pan | eCameraCapabilities.Tilt | eCameraCapabilities.Zoom | eCameraCapabilities.Focus;

            ParentCodec = codec;
        }

        #region IHasCameraPtzControl Members

        /// <summary>
        /// PositionHome method
        /// </summary>
        public void PositionHome()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Resetting to home position");
        }

        #endregion

        #region IHasCameraPanControl Members

        /// <summary>
        /// PanLeft method
        /// </summary>
        public void PanLeft()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Panning Left");
        }

        /// <summary>
        /// PanRight method
        /// </summary>
        public void PanRight()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Panning Right");
        }

        /// <summary>
        /// PanStop method
        /// </summary>
        public void PanStop()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Stopping Pan");
        }

        #endregion

        #region IHasCameraTiltControl Members

        /// <summary>
        /// TiltDown method
        /// </summary>
        public void TiltDown()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Tilting Down");
        }

        /// <summary>
        /// TiltUp method
        /// </summary>
        public void TiltUp()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Tilting Up");
        }

        /// <summary>
        /// TiltStop method
        /// </summary>
        public void TiltStop()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Stopping Tilt");
        }

        #endregion

        #region IHasCameraZoomControl Members

        /// <summary>
        /// ZoomIn method
        /// </summary>
        public void ZoomIn()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Zooming In");
        }

        /// <summary>
        /// ZoomOut method
        /// </summary>
        public void ZoomOut()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Zooming Out");
        }

        /// <summary>
        /// ZoomStop method
        /// </summary>
        public void ZoomStop()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Stopping Zoom");
        }

        #endregion

        #region IHasCameraFocusControl Members

        /// <summary>
        /// FocusNear method
        /// </summary>
        public void FocusNear()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Focusing Near");
        }

        /// <summary>
        /// FocusFar method
        /// </summary>
        public void FocusFar()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Focusing Far");
        }

        /// <summary>
        /// FocusStop method
        /// </summary>
        public void FocusStop()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Stopping Focus");
        }

        /// <summary>
        /// TriggerAutoFocus method
        /// </summary>
        public void TriggerAutoFocus()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "AutoFocus Triggered");
        }

        #endregion

        /// <summary>
        /// LinkToApi method
        /// </summary>
        public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            LinkCameraToApi(this, trilist, joinStart, joinMapKey, bridge);
        }
    }

    /// <summary>
    /// Represents a MockFarEndVCCamera
    /// </summary>
    public class MockFarEndVCCamera : CameraBase, IHasCameraPtzControl, IAmFarEndCamera, IBridgeAdvanced
    {
        /// <summary>
        /// Gets the parent video codec
        /// </summary>
        protected VideoCodecBase ParentCodec { get; private set; }

        /// <summary>
        /// Initializes a new instance of the MockFarEndVCCamera class
        /// </summary>
        /// <param name="key">The device key</param>
        /// <param name="name">The device name</param>
        /// <param name="codec">The parent video codec</param>
        public MockFarEndVCCamera(string key, string name, VideoCodecBase codec)
            : base(key, name)
        {
            Capabilities = eCameraCapabilities.Pan | eCameraCapabilities.Tilt | eCameraCapabilities.Zoom;

            ParentCodec = codec;
        }

        #region IHasCameraPtzControl Members

        /// <summary>
        /// PositionHome method
        /// </summary>
        public void PositionHome()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Resetting to home position");
        }

        #endregion

        #region IHasCameraPanControl Members

        /// <summary>
        /// PanLeft method
        /// </summary>
        public void PanLeft()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Panning Left");
        }

        /// <summary>
        /// PanRight method
        /// </summary>
        public void PanRight()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Panning Right");
        }

        /// <summary>
        /// PanStop method
        /// </summary>
        public void PanStop()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Stopping Pan");
        }

        #endregion

        #region IHasCameraTiltControl Members

        /// <summary>
        /// TiltDown method
        /// </summary>
        public void TiltDown()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Tilting Down");
        }

        /// <summary>
        /// TiltUp method
        /// </summary>
        public void TiltUp()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Tilting Up");
        }

        /// <summary>
        /// TiltStop method
        /// </summary>
        public void TiltStop()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Stopping Tilt");
        }

        #endregion

        #region IHasCameraZoomControl Members

        /// <summary>
        /// ZoomIn method
        /// </summary>
        public void ZoomIn()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Zooming In");
        }

        /// <summary>
        /// ZoomOut method
        /// </summary>
        public void ZoomOut()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Zooming Out");
        }

        /// <summary>
        /// ZoomStop method
        /// </summary>
        public void ZoomStop()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Stopping Zoom");
        }

        #endregion

        /// <summary>
        /// LinkToApi method
        /// </summary>
        public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            LinkCameraToApi(this, trilist, joinStart, joinMapKey, bridge);
        }
    }
}