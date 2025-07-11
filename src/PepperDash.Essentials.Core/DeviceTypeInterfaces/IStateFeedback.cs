using System;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{

  /// <summary>
  /// Interface for devices that provide audio meter feedback.
  /// This interface is used to standardize access to meter feedback across different devices.
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
