using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    /// <summary>
    /// Describes a device that has Standby Mode capability
    /// </summary>
    public interface IHasStandbyMode
    {
        /// <summary>
        /// Feedback that indicates whether Standby Mode is on
        /// </summary>
        BoolFeedback StandbyIsOnFeedback { get; }

        /// <summary>
        /// Activates Standby Mode
        /// </summary>
        void StandbyActivate();

        /// <summary>
        /// Deactivates Standby Mode
        /// </summary>
        void StandbyDeactivate();
    }

    /// <summary>
    /// Defines the contract for IHasHalfWakeMode
    /// </summary>
    public interface IHasHalfWakeMode : IHasStandbyMode
    {
        /// <summary>
        /// Feedback that indicates whether Half Wake Mode is on
        /// </summary>
        BoolFeedback HalfWakeModeIsOnFeedback { get; }

        /// <summary>
        /// Feedback that indicates whether the device is entering Standby Mode
        /// </summary>
        BoolFeedback EnteringStandbyModeFeedback { get; }

        /// <summary>
        /// Activates Half Wake Mode
        /// </summary>
        void HalfwakeActivate();
    }
}