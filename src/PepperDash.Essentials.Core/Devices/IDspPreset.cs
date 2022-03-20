using System.Collections.Generic;

namespace PepperDash.Essentials.Core
{
    public interface IHasDspPresets
    {
        List<IDspPreset> Presets { get; }

        void RecallPreset(IDspPreset preset);

    }

    public interface IDspPreset
    {
        string Name { get; }
    }
}