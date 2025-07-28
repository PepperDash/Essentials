using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Devices
{
    public abstract class AudioControlListItemBase
    {
        /// <summary>
        /// Key of the parent device in the DeviceManager
        /// </summary>
        [JsonProperty("parentDeviceKey")]
        /// <summary>
        /// Gets or sets the ParentDeviceKey
        /// </summary>
        public string ParentDeviceKey { get; set; }

        /// <summary>
        /// Optional key of the item in the parent device
        /// </summary>
        [JsonProperty("itemKey")]
        public string ItemKey { get; set; }

        /// <summary>
        /// A name that will override the items's name on the UI
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Indicates if the item should be included in the user accessible list
        /// </summary>
        [JsonProperty("includeInUserList")]
        public bool IncludeInUserList { get; set; }

        /// <summary>
        /// Used to specify the order of the items in the source list when displayed
        /// </summary>
        [JsonProperty("order")]
        public int Order { get; set; }
    }
}
