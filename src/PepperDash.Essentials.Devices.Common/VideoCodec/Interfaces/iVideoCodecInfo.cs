extern alias Full;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Full.Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    /// <summary>
    /// Implements a common set of data about a codec
    /// </summary>
    public interface iVideoCodecInfo
    {
        VideoCodecInfo CodecInfo { get; }
    }

    /// <summary>
    /// Stores general information about a codec
    /// </summary>
    public abstract class VideoCodecInfo
    {
        [JsonProperty("multiSiteOptionIsEnabled", NullValueHandling = NullValueHandling.Ignore)]
        public abstract bool MultiSiteOptionIsEnabled { get; }
        [JsonProperty("ipAddress", NullValueHandling = NullValueHandling.Ignore)]
        public abstract string IpAddress { get; }
        [JsonProperty("sipPhoneNumber", NullValueHandling = NullValueHandling.Ignore)]
        public abstract string SipPhoneNumber { get; }
        [JsonProperty("e164Alias", NullValueHandling = NullValueHandling.Ignore)]
        public abstract string E164Alias { get; }
        [JsonProperty("h323Id", NullValueHandling = NullValueHandling.Ignore)]
        public abstract string H323Id { get; }
        [JsonProperty("sipUri", NullValueHandling = NullValueHandling.Ignore)]
        public abstract string SipUri { get; }
        [JsonProperty("autoAnswerEnabled", NullValueHandling = NullValueHandling.Ignore)]
        public abstract bool AutoAnswerEnabled { get; }
    }
}