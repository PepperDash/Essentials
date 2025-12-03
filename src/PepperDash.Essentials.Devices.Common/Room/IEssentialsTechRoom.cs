using System.Collections.Generic;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Devices.Common.Displays;
using PepperDash.Essentials.Room.Config;


namespace PepperDash.Essentials.Devices.Common.Room
{
    /// <summary>
    /// Defines the contract for IEssentialsTechRoom
    /// </summary>
    public interface IEssentialsTechRoom : IEssentialsRoom, ITvPresetsProvider, IBridgeAdvanced, IRunDirectRouteAction
    {
        /// <summary>
        /// Gets the PropertiesConfig
        /// </summary>
        EssentialsTechRoomConfig PropertiesConfig { get; }

        /// <summary>
        /// Gets the Tuners
        /// </summary>
        Dictionary<string, IRSetTopBoxBase> Tuners { get; }

        /// <summary>
        /// Gets the Displays
        /// </summary>
        Dictionary<string, TwoWayDisplayBase> Displays { get; }

        /// <summary>
        /// Powers on the room
        /// </summary>
        void RoomPowerOn();

        /// <summary>
        /// Powers off the room
        /// </summary>
        void RoomPowerOff();
    }
}
