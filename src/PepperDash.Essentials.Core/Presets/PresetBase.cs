

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
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        [JsonProperty("id")]
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the Description
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets Defined
        /// </summary>
        [JsonProperty("defined")]
        public bool Defined { get; set; }

        /// <summary>
        /// Gets or sets the IsDefinable
        /// </summary>
        [JsonProperty("isDefinable")]
        public bool IsDefinable { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">id of the preset</param>
        /// <param name="description">description of the preset</param>
        /// <param name="def">whether the preset is defined</param>
        /// <param name="isDef">whether the preset is definable</param>
        public PresetBase(int id, string description, bool def, bool isDef)
        {
            ID = id;
            Description = description;
            Defined = def;
            IsDefinable = isDef;
        }
    }
}