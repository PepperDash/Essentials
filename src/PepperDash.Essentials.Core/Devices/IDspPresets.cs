using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Devices
{
    /// <summary>
    /// Defines the contract for IDspPresets
    /// </summary>
    public interface IDspPresets
    {
        Dictionary<string, IKeyName> Presets { get; }

        void RecallPreset(string key);
    }
}