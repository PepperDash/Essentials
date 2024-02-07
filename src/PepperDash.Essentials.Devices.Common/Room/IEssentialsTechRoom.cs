using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Devices.Common.Room
{
    public interface IEssentialsTechRoom:IEssentialsRoom, ITvPresetsProvider,IBridgeAdvanced,IRunDirectRouteAction
    {
        Dictionary<string, IRSetTopBoxBase> Tuners { get; }

        Dictionary<string, TwoWayDisplayBase> Displays { get; }

        void RoomPowerOn();

        void RoomPowerOff();
    }
}
