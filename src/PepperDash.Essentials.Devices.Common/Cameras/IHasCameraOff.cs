using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// To be implmented on codecs that can disable their camera(s) to blank the near end video
    /// </summary>
    public interface IHasCameraOff
    {
        BoolFeedback CameraIsOffFeedback { get; }
        void CameraOff();
    }
}