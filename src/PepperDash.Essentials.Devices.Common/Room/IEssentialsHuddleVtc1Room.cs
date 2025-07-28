using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.Feedbacks;
using PepperDash.Essentials.Core.Rooms;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Devices.Common.AudioCodec;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials.Devices.Common.Room
{
    /// <summary>
    /// Defines the contract for IEssentialsHuddleVtc1Room
    /// </summary>
    public interface IEssentialsHuddleVtc1Room : IEssentialsRoom, IHasCurrentSourceInfoChange, IHasCurrentVolumeControls, IRunRouteAction, IRunDefaultCallRoute, IHasVideoCodec, IHasAudioCodec, IHasDefaultDisplay, IHasInCallFeedback,
        IRoomOccupancy, IEmergency, IMicrophonePrivacy
    {
        EssentialsHuddleVtc1PropertiesConfig PropertiesConfig { get; }

        bool ExcludeFromGlobalFunctions { get; }

        void RunRouteAction(string routeKey);

        IHasScheduleAwareness ScheduleSource { get; }

        new BoolFeedback InCallFeedback { get; }

        new BoolFeedback PrivacyModeIsOnFeedback { get; }

        string DefaultCodecRouteString { get; }
    }
}