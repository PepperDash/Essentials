using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Interfaces
{
    public interface IHasDspPresets
    {
        List<IDspPreset> Presets { get; }
        void RecallPreset(IDspPreset preset);
    }
}