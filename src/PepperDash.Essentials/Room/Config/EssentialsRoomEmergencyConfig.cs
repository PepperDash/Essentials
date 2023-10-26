using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Room.Config
{
    /// <summary>
    /// 
    /// </summary>
    public class EssentialsRoomEmergencyConfig
    {
        public EssentialsRoomEmergencyTriggerConfig Trigger { get; set; }

        public string Behavior { get; set; }
    }
}