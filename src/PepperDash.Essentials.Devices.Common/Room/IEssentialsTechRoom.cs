using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using System.Collections.Generic;
using PepperDash.Essentials.Core.Room;
using PepperDash.Essentials.Core.Room.Config;
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
