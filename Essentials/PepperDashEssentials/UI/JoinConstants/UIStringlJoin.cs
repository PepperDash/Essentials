using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials
{
	/// <summary>
	/// Common string join number constants
	/// </summary>
	public class UIStringJoin
	{
        //******************************************************
        // Codec
        /// <summary>
        /// 1001
        /// </summary>
        public const uint CodecAddressEntryText = 1001;


        //******************************************************
        // Keyboard
        /// <summary>
        /// 1901
        /// </summary>
        //public const uint KeypadText = 2901;

        /// <summary>
        /// 3812
        /// </summary>
        public const uint AdvancedVolumeSlider1Text = 3812;
        /// <summary>
        /// 3822
        /// </summary>
        public const uint AdvancedVolumeSlider2Text = 3822;
        /// <summary>
        /// 3832
        /// </summary>
        public const uint AdvancedVolumeSlider3Text = 3832;
        /// <summary>
        /// 3842
        /// </summary>
        public const uint AdvancedVolumeSlider4Text = 3842;
        /// <summary>
        /// 3852
        /// </summary>
        public const uint AdvancedVolumeSlider5Text = 3852;
        /// <summary>
        /// 3862
        /// </summary>
        public const uint AdvancedVolumeSlider6Text = 3862;

        /// <summary>
        /// 3901
        /// </summary>
		public const uint CurrentRoomName = 3901;
        /// <summary>
        /// 3902
        /// </summary>
		public const uint CurrentSourceName = 3902;
        /// <summary>
        /// 3903
        /// </summary>
		public const uint CurrentSourceIcon = 3903;
        /// <summary>
        /// 3904 - Phone number for room header
        /// </summary>
        public const uint RoomPhoneText = 3904;
        /// <summary>
        /// 3905 - SIP address for room header
        /// </summary>
        public const uint RoomSipText = 3905;
        /// <summary>
        /// 3906 - The separator for verbose-header text on addresses
        /// </summary>
        public const uint RoomAddressPipeText = 3906;
        /// <summary>
        /// 3911
        /// </summary>
		public const uint PowerOffMessage = 3911;
        /// <summary>
        /// 3912
        /// </summary>
        public const uint StartPageMessage = 3912;
        /// <summary>
        /// 3913
        /// </summary>
        public const uint StartActivityText = 3913;
        /// <summary>
        /// 3914 Title bar label for source overlay
        /// </summary>
        public const uint SourceBackgroundOverlayTitle = 3914;

        /// <summary>
        /// 3915
        /// </summary>
        public const uint NotificationRibbonText = 3915;

        /// <summary>
        /// 3922
        /// </summary>
        public const uint HelpMessage = 3922;
        /// <summary>
        /// 3923
        /// </summary>
        public const uint LogoUrl = 3923;
        /// <summary>
        /// 3924 - the text on the "call help desk" button
        /// </summary>
        public const uint HelpPageCallButtonText = 3924;

        /// <summary>
        /// 3961 Name of source on display 1
        /// </summary>
        public const uint Display1SourceLabel = 3961;
        /// <summary>
        /// 3962 Title above display 1
        /// </summary>
        public const uint Display1TitleLabel = 3962;
        /// <summary>
        /// 3964 Name of source on display 2
        /// </summary>
        public const uint Display2SourceLabel = 3964;
        /// <summary>
        /// 3965 Title above display 2
        /// </summary>
        public const uint Display2TitleLabel = 3965;
    }
}