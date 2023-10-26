using System.Collections.Generic;

namespace PepperDash.Essentials.Core
{
    public abstract class EssentialsPluginDevelopmentDeviceFactory<T> : EssentialsDeviceFactory<T>, IPluginDevelopmentDeviceFactory where T : EssentialsDevice
    {
        /// <summary>
        /// Specifies the minimum version of Essentials required for a plugin to run.  Must use the format Major.Minor.Build (ex. "1.4.33")
        /// </summary>
        public string MinimumEssentialsFrameworkVersion { get; protected set; }

        public List<string>  DevelopmentEssentialsFrameworkVersions { get; protected set; }
    }
}