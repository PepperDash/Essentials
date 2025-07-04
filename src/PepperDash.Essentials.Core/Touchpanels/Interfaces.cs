using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core;


/// <summary>
/// Defines the contract for IHasBasicTriListWithSmartObject
/// </summary>
public interface IHasBasicTriListWithSmartObject
{
    /// <summary>
    /// Gets the Panel
    /// </summary>
    BasicTriListWithSmartObject Panel { get; }
}
