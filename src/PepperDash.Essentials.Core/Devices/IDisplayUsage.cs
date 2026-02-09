namespace PepperDash.Essentials.Core
{
  /// <summary>
  /// For display classes that can provide usage data
  /// </summary>
  public interface IDisplayUsage
  {
    /// <summary>
    /// Gets the LampHours
    /// </summary>
    IntFeedback LampHours { get; }
  }
}