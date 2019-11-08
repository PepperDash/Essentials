using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Newtonsoft.Json;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Room.Config;

namespace PepperDash.Essentials.Room.Types
{
    /// <summary>
    /// Base class for rooms with more than a single display
    /// </summary>
    public abstract class EssentialsNDisplayRoomBase : EssentialsRoomBase
    {
        //public event SourceInfoChangeHandler CurrentSingleSourceChange;


        public EssentialsNDisplayRoomBase(DeviceConfig config)
            : base (config)
        {

            var propertiesConfig = JsonConvert.DeserializeObject<EssentialsNDisplayRoomPropertiesConfig>(config.Properties.ToString());

        }
    }
}