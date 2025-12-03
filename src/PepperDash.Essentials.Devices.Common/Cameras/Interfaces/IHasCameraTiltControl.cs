
namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Defines the contract for IHasCameraTiltControl
    /// </summary>
    public interface IHasCameraTiltControl : IHasCameraControls
    {
        /// <summary>
        /// Tilts the camera down
        /// </summary>
        void TiltDown();

        /// <summary>
        /// Tilts the camera up
        /// </summary>
        void TiltUp();

        /// <summary>
        /// Stops the camera tilt movement
        /// </summary>
        void TiltStop();
    }
}
