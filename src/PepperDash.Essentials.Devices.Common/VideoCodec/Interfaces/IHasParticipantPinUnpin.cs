using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
  /// <summary>
  /// Defines the contract for IHasParticipantPinUnpin
  /// </summary>
  public interface IHasParticipantPinUnpin : IHasParticipants
  {
    /// <summary>
    /// Feedback that indicates the number of screens available for pinning participants
    /// </summary>
    IntFeedback NumberOfScreensFeedback { get; }

    /// <summary>
    /// Gets the screen index to pin the user to
    /// </summary>
    int ScreenIndexToPinUserTo { get; }

    /// <summary>
    /// Pins a participant to a screen
    /// </summary>
    /// <param name="userId">The user ID of the participant to pin</param>
    /// <param name="screenIndex">The screen index to pin the user to</param>
    void PinParticipant(int userId, int screenIndex);

    /// <summary>
    /// Unpins a participant
    /// </summary>
    /// <param name="userId">The user ID of the participant to unpin</param>
    void UnPinParticipant(int userId);

    /// <summary>
    /// Toggles the pin state of a participant
    /// </summary>
    /// <param name="userId">The user ID of the participant to toggle</param>
    /// <param name="screenIndex">The screen index to pin the user to</param>
    void ToggleParticipantPinState(int userId, int screenIndex);
  }
}