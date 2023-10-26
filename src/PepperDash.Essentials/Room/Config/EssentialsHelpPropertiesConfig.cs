extern alias Full;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Room.Config
{
    /// <summary>
    /// Properties for the help text box
    /// </summary>
    public class EssentialsHelpPropertiesConfig
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("showCallButton")]
        public bool ShowCallButton { get; set; }
        
        /// <summary>
        /// Defaults to "Call Help Desk"
        /// </summary>
        [JsonProperty("callButtonText")]
        public string CallButtonText { get; set; }

        public EssentialsHelpPropertiesConfig()
        {
            CallButtonText = "Call Help Desk";
        }
    }
}