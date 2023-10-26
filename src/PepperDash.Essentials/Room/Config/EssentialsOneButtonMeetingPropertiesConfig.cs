extern alias Full;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Room.Config
{
    /// <summary>
    /// 
    /// </summary>
    public class EssentialsOneButtonMeetingPropertiesConfig
    {
        [JsonProperty("enable")]
        public bool Enable { get; set; }
    }
}