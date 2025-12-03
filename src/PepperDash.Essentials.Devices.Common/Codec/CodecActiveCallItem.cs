

using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PepperDash.Essentials.Devices.Common.Codec

{
    /// <summary>
    /// Represents a CodecActiveCallItem
    /// </summary>
    public class CodecActiveCallItem
    {
        /// <summary>
        /// Gets or sets the Name
        /// </summary>
		[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Number
        /// </summary>
        [JsonProperty("number", NullValueHandling = NullValueHandling.Ignore)]
        public string Number { get; set; }

        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public eCodecCallType Type { get; set; }

        /// <summary>
        /// Gets or sets the Status
        /// </summary>
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public eCodecCallStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the Direction
        /// </summary>
        [JsonProperty("direction", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public eCodecCallDirection Direction { get; set; }

        /// <summary>
        /// Gets or sets the Id
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the IsOnHold
        /// </summary>
        [JsonProperty("isOnHold", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsOnHold { get; set; }

        /// <summary>
        /// Gets or sets the Duration
        /// </summary>
        [JsonProperty("duration", NullValueHandling = NullValueHandling.Ignore)]
        public TimeSpan Duration { get; set; }

        //public object CallMetaData { get; set; }

        /// <summary>
        /// Returns true when this call is any status other than 
        /// Unknown, Disconnected, Disconnecting
        /// </summary>
        [JsonProperty("isActiveCall", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsActiveCall
        {
            get
            {
                return !(Status == eCodecCallStatus.Disconnected
                    || Status == eCodecCallStatus.Disconnecting
                    || Status == eCodecCallStatus.Idle
                    || Status == eCodecCallStatus.Unknown);
            }
        }
    }

    /// <summary>
    /// Represents a CodecCallStatusItemChangeEventArgs
    /// </summary>
    public class CodecCallStatusItemChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the CallItem
        /// </summary>
        public CodecActiveCallItem CallItem { get; private set; }

        /// <summary>
        /// Initializes a new instance of the CodecCallStatusItemChangeEventArgs class
        /// </summary>
        /// <param name="item">The call item that changed</param>
        public CodecCallStatusItemChangeEventArgs(CodecActiveCallItem item)
        {
            CallItem = item;
        }
    }
}