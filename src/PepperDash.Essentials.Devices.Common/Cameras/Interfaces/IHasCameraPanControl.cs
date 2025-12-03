
namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Interface for camera pan control
    /// </summary>
    public interface IHasCameraPanControl : IHasCameraControls
    {
        /// <summary>
        /// Pans the camera left
        /// </summary>
        void PanLeft();

        /// <summary>
        /// Pans the camera right
        /// </summary>
        void PanRight();

        /// <summary>
        /// Stops the camera pan movement
        /// </summary>
        void PanStop();
    }
}
