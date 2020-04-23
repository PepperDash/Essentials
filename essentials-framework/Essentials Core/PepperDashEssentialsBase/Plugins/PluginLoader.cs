using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Reflection;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Deals with loading plugins at runtime
    /// </summary>
    public static class PluginLoader
    {
        /// <summary>
        /// The complete list of loaded assemblies. Includes Essentials Framework assemblies and plugins
        /// </summary>
        public static List<LoadedAssembly> LoadedAssemblies { get; private set; }

        /// <summary>
        /// The list of assemblies loaded from the plugins folder
        /// </summary>
        static List<LoadedAssembly> LoadedPluginFolderAssemblies;

        /// <summary>
        /// The directory to look in for .cplz plugin packages
        /// </summary>
        static string _pluginDirectory = Global.FilePathPrefix + "plugins";

        /// <summary>
        /// The directory where plugins will be moved to and loaded from
        /// </summary>
        static string _loadedPluginsDirectoryPath = _pluginDirectory + Global.DirectorySeparator + "loadedAssemblies";

        // The temp directory where .cplz archives will be unzipped to
        static string _tempDirectory = _pluginDirectory + Global.DirectorySeparator + "temp";


        static PluginLoader()
        {
            LoadedAssemblies = new List<LoadedAssembly>();
            LoadedPluginFolderAssemblies = new List<LoadedAssembly>();
        }

        /// <summary>
        /// Retrieves all the loaded assemblies from the program directory
        /// </summary>
        public static void AddProgramAssemblies()
        {
            Debug.Console(2, "Getting Assemblies loaded with Essentials");
            // Get the loaded assembly filenames
            var appDi = new DirectoryInfo(Global.ApplicationDirectoryPathPrefix);
            var assemblyFiles = appDi.GetFiles("*.dll");

            Debug.Console(2, "Found {0} Assemblies", assemblyFiles.Length);

            foreach (var fi in assemblyFiles)
            {
                string version = string.Empty;
                Assembly assembly = null;

                switch (fi.Name)
                {
                    case ("PepperDashEssentials.dll"):
                        {
                            version = Global.AssemblyVersion;
                            break;
                        }
                    case ("PepperDash_Essentials_Core.dll"):
                        {
                            version = Global.AssemblyVersion;
                            break;
                        }
                    case ("PepperDash_Essentials_DM.dll"):
                        {
                            version = Global.AssemblyVersion;
                            break;
                        }
                    case ("Essentials Devices Common.dll"):
                        {
                            version = Global.AssemblyVersion;
                            break;
                        }
                    case ("PepperDash_Core.dll"):
                        {
                            version = PepperDash.Core.Debug.PepperDashCoreVersion;
                            break;
                        }
                }

                LoadedAssemblies.Add(new LoadedAssembly(fi.Name, version, assembly));
            }

            if (Debug.Level > 1)
            {
                Debug.Console(2, "Loaded Assemblies:");

                foreach (var assembly in LoadedAssemblies)
                {
                    Debug.Console(2, "Assembly: {0}", assembly.Name);
                }
            }
        }


        public static void SetEssentialsAssembly(string name, Assembly assembly)
        {
            var loadedAssembly = LoadedAssemblies.FirstOrDefault(la => la.Name.Equals(name));

            if (loadedAssembly != null)
            {
                loadedAssembly.SetAssembly(assembly);
            }
        }

        /// <summary>
        /// Loads an assembly via Reflection and adds it to the list of loaded assemblies
        /// </summary>
        /// <param name="fileName"></param>
        static LoadedAssembly LoadAssembly(string filePath)
        {
            Debug.Console(2, "Attempting to load {0}", filePath);
            var assembly = Assembly.LoadFrom(filePath);
            if (assembly != null)
            {
                var assyVersion = GetAssemblyVersion(assembly);

                var loadedAssembly = new LoadedAssembly(assembly.GetName().Name, assyVersion, assembly);
                LoadedAssemblies.Add(loadedAssembly);
                Debug.Console(0, Debug.ErrorLogLevel.Notice, "Loaded assembly '{0}', version {1}", loadedAssembly.Name, loadedAssembly.Version);
                return loadedAssembly;
            }
            else
            {
                Debug.Console(0, Debug.ErrorLogLevel.Notice, "Unable to load assembly: '{0}'", filePath);
            }

            return null;

        }

        /// <summary>
        /// Attempts to get the assembly informational version and if not possible gets the version
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        static string GetAssemblyVersion(Assembly assembly)
        {
            var ver = assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
            if (ver != null && ver.Length > 0)
            {
                // Get the AssemblyInformationalVersion              
                AssemblyInformationalVersionAttribute verAttribute = ver[0] as AssemblyInformationalVersionAttribute;
                return verAttribute.InformationalVersion;
            }
            else
            {
                // Get the AssemblyVersion
                var version = assembly.GetName().Version;
                var verStr = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
                return verStr;
            }
        }

        /// <summary>
        /// Checks if the filename matches an already loaded assembly file's name
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>True if file already matches loaded assembly file.</returns>
        public static bool CheckIfAssemblyLoaded(string name)
        {
            Debug.Console(2, "Checking if assembly: {0} is loaded...", name);
            var loadedAssembly = LoadedAssemblies.FirstOrDefault(s => s.Name.Equals(name));

            if (loadedAssembly != null)
            {
                Debug.Console(2, "Assembly already loaded.");
                return true;
            }
            else
            {
                Debug.Console(2, "Assembly not loaded.");
                return false;
            }
        }

        /// <summary>
        /// Used by console command to report the currently loaded assemblies and versions
        /// </summary>
        /// <param name="command"></param>
        public static void ReportAssemblyVersions(string command)
        {
            Debug.Console(0, "Loaded Assemblies:");
            foreach (var assembly in LoadedAssemblies)
            {
                Debug.Console(0, "{0} Version: {1}", assembly.Name, assembly.Version);
            }
        }

        /// <summary>
        /// Moves any .dll assemblies not already loaded from the plugins folder to loadedPlugins folder
        /// </summary>
        static void MoveDllAssemblies()
        {
            Debug.Console(0, "Looking for .dll assemblies from plugins folder...");

            var pluginDi = new DirectoryInfo(_pluginDirectory);
            var pluginFiles = pluginDi.GetFiles("*.dll");

            if (pluginFiles.Length > 0)
            {
                if (!Directory.Exists(_loadedPluginsDirectoryPath))
                {
                    Directory.CreateDirectory(_loadedPluginsDirectoryPath);
                }
            }

            foreach (var pluginFile in pluginFiles)
            {
                try
                {
                    Debug.Console(0, "Found .dll: {0}", pluginFile.Name);

                    if (!CheckIfAssemblyLoaded(pluginFile.Name))
                    {
                        string filePath = string.Empty;

                        filePath = _loadedPluginsDirectoryPath + Global.DirectorySeparator + pluginFile.Name;

                        // Check if there is a previous file in the loadedPlugins directory and delete
                        if (File.Exists(filePath))
                        {
                            Debug.Console(0, "Found existing file in loadedPlugins: {0} Deleting and moving new file to replace it", filePath);
                            File.Delete(filePath);
                        }

                        // Move the file
                        File.Move(pluginFile.FullName, filePath);
                        Debug.Console(2, "Moved {0} to {1}", pluginFile.FullName, filePath);
                    }
                    else
                    {
                        Debug.Console(0, Debug.ErrorLogLevel.Notice, "Skipping assembly: {0}.  There is already an assembly with that name loaded.", pluginFile.FullName);
                    }
                }
                catch (Exception e)
                {
                    Debug.Console(2, "Error with plugin file {0} . Exception: {1}", pluginFile.FullName, e);
                    continue; //catching any load issues and continuing. There will be exceptions loading Crestron .dlls from the cplz Probably should do something different here
                }
            }

            Debug.Console(0, "Done with .dll assemblies");
        }

        /// <summary>
        /// Unzips each .cplz archive into the temp directory and moves any unloaded files into loadedPlugins
        /// </summary>
        static void UnzipAndMoveCplzArchives()
        {
            Debug.Console(0, "Looking for .cplz archives from plugins folder...");
            var di = new DirectoryInfo(_pluginDirectory);
            var zFiles = di.GetFiles("*.cplz");

            if (zFiles.Length > 0)
            {
                if (!Directory.Exists(_loadedPluginsDirectoryPath))
                {
                    Directory.CreateDirectory(_loadedPluginsDirectoryPath);
                }
            }

            foreach (var zfi in zFiles)
            {
                Directory.CreateDirectory(_tempDirectory);
                var tempDi = new DirectoryInfo(_tempDirectory);

                Debug.Console(0, "Found cplz: {0}. Unzipping into temp plugins directory", zfi.Name);
                var result = CrestronZIP.Unzip(zfi.FullName, tempDi.FullName);
                Debug.Console(0, "UnZip Result: {0}", result.ToString());

                var tempFiles = tempDi.GetFiles("*.dll");
                foreach (var tempFile in tempFiles)
                {
                    try
                    {
                        if (!CheckIfAssemblyLoaded(tempFile.Name))
                        {
                            string filePath = string.Empty;

                            filePath = _loadedPluginsDirectoryPath + Global.DirectorySeparator + tempFile.Name;

                            // Check if there is a previous file in the loadedPlugins directory and delete
                            if (File.Exists(filePath))
                            {
                                Debug.Console(0, "Found existing file in loadedPlugins: {0} Deleting and moving new file to replace it", filePath);
                                File.Delete(filePath);
                            }

                            // Move the file
                            File.Move(tempFile.FullName, filePath);
                            Debug.Console(2, "Moved {0} to {1}", tempFile.FullName, filePath);
                        }
                        else
                        {
                            Debug.Console(0, Debug.ErrorLogLevel.Notice, "Skipping assembly: {0}.  There is already an assembly with that name loaded.", tempFile.FullName);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Console(2, "Assembly {0} is not a custom assembly. Exception: {1}", tempFile.FullName, e);
                        continue; //catching any load issues and continuing. There will be exceptions loading Crestron .dlls from the cplz Probably should do something different here
                    }
                }

                // Delete the .cplz and the temp directory
                Directory.Delete(_tempDirectory, true);
                zfi.Delete();
            }

            Debug.Console(0, "Done with .cplz archives");
        }

        /// <summary>
        /// Attempts to load the assemblies from the loadedPlugins folder
        /// </summary>
        static void LoadPluginAssemblies()
        {
            Debug.Console(0, "Loading assemblies from loadedPlugins folder...");
            var pluginDi = new DirectoryInfo(_loadedPluginsDirectoryPath);
            var pluginFiles = pluginDi.GetFiles("*.dll");

            Debug.Console(2, "Found {0} plugin assemblies to load", pluginFiles.Length);

            foreach (var pluginFile in pluginFiles)
            {
                var loadedAssembly = LoadAssembly(pluginFile.FullName);

                LoadedPluginFolderAssemblies.Add(loadedAssembly);
            }

            Debug.Console(0, "All Plugins Loaded.");
        }

        /// <summary>
        /// Iterate the loaded assemblies and try to call the LoadPlugin method
        /// </summary>
        static void LoadCustomPluginTypes()
        {
            Debug.Console(0, "Loading Custom Plugin Types...");
            foreach (var loadedAssembly in LoadedPluginFolderAssemblies)
            {
                // iteratate this assembly's classes, looking for "LoadPlugin()" methods
                try
                {
                    var assy = loadedAssembly.Assembly;
                    var types = assy.GetTypes();
                    foreach (var type in types)
                    {
                        try
                        {
                            if (typeof(IPluginDeviceFactory).IsAssignableFrom(type))
                            {
                                var plugin = (IPluginDeviceFactory)Crestron.SimplSharp.Reflection.Activator.CreateInstance(type);
                                LoadCustomPlugin(plugin, loadedAssembly);
                            }
                            else
                            {
                                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
                                var loadPlugin = methods.FirstOrDefault(m => m.Name.Equals("LoadPlugin"));
                                if (loadPlugin != null)
                                {
                                    LoadCustomLegacyPlugin(type, loadPlugin, loadedAssembly);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.Console(2, "Load Plugin not found. {0}.{2} is not a plugin factory. Exception: {1}",
                                loadedAssembly.Name, e, type.Name);
                            continue;
                        }

                    }
                }
                catch (Exception e)
                {
                    Debug.Console(2, "Error Loading Assembly: {0} Exception: (1) ", loadedAssembly.Name, e);
                    continue;
                }
            }
            // plugin dll will be loaded.  Any classes in plugin should have a static constructor
            // that registers that class with the Core.DeviceFactory
            Debug.Console(0, "Done Loading Custom Plugin Types.");
        }

        /// <summary>
        /// Loads a
        /// </summary>
        /// <param name="plugin"></param>
        static void LoadCustomPlugin(IPluginDeviceFactory plugin, LoadedAssembly loadedAssembly)
        {
            var passed = Global.IsRunningMinimumVersionOrHigher(plugin.MinimumEssentialsFrameworkVersion);

            if (!passed)
            {
                Debug.Console(0, Debug.ErrorLogLevel.Error, "Plugin indicates minimum Essentials version {0}.  Dependency check failed.  Skipping Plugin", plugin.MinimumEssentialsFrameworkVersion);
                return;
            }
            else
            {
                Debug.Console(0, Debug.ErrorLogLevel.Notice, "Passed plugin passed dependency check (required version {0})", plugin.MinimumEssentialsFrameworkVersion);
            }

            Debug.Console(0, Debug.ErrorLogLevel.Notice, "Loading plugin: {0}", loadedAssembly.Name);
            plugin.LoadTypeFactories();
        }

        /// <summary>
        /// Loads a a custom plugin via the legacy method
        /// </summary>
        /// <param name="type"></param>
        /// <param name="loadPlugin"></param>
        static void LoadCustomLegacyPlugin(CType type, MethodInfo loadPlugin, LoadedAssembly loadedAssembly)
        {
            Debug.Console(2, "LoadPlugin method found in {0}", type.Name);

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

            var minimumVersion = fields.FirstOrDefault(p => p.Name.Equals("MinimumEssentialsFrameworkVersion"));
            if (minimumVersion != null)
            {
                Debug.Console(2, "MinimumEssentialsFrameworkVersion found");

                var minimumVersionString = minimumVersion.GetValue(null) as string;

                if (!string.IsNullOrEmpty(minimumVersionString))
                {
                    var passed = Global.IsRunningMinimumVersionOrHigher(minimumVersionString);

                    if (!passed)
                    {
                        Debug.Console(0, Debug.ErrorLogLevel.Error, "Plugin indicates minimum Essentials version {0}.  Dependency check failed.  Skipping Plugin", minimumVersionString);
                        return;
                    }
                    else
                    {
                        Debug.Console(0, Debug.ErrorLogLevel.Notice, "Passed plugin passed dependency check (required version {0})", minimumVersionString);
                    }
                }
                else
                {
                    Debug.Console(0, Debug.ErrorLogLevel.Warning, "MinimumEssentialsFrameworkVersion found but not set.  Loading plugin, but your mileage may vary.");
                }
            }
            else
            {
                Debug.Console(0, Debug.ErrorLogLevel.Warning, "MinimumEssentialsFrameworkVersion not found.  Loading plugin, but your mileage may vary.");
            }

            Debug.Console(0, Debug.ErrorLogLevel.Notice, "Loading legacy plugin: {0}", loadedAssembly.Name);
            loadPlugin.Invoke(null, null);

        }

        /// <summary>
        /// Loads plugins
        /// </summary>
        public static void LoadPlugins()
        {
            if (Directory.Exists(_pluginDirectory))
            {
                Debug.Console(0, Debug.ErrorLogLevel.Notice, "Plugins directory found, checking for plugins");

                // Deal with any .dll files
                MoveDllAssemblies();

                // Deal with any .cplz files
                UnzipAndMoveCplzArchives();

                if (Directory.Exists(_loadedPluginsDirectoryPath))
                {
                    // Load the assemblies from the loadedPlugins folder into the AppDomain
                    LoadPluginAssemblies();

                    // Load the types from any custom plugin assemblies
                    LoadCustomPluginTypes();
                }
            }
        }

    }

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
            Name = name;
            Version = version;
            Assembly = assembly;
        }

        public void SetAssembly(Assembly assembly)
        {
            Assembly = assembly;
        }
    }
}