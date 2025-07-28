using System;

namespace PepperDash.Essentials.Core.Shades
{
    public interface IShadesOpenClosePreset : IShadesOpenCloseStop
    {
        void RecallPreset(uint presetNumber);
        void SavePreset(uint presetNumber);
        string StopOrPresetButtonLabel { get; }

        event EventHandler PresetSaved;
    }
}