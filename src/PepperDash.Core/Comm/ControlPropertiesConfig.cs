using System;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PepperDash.Core
{
    /// <summary>
    /// Config properties that indicate how to communicate with a device for control
    /// </summary>
    public class ControlPropertiesConfig
    {
        /// <summary>
        /// The method of control
        /// </summary>
        [JsonProperty("method")]
        [JsonConverter(typeof(StringEnumConverter))]
        public eControlMethod Method { get; set; }

        /// <summary>
        /// The key of the device that contains the control port
        /// </summary>
        [JsonProperty("controlPortDevKey", NullValueHandling = NullValueHandling.Ignore)]
        public string ControlPortDevKey { get; set; }

        /// <summary>
        /// The number of the control port on the device specified by ControlPortDevKey
        /// </summary>
        [JsonProperty("controlPortNumber", NullValueHandling = NullValueHandling.Ignore)] // In case "null" is present in config on this value
        public uint? ControlPortNumber { get; set; }

        /// <summary>
        /// The name of the control port on the device specified by ControlPortDevKey
        /// </summary>
        [JsonProperty("controlPortName", NullValueHandling = NullValueHandling.Ignore)] // In case "null" is present in config on this value
        public string ControlPortName { get; set; }

        /// <summary>
        /// Properties for ethernet based communications
        /// </summary>
        [JsonProperty("tcpSshProperties", NullValueHandling = NullValueHandling.Ignore)]
        public TcpSshPropertiesConfig TcpSshProperties { get; set; }

        /// <summary>
        /// The filename and path for the IR file
        /// </summary>
        [JsonProperty("irFile", NullValueHandling = NullValueHandling.Ignore)]
        public string IrFile { get; set; }

        /// <summary>
        /// The IpId of a Crestron device
        /// </summary>
        [JsonProperty("ipId", NullValueHandling = NullValueHandling.Ignore)]
        public string IpId { get; set; }

        /// <summary>
        /// Readonly uint representation of the IpId
        /// </summary>
        [JsonIgnore]
        public uint IpIdInt { get { return Convert.ToUInt32(IpId, 16); } }

        /// <summary>
        /// Char indicating end of line
        /// </summary>
        [JsonProperty("endOfLineChar", NullValueHandling = NullValueHandling.Ignore)]
        public char EndOfLineChar { get; set; }

        /// <summary>
        /// Defaults to Environment.NewLine;
        /// </summary>
        [JsonProperty("endOfLineString", NullValueHandling = NullValueHandling.Ignore)]
        public string EndOfLineString { get; set; }

        /// <summary>
        /// Indicates 
        /// </summary>
        [JsonProperty("deviceReadyResponsePattern", NullValueHandling = NullValueHandling.Ignore)]
        public string DeviceReadyResponsePattern { get; set; }

        /// <summary>
        /// Used when communcating to programs running in VC-4
        /// </summary>
        [JsonProperty("roomId", NullValueHandling = NullValueHandling.Ignore)]
        public string RoomId { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ControlPropertiesConfig()
        {            
        }
    }
}