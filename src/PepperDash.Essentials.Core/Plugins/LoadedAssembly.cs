using Crestron.SimplSharp.Reflection;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Represents an assembly loaded at runtime and it's associated metadata
    /// </summary>
    public class LoadedAssembly
    {
        public string Name { get; private set; }
        public string Version { get; private set; }
        public Assembly Assembly { get; private set; }

        public LoadedAssembly(string name, string version, Assembly assembly)
        {
            Name     = name;
            Version  = version;
            Assembly = assembly;
        }

        public void SetAssembly(Assembly assembly)
        {
            Assembly = assembly;
        }
    }
}