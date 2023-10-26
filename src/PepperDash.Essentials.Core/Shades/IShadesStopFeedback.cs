namespace PepperDash.Essentials.Core.Shades
{
    /// <summary>
    /// Basic feedback for shades/scene stopped
    /// </summary>
    public interface IShadesStopFeedback : IShadesOpenCloseStop
    {
        BoolFeedback IsStoppedFeedback { get; }
    }
}