using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.Cameras
{
    public interface IHasCameraAutoMode : IHasCameraControls
    {
        void CameraAutoModeOn();
        void CameraAutoModeOff();
        void CameraAutoModeToggle();
        BoolFeedback CameraAutoModeIsOnFeedback { get; }
    }
}