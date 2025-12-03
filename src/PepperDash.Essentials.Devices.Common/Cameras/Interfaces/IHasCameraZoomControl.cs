
namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Defines the contract for IHasCameraZoomControl
    /// </summary>
    public interface IHasCameraZoomControl : IHasCameraControls
    {
        /// <summary>
        /// Zooms the camera in
        /// </summary>
        void ZoomIn();

        /// <summary>
        /// Zooms the camera out
        /// </summary>
        void ZoomOut();

        /// <summary>
        /// Stops the camera zoom movement
        /// </summary>
        void ZoomStop();
    }
}
