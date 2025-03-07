using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Room.Config;
using System.Collections.Generic;
using TwoWayDisplayBase = PepperDash.Essentials.Devices.Common.Displays.TwoWayDisplayBase;


namespace PepperDash.Essentials.Devices.Common.Room
{
    public interface IEssentialsTechRoom:IEssentialsRoom, ITvPresetsProvider,IBridgeAdvanced,IRunDirectRouteAction
    {
        EssentialsTechRoomConfig PropertiesConfig { get; }
        Dictionary<string, IRSetTopBoxBase> Tuners { get; }

        Dictionary<string, TwoWayDisplayBase> Displays { get; }

        void RoomPowerOn();

        void RoomPowerOff();
    }
}
