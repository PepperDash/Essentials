using PepperDash.Essentials.Core;


namespace PepperDash.Essentials.AppServer
{
    // ReSharper disable once InconsistentNaming
    public class MobileControlSIMPLRoomJoinMap : JoinMapBaseAdvanced
    {
        [JoinName("QrCodeUrl")]
        public JoinDataComplete QrCodeUrl =
            new JoinDataComplete(new JoinData { JoinNumber = 403, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "QR Code URL",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("PortalSystemUrl")]
        public JoinDataComplete PortalSystemUrl =
            new JoinDataComplete(new JoinData { JoinNumber = 404, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Portal System URL",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Serial
            });

        [JoinName("MasterVolume")]
        public JoinDataComplete MasterVolume =
            new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Master Volume Mute Toggle/FB/Level/Label",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.DigitalAnalogSerial
                });

        [JoinName("VolumeJoinStart")]
        public JoinDataComplete VolumeJoinStart =
            new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 8 },
                new JoinMetadata
                {
                    Description = "Volume Mute Toggle/FB/Level/Label",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.DigitalAnalogSerial
                });

        [JoinName("PrivacyMute")]
        public JoinDataComplete PrivacyMute =
            new JoinDataComplete(new JoinData { JoinNumber = 12, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Privacy Mute Toggle/FB",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("PromptForCode")]
        public JoinDataComplete PromptForCode =
            new JoinDataComplete(new JoinData { JoinNumber = 41, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Prompt User for Code",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("ClientJoined")]
        public JoinDataComplete ClientJoined =
            new JoinDataComplete(new JoinData { JoinNumber = 42, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Client Joined",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("ActivityPhoneCallEnable")]
        public JoinDataComplete ActivityPhoneCallEnable =
            new JoinDataComplete(new JoinData { JoinNumber = 48, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Enable Activity Phone Call",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("ActivityVideoCallEnable")]
        public JoinDataComplete ActivityVideoCallEnable =
            new JoinDataComplete(new JoinData { JoinNumber = 49, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Enable Activity Video Call",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("ActivityShare")]
        public JoinDataComplete ActivityShare =
            new JoinDataComplete(new JoinData { JoinNumber = 51, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Activity Share",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("ActivityPhoneCall")]
        public JoinDataComplete ActivityPhoneCall =
            new JoinDataComplete(new JoinData { JoinNumber = 52, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Activity Phone Call",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("ActivityVideoCall")]
        public JoinDataComplete ActivityVideoCall =
            new JoinDataComplete(new JoinData { JoinNumber = 53, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Activity Video Call",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("ShutdownPromptDuration")]
        public JoinDataComplete ShutdownPromptDuration =
            new JoinDataComplete(new JoinData { JoinNumber = 61, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Shutdown Cancel",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Analog
                });

        [JoinName("ShutdownCancel")]
        public JoinDataComplete ShutdownCancel =
            new JoinDataComplete(new JoinData { JoinNumber = 61, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Shutdown Cancel",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("ShutdownEnd")]
        public JoinDataComplete ShutdownEnd =
            new JoinDataComplete(new JoinData { JoinNumber = 62, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Shutdown End",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("ShutdownStart")]
        public JoinDataComplete ShutdownStart =
            new JoinDataComplete(new JoinData { JoinNumber = 63, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Shutdown Start",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("SourceHasChanged")]
        public JoinDataComplete SourceHasChanged =
            new JoinDataComplete(new JoinData { JoinNumber = 71, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Source Changed",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("CurrentSourceKey")]
        public JoinDataComplete CurrentSourceKey =
            new JoinDataComplete(new JoinData { JoinNumber = 71, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Key of selected source",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.Serial
                });


        [JoinName("ConfigIsLocal")]
        public JoinDataComplete ConfigIsLocal =
            new JoinDataComplete(new JoinData { JoinNumber = 100, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Config is local to Essentials",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("NumberOfAuxFaders")]
        public JoinDataComplete NumberOfAuxFaders =
            new JoinDataComplete(new JoinData { JoinNumber = 101, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Number of Auxilliary Faders",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Analog
                });

        [JoinName("SpeedDialNameStartJoin")]
        public JoinDataComplete SpeedDialNameStartJoin =
            new JoinDataComplete(new JoinData { JoinNumber = 241, JoinSpan = 10 },
                new JoinMetadata
                {
                    Description = "Speed Dial names",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("SpeedDialNumberStartJoin")]
        public JoinDataComplete SpeedDialNumberStartJoin =
            new JoinDataComplete(new JoinData { JoinNumber = 251, JoinSpan = 10 },
                new JoinMetadata
                {
                    Description = "Speed Dial numbers",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("SpeedDialVisibleStartJoin")]
        public JoinDataComplete SpeedDialVisibleStartJoin =
            new JoinDataComplete(new JoinData { JoinNumber = 261, JoinSpan = 10 },
                new JoinMetadata
                {
                    Description = "Speed Dial Visible",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("RoomIsOn")]
        public JoinDataComplete RoomIsOn =
            new JoinDataComplete(new JoinData { JoinNumber = 301, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Room Is On",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("UserCodeToSystem")]
        public JoinDataComplete UserCodeToSystem =
            new JoinDataComplete(new JoinData { JoinNumber = 401, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "User Code",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("ServerUrl")]
        public JoinDataComplete ServerUrl =
            new JoinDataComplete(new JoinData { JoinNumber = 402, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Server URL",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("ConfigRoomName")]
        public JoinDataComplete ConfigRoomName =
            new JoinDataComplete(new JoinData { JoinNumber = 501, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Room Name",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("ConfigHelpMessage")]
        public JoinDataComplete ConfigHelpMessage =
            new JoinDataComplete(new JoinData { JoinNumber = 502, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Room help message",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("ConfigHelpNumber")]
        public JoinDataComplete ConfigHelpNumber =
            new JoinDataComplete(new JoinData { JoinNumber = 503, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Room help number",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("ConfigRoomPhoneNumber")]
        public JoinDataComplete ConfigRoomPhoneNumber =
            new JoinDataComplete(new JoinData { JoinNumber = 504, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Room phone number",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("ConfigRoomURI")]
        public JoinDataComplete ConfigRoomUri =
            new JoinDataComplete(new JoinData { JoinNumber = 505, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Room URI",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("ApiOnlineAndAuthorized")]
        public JoinDataComplete ApiOnlineAndAuthorized =
            new JoinDataComplete(new JoinData { JoinNumber = 500, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Config info from SIMPL is ready",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("ConfigIsReady")]
        public JoinDataComplete ConfigIsReady =
            new JoinDataComplete(new JoinData { JoinNumber = 501, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Config info from SIMPL is ready",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("ReadyForConfig")]
        public JoinDataComplete ReadyForConfig =
            new JoinDataComplete(new JoinData { JoinNumber = 501, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Config info from SIMPL is ready",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("HideVideoConfRecents")]
        public JoinDataComplete HideVideoConfRecents =
            new JoinDataComplete(new JoinData { JoinNumber = 502, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Hide Video Conference Recents",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("ShowCameraWhenNotInCall")]
        public JoinDataComplete ShowCameraWhenNotInCall =
            new JoinDataComplete(new JoinData { JoinNumber = 503, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Show camera when not in call",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("UseSourceEnabled")]
        public JoinDataComplete UseSourceEnabled =
            new JoinDataComplete(new JoinData { JoinNumber = 504, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Use Source Enabled Joins",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });


        [JoinName("SourceShareDisableJoinStart")]
        public JoinDataComplete SourceShareDisableJoinStart =
            new JoinDataComplete(new JoinData { JoinNumber = 601, JoinSpan = 20 },
                new JoinMetadata
                {
                    Description = "Source is not sharable",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("SourceIsEnabledJoinStart")]
        public JoinDataComplete SourceIsEnabledJoinStart =
            new JoinDataComplete(new JoinData { JoinNumber = 621, JoinSpan = 20 },
                new JoinMetadata
                {
                    Description = "Source is enabled/visible",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("SourceIsControllableJoinStart")]
        public JoinDataComplete SourceIsControllableJoinStart =
            new JoinDataComplete(new JoinData { JoinNumber = 641, JoinSpan = 20 },
            new JoinMetadata
            {
                Description = "Source is controllable",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("SourceIsAudioSourceJoinStart")]
        public JoinDataComplete SourceIsAudioSourceJoinStart =
            new JoinDataComplete(new JoinData { JoinNumber = 661, JoinSpan = 20 },
            new JoinMetadata
            {
                Description = "Source is Audio Source",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });


        [JoinName("SourceNameJoinStart")]
        public JoinDataComplete SourceNameJoinStart =
            new JoinDataComplete(new JoinData { JoinNumber = 601, JoinSpan = 20 },
                new JoinMetadata
                {
                    Description = "Source Names",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("SourceIconJoinStart")]
        public JoinDataComplete SourceIconJoinStart =
            new JoinDataComplete(new JoinData { JoinNumber = 621, JoinSpan = 20 },
                new JoinMetadata
                {
                    Description = "Source Icons",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("SourceKeyJoinStart")]
        public JoinDataComplete SourceKeyJoinStart =
            new JoinDataComplete(new JoinData { JoinNumber = 641, JoinSpan = 20 },
                new JoinMetadata
                {
                    Description = "Source Keys",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("SourceControlDeviceKeyJoinStart")]
        public JoinDataComplete SourceControlDeviceKeyJoinStart =
            new JoinDataComplete(new JoinData { JoinNumber = 701, JoinSpan = 20 },
            new JoinMetadata
            {
                Description = "Source Control Device Keys",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Serial
            });

        [JoinName("SourceTypeJoinStart")]
        public JoinDataComplete SourceTypeJoinStart =
            new JoinDataComplete(new JoinData { JoinNumber = 661, JoinSpan = 20 },
                new JoinMetadata
                {
                    Description = "Source Types",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("CameraNearNameStart")]
        public JoinDataComplete CameraNearNameStart =
            new JoinDataComplete(new JoinData { JoinNumber = 761, JoinSpan = 10 },
                new JoinMetadata
                {
                    Description = "Near End Camera Names",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("CameraFarName")]
        public JoinDataComplete CameraFarName =
            new JoinDataComplete(new JoinData { JoinNumber = 771, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Far End Camera Name",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Serial
                });

        #region Advanced Sharing
        [JoinName("SupportsAdvancedSharing")]
        public JoinDataComplete SupportsAdvancedSharing =
            new JoinDataComplete(new JoinData { JoinNumber = 505, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Supports Advanced Sharing",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("UseDestinationEnable")]
        public JoinDataComplete UseDestinationEnable =
            new JoinDataComplete(new JoinData { JoinNumber = 506, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Use Destination Enable",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });


        [JoinName("UserCanChangeShareMode")]
        public JoinDataComplete UserCanChangeShareMode =
            new JoinDataComplete(new JoinData { JoinNumber = 507, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Share Mode Toggle Visible to User",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("DestinationNameJoinStart")]
        public JoinDataComplete DestinationNameJoinStart =
            new JoinDataComplete(new JoinData { JoinNumber = 801, JoinSpan = 10 },
            new JoinMetadata
            {
                Description = "Destination Name",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Serial
            });

        [JoinName("DestinationDeviceKeyJoinStart")]
        public JoinDataComplete DestinationDeviceKeyJoinStart =
            new JoinDataComplete(new JoinData { JoinNumber = 811, JoinSpan = 10 },
            new JoinMetadata
            {
                Description = "Destination Device Key",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Serial
            });

        [JoinName("DestinationTypeJoinStart")]
        public JoinDataComplete DestinationTypeJoinStart =
            new JoinDataComplete(new JoinData { JoinNumber = 821, JoinSpan = 10 },
            new JoinMetadata
            {
                Description = "Destination type. Should be Audio, Video, AudioVideo",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Serial
            });

        [JoinName("DestinationIsEnabledJoinStart")]
        public JoinDataComplete DestinationIsEnabledJoinStart =
            new JoinDataComplete(new JoinData { JoinNumber = 801, JoinSpan = 10 },
            new JoinMetadata
            {
                Description = "Show Destination on UI",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        #endregion

        public MobileControlSIMPLRoomJoinMap(uint joinStart)
            : base(joinStart, typeof(MobileControlSIMPLRoomJoinMap))
        {
        }
    }
}