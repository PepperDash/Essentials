using PepperDash.Core;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Defines the contract for a display device that has current input information. This is used to provide a common interface for the TwoWayDisplayBaseMessenger to get current input information without requiring the full TwoWayDisplayBase class
    /// </summary>
    public interface IDisplayCurrentInput : IKeyName
{
    /// <summary>
    /// Gets the Current Input feedback for the display device.
    /// </summary>
    StringFeedback CurrentInputFeedback { get; }
}