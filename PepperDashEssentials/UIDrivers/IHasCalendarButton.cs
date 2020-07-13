using PepperDash.Essentials.Core;

namespace PepperDash.Essentials
{
    public interface IHasCalendarButton
    {
        EssentialsRoomBase CurrentRoom { get; }
        void CalendarPress();
    }

    public interface IHasCallButton
    {
        EssentialsRoomBase CurrentRoom { get; }
        void ShowActiveCallsList();
    }
}