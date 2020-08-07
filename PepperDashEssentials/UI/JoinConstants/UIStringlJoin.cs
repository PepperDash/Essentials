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
        /// <summary>
        /// 1002
        /// </summary>
        public const uint CodecDirectorySearchEntryText = 1002;
        /// <summary>
        /// 1004
        /// </summary>
        public const uint CallSharedSourceNameText = 1004;


        /// <summary>
        /// 1201 - 1230 range of joins for recents list
        /// </summary>
        public const uint VCRecentListTextStart = 1201;
		//									RANGE IN USE
		public const uint VCRecentListTextEnd = 1230;
		/// <summary>
		/// 1231 - 1261 range of joins for recent list time 
		/// </summary>
		public const uint VCRecentListTimeTextStart = 1231;
        //									RANGE IN USE
        public const uint VCRecentListTimeTextEnd = 1260;
		/// <summary>
		/// 1291 - the current layout mode
		/// </summary>
		public const uint VCLayoutModeText = 1291;

		/// <summary>
		/// 1301 - 1400
		/// </summary>
		public const uint VCDirectoryListTextStart = 1301;
		//									RANGE IN USE
		public const uint VCDirectoryListTextEnd = 1556;

        /// <summary>
        /// 1611 - 1615
        /// </summary>
        public const uint VCFavoritesStart = 1611;
        //									RANGE IN USE
        public const uint VCFavoritesTextEnd = 1615;


        //******************************************************
        // Keyboard
        /// <summary>
        /// 1901
        /// </summary>
        //public const uint KeypadText = 2901;

        //******************************************************

        // Environment Joins

        /// <summary>
        /// 2001 - 2010
        /// </summary>
        public const uint EnvironmentColumnOneLabelBase = 2000;

        /// <summary>
        /// 2011 - 2020
        /// </summary>
        public const uint EnvironmentColumnTwoLabelBase = 2010;

        /// <summary>
        /// 2021 - 2030
        /// </summary>
        public const uint EnvironmentColumnThreeLabelBase = 2020;

        /// <summary>
        /// 2031 - 2040
        /// </summary>
        public const uint EnvironmentColumnFourLabelBase = 2030;

        // 2050, 2060, 2070 and 2080 reserved for column device name labels

        //******************************************************

        /// <summary>
        /// 3101 - This is the start of the range 3101 - 3120
        /// </summary>
        public const uint TechMenuButtonTextStart = 3101;

        //----- through 3120


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
        /// 3905 - Video address/number for room header
        /// </summary>
        public const uint RoomVideoAddressText = 3905;
        /// <summary>
        /// 3906 - The separator for verbose-header text on addresses
        /// </summary>
        public const uint RoomAddressPipeText = 3906;
		/// <summary>
		/// 3907 - The user code for mobile control
		/// </summary>
		public const uint RoomUserCode = 3907;
        /// <summary>
        /// 3908 - The url for the mobile control server
        /// </summary>
        public const uint RoomMcUrl = 3908;
        /// <summary>
        /// 3909 - The url for the mobile control QR Code image
        /// </summary>
        public const uint RoomMcQrCodeUrl = 3909;
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
		/// 3916 The "active call" label
		/// </summary>
		public const uint HeaderCallStatusLabel = 3916;
		/// <summary>
		/// 3919 Mesage on init page
		/// </summary>
		public const uint SystemInitializingMessage = 3919;
        /// <summary>
        /// 3922
        /// </summary>
        public const uint HelpMessage = 3922;
        /// <summary>
        /// 3923
        /// </summary>
        public const uint LogoUrlLightBkgnd = 3923;


        /// <summary>
        /// 3924 - the text on the "call help desk" button
        /// </summary>
        public const uint HelpPageCallButtonText = 3924;

        /// <summary>
        /// 3925
        /// </summary>
        public const uint LogoUrlDarkBkgnd = 3923;

		/// <summary>
		/// 3951
		/// </summary>
		public const uint HeaderButtonIcon1 = 3951;
		/// <summary>
		/// 3952
		/// </summary>
		public const uint HeaderButtonIcon2 = 3952;
		/// <summary>
		/// 3953
		/// </summary>
		public const uint HeaderButtonIcon3 = 3953;
		/// <summary>
		/// 3954
		/// </summary>
		public const uint HeaderButtonIcon4 = 3954;
		/// <summary>
		/// 3955
		/// </summary>
		public const uint HeaderButtonIcon5 = 3955;

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

		/// <summary>
		/// 3966
		/// </summary>
		public const uint NextMeetingStartTimeText = 3966;
		/// <summary>
		/// 3967
		/// </summary>
		public const uint NextMeetingEndTimeText = 3967;
		/// <summary>
		/// 3968
		/// </summary>
		public const uint NextMeetingTitleText = 3968;
		/// <summary>
		/// 3969
		/// </summary>
		public const uint NextMeetingNameText = 3969;
		/// <summary>
		/// 3970
		/// </summary>
		public const uint NextMeetingButtonLabel = 3970;
		/// <summary>
		/// 3971
		/// </summary>
		public const uint NextMeetingSecondaryButtonLabel = 3971;
		/// <summary>
		/// 3972
		/// </summary>
		public const uint NextMeetingFollowingMeetingText = 3972;
        /// <summary>
        /// 3976
        /// </summary>
        public const uint MeetingsOrContactMethodListIcon = 3976;
		/// <summary>
		/// 3977
		/// </summary>
		public const uint MeetingsOrContactMethodListTitleText = 3977;

		// ------------------------------------
		//
		// MODAL JOINS 3991 - 3999
		//
		// ------------------------------------
    }
}