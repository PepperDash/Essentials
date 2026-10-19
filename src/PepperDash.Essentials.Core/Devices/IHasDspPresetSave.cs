namespace PepperDash.Essentials.Core;

/// <summary>
/// Defines the contract for a DSP that can store its current state to a named preset, in addition to recalling one.
/// </summary>
public interface IHasDspPresetSave : IDspPresets
{
    /// <summary>
    /// Saves the current state to the preset with the given key
    /// </summary>
    /// <param name="key">key of preset to save</param>
    void SavePreset(string key);
}
