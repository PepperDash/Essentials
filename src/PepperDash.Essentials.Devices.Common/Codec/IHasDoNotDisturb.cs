using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    /// <summary>
    /// Describes a device that has Do Not Disturb mode capability
    /// </summary>
    public interface IHasDoNotDisturbMode
    {
        /// <summary>
        /// Indictes whether Do Not Disturb mode is on (Activated)
        /// </summary>
        BoolFeedback DoNotDisturbModeIsOnFeedback { get; }

        /// <summary>
        /// Activates Do Not Disturb mode
        /// </summary>
        void ActivateDoNotDisturbMode();

        /// <summary>
        /// Deactivates Do Not Disturb mode
        /// </summary>
        void DeactivateDoNotDisturbMode();

        /// <summary>
        /// Toggles Do Not Disturb mode
        /// </summary>
        void ToggleDoNotDisturbMode();
    }

    /// <summary>
    /// Defines the contract for devices that support Do Not Disturb mode with timeout functionality
    /// </summary>
    public interface IHasDoNotDisturbModeWithTimeout : IHasDoNotDisturbMode
    {
        /// <summary>
        /// Activates Do Not Disturb mode with a timeout
        /// </summary>
        /// <param name="timeout"></param>
        void ActivateDoNotDisturbMode(int timeout);
    }
}