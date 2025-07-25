using PepperDash.Essentials.Core;
using PepperDash.Essentials.Room.Config;

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