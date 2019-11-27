using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Room.MobileControl
{
	public class Volumes
	{
		[JsonProperty("master")]
		public Volume Master { get; set; }

		[JsonProperty("auxFaders")]
		public Dictionary<string, Volume> AuxFaders { get; set; }

		[JsonProperty("numberOfAuxFaders")]
		public int NumberOfAuxFaders { get; set; }

		public Volumes()
		{
			AuxFaders = new Dictionary<string, Volume>();	
		}
	}

	public class Volume
	{
		[JsonProperty("key")]
		public string Key { get; set; }

		[JsonProperty("level")]
		public ushort Level { get; set; }

		[JsonProperty("muted")]
		public bool Muted { get; set; }

		[JsonProperty("label")]
		public string Label { get; set; }

		[JsonProperty("hasMute")]
		public bool HasMute { get; set; }

		[JsonProperty("hasPrivacyMute")]
		public bool HasPrivacyMute { get; set; }

		[JsonProperty("privacyMuted")]
		public bool PrivacyMuted { get; set; }


		[JsonProperty("muteIcon")]
		public string MuteIcon { get; set; }

		public Volume(string key, ushort level, bool muted, string label, bool hasMute, string muteIcon)
		{
			Key = key;
			Level = level;
			Muted = muted;
			Label = label;
			HasMute = hasMute;
			MuteIcon = muteIcon;
		}
	}
}