using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    /// <summary>
    /// Defines the required elements for layout control
    /// </summary>
    public interface IHasCodecLayouts
    {
        /// <summary>
        /// Feedback that indicates the current layout on the local display
        /// </summary>
        StringFeedback LocalLayoutFeedback { get; }

        /// <summary>
        /// Toggles the local layout
        /// </summary>
        void LocalLayoutToggle();

        /// <summary>
        /// Toggles the local layout to single prominent
        /// </summary>
        void LocalLayoutToggleSingleProminent();

        /// <summary>
        /// Toggle the MinMax layout
        /// </summary>
        void MinMaxLayoutToggle();
    }
}