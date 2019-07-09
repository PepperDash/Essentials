using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

using PepperDash.Essentials.Core;

namespace PepperDash.Essentials
{
	public static class RoomCue
	{
		// Commands/
		//bools
		public static readonly Cue RoomOnToggle = Cue.BoolCue("RoomOnToggle", 2001);
		public static readonly Cue RoomOn = Cue.BoolCue("RoomOn", 2002);
		public static readonly Cue RoomOff = Cue.BoolCue("RoomOff", 2003);
		public static readonly Cue VolumeUp = Cue.BoolCue("VolumeUp", 2011);
		public static readonly Cue VolumeDown = Cue.BoolCue("VolumeDown", 2012);
		public static readonly Cue VolumeDefault = Cue.BoolCue("VolumeDefault", 2013);
		public static readonly Cue MuteToggle = Cue.BoolCue("MuteToggle", 2014);
		public static readonly Cue MuteOn = Cue.BoolCue("MuteOn", 2015);
		public static readonly Cue MuteOff = Cue.BoolCue("MuteOff", 2016);

		//ushorts
		public static readonly Cue SelectSourceByNumber = Cue.UShortCue("SelectSourceByNumber", 2001);
		public static readonly Cue VolumeLevel = Cue.UShortCue("VolumeLevel", 2011);
		public static readonly Cue VolumeLevelPercent = Cue.UShortCue("VolumeLevelPercent", 2012);

		//strings
		public static readonly Cue SelectSourceByKey = Cue.StringCue("SelectSourceByKey", 2001);

		// Outputs
		//Bools
		public static readonly Cue RoomIsOn = Cue.BoolCue("RoomIsOn", 2002);
		public static readonly Cue RoomIsOnStandby = Cue.BoolCue("RoomIsOnStandby", 2003);
		public static readonly Cue RoomIsWarmingUp = Cue.BoolCue("RoomIsWarmingUp", 2004);
		public static readonly Cue RoomIsCoolingDown = Cue.BoolCue("RoomIsCoolingDown", 2005);
		public static readonly Cue RoomIsOccupied = Cue.BoolCue("RoomIsOccupied", 2006);

		//Ushorts
		public static readonly Cue SourcesCount = Cue.UShortCue("SourcesCount", 2001);
		public static readonly Cue CurrentSourceNumber = Cue.UShortCue("CurrentSourceNumber", 2002);
		public static readonly Cue CurrentSourceType = Cue.UShortCue("CurrentSourceType", 2003);

		//Strings
		public static readonly Cue CurrentSourceKey = Cue.StringCue("CurrentSourceKey", 2001);
		public static readonly Cue CurrentSourceName = Cue.StringCue("CurrentSourceName", 2002);

		public static readonly Cue VolumeLevelText = Cue.StringCue("VolumeLevelText", 2012);

		public static readonly Cue Key = Cue.StringCue("RoomKey", 2021);
		public static readonly Cue Name = Cue.StringCue("RoomName", 2022);
		public static readonly Cue Description = Cue.StringCue("Description", 2023);
		public static readonly Cue HelpMessage = Cue.StringCue("HelpMessage", 2024);

		//Special
		public static readonly Cue Source = new Cue("Source", 0, eCueType.Other);
	}
}