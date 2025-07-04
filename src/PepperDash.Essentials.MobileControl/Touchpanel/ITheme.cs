using PepperDash.Core;

namespace PepperDash.Essentials.Touchpanel;

public interface ITheme : IKeyed
{
    string Theme { get; }

    void UpdateTheme(string theme);
}
