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

        //****************************************************
        // Codec General
        
        /// <summary>
        /// 1001
        /// </summary>
        public const uint CallEndPress = 1001;
        /// <summary>
        /// 1002
        /// </summary>
        public const uint CallEndAllConfirmPress = 1002;
        /// <summary>
        /// 1003 - For tapping the text field to reveal the keyboard
        /// </summary>
        public const uint CodecDirectorySearchTextPress = 1003;
        /// <summary>
        /// 1004
        /// </summary>
        public const uint CallStopSharingPress = 1004;
        /// <summary>
        /// 1005
        /// </summary>
        public const uint CallSharedSourceInfoVisible = 1005;
		/// <summary>
		/// 1006
		/// </summary>
		public const uint CallEndAllConfirmVisible = 1006;
        /// <summary>
        /// 1007
        /// </summary>
        public const uint MeetingPasswordVisible = 1007;
        /// <summary>
        /// 1008
        /// </summary>
        public const uint MeetingLeavePress = 1008;





        // Audio Conference
        /// <summary>
        /// 1101
        /// </summary>
        public const uint ACKeypadVisible = 1101;
        /// <summary>
        /// 1102
        /// </summary>
        public const uint ACStagingPopoverVisible = 1102;
        /// <summary>
        /// 1111
        /// </summary>
        public const uint ACSpeedDial1Press = 1111;
        /// <summary>
        /// 1112
        /// </summary>
        public const uint ACSpeedDial2Press = 1112;
        /// <summary>
        /// 1113
        /// </summary>
        public const uint ACSpeedDial3Press = 1113;
        /// <summary>
        /// 1114
        /// </summary>
        public const uint ACSpeedDial4Press = 1114;
        /// <summary>
        /// 1121
        /// </summary>
        public const uint ACSpeedDial1Visible = 1121;
        /// <summary>
        /// 1122
        /// </summary>
        public const uint ACSpeedDial2Visible = 1122;
        /// <summary>
        /// 1123
        /// </summary>
        public const uint ACSpeedDial3Visible = 1123;
        /// <summary>
        /// 1124
        /// </summary>
        public const uint ACSpeedDial4Visible = 1124;

        //******************************************************
        // Video Conference
        /// <summary>
        /// 1201
        /// </summary>
        public const uint VCKeypadWithFavoritesVisible = 1201;
        /// <summary>
        /// 1202
        /// </summary>
        public const uint VCStagingInactivePopoverWithRecentsVisible = 1202;
        /// <summary>
        /// 
        /// </summary>
        public const uint VCStagingActivePopoverVisible = 1203;
		/// <summary>
		/// 
		/// </summary>
		public const uint VCKeypadVisible = 1204;
        /// <summary>
        /// 1205
        /// </summary>
        public const uint VCDirectoryVisible = 1205;
        /// <summary>
        /// 1206
        /// </summary>
        public const uint VCRecentsVisible = 1206;

        /// <summary>
        /// 1202
        /// </summary>
        public const uint VCStagingInactivePopoverWithoutRecentsVisible = 1207;

		/// <summary>
		/// 1208
		/// </summary>
		public const uint VCCameraAutoVisible = 1208;

        /// <summary>
        /// 1209
        /// </summary>
        public const uint VCCameraManualVisible = 1209;

        /// <summary>
        /// 1210
        /// </summary>
        public const uint VCCameraOffVisible = 1210;

        /// <summary>
        /// 1211 - 1215
        /// </summary>
        public const uint VCFavoritePressStart = 1211;
        //									RANGE IN USE
        public const uint VCFavoritePressEnd = 1215;
        /// <summary>
        /// 1221 - 1225
        /// </summary>
        public const uint VCFavoriteVisibleStart = 1221;
        //									RANGE IN USE
        public const uint VCFavoriteVisibleEnd = 1225;

        /// <summary>
        /// 1230
        /// </summary>
        public const uint VCStagingMeetNowPress = 1230;
        /// <summary>
        /// 1231
        /// </summary>
        public const uint VCStagingRecentsPress = 1231;
        /// <summary>
        /// 1232
        /// </summary>
        public const uint VCStagingDirectoryPress = 1232;
        /// <summary>
        /// 1233
        /// </summary>
        public const uint VCStagingKeypadPress = 1233;
        /// <summary>
        /// 1234
        /// </summary>
        public const uint VCStagingConnectPress = 1234;
        /// <summary>
        /// 1235
        /// </summary>
        public const uint VCStagingCameraPress = 1235;
        /// <summary>
        /// 1236
        /// </summary>
        public const uint VCStagingConnectEnable = 1236;
        /// <summary>
        /// 1237 - When the user touches the text field, should trigger keyboard
        /// </summary>
        public const uint VCKeypadTextPress = 1237;
        /// <summary>
        /// 1238
        /// </summary>
        public const uint VCKeypadBackspacePress = 1238;
        /// <summary>
        /// 1239
        /// </summary>
        public const uint VCKeypadBackspaceVisible = 1239;
		/// <summary>
		/// 1240
		/// </summary>
		public const uint VCDirectoryBackPress = 1240;
		/// <summary>
		/// 1241 For touching the text area to bring up keyboard
		/// </summary>
		public const uint VCDirectorySearchTextPress = 1241;
		/// <summary>
		/// 1242
		/// </summary>
		public const uint VCStagingSelfViewLayoutPress = 1242;
        /// <summary>
        /// 1243
        /// </summary>
        public const uint VCDirectoryBackVisible = 1243;
        /// <summary>
        /// 1244
        /// </summary>
        public const uint VCDirectoryBackspacePress = 1244;
        /// <summary>
        /// 1245
        /// </summary>
        public const uint VCDirectoryBackspaceVisible = 1245;
		/// <summary>
		/// 1251
		/// </summary>
		public const uint VCSelfViewTogglePress = 1251;
		/// <summary>
		/// 1252
		/// </summary>
		public const uint VCLayoutTogglePress = 1252;
		/// <summary>
		/// 1253
		/// </summary>
		public const uint VCSelfViewPipTogglePress = 1253;
		/// <summary>
		/// 1254
		/// </summary>
		public const uint VCLayoutToggleEnable = 1254;
		/// <summary>
		/// 1255
		/// </summary>
		public const uint VCMinMaxPress = 1255;
		/// <summary>
		/// 1256
		/// </summary>
		public const uint VCMinMaxEnable = 1256;

        /// <summary>
        /// 1260
        /// </summary>
        public const uint VCCameraModeBarVisible = 1260;

        /// <summary>
        /// 1261
        /// </summary>
        public const uint VCCameraSelectBarWithoutModeVisible = 1261;

        /// <summary>
        /// 1262
        /// </summary>
        public const uint VCCameraAutoModeIsOnFb = 1262;

        /// <summary>
        /// 1271
        /// </summary>
        public const uint VCCameraZoomIn = 1271;
        /// <summary>
        /// 1272
        /// </summary>
        public const uint VCCameraZoomOut = 1272;


        /// <summary>
        /// 1280
        /// </summary>
        public const uint VCCameraPresetSavedLabelVisible = 1280;
        /// <summary>
        /// 1281
        /// </summary>
        public const uint VCCameraPreset1 = 1281;
        /// <summary>
        /// 1282
        /// </summary>
        public const uint VCCameraPreset2 = 1282;
        /// <summary>
        /// 1283
        /// </summary>
        public const uint VCCameraPreset3 = 1283;


        /// <summary>
        /// 1291
        /// </summary>
        public const uint VCCameraPreset1Visible = 1291;
        /// <summary>
        /// 1292
        /// </summary>
        public const uint VCCameraPreset2Visible = 1292;
        /// <summary>
        /// 1293
        /// </summary>
        public const uint VCCameraPreset3Visible = 1293;


        // Letter joins start at 2921;

        //******************************************************

        // Environment Joins

        // Popup Container

        /// <summary>
        /// 2001 - 2004
        /// </summary>
        public const uint EnvironmentBackgroundSubpageVisibleBase = 2000;


        // ColumnOne

        /// <summary>
        /// 2011 - 2015
        /// </summary>
        public const uint EnvironmentColumnOneLightingTypeVisibleBase = 2010;

        /// <summary>
        /// 2016 - 2020
        /// </summary>
        public const uint EnvironmentColumnOneShadingTypeVisibleBase = 2015;

        // ColumnTwo

        /// <summary>
        /// 2021 - 2025
        /// </summary>
        public const uint EnvironmentColumnTwoLightingTypeVisibleBase = 2020;

        /// <summary>
        /// 2026 - 2030
        /// </summary>
        public const uint EnvironmentColumnTwoShadingTypeVisibleBase = 2025;

        // ColumnThree

        /// <summary>
        /// 2031 - 2035
        /// </summary>
        public const uint EnvironmentColumnThreeLightingTypeVisibleBase = 2030;

        /// <summary>
        /// 2036 - 2040
        /// </summary>
        public const uint EnvironmentColumnThreeShadingTypeVisibleBase = 2035;

        // ColumnFour

        /// <summary>
        /// 2041 - 2045
        /// </summary>
        public const uint EnvironmentColumnFourLightingTypeVisibleBase = 2040;

        /// <summary>
        /// 2046 - 2050
        /// </summary>
        public const uint EnvironmentColumnFourShadingTypeVisibleBase = 2045;

        // Button press

        /// <summary>
        /// 2051 - 2060
        /// </summary>
        public const uint EnvironmentColumnOneButtonPressBase = 2050;

        /// <summary>
        /// 2061 - 2070
        /// </summary>
        public const uint EnvironmentColumnTwoButtonPressBase = 2060;

        /// <summary>
        /// 2071 - 2080
        /// </summary>
        public const uint EnvironmentColumnThreeButtonPressBase = 2070;

        /// <summary>
        /// 2081 - 2090
        /// </summary>
        public const uint EnvironmentColumnFourButtonPressBase = 2080;

        // Button visibility

        /// <summary>
        /// 2151 - 2160
        /// </summary>
        public const uint EnvironmentColumnOneButtonVisibleBase = 2150;

        /// <summary>
        /// 2161 - 2170
        /// </summary>
        public const uint EnvironmentColumnTwoButtonVisibleBase = 2160;

        /// <summary>
        /// 2171 - 2180
        /// </summary>
        public const uint EnvironmentColumnThreeButtonVisibleBase = 2170;
        
        /// <summary>
        /// 2181 - 2190
        /// </summary>
        public const uint EnvironmentColumnFourButtonVisibleBase = 2180;


        //******************************************************

        /// <summary>
        /// 3101
        /// </summary>
        public const uint TechExitButton = 3101;
        /// <summary>
        /// 3106
        /// </summary>
        public const uint TechCommonItemsVisbible = 3106;
        /// <summary>
        /// 3107
        /// </summary>
        public const uint TechSystemStatusVisible = 3107;
        /// <summary>
        /// 3108
        /// </summary>
        public const uint TechDisplayControlsVisible = 3108;
        /// <summary>
        /// 3109
        /// </summary>
        public const uint TechPanelSetupVisible = 3109;
        /// <summary>
        /// 3110
        /// </summary>
        public const uint TechAdvancedVolumeVisible = 3110;
        /// <summary>
        /// 3111
        /// </summary>
        public const uint TechAboutVisible = 3111;
        /// <summary>
        /// 3112
        /// </summary>
        public const uint TechSchedulerVisible = 3112;

        //*****************************************************
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
		/// 3869 - when the system is off and the gear is pressed
		/// </summary>
		public const uint VolumesPagePowerOffVisible = 3869;
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
		/// 3951
		/// </summary
		
		public const uint HeaderIcon1Press = 3951;
		/// <summary>
		/// 3952
		/// </summary>
		public const uint HeaderIcon2Press = 3952;
		/// <summary>
		/// 3953
		/// </summary>
		public const uint HeaderIcon3Press = 3953;
		/// <summary>
		/// 3954
		/// </summary>
		public const uint HeaderIcon4Press = 3954;
		/// <summary>
		/// 3955
		/// </summary>
		public const uint HeaderIcon5Press = 3955;

        /// 3960
        /// </summary>
        public const uint HeaderPopupCaretsSubpageVisibile = 3960;
        /// <summary>
        /// 3961
        /// </summary>
        public const uint HeaderCaret1Visible = 3961;
        /// <summary>
        /// 3962
        /// </summary>
        public const uint HeaderCaret2Visible = 3962;
        /// <summary>
        /// 3963
        /// </summary>
        public const uint HeaderCaret3Visible = 3963;
        /// <summary>
        /// 3964
        /// </summary>
        public const uint HeaderCaret4Visible = 3964;
        /// <summary>
        /// 3965
        /// </summary>
        public const uint HeaderCaret5Visible = 3965;

        /// <summary>
        /// 3999
        /// </summary>
		public const uint GenericModalVisible = 3999;
        /// <summary>
        /// 12345
        /// </summary>
		public const uint AvNoControlsSubVisible = 12345;

        // 10000 - 14999 are general "source" pages

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
        /// 15011 - Top bar with room name and button that pops up dialog with room data
        /// </summary>
		public const uint TopBarHabaneroVisible = 15011;
        /// <summary>
        /// 15012
        /// </summary>
		public const uint SourceStagingBarVisible = 15012;
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
		/// 15018 The Header with dynamic buttons
		/// </summary>
		public const uint TopBarHabaneroDynamicVisible = 15018;
		/// <summary>
		/// 15019 Shown when system is starting and not ready for use
		/// </summary>
		public const uint SystemInitializingVisible = 15019;
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
        /// 15024
        /// </summary>
        public const uint HeaderCallStatusLeftPositionVisible = 15024;
        /// <summary>
        /// 15025
        /// </summary>
        public const uint HeaderCallStatusRightPositionVisible = 15025;
        /// <summary>
        /// 15027
        /// </summary>
        public const uint HeaderCallStatusLabelPress = 15027;
        /// <summary>
        /// 15028 The gear button in header
        /// </summary>
        public const uint FIXFIX_HeaderGearButtonPress_FIXFIX = 15028;
        /// <summary>
        /// 15029 the room button in header
        /// </summary>
        public const uint HeaderRoomButtonPress = 15029;
        /// <summary>
        /// 15030 Visibility for room data popup
        /// </summary>
        public const uint RoomHeaderInfoPageVisible = 15030;
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
        /// 15038
        /// </summary>
        public const uint CalendarHeaderButtonVisible = 15038;
        /// <summary>
        /// 15039
        /// </summary>
        public const uint CalendarHeaderButtonPress = 15039;
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
        /// 15045
        /// </summary>
        public const uint ZoomRoomContentSharingVisible = 15045;
		/// <summary>
		/// 15046
		/// </summary>
		public const uint MeetingsOrContacMethodsListVisible = 15046;
		/// <summary>
		/// 15047 The "Join" button on the next meeting ribbon
		/// </summary>
		public const uint NextMeetingJoinPress = 15047;
		/// <summary>
		/// 15048 Dismisses the ribbon
		/// </summary>
		public const uint NextMeetingModalClosePress = 15048;
		/// <summary>
		/// 15049
		/// </summary>
		public const uint NextMeetingModalVisible = 15049;
        /// <summary>
        /// 15050
        /// </summary>
        public const uint NextMeetingNotificationRibbonVisible = 15050;
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
        /// 15066 - Reveals the active calls header item
        /// </summary>
        public const uint HeaderActiveCallsListVisible = 15066;
        /// <summary>
        /// 15067
        /// </summary>
        public const uint NotificationRibbonVisible = 15067;
        /// <summary>
        /// 15068
        /// </summary>
        public const uint HeaderMeetingInfoVisible = 15068;

        /// <summary>
        /// 15083 - Press for Call help desk on AC/VC
        /// </summary>
        public const uint HelpPageShowCallButtonPress = 15083;
        /// <summary>
        /// 15084 - Show the "call help desk" button on help page
        /// </summary>
        public const uint HelpPageShowCallButtonVisible = 15084;
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

        /// <summary>
        /// 15092
        /// </summary>
        public const uint StartMCPageVisible = 15092;


        /// <summary>
        /// 15093
        /// </summary>
        public const uint RoomHeaderInfoMCPageVisible = 15093;

        /// <summary>
        /// 15094
        /// </summary>
        public const uint MCScreenSaverVisible = 15094;

        /// <summary>
        /// 15095
        /// </summary>
        public const uint MCScreenSaverPosition1Visible = 15095;

        /// <summary>
        /// 15096
        /// </summary>
        public const uint MCScreenSaverPosition2Visible = 15096;

        /// <summary>
        /// 15097
        /// </summary>
        public const uint MCScreenSaverPosition3Visible = 15097;

        /// <summary>
        /// 15098
        /// </summary>
        public const uint MCScreenSaverPosition4Visible = 15098;

        /// <summary>
        /// 15099
        /// </summary>
        public const uint MCScreenSaverClosePress = 15099;

        // PIN dialogs ************************************

        /// <summary>
        /// 15201
        /// </summary>
        public const uint PinDialog4DigitVisible = 15201;
        /// <summary>
        /// 15206
        /// </summary>
        public const uint PinDialogCancelPress = 15206;
        /// <summary>
        /// 15207
        /// </summary>
        public const uint PinDialogErrorVisible = 15207;
        /// <summary>
        /// 15211
        /// </summary>
        public const uint PinDialogDot1 = 15211;
        /// <summary>
        /// 15212
        /// </summary>
        public const uint PinDialogDot2 = 15212;
        /// <summary>
        /// 15213
        /// </summary>
        public const uint PinDialogDot3 = 15213;
        /// <summary>
        /// 15214
        /// </summary>
        public const uint PinDialogDot4 = 15214;

        //  Password Prompt Dialog **************************

        /// <summary>
        /// 15301
        /// </summary>
        public const uint PasswordPromptDialogVisible = 15301;
        /// <summary>
        /// 15302
        /// </summary>
        public const uint PasswordPromptTextPress = 15302;
        /// <summary>
        /// 15306
        /// </summary>
        public const uint PasswordPromptCancelPress = 15306;
        /// <summary>
        /// 15307
        /// </summary>
        public const uint PasswordPromptErrorVisible = 15307;
    }
}