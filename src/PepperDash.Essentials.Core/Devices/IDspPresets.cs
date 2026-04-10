using PepperDash.Core;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines the contract for IDspPresets
    /// </summary>
    public interface IDspPresets
    {
        /// <summary>
        /// Gets the Presets
        /// </summary>
        Dictionary<string, IKeyName> Presets { get; }

        /// <summary>
        /// Recalls the preset by key
        /// </summary>
        /// <param name="key">key of preset to recall</param>
        void RecallPreset(string key);
    }
}