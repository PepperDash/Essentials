using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    public interface IHumiditySensor
    {
        /// <summary>
        ///  Reports the relative humidity level. Level ranging from 0 to 100 (for 0% to 100%
        ///  RH). EventIds: HumidityFeedbackFeedbackEventId will trigger to indicate change.
        /// </summary>
        IntFeedback HumidityFeedback { get; }

    }
}
