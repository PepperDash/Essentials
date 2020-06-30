using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.Rooms.Config
{
	public class DDVC01RoomPropertiesConfig : EssentialsHuddleVtc1PropertiesConfig
	{
		[JsonProperty("roomPhoneNumber")]
		public string RoomPhoneNumber { get; set; }
		[JsonProperty("roomURI")]
		public string RoomURI { get; set; }
		[JsonProperty("speedDials")]
		public List<DDVC01SpeedDial> SpeedDials { get; set; }
		[JsonProperty("volumeSliderNames")]
		public List<string> VolumeSliderNames { get; set; }
	}

	public class DDVC01SpeedDial
	{
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("number")]
		public string Number { get; set; }
	}
}