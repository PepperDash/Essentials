using System.Collections.Generic;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.Devices
{
    public interface IDspPresets
    {
        Dictionary<string, IKeyName> Presets { get; }

        void RecallPreset(string key);
    }
}