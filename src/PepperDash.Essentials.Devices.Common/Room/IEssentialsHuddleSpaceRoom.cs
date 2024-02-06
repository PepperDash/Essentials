using System;
using PepperDash.Essentials.Core;

namespace PDT.Plugins.Essentials.Rooms
{
    public interface IEssentialsHuddleSpaceRoom : IEssentialsRoom, IHasCurrentSourceInfoChange, IRunRouteAction, IHasDefaultDisplay, IHasCurrentVolumeControls, IRoomOccupancy,
        IEmergency, IMicrophonePrivacy
    {
        bool ExcludeFromGlobalFunctions { get; }

        void RunRouteAction(string routeKey);

        // EssentialsHuddleRoomPropertiesConfig PropertiesConfig { get; }

        IBasicVolumeControls CurrentVolumeControls { get; }

        event EventHandler<VolumeDeviceChangeEventArgs> CurrentVolumeDeviceChange;
    }
}