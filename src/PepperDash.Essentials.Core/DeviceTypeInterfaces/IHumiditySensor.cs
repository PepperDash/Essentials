using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Defines the contract for IHumiditySensor
    /// </summary>
    public interface IHumiditySensor
    {
        /// <summary>
        ///  Reports the relative humidity level. Level ranging from 0 to 100 (for 0% to 100%
        ///  RH). EventIds: HumidityFeedbackFeedbackEventId will trigger to indicate change.
        /// </summary>
        IntFeedback HumidityFeedback { get; }

    }
}
