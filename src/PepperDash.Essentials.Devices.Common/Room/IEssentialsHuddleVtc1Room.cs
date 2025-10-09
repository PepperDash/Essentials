using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.AudioCodec;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.VideoCodec;
using PepperDash.Essentials.Room.Config;

namespace PepperDash.Essentials.Devices.Common.Room
{
    /// <summary>
    /// Defines the contract for IEssentialsHuddleVtc1Room
    /// </summary>
    public interface IEssentialsHuddleVtc1Room : IEssentialsRoom, IHasCurrentSourceInfoChange, IHasCurrentVolumeControls, IRunRouteAction, IRunDefaultCallRoute, IHasVideoCodec, IHasAudioCodec, IHasDefaultDisplay, IHasInCallFeedback,
        IRoomOccupancy, IEmergency, IMicrophonePrivacy
    {
        /// <summary>
        /// Gets the PropertiesConfig
        /// </summary>
        EssentialsHuddleVtc1PropertiesConfig PropertiesConfig { get; }

        /// <summary>
        /// Gets whether to exclude this room from global functions
        /// </summary>
        bool ExcludeFromGlobalFunctions { get; }

        /// <summary>
        /// Runs the route action for the given routeKey and sourceListKey
        /// </summary>
        /// <param name="routeKey">The route key</param>
        void RunRouteAction(string routeKey);

        /// <summary>
        /// Gets the ScheduleSource
        /// </summary>
        IHasScheduleAwareness ScheduleSource { get; }

        /// <summary>
        /// Gets the InCallFeedback
        /// </summary>
        new BoolFeedback InCallFeedback { get; }

        /// <summary>
        /// Gets the PrivacyModeIsOnFeedback
        /// </summary>
        new BoolFeedback PrivacyModeIsOnFeedback { get; }

        /// <summary>
        /// Gets the DefaultCodecRouteString
        /// </summary>
        string DefaultCodecRouteString { get; }
    }
}