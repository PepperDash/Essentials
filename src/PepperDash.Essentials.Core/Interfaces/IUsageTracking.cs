using PepperDash.Essentials.Core.Devices;

namespace PepperDash.Essentials.Core.Interfaces
{
    public interface IUsageTracking
    {
        UsageTracking UsageTracker { get; set; }
    }

    //public static class IUsageTrackingExtensions
    //{
    //    public static void EnableUsageTracker(this IUsageTracking device)
    //    {
    //        device.UsageTracker = new UsageTracking();
    //    }
    //}
}