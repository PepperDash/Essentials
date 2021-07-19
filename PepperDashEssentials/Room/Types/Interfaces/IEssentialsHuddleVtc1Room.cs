
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Room.Config;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.VideoCodec;
using PepperDash.Essentials.Devices.Common.AudioCodec;

namespace PepperDash.Essentials
{
    public interface IEssentialsHuddleVtc1Room : IEssentialsRoom, IHasCurrentSourceInfoChange,
         IPrivacy, IHasCurrentVolumeControls, IRunRouteAction, IRunDefaultCallRoute, IHasVideoCodec, IHasAudioCodec, IHasDefaultDisplay, IHasInCallFeedback
    {
        EssentialsHuddleVtc1PropertiesConfig PropertiesConfig { get; }

        void RunRouteAction(string routeKey);

        IHasScheduleAwareness ScheduleSource { get; }

        new BoolFeedback InCallFeedback { get; }

        new BoolFeedback PrivacyModeIsOnFeedback { get; }

        string DefaultCodecRouteString { get; }
    }
}