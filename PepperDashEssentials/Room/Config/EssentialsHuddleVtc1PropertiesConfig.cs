using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Room.Config
{

    public class EssentialsHuddleVtc1PropertiesConfig : EssentialsRoomPropertiesConfig
    {
		[JsonProperty("defaultDisplayKey")]
		public string DefaultDisplayKey { get; set; }
		[JsonProperty("defaultAudioKey")]
		public string DefaultAudioKey { get; set; }
		[JsonProperty("sourceListKey")]
		public string SourceListKey { get; set; }
		[JsonProperty("defaultSourceItem")]
		public string DefaultSourceItem { get; set; }
		[JsonProperty("videoCodecKey")]
		public string VideoCodecKey { get; set; }
    }
}