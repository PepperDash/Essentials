using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.AudioCodec;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PDT.Plugins.Essentials.Rooms
{
    public interface IEssentialsHuddleVtc1Room : IEssentialsRoom, IHasCurrentSourceInfoChange, IHasCurrentVolumeControls, IRunRouteAction, IRunDefaultCallRoute, IHasVideoCodec, IHasAudioCodec, IHasDefaultDisplay, IHasInCallFeedback,
        IRoomOccupancy, IEmergency, IMicrophonePrivacy
    {
        // EssentialsHuddleVtc1PropertiesConfig PropertiesConfig { get; }

        bool ExcludeFromGlobalFunctions { get; }

        void RunRouteAction(string routeKey);

        IHasScheduleAwareness ScheduleSource { get; }

        new BoolFeedback InCallFeedback { get; }

        new BoolFeedback PrivacyModeIsOnFeedback { get; }

        string DefaultCodecRouteString { get; }
    }
}