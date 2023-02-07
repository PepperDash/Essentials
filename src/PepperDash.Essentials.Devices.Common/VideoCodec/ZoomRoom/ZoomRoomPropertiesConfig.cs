extern alias Full;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Full.Newtonsoft.Json;
using Full.Newtonsoft.Json.Converters;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.ZoomRoom
{
    public class ZoomRoomPropertiesConfig
    {
        [JsonProperty("communicationMonitorProperties")]
        public CommunicationMonitorConfig CommunicationMonitorProperties { get; set; }

        [JsonProperty("disablePhonebookAutoDownload")]
        public bool DisablePhonebookAutoDownload { get; set; }

        [JsonProperty("supportsCameraAutoMode")]
        public bool SupportsCameraAutoMode { get; set; }

        [JsonProperty("supportsCameraOff")]
        public bool SupportsCameraOff { get; set; }

        //if true, the layouts will be set automatically when sharing starts/ends or a call is joined
        [JsonProperty("autoDefaultLayouts")]
        public bool AutoDefaultLayouts { get; set; }

        /* This layout will be selected when Sharing starts (either from Far end or locally)*/
        [JsonProperty("defaultSharingLayout")]
        [JsonConverter(typeof(StringEnumConverter))]
        public zConfiguration.eLayoutStyle DefaultSharingLayout { get; set; }

        //This layout will be selected when a call is connected and no content is being shared
        [JsonProperty("defaultCallLayout")]
        [JsonConverter(typeof(StringEnumConverter))]
        public zConfiguration.eLayoutStyle DefaultCallLayout { get; set; }

        [JsonProperty("minutesBeforeMeetingStart")]
        public int MinutesBeforeMeetingStart { get; set; }
    }
}