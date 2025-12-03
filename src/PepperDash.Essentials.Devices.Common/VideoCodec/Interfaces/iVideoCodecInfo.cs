using Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    /// <summary>
    /// Implements a common set of data about a codec
    /// </summary>
    public interface iVideoCodecInfo
    {
        /// <summary>
        /// Gets the codec information
        /// </summary>
        VideoCodecInfo CodecInfo { get; }
    }

    /// <summary>
    /// Stores general information about a codec
    /// </summary>
    public abstract class VideoCodecInfo
    {
        /// <summary>
        /// Gets a value indicating whether the multi-site option is enabled
        /// </summary>
        [JsonProperty("multiSiteOptionIsEnabled", NullValueHandling = NullValueHandling.Ignore)]
        public abstract bool MultiSiteOptionIsEnabled { get; }
        /// <summary>
        /// Gets the IP address of the codec
        /// </summary>
        [JsonProperty("ipAddress", NullValueHandling = NullValueHandling.Ignore)]
        public abstract string IpAddress { get; }
        /// <summary>
        /// Gets the SIP phone number for the codec
        /// </summary>
        [JsonProperty("sipPhoneNumber", NullValueHandling = NullValueHandling.Ignore)]
        public abstract string SipPhoneNumber { get; }
        /// <summary>
        /// Gets the E164 alias for the codec
        /// </summary>
        [JsonProperty("e164Alias", NullValueHandling = NullValueHandling.Ignore)]
        public abstract string E164Alias { get; }
        /// <summary>
        /// Gets the H323 ID for the codec
        /// </summary>
        [JsonProperty("h323Id", NullValueHandling = NullValueHandling.Ignore)]
        public abstract string H323Id { get; }
        /// <summary>
        /// Gets the SIP URI for the codec
        /// </summary>
        [JsonProperty("sipUri", NullValueHandling = NullValueHandling.Ignore)]
        public abstract string SipUri { get; }
        /// <summary>
        /// Gets a value indicating whether auto-answer is enabled
        /// </summary>
        [JsonProperty("autoAnswerEnabled", NullValueHandling = NullValueHandling.Ignore)]
        public abstract bool AutoAnswerEnabled { get; }
    }
}