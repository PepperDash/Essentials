namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// For rooms with a single display
    /// </summary>
    public interface IHasDefaultDisplay
    {
        IRoutingSinkWithSwitching DefaultDisplay { get; }
    }
}