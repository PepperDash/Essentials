extern alias Full;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Full.Newtonsoft.Json;

namespace PepperDash.Essentials.Room.Config
{

    public class EssentialsHuddleVtc1PropertiesConfig : EssentialsConferenceRoomPropertiesConfig
    {
		[JsonProperty("defaultDisplayKey")]
		public string DefaultDisplayKey { get; set; }

    }
}