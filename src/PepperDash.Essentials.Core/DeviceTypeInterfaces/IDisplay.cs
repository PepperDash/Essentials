using PepperDash.Core;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    public interface IDisplay: IHasFeedback, IRoutingSinkWithSwitching, IHasPowerControl, IWarmingCooling, IUsageTracking, IKeyName
    {
    }
}
