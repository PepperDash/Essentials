using PepperDash.Core;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core;

public interface IDspPresets
{
    Dictionary<string, IKeyName> Presets { get; }

    void RecallPreset(string key);
}