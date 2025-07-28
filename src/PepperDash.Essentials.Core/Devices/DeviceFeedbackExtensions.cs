using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.Devices
{
    public static class DeviceFeedbackExtensions
    {
        /// <summary>
        /// Attempts to get and return a feedback property from a device by name.
        /// If unsuccessful, returns null.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        /// <summary>
        /// GetFeedbackProperty method
        /// </summary>
        public static Feedback GetFeedbackProperty(this Device device, string propertyName)
        {
            var feedback = DeviceJsonApi.GetPropertyByName(device.Key, propertyName) as Feedback;

            if (feedback != null)
            {
                return feedback;
            }

            return null;
        }
    }
}