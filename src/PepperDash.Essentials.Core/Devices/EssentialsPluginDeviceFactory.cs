namespace PepperDash.Essentials.Core
{
  /// <summary>
  /// Devices the basic needs for a Device Factory
  /// </summary>
  public abstract class EssentialsPluginDeviceFactory<T> : EssentialsDeviceFactory<T>, IPluginDeviceFactory where T : EssentialsDevice
  {
    /// <summary>
    /// Specifies the minimum version of Essentials required for a plugin to run.  Must use the format Major.Minor.Build (ex. "1.4.33")
    /// </summary>
    public string MinimumEssentialsFrameworkVersion { get; protected set; }
  }

}