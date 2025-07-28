using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.Rooms
{
    /// <summary>
    /// Defines the contract for IOccupancyStatusProvider
    /// </summary>
    public interface IOccupancyStatusProvider
    {
        BoolFeedback RoomIsOccupiedFeedback { get; }
    }
}