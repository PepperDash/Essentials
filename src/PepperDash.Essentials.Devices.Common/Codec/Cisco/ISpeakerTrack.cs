using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.Codec.Cisco
{
    /// <summary>
    /// Describes the available tracking modes for a Cisco codec
    /// </summary>
    public interface ISpeakerTrack : IKeyed
    {
        /// <summary>
        /// Indicates whether Speaker Track is available on the codec.
        /// </summary>
        bool SpeakerTrackAvailability { get; }

        /// <summary>
        /// 
        /// </summary>
        BoolFeedback SpeakerTrackAvailableFeedback { get; }

        /// <summary>
        /// Feedback indicating the current status of Speaker Track is off
        /// </summary>
        bool SpeakerTrackStatus { get; }

        /// <summary>
        /// Turns Speaker Track off
        /// </summary>
        void SpeakerTrackOff();
        /// <summary>
        /// Turns Speaker Track on
        /// </summary>
        void SpeakerTrackOn();
    }
}
