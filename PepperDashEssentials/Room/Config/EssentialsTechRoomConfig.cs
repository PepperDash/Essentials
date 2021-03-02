using System.Collections.Generic;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Room.Config
{
    public class EssentialsTechRoomConfig
    {
        /// <summary>
        /// The key of the dummy device used to enable routing
        /// </summary>
        [JsonProperty("dummySourceKey")]
        public string DummySourceKey { get; set; }

        /// <summary>
        /// The keys of the displays assigned to this room
        /// </summary>
        [JsonProperty("displays")]
        public List<string> Displays { get; set; }
        
        /// <summary>
        /// The keys of the tuners assinged to this room
        /// </summary>
        [JsonProperty("tuners")]
        public List<string> Tuners { get; set; }

        /// <summary>
        /// PIN to access the room as a normal user
        /// </summary>
        [JsonProperty("userPin")]
        public string UserPin { get; set; }

        /// <summary>
        /// PIN to access the room as a tech user
        /// </summary>
        [JsonProperty("techPin")]
        public string TechPin { get; set; }

        /// <summary>
        /// Name of the presets file.  Path prefix is assumed to be /html/presets/lists/
        /// </summary>
        [JsonProperty("presetsFileName")]
        public string PresetsFileName { get; set; }

        [JsonProperty("scheduledEvents")]
        public List<ScheduledEventConfig> ScheduledEvents { get; set; }

        /// <summary>
        /// Indicates that the room is the primary when true
        /// </summary>
        [JsonProperty("isPrimary")]
        public bool IsPrimary { get; set; }

        /// <summary>
        /// Indicates which tuners should mirror preset recall when two rooms are configured in a primary->secondary scenario
        /// </summary>
        [JsonProperty("mirroredTuners")]
        public List<string> MirroredTuners { get; set; }

        /// <summary>
        /// Indicates the room 
        /// </summary>
        [JsonProperty("isTvPresetsProvider")] 
        public bool IsTvPresetsProvider;

        public EssentialsTechRoomConfig()
        {
            Displays = new List<string>();
            Tuners = new List<string>();
            ScheduledEvents = new List<ScheduledEventConfig>();
        }
    }
}