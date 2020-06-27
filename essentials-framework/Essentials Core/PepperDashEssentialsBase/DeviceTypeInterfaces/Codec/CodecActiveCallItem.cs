using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PepperDash.Essentials.Core.Devices.Codec

{
    public class CodecActiveCallItem
    {
		[JsonProperty("name")]
        public string Name { get; set; }

		[JsonProperty("number")]
        public string Number { get; set; }

		[JsonProperty("type")]
		[JsonConverter(typeof(StringEnumConverter))]
        public eCodecCallType Type { get; set; }

		[JsonProperty("status")]
		[JsonConverter(typeof(StringEnumConverter))]
        public eCodecCallStatus Status { get; set; }

		[JsonProperty("direction")]
		[JsonConverter(typeof(StringEnumConverter))]
        public eCodecCallDirection Direction { get; set; }

		[JsonProperty("id")]
        public string Id { get; set; }

        //public object CallMetaData { get; set; }

        /// <summary>
        /// Returns true when this call is any status other than 
        /// Unknown, Disconnected, Disconnecting
        /// </summary>
		[JsonProperty("isActiveCall")]
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