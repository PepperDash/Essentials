namespace PepperDash.Essentials.Devices.Common.Codec
{
    public interface IHasDoNotDisturbModeWithTimeout : IHasDoNotDisturbMode
    {
        /// <summary>
        /// Activates Do Not Disturb mode with a timeout
        /// </summary>
        /// <param name="timeout"></param>
        void ActivateDoNotDisturbMode(int timeout);
    }
}