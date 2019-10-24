using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Room.Config
{
    public class EssentialsDualDisplayRoomPropertiesConfig : EssentialsNDisplayRoomPropertiesConfig
    {

        public const string LeftDisplayId = "leftDisplay";
        public const string RightDisplayId = "rightDisplay";
    }
}