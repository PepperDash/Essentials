

using Newtonsoft.Json;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core
{
    public class DestinationListItem
    {
        [JsonProperty("sinkKey")]
        public string SinkKey { get; set; }

        private EssentialsDevice _sinkDevice;

        [JsonIgnore]
        public EssentialsDevice SinkDevice
        {
            get { return _sinkDevice ?? (_sinkDevice = DeviceManager.GetDeviceForKey(SinkKey) as EssentialsDevice); }
        }

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

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("includeInDestinationList")]
        public bool IncludeInDestinationList { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }

        [JsonProperty("surfaceLocation")]
        public int SurfaceLocation { get; set; }

        [JsonProperty("verticalLocation")]
        public int VerticalLocation { get; set; }
        
        [JsonProperty("horizontalLocation")]
        public int HorizontalLocation { get; set; }

        [JsonProperty("sinkType")]
        public eRoutingSignalType SinkType { get; set; }
    }
}