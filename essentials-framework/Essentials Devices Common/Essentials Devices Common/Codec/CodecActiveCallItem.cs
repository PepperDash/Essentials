using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PepperDash.Essentials.Devices.Common.Codec

{
    public class CodecActiveCallItem
    {
		[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("number", NullValueHandling = NullValueHandling.Ignore)]
        public string Number { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
		[JsonConverter(typeof(StringEnumConverter))]
        public eCodecCallType Type { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
		[JsonConverter(typeof(StringEnumConverter))]
        public eCodecCallStatus Status { get; set; }

        [JsonProperty("direction", NullValueHandling = NullValueHandling.Ignore)]
		[JsonConverter(typeof(StringEnumConverter))]
        public eCodecCallDirection Direction { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("isOnHold", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsOnHold { get; set; }

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
    /// 
    /// </summary>
    public class CodecCallStatusItemChangeEventArgs : EventArgs
    {
        public CodecActiveCallItem CallItem { get; private set; }

        public CodecCallStatusItemChangeEventArgs(CodecActiveCallItem item)
        {
            CallItem = item;
        }
    }
}