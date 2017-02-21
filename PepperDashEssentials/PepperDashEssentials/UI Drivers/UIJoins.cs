using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials
{
	public class UISmartObjectJoin
	{
		public const uint StagingListSRL = 3200;
        public const uint ActivityFooterSRL = 15022;
	}

	/// <summary>
	/// 
	/// </summary>
	public class UIBoolJoin
	{
		public const uint VolumeUpPress = 901;
		public const uint VolumeDownPress = 902;

        public const uint VolumeSingleMute1Visible = 3811;
        public const uint VolumeSlider1Press = 3812;
        public const uint Volume1ProgramMutePressAndFB = 3813;
        public const uint VolumeDualMute1Visible = 3871;
        public const uint Volume1SpeechMutePressAndFB = 3874;
        public const uint Volume1BackerVisibility = 3875;

        public const uint Volume2Visible = 3821;
        public const uint VolumeSlider2Press = 3822;
        public const uint Volume2MutePressAndFB = 3823;
        public const uint Volume3Visible = 3831;
        public const uint VolumeSlider3Press = 3832;
        public const uint Volume3MutePressAndFB = 3833;
        public const uint Volume4Visible = 3841;
        public const uint VolumeSlider4Press = 3842;
        public const uint Volume4MutePressAndFB = 3843;
        public const uint Volume5Visible = 3851;
        public const uint VolumeSlider5Press = 3852;
        public const uint Volume5MutePressAndFB = 3853;
        public const uint Volume6Visible = 3861;
        public const uint VolumeSlider6Press = 3862;
        public const uint Volume6MutePressAndFB = 3863;

        /// <summary>
        /// 3870
        /// </summary>
        public const uint VolumesPageVisible = 3870;

		public const uint GenericModalVisible = 3999;

		public const uint AvNoControlsSubVisible = 12345;
        public const uint StartPageVisible = 15001;
        /// <summary>
        /// 15002 Shows the start page in the source controls area of the screen
        /// </summary>
		public const uint TapToBeginVisible = 15002;
		public const uint SelectASourceVisible = 15003;
		public const uint RoomIsOn = 15004;
        /// <summary>
        /// 15005 Shows always-on volume control subpage with only audio mute
        /// </summary>
        public const uint VolumeControlsSingleMuteVisible = 15005;
        /// <summary>
        /// 15006 Shows always-on volume control subpage with mic and audio mutes
        /// </summary>
        public const uint VolumeControlsDualMuteVisible = 15006;
        public const uint ShowPanelSetupPress = 15010;
		public const uint TopBarVisible = 15011;
		public const uint StagingPageVisible = 15012;
		public const uint PowerOffStep1Visible = 15013;
		public const uint PowerOffStep2Visible = 15014;
		public const uint ShowPowerOffPress = 15015;
		public const uint PowerOffMorePress = 15016;
        public const uint StagingPageAdditionalArrowsVisible = 15017;
		public const uint PanelSetupVisible = 15020;
		public const uint SourceWaitOverlayVisible = 15021;
        public const uint ActivityFooterVisible = 15022;
        public const uint LightsHeaderButtonVisible = 15023;
        public const uint CallRightHeaderButtonVisible = 15024;
        public const uint CallLeftHeaderButtonVisible = 15025;
        public const uint LightsHeaderButtonPress = 15026;
        public const uint CallHeaderButtonPress = 15027;
        /// <summary>
        /// 15028 The gear button in header
        /// </summary>
        public const uint GearHeaderButtonPress = 15028;
        /// <summary>
        /// 15029 the room button in header
        /// </summary>
        public const uint RoomHeaderButtonPress = 15029;
        /// <summary>
        /// 15030 Visibility for room data popup
        /// </summary>
        public const uint RoomHeaderPageVisible = 15030;
        public const uint AllRoomsOffPress = 15031;
		public const uint DisplayPowerTogglePress = 15032;
		public const uint PowerOffCancelPress = 15033;
		public const uint PowerOffConfirmPress = 15034;
		public const uint VolumeButtonPopupPress = 15035;
		public const uint VolumeButtonPopupVisible = 15035;
		public const uint VolumeGaugePopupVisible = 15036;
        public const uint CallStatusPageVisible = 15040;
        public const uint LightsPageVisible = 15041;
        /// <summary>
        /// 15042 Closes whichever interlocked modal is open
        /// </summary>
        public const uint InterlockedModalClosePress = 15042;

        public const uint Display1SelectPress = 15051;
        public const uint Display1ControlButtonEnable = 15052;
        public const uint Display1ControlButtonPress = 15053;
        public const uint Display1AudioButtonEnable = 15054;
        public const uint Display1AudioButtonPressAndFb = 15055;
        public const uint Display2SelectPress = 15056;
        public const uint Display2ControlButtonEnable = 15057;
        public const uint Display2ControlButtonPress = 15058;
        public const uint Display2AudioButtonEnable = 15059;
        public const uint Display2AudioButtonPressAndFb = 15060;

        /// <summary>
        /// 15061 Reveals the dual-display subpage
        /// </summary>
        public const uint DualDisplayPageVisible = 15061;
        public const uint ToggleSharingModeVisible = 15062;
        public const uint ToggleSharingModePress = 15063;



        /// <summary>
        /// 15085 Visibility join for help subpage
        /// </summary>
        public const uint HelpPageVisible = 15085;
        /// <summary>
        /// 15086 Press for help header button
        /// </summary>
        public const uint HelpPress = 15086;

		public const uint DateOnlyVisible = 15088;
		public const uint TimeOnlyVisible = 15089;
		public const uint DateAndTimeVisible = 15090;
		public const uint SetupFullDistrib = 15091;
    }

	/// <summary>
	/// 
	/// </summary>
	public class UIUshortJoin
	{
        //public const uint VolumeLevel = 3812;
        public const uint VolumeSlider1Value = 3812;
        public const uint VolumeSlider2Value = 3822;
        public const uint VolumeSlider3Value = 3832;
        public const uint VolumeSlider4Value = 3842;
        public const uint VolumeSlider5Value = 3852;
        public const uint VolumeSlider6Value = 3862;

        public const uint PresentationListCaretMode = 3922;
	}

	/// <summary>
	/// 
	/// </summary>
	public class UIStringJoin
	{
        public const uint AdvancedVolumeSlider1Text = 3812;
        public const uint AdvancedVolumeSlider2Text = 3822;
        public const uint AdvancedVolumeSlider3Text = 3832;
        public const uint AdvancedVolumeSlider4Text = 3842;
        public const uint AdvancedVolumeSlider5Text = 3852;
        public const uint AdvancedVolumeSlider6Text = 3862;

		public const uint CurrentRoomName = 3901;
		public const uint CurrentSourceName = 3902;
		public const uint CurrentSourceIcon = 3903;
		public const uint PowerOffMessage = 3911;
        public const uint StartPageMessage = 3912;
        public const uint StartActivityText = 3913;
        public const uint HelpMessage = 3922;

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