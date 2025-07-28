using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{

  /// <summary>
  /// Interface for devices that provide audio meter feedback.
  /// This interface is used to standardize access to meter feedback across different devices.
  /// </summary>
  public interface IMeterFeedback
  {
    /// <summary>
    /// Gets the meter feedback for the device.
    /// This property provides an IntFeedback that represents the current audio level or meter value.
    /// </summary>
    IntFeedback MeterFeedback { get; }
  }
}
