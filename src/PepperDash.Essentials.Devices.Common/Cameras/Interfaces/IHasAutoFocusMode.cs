
namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Interface for devices that have auto focus mode control
    /// </summary>
    public interface IHasAutoFocusMode : IHasCameraControls
    {
        /// <summary>
        /// Sets the focus mode to auto or manual, or toggles between them.
        /// </summary>
        void SetFocusModeAuto();

        /// <summary>
        /// Sets the focus mode to manual, allowing for manual focus adjustments.
        /// </summary>
        void SetFocusModeManual();

        /// <summary>
        /// Toggles the focus mode between auto and manual.
        /// </summary>
        void ToggleFocusMode();
    }
}
