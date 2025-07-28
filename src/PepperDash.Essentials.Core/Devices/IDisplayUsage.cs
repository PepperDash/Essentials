using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.Devices
{
  /// <summary>
  /// For display classes that can provide usage data
  /// </summary>
  public interface IDisplayUsage
  {
    IntFeedback LampHours { get; }
  }
}