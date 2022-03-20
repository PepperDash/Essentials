using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Room.Config;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.VideoCodec;
using PepperDash.Essentials.Devices.Common.AudioCodec;


using PepperDash.Core;

namespace PepperDash.Essentials
{
    public interface IEssentialsHuddleSpaceRoom : IEssentialsRoom, IHasCurrentSourceInfoChange, IRunRouteAction, IHasDefaultDisplay
    {
        bool ExcludeFromGlobalFunctions { get; }

        void RunRouteAction(string routeKey);

        EssentialsHuddleRoomPropertiesConfig PropertiesConfig { get; }

        IBasicVolumeControls CurrentVolumeControls { get; }

        event EventHandler<VolumeDeviceChangeEventArgs> CurrentVolumeDeviceChange;
    }

    public interface IEssentialsHuddleVtc1Room : IEssentialsRoom, IHasCurrentSourceInfoChange,
        IHasCurrentVolumeControls, IRunRouteAction, IRunDefaultCallRoute, IHasVideoCodec, IHasAudioCodec, IHasDefaultDisplay
    {
        EssentialsHuddleVtc1PropertiesConfig PropertiesConfig { get; }

        void RunRouteAction(string routeKey);

        IHasScheduleAwareness ScheduleSource { get; }

        string DefaultCodecRouteString { get; }
    }
}