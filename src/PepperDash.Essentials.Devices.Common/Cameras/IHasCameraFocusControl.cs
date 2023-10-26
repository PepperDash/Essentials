namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Interface for camera focus control
    /// </summary>
    public interface IHasCameraFocusControl : IHasCameraControls
    {
        void FocusNear();
        void FocusFar();
        void FocusStop();

        void TriggerAutoFocus();
    }
}