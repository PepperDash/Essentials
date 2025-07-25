

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.CrestronIO
{
    /// <summary>
    /// Represents a IOPortConfig
    /// </summary>
    public class IOPortConfig
    {
        [JsonProperty("portDeviceKey")]
        /// <summary>
        /// Gets or sets the PortDeviceKey
        /// </summary>
        public string PortDeviceKey { get; set; }
        [JsonProperty("portNumber")]
        /// <summary>
        /// Gets or sets the PortNumber
        /// </summary>
        public uint PortNumber { get; set; }
        [JsonProperty("disablePullUpResistor")]
        /// <summary>
        /// Gets or sets the DisablePullUpResistor
        /// </summary>
        public bool DisablePullUpResistor { get; set; }
        [JsonProperty("minimumChange")]
        /// <summary>
        /// Gets or sets the MinimumChange
        /// </summary>
        public int MinimumChange { get; set; }
    }
}