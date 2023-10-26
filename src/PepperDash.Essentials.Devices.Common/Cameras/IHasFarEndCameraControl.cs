using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.Cameras
{
    public interface IHasFarEndCameraControl
    {
        CameraBase FarEndCamera { get; }

        BoolFeedback ControllingFarEndCameraFeedback { get; }

    }
}