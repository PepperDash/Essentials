using System.Collections.Generic;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Describes environmental controls available on a room such as lighting, shades, temperature, etc.
    /// </summary>
    public interface IEnvironmentalControls
    {
        List<EssentialsDevice> EnvironmentalControlDevices { get; }

        bool HasEnvironmentalControlDevices { get; }
    }
}