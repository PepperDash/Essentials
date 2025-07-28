using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core.Touchpanels
{
    /// <summary>
    /// Defines the contract for IHasBasicTriListWithSmartObject
    /// </summary>
    public interface IHasBasicTriListWithSmartObject
    {
        BasicTriListWithSmartObject Panel { get; }
    }
}