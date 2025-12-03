using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    /// <summary>
    /// Defines the requred elements for selfview control
    /// </summary>
    public interface IHasCodecSelfView
    {
        /// <summary>
        /// Feedback that indicates whether Selfview is on
        /// </summary>
        BoolFeedback SelfviewIsOnFeedback { get; }

        /// <summary>
        /// Setting that indicates whether the device shows selfview by default
        /// </summary>
        bool ShowSelfViewByDefault { get; }

        /// <summary>
        /// Turns selfview on
        /// </summary>
        void SelfViewModeOn();

        /// <summary>
        /// Turns selfview off
        /// </summary>
        void SelfViewModeOff();

        /// <summary>
        /// Toggles selfview mode
        /// </summary>
        void SelfViewModeToggle();
    }
}