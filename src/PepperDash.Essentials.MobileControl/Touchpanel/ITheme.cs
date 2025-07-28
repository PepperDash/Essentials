using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Touchpanel
{
    /// <summary>
    /// Defines the contract for ITheme
    /// </summary>
    public interface ITheme : IKeyed
    {
        string Theme { get; }

        void UpdateTheme(string theme);
    }
}
