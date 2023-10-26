using System;
using System.Collections.Generic;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.Cameras
{
    public interface IHasCameras
    {
        event EventHandler<CameraSelectedEventArgs> CameraSelected;

        List<CameraBase> Cameras { get; }

        CameraBase SelectedCamera { get; }

        StringFeedback SelectedCameraFeedback { get; }

        void SelectCamera(string key);
    }
}