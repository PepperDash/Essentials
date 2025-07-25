using PepperDash.Essentials.Room.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Devices.Common.Room
{
    /// <summary>
    /// Defines the contract for IEssentialsRoomPropertiesConfig
    /// </summary>
    public interface IEssentialsRoomPropertiesConfig
    {
        EssentialsRoomPropertiesConfig PropertiesConfig { get; }
    }
}
