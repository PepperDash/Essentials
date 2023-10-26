using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core
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