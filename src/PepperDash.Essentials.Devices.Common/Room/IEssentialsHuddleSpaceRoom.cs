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
        /// Gets the PropertiesConfig
        /// </summary>
        EssentialsHuddleRoomPropertiesConfig PropertiesConfig { get; }
    }
}