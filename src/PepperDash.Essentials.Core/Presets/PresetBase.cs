

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Presets
{
    /// <summary>
    /// Represents a PresetBase
    /// </summary>
    public class PresetBase
    {
        [JsonProperty("id")]
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Used to store the name of the preset
        /// </summary>
        [JsonProperty("description")]
        /// <summary>
        /// Gets or sets the Description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Indicates if the preset is defined(stored) in the codec
        /// </summary>
        [JsonProperty("defined")]
        /// <summary>
        /// Gets or sets the Defined
        /// </summary>
        public bool Defined { get; set; }
        /// <summary>
        /// Indicates if the preset has the capability to be defined
        /// </summary>
        [JsonProperty("isDefinable")]
        /// <summary>
        /// Gets or sets the IsDefinable
        /// </summary>
        public bool IsDefinable { get; set; }

        public PresetBase(int id, string description, bool def, bool isDef)
        {
            ID = id;
            Description = description;
            Defined = def;
            IsDefinable = isDef;
        }
    }
}