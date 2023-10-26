extern alias Full;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;

using Full.Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    public class CiscoSparkCodecPropertiesConfig
    {
        [JsonProperty("communicationMonitorProperties")]
        public CommunicationMonitorConfig CommunicationMonitorProperties { get; set; }

        [JsonProperty("favorites")]
        public List<CodecActiveCallItem> Favorites { get; set; }

        /// <summary>
        /// Valid values: "Local" or "Corporate"
        /// </summary>
        [JsonProperty("phonebookMode")]
        public string PhonebookMode { get; set; }

        [JsonProperty("showSelfViewByDefault")]
        public bool ShowSelfViewByDefault { get; set; }

        [JsonProperty("sharing")]
        public SharingProperties Sharing { get; set; }

        /// <summary>
        /// Enables external source switching capability
        /// </summary>
		[JsonProperty("externalSourceListEnabled")]
		public bool ExternalSourceListEnabled { get; set; }

        /// <summary>
        /// The name of the routing input port on the codec to which the external switch is connected
        /// </summary>
        [JsonProperty("externalSourceInputPort")]
        public string ExternalSourceInputPort { get; set; }

        /// <summary>
        /// Optionsal property to set the limit of any phonebook queries for directory or searching
        /// </summary>
        [JsonProperty("phonebookResultsLimit")]
        public uint PhonebookResultsLimit { get; set; }

        [JsonProperty("UiBranding")]
        public BrandingLogoProperties UiBranding { get; set; }

        [JsonProperty("cameraInfo")]
        public List<CameraInfo> CameraInfo { get; set; }


        public CiscoSparkCodecPropertiesConfig()
        {
            CameraInfo = new List<CameraInfo>();
        }
    }
}