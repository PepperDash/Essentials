using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.Room
{
    public interface IOccupancyStatusProvider
    {
        BoolFeedback RoomIsOccupiedFeedback { get; }
    }
}