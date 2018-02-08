using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Room.Config
{
	public class DDVC01RoomPropertiesConfig : EssentialsHuddleVtc1PropertiesConfig
	{
		public string RoomPhoneNumber { get; set; }
		public string RoomURI { get; set; }
		public List<DDVC01SpeedDial> SpeedDials { get; set; }
		public List<string> VolumeSliderNames { get; set; }
	}

	public class DDVC01SpeedDial
	{
		public string Name { get; set; }
		public string Number { get; set; }
	}
}