using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    /// <summary>
    /// Describes a device that has Standby Mode capability
    /// </summary>
    public interface IHasStandbyMode
    {
        BoolFeedback StandbyIsOnFeedback { get; }

        void StandbyActivate();

        void StandbyDeactivate();
    }

    /// <summary>
    /// Defines the contract for IHasHalfWakeMode
    /// </summary>
    public interface IHasHalfWakeMode : IHasStandbyMode
    {
        BoolFeedback HalfWakeModeIsOnFeedback { get; }

        BoolFeedback EnteringStandbyModeFeedback { get; }

        void HalfwakeActivate();
    }
}