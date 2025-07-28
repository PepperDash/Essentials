using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.Devices
{
  /// <summary>
  /// Defines the contract for IOnline
  /// </summary>
  public interface IOnline
  {
    /// <summary>
    /// Gets a value indicating whether the device is online.
    /// </summary>
    BoolFeedback IsOnline { get; }
  }
}