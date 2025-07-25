using System.Collections.Generic;

namespace PepperDash.Essentials.Room.Config
{
    /// <summary>
    /// Represents a EssentialsPresentationRoomPropertiesConfig
    /// </summary>
    public class EssentialsPresentationRoomPropertiesConfig : EssentialsRoomPropertiesConfig
    {
        /// <summary>
        /// Gets or sets the DefaultAudioBehavior
        /// </summary>
        public string DefaultAudioBehavior { get; set; }
        /// <summary>
        /// Gets or sets the DefaultAudioKey
        /// </summary>
        public string DefaultAudioKey { get; set; }
        public string DefaultVideoBehavior { get; set; }
        public List<string> DisplayKeys { get; set; }
        public string SourceListKey { get; set; }
        public bool HasDsp { get; set; }

        public EssentialsPresentationRoomPropertiesConfig()
        {
            DisplayKeys = new List<string>();
        }
    }
}