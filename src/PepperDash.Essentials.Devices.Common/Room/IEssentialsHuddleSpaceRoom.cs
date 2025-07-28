using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.Rooms;
using PepperDash.Essentials.Core.Rooms.Config;
using PepperDash.Essentials.Core.Routing;

namespace PepperDash.Essentials.Devices.Common.Room
{
    /// <summary>
    /// Defines the contract for IEssentialsHuddleSpaceRoom
    /// </summary>
    public interface IEssentialsHuddleSpaceRoom : IEssentialsRoom, IHasCurrentSourceInfoChange, IRunRouteAction, IHasDefaultDisplay, IHasCurrentVolumeControls, IRoomOccupancy,
        IEmergency, IMicrophonePrivacy
    {
        bool ExcludeFromGlobalFunctions { get; }

        void RunRouteAction(string routeKey);

        EssentialsHuddleRoomPropertiesConfig PropertiesConfig { get; }
    }
}