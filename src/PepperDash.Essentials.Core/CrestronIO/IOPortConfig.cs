

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
        /// <summary>
        /// Gets or sets the PortDeviceKey
        /// </summary>
        [JsonProperty("portDeviceKey")]
        public string PortDeviceKey { get; set; }

        /// <summary>
        /// Gets or sets the PortNumber
        /// </summary>
        [JsonProperty("portNumber")]
        public uint PortNumber { get; set; }

        /// <summary>
        /// Gets or sets the DisablePullUpResistor
        /// </summary>
        [JsonProperty("disablePullUpResistor")]
        public bool DisablePullUpResistor { get; set; }

        /// <summary>
        /// Gets or sets the MinimumChange
        /// </summary>
        [JsonProperty("minimumChange")]
        public int MinimumChange { get; set; }
    }
}