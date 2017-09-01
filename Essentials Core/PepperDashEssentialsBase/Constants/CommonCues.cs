using Crestron.SimplSharpPro;

namespace PepperDash.Essentials.Core
{
	public static class CommonBoolCue
	{
		public static readonly Cue Power = new Cue("Power", 101, eCueType.Bool);
		public static readonly Cue PowerOn = new Cue("PowerOn", 102, eCueType.Bool);
		public static readonly Cue PowerOff = new Cue("PowerOff", 103, eCueType.Bool);

		public static readonly Cue HasPowerFeedback = new Cue("HasPowerFeedback", 101, eCueType.Bool);
		public static readonly Cue PowerOnFeedback = new Cue("PowerOnFeedback", 102, eCueType.Bool);
		public static readonly Cue IsOnlineFeedback = new Cue("IsOnlineFeedback", 104, eCueType.Bool);
		public static readonly Cue IsWarmingUp = new Cue("IsWarmingUp", 105, eCueType.Bool);
		public static readonly Cue IsCoolingDown = new Cue("IsCoolingDown", 106, eCueType.Bool);

		public static readonly Cue Dash = new Cue("Dash", 109, eCueType.Bool);
		public static readonly Cue Digit0 = new Cue("Digit0", 110, eCueType.Bool);
		public static readonly Cue Digit1 = new Cue("Digit1", 111, eCueType.Bool);
		public static readonly Cue Digit2 = new Cue("Digit2", 112, eCueType.Bool);
		public static readonly Cue Digit3 = new Cue("Digit3", 113, eCueType.Bool);
		public static readonly Cue Digit4 = new Cue("Digit4", 114, eCueType.Bool);
		public static readonly Cue Digit5 = new Cue("Digit5", 115, eCueType.Bool);
		public static readonly Cue Digit6 = new Cue("Digit6", 116, eCueType.Bool);
		public static readonly Cue Digit7 = new Cue("Digit7", 117, eCueType.Bool);
		public static readonly Cue Digit8 = new Cue("Digit8", 118, eCueType.Bool);
		public static readonly Cue Digit9 = new Cue("Digit9", 119, eCueType.Bool);
		public static readonly Cue KeypadMisc1 = new Cue("KeypadMisc1", 120, eCueType.Bool);
		public static readonly Cue KeypadMisc2 = new Cue("KeypadMisc2", 121, eCueType.Bool);

		public static readonly Cue NumericEnter = new Cue("Enter", 122, eCueType.Bool);
		public static readonly Cue ChannelUp = new Cue("ChannelUp", 123, eCueType.Bool);
		public static readonly Cue ChannelDown = new Cue("ChannelDown", 124, eCueType.Bool);
		public static readonly Cue Last = new Cue("Last", 125, eCueType.Bool);
		public static readonly Cue OpenClose = new Cue("OpenClose", 126, eCueType.Bool);
		public static readonly Cue Subtitle = new Cue("Subtitle", 127, eCueType.Bool);
		public static readonly Cue Audio = new Cue("Audio", 128, eCueType.Bool);
		public static readonly Cue Info = new Cue("Info", 129, eCueType.Bool);
		public static readonly Cue Menu = new Cue("Menu", 130, eCueType.Bool);
		public static readonly Cue DeviceMenu = new Cue("DeviceMenu", 131, eCueType.Bool);
		public static readonly Cue Return = new Cue("Return", 132, eCueType.Bool);
		public static readonly Cue Back = new Cue("Back", 133, eCueType.Bool);
		public static readonly Cue Exit = new Cue("Exit", 134, eCueType.Bool);
		public static readonly Cue Clear = new Cue("Clear", 135, eCueType.Bool);
		public static readonly Cue List = new Cue("List", 136, eCueType.Bool);
		public static readonly Cue Guide = new Cue("Guide", 137, eCueType.Bool);
		public static readonly Cue Am = new Cue("Am", 136, eCueType.Bool);
		public static readonly Cue Fm = new Cue("Fm", 137, eCueType.Bool);
		public static readonly Cue Up = new Cue("Up", 138, eCueType.Bool);
		public static readonly Cue Down = new Cue("Down", 139, eCueType.Bool);
		public static readonly Cue Left = new Cue("Left", 140, eCueType.Bool);
		public static readonly Cue Right = new Cue("Right", 141, eCueType.Bool);
		public static readonly Cue Select = new Cue("Select", 142, eCueType.Bool);
		public static readonly Cue SmartApps = new Cue("SmartApps", 143, eCueType.Bool);
		public static readonly Cue Dvr = new Cue("Dvr", 144, eCueType.Bool);

		public static readonly Cue Play = new Cue("Play", 145, eCueType.Bool);
		public static readonly Cue Pause = new Cue("Pause", 146, eCueType.Bool);
		public static readonly Cue Stop = new Cue("Stop", 147, eCueType.Bool);
		public static readonly Cue ChapNext = new Cue("ChapNext", 148, eCueType.Bool);
		public static readonly Cue ChapPrevious = new Cue("ChapPrevious", 149, eCueType.Bool);
		public static readonly Cue Rewind = new Cue("Rewind", 150, eCueType.Bool);
		public static readonly Cue Ffwd = new Cue("Ffwd", 151, eCueType.Bool);
		public static readonly Cue Replay = new Cue("Replay", 152, eCueType.Bool);
		public static readonly Cue Advance = new Cue("Advance", 153, eCueType.Bool);
		public static readonly Cue Record = new Cue("Record", 154, eCueType.Bool);
		public static readonly Cue Red = new Cue("Red", 155, eCueType.Bool);
		public static readonly Cue Green = new Cue("Green", 156, eCueType.Bool);
		public static readonly Cue Yellow = new Cue("Yellow", 157, eCueType.Bool);
		public static readonly Cue Blue = new Cue("Blue", 158, eCueType.Bool);
		public static readonly Cue Home = new Cue("Home", 159, eCueType.Bool);
		public static readonly Cue PopUp = new Cue("PopUp", 160, eCueType.Bool);
		public static readonly Cue PageUp = new Cue("PageUp", 161, eCueType.Bool);
		public static readonly Cue PageDown = new Cue("PageDown", 162, eCueType.Bool);
		public static readonly Cue Search = new Cue("Search", 163, eCueType.Bool);
		public static readonly Cue Setup = new Cue("Setup", 164, eCueType.Bool);
		public static readonly Cue RStep = new Cue("RStep", 165, eCueType.Bool);
		public static readonly Cue FStep = new Cue("FStep", 166, eCueType.Bool);

		public static readonly Cue IsConnected = new Cue("IsConnected", 281, eCueType.Bool);
		public static readonly Cue IsOk = new Cue("IsOk", 282, eCueType.Bool);
		public static readonly Cue InWarning = new Cue("InWarning", 283, eCueType.Bool);
		public static readonly Cue InError = new Cue("InError", 284, eCueType.Bool);
		public static readonly Cue StatusUnknown = new Cue("StatusUnknown", 285, eCueType.Bool);
        
        public static readonly Cue VolumeUp = new Cue("VolumeUp", 401, eCueType.Bool);
		public static readonly Cue VolumeDown = new Cue("VolumeDown", 402, eCueType.Bool);
		public static readonly Cue MuteOn = new Cue("MuteOn", 403, eCueType.Bool);
		public static readonly Cue MuteOff = new Cue("MuteOff", 404, eCueType.Bool);
		public static readonly Cue MuteToggle = new Cue("MuteToggle", 405, eCueType.Bool);
		public static readonly Cue ShowVolumeButtons = new Cue("ShowVolumeButtons", 406, eCueType.Bool);
		public static readonly Cue ShowVolumeSlider = new Cue("ShowVolumeSlider", 407, eCueType.Bool);

		public static readonly Cue Hdmi1 = new Cue("Hdmi1", 451, eCueType.Bool);
		public static readonly Cue Hdmi2 = new Cue("Hdmi2", 452, eCueType.Bool);
		public static readonly Cue Hdmi3 = new Cue("Hdmi3", 453, eCueType.Bool);
		public static readonly Cue Hdmi4 = new Cue("Hdmi4", 454, eCueType.Bool);
		public static readonly Cue Hdmi5 = new Cue("Hdmi5", 455, eCueType.Bool);
		public static readonly Cue Hdmi6 = new Cue("Hdmi6", 456, eCueType.Bool);
		public static readonly Cue DisplayPort1 = new Cue("DisplayPort1", 457, eCueType.Bool);
		public static readonly Cue DisplayPort2 = new Cue("DisplayPort2", 458, eCueType.Bool);
		public static readonly Cue Dvi1 = new Cue("Dvi1", 459, eCueType.Bool);
		public static readonly Cue Dvi2 = new Cue("Dvi2", 460, eCueType.Bool);
		public static readonly Cue Video1 = new Cue("Video1", 461, eCueType.Bool);
		public static readonly Cue Video2 = new Cue("Video2", 462, eCueType.Bool);
		public static readonly Cue Component1 = new Cue("Component1", 463, eCueType.Bool);
		public static readonly Cue Component2 = new Cue("Component2", 464, eCueType.Bool);
		public static readonly Cue Vga1 = new Cue("Vga1", 465, eCueType.Bool);
		public static readonly Cue Vga2 = new Cue("Vga2", 466, eCueType.Bool);
		public static readonly Cue Rgb1 = new Cue("Rgb1", 467, eCueType.Bool);
		public static readonly Cue Rgb2 = new Cue("Rgb2", 468, eCueType.Bool);
		public static readonly Cue Antenna = new Cue("Antenna", 469, eCueType.Bool);

        public static readonly Cue InCall = new Cue("InCall", 501, eCueType.Bool);
	}

	public static class CommonIntCue
	{
		public static readonly Cue MainVolumeLevel = new Cue("MainVolumeLevel", 401, eCueType.Int);
		public static readonly Cue MainVolumeLevelFeedback = new Cue("MainVolumeLevelFeedback", 401, eCueType.Int);
	}

	public static class CommonStringCue
	{
		public static readonly Cue IpConnectionsText = new Cue("IpConnectionsText", 9999, eCueType.String);
	}
}