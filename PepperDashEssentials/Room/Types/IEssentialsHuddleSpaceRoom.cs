using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Room.Config;
using PepperDash.Essentials.Core.Devices;

using PepperDash.Core;

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