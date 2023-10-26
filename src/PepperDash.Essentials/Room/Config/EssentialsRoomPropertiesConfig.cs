extern alias Full;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Room.Config
{
    /// <summary>
    /// 
    /// </summary>
    public class EssentialsRoomPropertiesConfig
    {
        [JsonProperty("addresses")]
        public EssentialsRoomAddressPropertiesConfig Addresses { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("emergency")]
        public EssentialsRoomEmergencyConfig Emergency { get; set; }

        [JsonProperty("help")]
        public EssentialsHelpPropertiesConfig Help { get; set; }

        [JsonProperty("helpMessage")]
        public string HelpMessage { get; set; }

        /// <summary>
        /// Read this value to get the help message.  It checks for the old and new config format.
        /// </summary>
        public string HelpMessageForDisplay
        {
            get
            {
                if(Help != null && !string.IsNullOrEmpty(Help.Message))
                {
                    return Help.Message;
                }
                else
                {
                    return HelpMessage; 
                }
            }
        }

        [JsonProperty("environment")]
        public EssentialsEnvironmentPropertiesConfig Environment { get; set; }

        [JsonProperty("logo")]
        public EssentialsLogoPropertiesConfig LogoLight { get; set; }

        [JsonProperty("logoDark")]
        public EssentialsLogoPropertiesConfig LogoDark { get; set; }
	
        [JsonProperty("microphonePrivacy")]
        public EssentialsRoomMicrophonePrivacyConfig MicrophonePrivacy { get; set; }

        [JsonProperty("occupancy")]
        public EssentialsRoomOccSensorConfig Occupancy { get; set; }

        [JsonProperty("oneButtonMeeting")]
        public EssentialsOneButtonMeetingPropertiesConfig OneButtonMeeting { get; set; }

        [JsonProperty("shutdownVacancySeconds")]
        public int ShutdownVacancySeconds { get; set; }

        [JsonProperty("shutdownPromptSeconds")]
        public int ShutdownPromptSeconds { get; set; }

        [JsonProperty("tech")]
        public EssentialsRoomTechConfig Tech { get; set; }

        [JsonProperty("volumes")]
        public EssentialsRoomVolumesConfig Volumes { get; set; }

        [JsonProperty("fusion")]
        public EssentialsRoomFusionConfig Fusion { get; set; }

        [JsonProperty("essentialsRoomUiBehaviorConfig", NullValueHandling=NullValueHandling.Ignore)]
        public EssentialsRoomUiBehaviorConfig UiBehavior { get; set; }

        [JsonProperty("zeroVolumeWhenSwtichingVolumeDevices")]
        public bool ZeroVolumeWhenSwtichingVolumeDevices { get; set; }

        /// <summary>
        /// Indicates if this room represents a combination of other rooms
        /// </summary>
        [JsonProperty("isRoomCombinationScenario")]
        public bool IsRoomCombinationScenario { get; set; }

        public EssentialsRoomPropertiesConfig()
        {
            LogoLight = new EssentialsLogoPropertiesConfig();
            LogoDark  = new EssentialsLogoPropertiesConfig();
        }
    }
}