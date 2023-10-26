using System.Collections.Generic;

namespace PepperDash.Essentials.Core
{
    public interface IPluginDevelopmentDeviceFactory : IPluginDeviceFactory
    {
        List<string> DevelopmentEssentialsFrameworkVersions { get; }
    }
}