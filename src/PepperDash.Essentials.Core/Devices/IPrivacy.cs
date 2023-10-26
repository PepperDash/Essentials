namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Adds control of codec privacy function (microphone mute)
    /// </summary>
    public interface IPrivacy
    {
        void PrivacyModeOn();
        void PrivacyModeOff();
        void PrivacyModeToggle();
        BoolFeedback PrivacyModeIsOnFeedback { get; }
    }
}