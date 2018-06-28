using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Room.Config
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