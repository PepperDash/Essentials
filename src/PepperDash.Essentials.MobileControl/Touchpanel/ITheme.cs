using PepperDash.Core;

namespace PepperDash.Essentials.Touchpanel
{
    /// <summary>
    /// Defines the contract for ITheme
    /// </summary>
    public interface ITheme : IKeyed
    {
        /// <summary>
        /// Current theme
        /// </summary>
        string Theme { get; }

        /// <summary>
        /// Set the theme with the given value
        /// </summary>
        /// <param name="theme">The theme to set</param>
        void UpdateTheme(string theme);
    }
}
