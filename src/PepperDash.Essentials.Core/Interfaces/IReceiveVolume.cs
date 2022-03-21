namespace PepperDash.Essentials.Core.Interfaces
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
}