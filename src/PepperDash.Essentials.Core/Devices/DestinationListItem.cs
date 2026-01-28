

using Newtonsoft.Json;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Represents a destination item in a routing system that can receive audio/video signals.
    /// Contains information about the destination device, its properties, and location settings.
    /// </summary>
    public class DestinationListItem
    {
        /// <summary>
        /// Gets or sets the key identifier for the sink device that this destination represents.
        /// </summary>
        [JsonProperty("sinkKey")]
        public string SinkKey { get; set; }

        private EssentialsDevice _sinkDevice;

        /// <summary>
        /// Gets the actual device instance for this destination. 
        /// Lazily loads the device from the DeviceManager using the SinkKey.
        /// </summary>
        [JsonIgnore]
        public EssentialsDevice SinkDevice
        {
            get { return _sinkDevice ?? (_sinkDevice = DeviceManager.GetDeviceForKey(SinkKey) as EssentialsDevice); }
        }

        /// <summary>
        /// Gets the preferred display name for this destination.
        /// Returns the custom Name if set, otherwise returns the SinkDevice name, or "---" if no device is found.
        /// </summary>
        [JsonProperty("preferredName")]
        public string PreferredName
        {
            get
            {
                if (!string.IsNullOrEmpty(Name))
                {
                    return Name;
                }

                return SinkDevice == null ? "---" : SinkDevice.Name;
            }
        }

        /// <summary>
        /// Gets or sets the custom name for this destination. 
        /// If set, this name will be used as the PreferredName instead of the device name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this destination should be included in destination lists.
        /// </summary>
        [JsonProperty("includeInDestinationList")]
        public bool IncludeInDestinationList { get; set; }

        /// <summary>
        /// Gets or sets the display order for this destination in lists.
        /// Lower values appear first in sorted lists.
        /// </summary>
        [JsonProperty("order")]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the surface location identifier for this destination.
        /// Used to specify which surface or screen this destination is located on.
        /// </summary>
        [JsonProperty("surfaceLocation")]
        public int SurfaceLocation { get; set; }

        /// <summary>
        /// Gets or sets the vertical location position for this destination.
        /// Used for spatial positioning in multi-display configurations.
        /// </summary>
        [JsonProperty("verticalLocation")]
        public int VerticalLocation { get; set; }

        /// <summary>
        /// Gets or sets the horizontal location position for this destination.
        /// Used for spatial positioning in multi-display configurations.
        /// </summary>
        [JsonProperty("horizontalLocation")]
        public int HorizontalLocation { get; set; }

        /// <summary>
        /// Gets or sets the signal type that this destination can receive (Audio, Video, AudioVideo, etc.).
        /// </summary>
        [JsonProperty("sinkType")]
        public eRoutingSignalType SinkType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this destination is used for codec content sharing.
        /// </summary>
        [JsonProperty("isCodecContentDestination")]
        public bool isCodecContentDestination { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this destination is used for program audio output.
        /// </summary>
        [JsonProperty("isProgramAudioDestination")]
        public bool isProgramAudioDestination { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this destination supports USB connections.
        /// Indicates if the destination can handle USB functionality, such as USB signal routing or device connections.
        /// This property is used to determine compatibility with USB-based devices or systems.
        /// </summary>
        [JsonProperty("supportsUsb")]
        public bool SupportsUsb { get; set; }

        /// <summary>
        /// The key of the destination port associated with this destination item
        /// This is used to identify the specific port on the destination device that this item refers to for advanced routing
        /// </summary>
        [JsonProperty("destinationPortKey")]
        public string DestinationPortKey { get; set; }
    }
}