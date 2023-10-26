namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// For display classes that can provide usage data
    /// </summary>
    public interface IDisplayUsage
    {
        IntFeedback LampHours { get; }
    }
}