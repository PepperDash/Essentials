namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Adds control of codec receive volume
    /// </summary>
    public interface IReceiveVolume
    {
        // Break this out into 3 interfaces
        void SetReceiveVolume(ushort level);
        void ReceiveMuteOn();
        void ReceiveMuteOff();
        void ReceiveMuteToggle();
        IntFeedback ReceiveLevelFeedback { get; }
        BoolFeedback ReceiveMuteIsOnFeedback { get; }
    }

    /// <summary>
    /// Defines the contract for ITransmitVolume
    /// </summary>
    public interface ITransmitVolume
    {
        void SetTransmitVolume(ushort level);
        void TransmitMuteOn();
        void TransmitMuteOff();
        void TransmitMuteToggle();
        IntFeedback TransmitLevelFeedback { get; }
        BoolFeedback TransmitMuteIsOnFeedback { get; }
    }

    /// <summary>
    /// Defines the contract for IPrivacy
    /// </summary>
    public interface IPrivacy
    {
        void PrivacyModeOn();
        void PrivacyModeOff();
        void PrivacyModeToggle();
        BoolFeedback PrivacyModeIsOnFeedback { get; }
    }
}