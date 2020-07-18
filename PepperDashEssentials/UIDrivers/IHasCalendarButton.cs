using PepperDash.Essentials.Core;

namespace PepperDash.Essentials
{
    public interface IHasCalendarButton
    {
        EssentialsHuddleVtc1Room CurrentRoom { get; }
        void CalendarPress();
    }

    public interface IHasCallButton
    {
        EssentialsHuddleVtc1Room CurrentRoom { get; }
        void ShowActiveCallsList();
    }
}