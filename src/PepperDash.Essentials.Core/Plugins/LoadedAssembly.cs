using System.Reflection;
using Newtonsoft.Json;


namespace PepperDash.Essentials;

/// <summary>
/// Represents an assembly that has been loaded, including its name, version, and the associated <see
/// cref="System.Reflection.Assembly"/> instance.
/// </summary>
/// <remarks>This class provides information about a loaded assembly, including its name and version as strings, 
/// and the associated <see cref="System.Reflection.Assembly"/> object. The assembly instance can be updated  using the
/// <see cref="SetAssembly(Assembly)"/> method.</remarks>
/// <param name="name"></param>
/// <param name="version"></param>
/// <param name="assembly"></param>
public class LoadedAssembly(string name, string version, Assembly assembly)
{
    /// <summary>
    /// Gets the name associated with the object.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; private set; } = name;

    /// <summary>
    /// Gets the version of the object as a string.
    /// </summary>
    [JsonProperty("version")]
    public string Version { get; private set; } = version;

    /// <summary>
    /// Gets the assembly associated with the current instance.
    /// </summary>
    [JsonIgnore]
    public Assembly Assembly { get; private set; } = assembly;

    /// <summary>
    /// Sets the assembly associated with the current instance.
    /// </summary>
    /// <param name="assembly">The <see cref="System.Reflection.Assembly"/> to associate with the current instance. Cannot be <see
    /// langword="null"/>.</param>
    public void SetAssembly(Assembly assembly)
    {
        Assembly = assembly;
    }
}
