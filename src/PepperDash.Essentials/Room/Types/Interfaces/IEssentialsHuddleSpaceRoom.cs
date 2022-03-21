using System;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Interfaces;
using PepperDash.Essentials.Room.Config;



namespace PepperDash.Essentials
{
    public interface IEssentialsHuddleSpaceRoom : IEssentialsRoom, IHasCurrentSourceInfoChange, IRunRouteAction, IRunDefaultPresentRoute, IHasDefaultDisplay
    {
        bool ExcludeFromGlobalFunctions { get; }

        void RunRouteAction(string routeKey);

        EssentialsHuddleRoomPropertiesConfig PropertiesConfig { get; }

        IBasicVolumeControls CurrentVolumeControls { get; }

        event EventHandler<VolumeDeviceChangeEventArgs> CurrentVolumeDeviceChange;
    }

 
}