using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Defines the contract for IHasSelfviewPosition
    /// </summary>
    public interface IHasSelfviewPosition
    {
        /// <summary>
        /// Gets the SelfviewPipPositionFeedback
        /// </summary>
        StringFeedback SelfviewPipPositionFeedback { get; }

        /// <summary>
        /// Sets the selfview position
        /// </summary>
        void SelfviewPipPositionSet(CodecCommandWithLabel position);

        /// <summary>
        /// Toggles the selfview position
        /// </summary>
        void SelfviewPipPositionToggle();
    }
}