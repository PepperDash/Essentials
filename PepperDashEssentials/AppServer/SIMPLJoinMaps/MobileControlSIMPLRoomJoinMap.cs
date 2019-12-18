using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core;


namespace PepperDash.Essentials.AppServer
{
    public class MobileControlSIMPLRoomJoinMap : JoinMapBase
    {
        public const string ConfigIsLocal = "ConfigIsLocal";
        public const string RoomIsOn = "RoomIsOn";
        public const string PrivacyMute = "PrivacyMute";
        public const string PromptForCode = "PromptForCode";
        public const string ClientJoined = "ClientJoined";
        public const string ActivityShare = "ActivityShare";
        public const string ActivityPhoneCall = "ActivityPhoneCall";
        public const string ActivtyVideoCall = "ActivityVideoCall";
        public const string MasterVolume = "MasterVolumeMute";
        public const string VolumeJoinStart = "VolumeMutesJoinStart";
        public const string ShutdownCancel = "ShutdownCancel";
        public const string ShutdownEnd = "ShutdownEnd";
        public const string ShutdownStart = "ShutdownStart";
        public const string SourceHasChanged = "SourceHasChanged";
        public const string SpeedDialVisibleStartJoin = "SpeedDialVisibleStartJoin";
        public const string ConfigIsReady = "ConfigIsReady";
        public const string HideVideoConfRecents = "HideVideoConfRecents";
        public const string ShowCameraWhenNotInCall = "ShowCameraWhenNotInCall";
        public const string UseSourceEnabled = "UseSourceEnabled";
        public const string SourceShareDisableJoinStart = "SourceShareDisableJoinStart";
        public const string SourceIsEnabledJoinStart = "SourceIsEnabledJoinStart";

        //public const string MasterVolumeLevel = "MasterVolumeLevel";
        public const string VolumeSlidersJoinStart = "VolumeSlidersJoinStart";
        public const string ShutdownPromptDuration = "ShutdownPromptDuration";
        public const string NumberOfAuxFaders = "NumberOfAuxFaders";

        public const string VolumeSliderNamesJoinStart = "VolumeSliderNamesJoinStart";
        public const string SelectedSourceKey = "SelectedSourceKey";
        public const string SpeedDialNameStartJoin = "SpeedDialNameStartJoin";
        public const string SpeedDialNumberStartJoin = "SpeedDialNumberStartJoin";
        public const string ConfigRoomName = "ConfigRoomName";
        public const string ConfigHelpMessage = "ConfigHelpMessage";
        public const string ConfigHelpNumber = "ConfigHelpNumber";
        public const string ConfigRoomPhoneNumber = "ConfigRoomPhoneNumber";
        public const string ConfigRoomURI = "ConfigRoomURI";
        public const string UserCodeToSystem = "UserCodeToSystem";
        public const string ServerUrl = "ServerUrl";
        public const string RoomSpeedDialNamesJoinStart = "RoomSpeedDialNamesJoinStart";
        public const string RoomSpeedDialNumberssJoinStart = "RoomSpeedDialNumberssJoinStart";
        public const string SourceNameJoinStart = "SourceNameJoinStart";
        public const string SourceIconJoinStart = "SourceIconJoinStart";
        public const string SourceKeyJoinStart = "SourceKeyJoinStart";
        public const string SourceTypeJoinStart = "SourceTypeJoinStart";
        public const string CameraNearNameStart = "CameraNearNameStart";
        public const string CameraFarName = "CameraFarName";

        public MobileControlSIMPLRoomJoinMap()
        {
            Joins.Add(ConfigIsLocal, new JoinMetadata() { JoinNumber = 100, Label = "Config is local to Essentials", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(RoomIsOn, new JoinMetadata() { JoinNumber = 301, Label = "Room Is On", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(PrivacyMute, new JoinMetadata() { JoinNumber = 12, Label = "Privacy Mute Toggle/FB", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(PromptForCode, new JoinMetadata() { JoinNumber = 41, Label = "Prompt User for Code", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(ClientJoined, new JoinMetadata() { JoinNumber = 42, Label = "Client Joined", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(ActivityShare, new JoinMetadata() { JoinNumber = 51, Label = "Activity Share", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(ActivityPhoneCall, new JoinMetadata() { JoinNumber = 52, Label = "Activity Phone Call", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(ActivtyVideoCall, new JoinMetadata() { JoinNumber = 53, Label = "Activity Video Call", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(MasterVolume, new JoinMetadata() { JoinNumber = 1, Label = "Master Volume Mute Toggle/FB/Level/Label", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinSpan = 1, JoinType = eJoinType.DigitalAnalogSerial });
            Joins.Add(VolumeJoinStart, new JoinMetadata() { JoinNumber = 2, Label = "Volume Mute Toggle/FB/Level/Label", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinSpan = 9, JoinType = eJoinType.DigitalAnalogSerial });
            Joins.Add(ShutdownCancel, new JoinMetadata() { JoinNumber = 61, Label = "Shutdown Cancel", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(ShutdownEnd, new JoinMetadata() { JoinNumber = 62, Label = "Shutdown End", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(ShutdownStart, new JoinMetadata() { JoinNumber = 63, Label = "ShutdownStart", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(SourceHasChanged, new JoinMetadata() { JoinNumber = 71, Label = "Source Changed", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });            
            // Possibly move these to Audio Codec Messenger
            Joins.Add(SpeedDialVisibleStartJoin, new JoinMetadata() { JoinNumber = 261, Label = "Speed Dial Visible", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinSpan = 10, JoinType = eJoinType.Digital });
            Joins.Add(ConfigIsReady, new JoinMetadata() { JoinNumber = 501, Label = "Config info from SIMPL is ready", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(HideVideoConfRecents, new JoinMetadata() { JoinNumber = 502, Label = "Hide Video Conference Recents", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(ShowCameraWhenNotInCall, new JoinMetadata() { JoinNumber = 503, Label = "Show camera when not in call", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(UseSourceEnabled, new JoinMetadata() { JoinNumber = 504, Label = "Use Source Enabled Joins", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(SourceShareDisableJoinStart, new JoinMetadata() { JoinNumber = 601, Label = "Source is not sharable", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 20, JoinType = eJoinType.Digital });
            Joins.Add(SourceIsEnabledJoinStart, new JoinMetadata() { JoinNumber = 621, Label = "Source is enabled", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 20, JoinType = eJoinType.Digital });


            Joins.Add(ShutdownPromptDuration, new JoinMetadata() { JoinNumber = 61, Label = "Shutdown Prompt Timer Duration", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Analog });
            Joins.Add(NumberOfAuxFaders, new JoinMetadata() { JoinNumber = 101, Label = "Number of Auxilliary Faders", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Analog });


            Joins.Add(SelectedSourceKey, new JoinMetadata() { JoinNumber = 71, Label = "Key of selected source", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinSpan = 1, JoinType = eJoinType.Serial });

            // Possibly move these to Audio Codec Messenger
            Joins.Add(SpeedDialNameStartJoin, new JoinMetadata() { JoinNumber = 241, Label = "Speed Dial names", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 10, JoinType = eJoinType.Serial });
            Joins.Add(SpeedDialNumberStartJoin, new JoinMetadata() { JoinNumber = 251, Label = "Speed Dial numbers", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 10, JoinType = eJoinType.Serial });
            Joins.Add(UserCodeToSystem, new JoinMetadata() { JoinNumber = 401, Label = "User Ccde", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Serial });
            Joins.Add(ServerUrl, new JoinMetadata() { JoinNumber = 402, Label = "Server URL", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Serial });
            Joins.Add(ConfigRoomName, new JoinMetadata() { JoinNumber = 501, Label = "Room Nnme", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Serial });
            Joins.Add(ConfigHelpMessage, new JoinMetadata() { JoinNumber = 502, Label = "Room help message", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Serial });
            Joins.Add(ConfigHelpNumber, new JoinMetadata() { JoinNumber = 503, Label = "Room help number", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Serial });
            Joins.Add(ConfigRoomPhoneNumber, new JoinMetadata() { JoinNumber = 504, Label = "Room phone number", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Serial });
            Joins.Add(ConfigRoomURI, new JoinMetadata() { JoinNumber = 505, Label = "Room URI", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Serial });
            Joins.Add(SourceNameJoinStart, new JoinMetadata() { JoinNumber = 601, Label = "Source Names", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 20, JoinType = eJoinType.Serial });
            Joins.Add(SourceIconJoinStart, new JoinMetadata() { JoinNumber = 621, Label = "Source Icons", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 20, JoinType = eJoinType.Serial });
            Joins.Add(SourceKeyJoinStart, new JoinMetadata() { JoinNumber = 641, Label = "Source Keys", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 20, JoinType = eJoinType.Serial });
            Joins.Add(SourceTypeJoinStart, new JoinMetadata() { JoinNumber = 661, Label = "Source Types", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 20, JoinType = eJoinType.Serial });

            Joins.Add(CameraNearNameStart, new JoinMetadata() { JoinNumber = 761, Label = "Near End Camera Names", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 10, JoinType = eJoinType.Serial });
            Joins.Add(CameraNearNameStart, new JoinMetadata() { JoinNumber = 770, Label = "Far End Camera Name", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Serial });
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;

            foreach (var join in Joins)
            {
                join.Value.JoinNumber = join.Value.JoinNumber + joinOffset;
            }

            PrintJoinMapInfo();
        }
    }
}