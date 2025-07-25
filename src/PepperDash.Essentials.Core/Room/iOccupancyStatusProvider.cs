namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines the contract for IOccupancyStatusProvider
    /// </summary>
    public interface IOccupancyStatusProvider
    {
        BoolFeedback RoomIsOccupiedFeedback { get; }
    }
}