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

        /// <summary>
        /// Gets or sets the DefaultVideoBehavior
        /// </summary>
        public string DefaultVideoBehavior { get; set; }

        /// <summary>
        /// Gets or sets the DisplayKeys
        /// </summary>
        public List<string> DisplayKeys { get; set; }

        /// <summary>
        /// Gets or sets the SourceListKey
        /// </summary>
        public string SourceListKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the room has a DSP
        /// </summary>
        public bool HasDsp { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public EssentialsPresentationRoomPropertiesConfig()
        {
            DisplayKeys = new List<string>();
        }
    }
}