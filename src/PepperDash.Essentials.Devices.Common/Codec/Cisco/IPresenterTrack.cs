using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.Codec.Cisco
{
    /// <summary>
    /// Describes the available tracking modes for a Cisco codec's Presenter Track feature.
    /// </summary>
    public enum ePresenterTrackMode
    {
        /// <summary>
        /// Presenter Track is turned off.
        /// </summary>
        Off,
        /// <summary>
        /// Presenter Track follows the speaker's movements.
        /// </summary>
        Follow,
        /// <summary>
        /// Presenter Track is set to background mode, where it tracks the speaker but does not actively follow.
        /// </summary>
        Background,
        /// <summary>
        /// Presenter Track is set to persistent mode, where it maintains a fixed position or focus on the speaker.
        /// </summary>
        Persistent
    }


    /// <summary>
    /// Describes the Presenter Track controls for a Cisco codec.
    /// </summary>
    public interface IPresenterTrack : IKeyed
    {
        /// <summary>
        /// 
        /// </summary>
        bool PresenterTrackAvailability { get; }

        /// <summary>
        /// Feedback indicating whether Presenter Track is available.
        /// </summary>
        BoolFeedback PresenterTrackAvailableFeedback { get; }

        /// <summary>
        /// Feedback indicating the current status of Presenter Track is off
        /// </summary>
        BoolFeedback PresenterTrackStatusOffFeedback { get; }

        /// <summary>
        /// Feedback indicating the current status of Presenter Track is follow
        /// </summary>
        BoolFeedback PresenterTrackStatusFollowFeedback { get; }

        /// <summary>
        /// Feedback indicating the current status of Presenter Track is background
        /// </summary>
        BoolFeedback PresenterTrackStatusBackgroundFeedback { get; }

        /// <summary>
        /// Feedback indicating the current status of Presenter Track is persistent
        /// </summary>
        BoolFeedback PresenterTrackStatusPersistentFeedback { get; }

        /// <summary>
        /// Indicates the current status of Presenter Track.
        /// </summary>
        bool PresenterTrackStatus { get; }

        /// <summary>
        /// Turns off Presenter Track.
        /// </summary>
        void PresenterTrackOff();

        /// <summary>
        /// Turns on Presenter Track in follow mode.
        /// </summary>
        void PresenterTrackFollow();

        /// <summary>
        /// Turns on Presenter Track in background mode.
        /// </summary>
        void PresenterTrackBackground();

        /// <summary>
        /// Turns on Presenter Track in persistent mode.
        /// </summary>
        void PresenterTrackPersistent();
    }
}
