namespace PepperDash.Essentials.Core.Interfaces
{
    /// <summary>
    /// For display classes that can provide usage data
    /// </summary>
    public interface IDisplayUsage
    {
        IntFeedback LampHours { get; }
    }
}