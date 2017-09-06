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
	/// Where all UI element common joins are defined
	/// </summary>
	public class UIBoolJoin
	{
        /// <summary>
        /// 901
        /// </summary>
		public const uint VolumeUpPress = 901;
        /// <summary>
        /// 902
        /// </summary>
		public const uint VolumeDownPress = 902;

        /// <summary>
        /// 3811
        /// </summary>
        public const uint VolumeSingleMute1Visible = 3811;
        /// <summary>
        /// 3812
        /// </summary>
        public const uint VolumeSlider1Press = 3812;
        /// <summary>
        /// 3813
        /// </summary>
        public const uint Volume1ProgramMutePressAndFB = 3813;
        /// <summary>
        /// 3821
        /// </summary>
        public const uint Volume2Visible = 3821;
        /// <summary>
        /// 3822
        /// </summary>
        public const uint VolumeSlider2Press = 3822;
        /// <summary>
        /// 3823
        /// </summary>
        public const uint Volume2MutePressAndFB = 3823;
        /// <summary>
        /// 3831
        /// </summary>
        public const uint Volume3Visible = 3831;
        /// <summary>
        /// 3832
        /// </summary>
        public const uint VolumeSlider3Press = 3832;
        /// <summary>
        /// 3833
        /// </summary>
        public const uint Volume3MutePressAndFB = 3833;
        /// <summary>
        /// 3841
        /// </summary>
        public const uint Volume4Visible = 3841;
        /// <summary>
        /// 3842
        /// </summary>
        public const uint VolumeSlider4Press = 3842;
        /// <summary>
        /// 3843
        /// </summary>
        public const uint Volume4MutePressAndFB = 3843;
        /// <summary>
        /// 3851
        /// </summary>
        public const uint Volume5Visible = 3851;
        /// <summary>
        /// 3852
        /// </summary>
        public const uint VolumeSlider5Press = 3852;
        /// <summary>
        /// 3853
        /// </summary>
        public const uint Volume5MutePressAndFB = 3853;
        /// <summary>
        /// 3861
        /// </summary>
        public const uint Volume6Visible = 3861;
        /// <summary>
        /// 3862
        /// </summary>
        public const uint VolumeSlider6Press = 3862;
        /// <summary>
        /// 3863
        /// </summary>
        public const uint Volume6MutePressAndFB = 3863;

        /// <summary>
        /// 3870
        /// </summary>
        public const uint VolumesPageVisible = 3870;
        /// <summary>
        /// 3871
        /// </summary>
        public const uint VolumeDualMute1Visible = 3871;
        /// <summary>
        /// 3874
        /// </summary>
        public const uint Volume1SpeechMutePressAndFB = 3874;
        /// <summary>
        /// 3875
        /// </summary>
        public const uint Volume1BackerVisibility = 3875;
        /// <summary>
        /// 3891
        /// </summary>
        public const uint VolumeDefaultPress = 3891;
        /// <summary>
        /// 3901
        /// </summary>
        public const uint TechPagesExitButton = 3901;
        /// <summary>
        /// 3902
        /// </summary>
        public const uint TechPanelSetupVisible = 3902;
        /// <summary>
        /// 3999
        /// </summary>
		public const uint GenericModalVisible = 3999;
        /// <summary>
        /// 12345
        /// </summary>
		public const uint AvNoControlsSubVisible = 12345;
        /// <summary>
        /// 15001
        /// </summary>
        public const uint StartPageVisible = 15001;
        /// <summary>
        /// 15002 Shows the start page in the source controls area of the screen
        /// </summary>
		public const uint TapToBeginVisible = 15002;
        /// <summary>
        /// 15003 Message text when no source is showing
        /// </summary>
		public const uint SelectASourceVisible = 15003;
        /// <summary>
        /// 15004
        /// </summary>
		public const uint RoomIsOn = 15004;
        /// <summary>
        /// 15005 Shows always-on volume control subpage with only audio mute
        /// </summary>
        public const uint VolumeControlsSingleMuteVisible = 15005;
        /// <summary>
        /// 15006 Shows always-on volume control subpage with mic and audio mutes
        /// </summary>
        public const uint VolumeControlsDualMuteVisible = 15006;
        /// <summary>
        /// 15010 
        /// </summary>
        public const uint ShowPanelSetupPress = 15010;
        /// <summary>
        /// 15011
        /// </summary>
		public const uint TopBarVisible = 15011;
        /// <summary>
        /// 15012
        /// </summary>
		public const uint StagingPageVisible = 15012;
        /// <summary>
        /// 15013
        /// </summary>
		public const uint PowerOffStep1Visible = 15013;
        /// <summary>
        /// 15014
        /// </summary>
		public const uint PowerOffStep2Visible = 15014;
        /// <summary>
        /// 15015
        /// </summary>
		public const uint ShowPowerOffPress = 15015;
        /// <summary>
        /// 15016
        /// </summary>
		public const uint PowerOffMorePress = 15016;
        /// <summary>
        /// 15017
        /// </summary>
        public const uint StagingPageAdditionalArrowsVisible = 15017;
        /// <summary>
        /// 15020
        /// </summary>
		public const uint PanelSetupVisible = 15020;
        /// <summary>
        /// 15021
        /// </summary>
		public const uint SourceWaitOverlayVisible = 15021;
        /// <summary>
        /// 15022
        /// </summary>
        public const uint ActivityFooterVisible = 15022;
        /// <summary>
        /// 15023
        /// </summary>
        public const uint LightsHeaderButtonVisible = 15023;
        /// <summary>
        /// 15024
        /// </summary>
        public const uint CallRightHeaderButtonVisible = 15024;
        /// <summary>
        /// 15025
        /// </summary>
        public const uint CallLeftHeaderButtonVisible = 15025;
        /// <summary>
        /// 15026
        /// </summary>
        public const uint LightsHeaderButtonPress = 15026;
        /// <summary>
        /// 15027
        /// </summary>
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
        /// <summary>
        /// 15031
        /// </summary>
        public const uint AllRoomsOffPress = 15031;
        /// <summary>
        /// 15032
        /// </summary>
		public const uint DisplayPowerTogglePress = 15032;
        /// <summary>
        /// 15033
        /// </summary>
		public const uint PowerOffCancelPress = 15033;
        /// <summary>
        /// 15034
        /// </summary>
		public const uint PowerOffConfirmPress = 15034;
        /// <summary>
        /// 15035
        /// </summary>
		public const uint VolumeButtonPopupPress = 15035;
        /// <summary>
        /// 15035
        /// </summary>
		public const uint VolumeButtonPopupVisible = 15035;
        /// <summary>
        /// 15036
        /// </summary>
		public const uint VolumeGaugePopupVisible = 15036;
        /// <summary>
        /// 15037
        /// </summary>
        public const uint GearButtonVisible = 15037;
        /// <summary>
        /// 15040
        /// </summary>
        public const uint CallStatusPageVisible = 15040;
        /// <summary>
        /// 15041
        /// </summary>
        public const uint LightsPageVisible = 15041;
        /// <summary>
        /// 15042 Closes whichever interlocked modal is open
        /// </summary>
        public const uint InterlockedModalClosePress = 15042;
        /// <summary>
        /// 15043 Vis for modal backer for full-screen source
        /// </summary>
        public const uint SourceBackgroundOverlayVisible = 15043;
        /// <summary>
        /// 15044 Close button for source modal overlay
        /// </summary>
        public const uint SourceBackgroundOverlayClosePress = 15044;


        /// <summary>
        /// 15051
        /// </summary>
        public const uint Display1SelectPressAndFb = 15051;
        /// <summary>
        /// 15052
        /// </summary>
        public const uint Display1ControlButtonEnable = 15052;
        /// <summary>
        /// 15053
        /// </summary>
        public const uint Display1ControlButtonPress = 15053;
        /// <summary>
        /// 15054
        /// </summary>
        public const uint Display1AudioButtonEnable = 15054;
        /// <summary>
        /// 15055
        /// </summary>
        public const uint Display1AudioButtonPressAndFb = 15055;
        /// <summary>
        /// 15056
        /// </summary>
        public const uint Display2SelectPressAndFb = 15056;
        /// <summary>
        /// 15057
        /// </summary>
        public const uint Display2ControlButtonEnable = 15057;
        /// <summary>
        /// 15058
        /// </summary>
        public const uint Display2ControlButtonPress = 15058;
        /// <summary>
        /// 15059
        /// </summary>
        public const uint Display2AudioButtonEnable = 15059;
        /// <summary>
        /// 15060
        /// </summary>
        public const uint Display2AudioButtonPressAndFb = 15060;

        /// <summary>
        /// 15061 Reveals the dual-display subpage
        /// </summary>
        public const uint DualDisplayPageVisible = 15061;

        /// <summary>
        /// 15062 Reveals the toggle switch for the sharing mode
        /// </summary>
        public const uint ToggleSharingModeVisible = 15062;

        /// <summary>
        /// 15063 Press for the toggle mode switch
        /// </summary>
        public const uint ToggleSharingModePress = 15063;

        /// <summary>
        /// 15064
        /// </summary>
        public const uint LogoDefaultVisible = 15064;

        /// <summary>
        /// 15065
        /// </summary>
        public const uint LogoUrlVisible = 15065;

        /// <summary>
        /// 15085 Visibility join for help subpage
        /// </summary>
        public const uint HelpPageVisible = 15085;

        /// <summary>
        /// 15086 Press for help header button
        /// </summary>
        public const uint HelpPress = 15086;

        /// <summary>
        /// 15088
        /// </summary>
		public const uint DateOnlyVisible = 15088;
        /// <summary>
        /// 15089
        /// </summary>
		public const uint TimeOnlyVisible = 15089;
        /// <summary>
        /// 15090
        /// </summary>
		public const uint DateAndTimeVisible = 15090;
        /// <summary>
        /// 15091
        /// </summary>
		public const uint SetupFullDistrib = 15091;
    }
}