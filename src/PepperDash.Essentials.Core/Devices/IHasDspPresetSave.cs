namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines the contract for IHasDspPresetSave
    /// </summary>
    public interface IHasDspPresetSave : IDspPresets
    {
        /// <summary>
        /// Saves the DSP preset by key
        /// </summary>
        /// <param name="presetKey"></param>
        void SavePresetByKey(string presetKey); // mirrors RecallPreset(string key)
    }
}