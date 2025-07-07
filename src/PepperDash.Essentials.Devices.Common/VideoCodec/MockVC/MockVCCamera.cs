using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Devices.Common.VideoCodec;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.Cameras;

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
        Debug.LogMessage(LogEventLevel.Debug, this, "Resetting to home position");
    }

    #endregion

    #region IHasCameraPanControl Members

    public void PanLeft()
    {
        Debug.LogMessage(LogEventLevel.Debug, this, "Panning Left");
    }

    public void PanRight()
    {
        Debug.LogMessage(LogEventLevel.Debug, this, "Panning Right");
    }

    public void PanStop()
    {
        Debug.LogMessage(LogEventLevel.Debug, this, "Stopping Pan");
    }

    #endregion

    #region IHasCameraTiltControl Members

    public void TiltDown()
    {
        Debug.LogMessage(LogEventLevel.Debug, this, "Tilting Down");
    }

    public void TiltUp()
    {
        Debug.LogMessage(LogEventLevel.Debug, this, "Tilting Up");
    }

    public void TiltStop()
    {
        Debug.LogMessage(LogEventLevel.Debug, this, "Stopping Tilt");
    }

    #endregion

    #region IHasCameraZoomControl Members

    public void ZoomIn()
    {
        Debug.LogMessage(LogEventLevel.Debug, this, "Zooming In");
    }

    public void ZoomOut()
    {
        Debug.LogMessage(LogEventLevel.Debug, this, "Zooming Out");
    }

    public void ZoomStop()
    {
        Debug.LogMessage(LogEventLevel.Debug, this, "Stopping Zoom");
    }

    #endregion

    #region IHasCameraFocusControl Members

    public void FocusNear()
    {
        Debug.LogMessage(LogEventLevel.Debug, this, "Focusing Near");
    }

    public void FocusFar()
    {
        Debug.LogMessage(LogEventLevel.Debug, this, "Focusing Far");
    }

    public void FocusStop()
    {
        Debug.LogMessage(LogEventLevel.Debug, this, "Stopping Focus");
    }

    public void TriggerAutoFocus()
    {
        Debug.LogMessage(LogEventLevel.Debug, this, "AutoFocus Triggered");
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
        Debug.LogMessage(LogEventLevel.Debug, this, "Resetting to home position");
    }

    #endregion

    #region IHasCameraPanControl Members

    public void PanLeft()
    {
        Debug.LogMessage(LogEventLevel.Debug, this, "Panning Left");
    }

    public void PanRight()
    {
        Debug.LogMessage(LogEventLevel.Debug, this, "Panning Right");
    }

    public void PanStop()
    {
        Debug.LogMessage(LogEventLevel.Debug, this, "Stopping Pan");
    }

    #endregion

    #region IHasCameraTiltControl Members

    public void TiltDown()
    {
        Debug.LogMessage(LogEventLevel.Debug, this, "Tilting Down");
    }

    public void TiltUp()
    {
        Debug.LogMessage(LogEventLevel.Debug, this, "Tilting Up");
    }

    public void TiltStop()
    {
        Debug.LogMessage(LogEventLevel.Debug, this, "Stopping Tilt");
    }

    #endregion

    #region IHasCameraZoomControl Members

    public void ZoomIn()
    {
        Debug.LogMessage(LogEventLevel.Debug, this, "Zooming In");
    }

    public void ZoomOut()
    {
        Debug.LogMessage(LogEventLevel.Debug, this, "Zooming Out");
    }

    public void ZoomStop()
    {
        Debug.LogMessage(LogEventLevel.Debug, this, "Stopping Zoom");
    }

    #endregion

    public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
    {
        LinkCameraToApi(this, trilist, joinStart, joinMapKey, bridge);
    }
}