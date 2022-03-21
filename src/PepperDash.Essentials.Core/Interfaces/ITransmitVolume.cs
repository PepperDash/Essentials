namespace PepperDash.Essentials.Core.Interfaces
{
    /// <summary>
    /// Adds control of codec transmit volume
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
}