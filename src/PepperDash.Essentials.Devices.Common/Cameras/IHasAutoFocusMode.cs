namespace PepperDash.Essentials.Devices.Common.Cameras
{
    public interface IHasAutoFocusMode
    {
        void SetFocusModeAuto();
        void SetFocusModeManual();
        void ToggleFocusMode();
    }
}