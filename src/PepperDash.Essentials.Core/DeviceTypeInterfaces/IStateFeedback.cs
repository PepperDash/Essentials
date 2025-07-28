using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{

  /// <summary>
  /// Interface for devices that provide state feedback.
  /// This interface is used to standardize access to state feedback across different devices.
  /// </summary>
  public interface IStateFeedback
  {
    /// <summary>
    /// Gets the state feedback for the device.
    /// This property provides a BoolFeedback that represents the current state (on/off) of the device.
    /// </summary>
    BoolFeedback StateFeedback { get; }
  }
}
