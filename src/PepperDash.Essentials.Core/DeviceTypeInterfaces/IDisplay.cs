using PepperDash.Core;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Interface for display devices that can be controlled and monitored.
    /// This interface combines functionality for feedback, routing, power control,
    /// warming/cooling, usage tracking, and key name management.
    /// It is designed to be implemented by devices that require these capabilities,
    /// such as projectors, displays, and other visual output devices.
    /// </summary>
    public interface IDisplay : IHasFeedback, IRoutingSinkWithSwitching, IHasPowerControl, IWarmingCooling, IUsageTracking, IKeyName
    {
    }
}
