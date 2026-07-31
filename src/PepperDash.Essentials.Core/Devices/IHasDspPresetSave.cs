namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines the contract for IHasDspPresetSave
    /// </summary>
    public interface IHasDspPresetSave : IDspPresets
    {
        /// <summary>
        /// Saves the preset by key
        /// </summary>
        /// <param name="key">key of preset to save</param>
        void SavePreset(string key);
    }
}