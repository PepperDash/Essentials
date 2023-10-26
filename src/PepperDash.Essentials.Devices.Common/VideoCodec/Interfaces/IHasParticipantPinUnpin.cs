using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
    /// <summary>
    /// Describes the ability to pin and unpin a participant in a meeting
    /// </summary>
    public interface IHasParticipantPinUnpin : IHasParticipants
    {
        IntFeedback NumberOfScreensFeedback { get; }
        int ScreenIndexToPinUserTo { get; }

        void PinParticipant(int userId, int screenIndex);
        void UnPinParticipant(int userId);
        void ToggleParticipantPinState(int userId, int screenIndex);
    }
}