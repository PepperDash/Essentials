using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core;


namespace PepperDash.Essentials.AppServer
{
    public class MobileControlSIMPLRoomJoinMap : JoinMapBaseAdvanced
    {
        [JoinName("MasterVolume")]
        public JoinDataComplete MasterVolume = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 }, new JoinMetadata() {Label = "Master Volume Mute Toggle/FB/Level/Label", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.DigitalAnalogSerial });
        [JoinName("VolumeJoinStart")]
        public JoinDataComplete VolumeJoinStart = new JoinDataComplete(new JoinData() { JoinNumber = 2, JoinSpan = 8 }, new JoinMetadata() {Label = "Volume Mute Toggle/FB/Level/Label", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.DigitalAnalogSerial });

        [JoinName("PrivacyMute")]
        public JoinDataComplete PrivacyMute = new JoinDataComplete(new JoinData() { JoinNumber = 12, JoinSpan = 1 }, new JoinMetadata() { Label = "Privacy Mute Toggle/FB", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("PromptForCode")]
        public JoinDataComplete PromptForCode = new JoinDataComplete(new JoinData() { JoinNumber = 41, JoinSpan = 1 }, new JoinMetadata() {Label = "Prompt User for Code", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });
        [JoinName("ClientJoined")]
        public JoinDataComplete ClientJoined = new JoinDataComplete(new JoinData() { JoinNumber = 42, JoinSpan = 1 }, new JoinMetadata() { Label = "Client Joined", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });
        [JoinName("ActivityShare")]
        public JoinDataComplete ActivityShare = new JoinDataComplete(new JoinData() { JoinNumber = 51, JoinSpan = 1 }, new JoinMetadata() {Label = "Activity Share", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        [JoinName("ActivityPhoneCall")]
        public JoinDataComplete ActivityPhoneCall = new JoinDataComplete(new JoinData() { JoinNumber = 52, JoinSpan = 1 }, new JoinMetadata() { Label = "Activity Phone Call", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        [JoinName("ActivityVideoCall")]
        public JoinDataComplete ActivityVideoCall = new JoinDataComplete(new JoinData() { JoinNumber = 53, JoinSpan = 1 }, new JoinMetadata() { Label = "Activity Video Call", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("ShutdownPromptDuration")]
        public JoinDataComplete ShutdownPromptDuration = new JoinDataComplete(new JoinData() { JoinNumber = 61, JoinSpan = 1 }, new JoinMetadata() { Label ="Shutdown Cancel", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Analog });
        [JoinName("ShutdownCancel")]
        public JoinDataComplete ShutdownCancel = new JoinDataComplete(new JoinData() { JoinNumber = 61, JoinSpan = 1 }, new JoinMetadata() { Label ="Shutdown Cancel", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        [JoinName("ShutdownEnd")]
        public JoinDataComplete ShutdownEnd = new JoinDataComplete(new JoinData() { JoinNumber = 62, JoinSpan = 1 }, new JoinMetadata() { Label = "Shutdown End", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        [JoinName("ShutdownStart")]
        public JoinDataComplete ShutdownStart = new JoinDataComplete(new JoinData() { JoinNumber = 63, JoinSpan = 1 }, new JoinMetadata() { Label = "Shutdown Start", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("SourceHasChanged")]
        public JoinDataComplete SourceHasChanged = new JoinDataComplete(new JoinData() { JoinNumber = 71, JoinSpan = 1 }, new JoinMetadata() { Label = "Source Changed", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        [JoinName("CurrentSourceKey")]
        public JoinDataComplete CurrentSourceKey = new JoinDataComplete(new JoinData() { JoinNumber = 71, JoinSpan = 1 }, new JoinMetadata() { Label = "Key of selected source", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Serial });


        [JoinName("ConfigIsLocal")]
        public JoinDataComplete ConfigIsLocal = new JoinDataComplete(new JoinData() { JoinNumber = 100, JoinSpan = 1 }, new JoinMetadata() { Label = "Config is local to Essentials", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        [JoinName("NumberOfAuxFaders")]
        public JoinDataComplete NumberOfAuxFaders = new JoinDataComplete(new JoinData() { JoinNumber = 101, JoinSpan = 1 }, new JoinMetadata() { Label = "Number of Auxilliary Faders", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("SpeedDialNameStartJoin")]
        public JoinDataComplete SpeedDialNameStartJoin = new JoinDataComplete(new JoinData() { JoinNumber = 241, JoinSpan = 10 }, new JoinMetadata() { Label = "Speed Dial names", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Serial });
        [JoinName("SpeedDialNumberStartJoin")]
        public JoinDataComplete SpeedDialNumberStartJoin = new JoinDataComplete(new JoinData() { JoinNumber = 251, JoinSpan = 10 }, new JoinMetadata() { Label = "Speed Dial numbers", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Serial });

        [JoinName("SpeedDialVisibleStartJoin")]
        public JoinDataComplete SpeedDialVisibleStartJoin = new JoinDataComplete(new JoinData() { JoinNumber = 261, JoinSpan = 10 }, new JoinMetadata() { Label =  "Speed Dial Visible", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
      
        [JoinName("RoomIsOn")]
        public JoinDataComplete RoomIsOn = new JoinDataComplete(new JoinData() { JoinNumber = 301, JoinSpan = 1 }, new JoinMetadata() { Label = "Room Is On", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("UserCodeToSystem")]
        public JoinDataComplete UserCodeToSystem = new JoinDataComplete(new JoinData() { JoinNumber = 401, JoinSpan = 1 }, new JoinMetadata() { Label = "User Ccde", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });
        [JoinName("ServerUrl")]
        public JoinDataComplete ServerUrl = new JoinDataComplete(new JoinData() { JoinNumber = 402, JoinSpan = 1 }, new JoinMetadata() { Label ="Server URL", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });
        
        [JoinName("ConfigRoomName")]
        public JoinDataComplete ConfigRoomName = new JoinDataComplete(new JoinData() { JoinNumber = 501, JoinSpan = 1 }, new JoinMetadata() {Label = "Room Nnme", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Serial });
        [JoinName("ConfigHelpMessage")]
        public JoinDataComplete ConfigHelpMessage = new JoinDataComplete(new JoinData() { JoinNumber = 502, JoinSpan = 1 }, new JoinMetadata() { Label = "Room help message", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Serial });
        [JoinName("ConfigHelpNumber")]
        public JoinDataComplete ConfigHelpNumber = new JoinDataComplete(new JoinData() { JoinNumber = 503, JoinSpan = 1 }, new JoinMetadata() {  Label = "Room help number", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Serial });
        [JoinName("ConfigRoomPhoneNumber")]
        public JoinDataComplete ConfigRoomPhoneNumber = new JoinDataComplete(new JoinData() { JoinNumber = 504, JoinSpan = 1 }, new JoinMetadata() { Label = "Room phone number", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Serial });
        [JoinName("ConfigRoomURI")]
        public JoinDataComplete ConfigRoomURI = new JoinDataComplete(new JoinData() { JoinNumber = 505, JoinSpan = 1 }, new JoinMetadata() {  Label = "Room URI", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Serial });

        [JoinName("ConfigIsReady")]
        public JoinDataComplete ConfigIsReady = new JoinDataComplete(new JoinData() { JoinNumber = 501, JoinSpan = 1 }, new JoinMetadata() { Label =  "Config info from SIMPL is ready", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        [JoinName("HideVideoConfRecents")]
        public JoinDataComplete HideVideoConfRecents = new JoinDataComplete(new JoinData() { JoinNumber = 502, JoinSpan = 1 }, new JoinMetadata() { Label = "Hide Video Conference Recents", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        [JoinName("ShowCameraWhenNotInCall")]
        public JoinDataComplete ShowCameraWhenNotInCall = new JoinDataComplete(new JoinData() { JoinNumber = 503, JoinSpan = 1 }, new JoinMetadata() { Label = "Show camera when not in call", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        [JoinName("UseSourceEnabled")]
        public JoinDataComplete UseSourceEnabled = new JoinDataComplete(new JoinData() { JoinNumber = 504, JoinSpan = 1 }, new JoinMetadata() { Label = "Use Source Enabled Joins", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });


        [JoinName("SourceShareDisableJoinStart")]
        public JoinDataComplete SourceShareDisableJoinStart = new JoinDataComplete(new JoinData() { JoinNumber = 601, JoinSpan = 20 }, new JoinMetadata() { Label = "Source is not sharable", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        [JoinName("SourceIsEnabledJoinStart")]
        public JoinDataComplete SourceIsEnabledJoinStart = new JoinDataComplete(new JoinData() { JoinNumber = 621, JoinSpan = 20 }, new JoinMetadata() { Label = "Source is enabled", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("SourceNameJoinStart")]
        public JoinDataComplete SourceNameJoinStart = new JoinDataComplete(new JoinData() { JoinNumber = 601, JoinSpan = 20 }, new JoinMetadata() { Label = "Source Names", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Serial });
        [JoinName("SourceIconJoinStart")]
        public JoinDataComplete SourceIconJoinStart = new JoinDataComplete(new JoinData() { JoinNumber = 621, JoinSpan = 20 }, new JoinMetadata() { Label = "Source Icons", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Serial });
        [JoinName("SourceKeyJoinStart")]
        public JoinDataComplete SourceKeyJoinStart = new JoinDataComplete(new JoinData() { JoinNumber = 641, JoinSpan = 20 }, new JoinMetadata() { Label = "Source Keys", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Serial });
        [JoinName("SourceTypeJoinStart")]
        public JoinDataComplete SourceTypeJoinStart = new JoinDataComplete(new JoinData() { JoinNumber = 661, JoinSpan = 20 }, new JoinMetadata() { Label = "Source Types", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Serial });

        [JoinName("CameraNearNameStart")]
        public JoinDataComplete CameraNearNameStart = new JoinDataComplete(new JoinData() { JoinNumber = 761, JoinSpan = 10 }, new JoinMetadata() { Label = "Near End Camera Names", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Serial });
        [JoinName("CameraFarName")]
        public JoinDataComplete CameraFarName = new JoinDataComplete(new JoinData() { JoinNumber = 770, JoinSpan = 1 }, new JoinMetadata() { Label = "Far End Camera Name", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Serial });

        public MobileControlSIMPLRoomJoinMap(uint joinStart)
            :base(joinStart)
        {
         
        }
    }
}