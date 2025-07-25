

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PepperDash.Essentials.Devices.Common.Codec

{
    /// <summary>
    /// Represents a CodecActiveCallItem
    /// </summary>
    public class CodecActiveCallItem
    {
		[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get; set; }

        [JsonProperty("number", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the Number
        /// </summary>
        public string Number { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
		[JsonConverter(typeof(StringEnumConverter))]
        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        public eCodecCallType Type { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
		[JsonConverter(typeof(StringEnumConverter))]
        /// <summary>
        /// Gets or sets the Status
        /// </summary>
        public eCodecCallStatus Status { get; set; }

        [JsonProperty("direction", NullValueHandling = NullValueHandling.Ignore)]
		[JsonConverter(typeof(StringEnumConverter))]
        /// <summary>
        /// Gets or sets the Direction
        /// </summary>
        public eCodecCallDirection Direction { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the Id
        /// </summary>
        public string Id { get; set; }

        [JsonProperty("isOnHold", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the IsOnHold
        /// </summary>
        public bool IsOnHold { get; set; }

        [JsonProperty("duration", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the Duration
        /// </summary>
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

        public CodecCallStatusItemChangeEventArgs(CodecActiveCallItem item)
        {
            CallItem = item;
        }
    }
}