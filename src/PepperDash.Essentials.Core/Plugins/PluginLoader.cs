using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using Serilog.Events;

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
        /// The assembly for the Essentials Framework
        /// </summary>
        public static LoadedAssembly EssentialsAssembly { get; private set; }

        /// <summary>
        /// The assembly for the PepperDash Core
        /// </summary>
        public static LoadedAssembly PepperDashCoreAssembly { get; private set; }

        /// <summary>
        /// The list of assemblies loaded from the Essentials plugins folder
        /// </summary>
        public static List<LoadedAssembly> EssentialsPluginAssemblies { get; private set; }

        /// <summary>
        /// The directory to look in for .cplz plugin packages
        /// </summary>
        static string _pluginDirectory => Global.FilePathPrefix + "plugins";

        /// <summary>
        /// The directory where plugins will be moved to and loaded from
        /// </summary>
        static string _loadedPluginsDirectoryPath => _pluginDirectory + Global.DirectorySeparator + "loadedAssemblies";

        // The temp directory where .cplz archives will be unzipped to
        static string _tempDirectory => _pluginDirectory + Global.DirectorySeparator + "temp";


        static PluginLoader()
        {
            LoadedAssemblies = new List<LoadedAssembly>();
            LoadedPluginFolderAssemblies = new List<LoadedAssembly>();
            EssentialsPluginAssemblies = new List<LoadedAssembly>();
        }

        /// <summary>
        /// AddProgramAssemblies method
        /// </summary>
        public static void AddProgramAssemblies()
        {
            Debug.LogMessage(LogEventLevel.Verbose, "Getting Assemblies loaded with Essentials");
            // Get the loaded assembly filenames
            var appDi = new DirectoryInfo(Global.ApplicationDirectoryPathPrefix);
            var assemblyFiles = appDi.GetFiles("*.dll");

            Debug.LogMessage(LogEventLevel.Verbose, "Found {0} Assemblies", assemblyFiles.Length);

            foreach (var fi in assemblyFiles.Where(fi => fi.Name.Contains("Essentials") || fi.Name.Contains("PepperDash")))
            {
                string version = string.Empty;
                Assembly assembly = null;

                switch (fi.Name)
                {
                    case ("PepperDashEssentials.dll"):
                        {
                            version = Global.AssemblyVersion;
                            EssentialsAssembly = new LoadedAssembly(fi.Name, version, assembly);
                            break;
                        }
                    case ("PepperDash_Essentials_Core.dll"):
                        {
                            version = Global.AssemblyVersion;
                            break;
                        }
                    case ("Essentials Devices Common.dll"):
                        {
                            version = Global.AssemblyVersion;
                            break;
                        }
                    case ("PepperDashCore.dll"):
                        {
                            Debug.LogMessage(LogEventLevel.Verbose, "Found PepperDash_Core.dll");
                            version = Debug.PepperDashCoreVersion;
                            Debug.LogMessage(LogEventLevel.Verbose, "PepperDash_Core Version: {0}", version);
                            PepperDashCoreAssembly = new LoadedAssembly(fi.Name, version, assembly);
                            break;
                        }
                }

                LoadedAssemblies.Add(new LoadedAssembly(fi.Name, version, assembly));
            }

            if (Debug.Level > 1)
            {
                Debug.LogMessage(LogEventLevel.Verbose, "Loaded Assemblies:");

                foreach (var assembly in LoadedAssemblies)
                {
                    Debug.LogMessage(LogEventLevel.Verbose, "Assembly: {0}", assembly.Name);
                }
            }
        }


        /// <summary>
        /// SetEssentialsAssembly method
        /// </summary>
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
        /// <param name="filePath"></param>
        static LoadedAssembly LoadAssembly(string filePath)
        {
            try
            {
                //Debug.LogMessage(LogEventLevel.Verbose, "Attempting to load {0}", filePath);
                var assembly = Assembly.LoadFrom(filePath);
                if (assembly != null)
                {
                    var assyVersion = GetAssemblyVersion(assembly);

                    var loadedAssembly = new LoadedAssembly(assembly.GetName().Name, assyVersion, assembly);
                    LoadedAssemblies.Add(loadedAssembly);
                    Debug.LogMessage(LogEventLevel.Information, "Loaded assembly '{0}', version {1}", loadedAssembly.Name, loadedAssembly.Version);
                    return loadedAssembly;
                }
                else
                {
                    Debug.LogMessage(LogEventLevel.Information, "Unable to load assembly: '{0}'", filePath);
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Error loading assembly from {path}", null, filePath);
                return null;
            }

        }

        /// <summary>
        /// Attempts to get the assembly informational version and if not possible gets the version
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static string GetAssemblyVersion(Assembly assembly)
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
        /// <param name="name">name of loaded assembly</param>
        /// <returns>True if file already matches loaded assembly file.</returns>
        public static bool CheckIfAssemblyLoaded(string name)
        {
            Debug.LogMessage(LogEventLevel.Verbose, "Checking if assembly: {0} is loaded...", name);
            var loadedAssembly = LoadedAssemblies.FirstOrDefault(s => s.Name.Equals(name));

            if (loadedAssembly != null)
            {
                Debug.LogMessage(LogEventLevel.Verbose, "Assembly already loaded.");
                return true;
            }
            else
            {
                Debug.LogMessage(LogEventLevel.Verbose, "Assembly not loaded.");
                return false;
            }
        }

        /// <summary>
        /// Associates the specified assembly with the given name in the loaded assemblies collection.
        /// </summary>
        /// <remarks>If an assembly with the specified name already exists in the loaded assemblies collection, 
        /// this method updates its associated assembly. If no matching name is found, the method does nothing.</remarks>
        /// <param name="name">The name used to identify the assembly. This value is case-sensitive and must not be null or empty.</param>
        /// <param name="assembly">The assembly to associate with the specified name. This value must not be null.</param>
        public static void AddLoadedAssembly(string name, Assembly assembly)
        {
            var loadedAssembly = LoadedAssemblies.FirstOrDefault(la => la.Name.Equals(name));

            loadedAssembly?.SetAssembly(assembly);
        }

        /// <summary>
        /// Used by console command to report the currently loaded assemblies and versions
        /// </summary>
        /// <param name="command"></param>
        public static void ReportAssemblyVersions(string command)
        {
            CrestronConsole.ConsoleCommandResponse("Essentials Version: {0}" + CrestronEnvironment.NewLine, Global.AssemblyVersion);
            CrestronConsole.ConsoleCommandResponse("PepperDash Core Version: {0}" + CrestronEnvironment.NewLine, PepperDashCoreAssembly.Version);
            CrestronConsole.ConsoleCommandResponse("Essentials Plugin Versions:" + CrestronEnvironment.NewLine);
            foreach (var assembly in EssentialsPluginAssemblies)
            {
                CrestronConsole.ConsoleCommandResponse("{0} Version: {1}" + CrestronEnvironment.NewLine, assembly.Name, assembly.Version);
            }

            //CrestronConsole.ConsoleCommandResponse("Loaded Assemblies:" + CrestronEnvironment.NewLine);
            //foreach (var assembly in LoadedAssemblies)
            //{
            //    CrestronConsole.ConsoleCommandResponse("{0} Version: {1}" + CrestronEnvironment.NewLine, assembly.Name, assembly.Version);
            //}
        }
        
        /// <summary>
        /// Moves any .dll assemblies not already loaded from the plugins folder to loadedPlugins folder
        /// </summary>
        static void MoveDllAssemblies()
        {
            Debug.LogMessage(LogEventLevel.Information, "Looking for .dll assemblies from plugins folder...");

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
                    Debug.LogMessage(LogEventLevel.Information, "Found .dll: {0}", pluginFile.Name);

                    if (!CheckIfAssemblyLoaded(pluginFile.Name))
                    {
                        string filePath = string.Empty;

                        filePath = _loadedPluginsDirectoryPath + Global.DirectorySeparator + pluginFile.Name;

                        // Check if there is a previous file in the loadedPlugins directory and delete
                        if (File.Exists(filePath))
                        {
                            Debug.LogMessage(LogEventLevel.Information, "Found existing file in loadedPlugins: {0} Deleting and moving new file to replace it", filePath);
                            File.Delete(filePath);
                        }

                        // Move the file
                        File.Move(pluginFile.FullName, filePath);
                        Debug.LogMessage(LogEventLevel.Verbose, "Moved {0} to {1}", pluginFile.FullName, filePath);
                    }
                    else
                    {
                        Debug.LogMessage(LogEventLevel.Information, "Skipping assembly: {0}.  There is already an assembly with that name loaded.", pluginFile.FullName);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogMessage(LogEventLevel.Verbose, "Error with plugin file {0} . Exception: {1}", pluginFile.FullName, e);
                    continue; //catching any load issues and continuing. There will be exceptions loading Crestron .dlls from the cplz Probably should do something different here
                }
            }

            Debug.LogMessage(LogEventLevel.Information, "Done with .dll assemblies");
        }

        /// <summary>
        /// Unzips each .cplz archive into the temp directory and moves any unloaded files into loadedPlugins
        /// </summary>
        static void UnzipAndMoveCplzArchives()
        {
            Debug.LogMessage(LogEventLevel.Information, "Looking for .cplz archives from user folder...");
            //var di = new DirectoryInfo(_pluginDirectory);
            //var zFiles = di.GetFiles("*.cplz");

            //// Find cplz files at the root of the user folder. Makes development/testing easier for VC-4, and helps with mistakes by end users

            //var userDi = new DirectoryInfo(Global.FilePathPrefix);
            //var userZFiles = userDi.GetFiles("*.cplz");

            Debug.LogInformation("Checking {folder} for .cplz files", Global.FilePathPrefix);
            var cplzFiles = Directory.GetFiles(Global.FilePathPrefix, "*.cplz", SearchOption.AllDirectories)
                .Select(f => new FileInfo(f))
                .ToArray();

            if (cplzFiles.Length > 0)
            {
                if (!Directory.Exists(_loadedPluginsDirectoryPath))
                {
                    Directory.CreateDirectory(_loadedPluginsDirectoryPath);
                }
            }

            foreach (var zfi in cplzFiles)
            {
                Directory.CreateDirectory(_tempDirectory);
                var tempDi = new DirectoryInfo(_tempDirectory);

                Debug.LogMessage(LogEventLevel.Information, "Found cplz: {0}. Unzipping into temp plugins directory", zfi.FullName);
                var result = CrestronZIP.Unzip(zfi.FullName, tempDi.FullName);
                Debug.LogMessage(LogEventLevel.Information, "UnZip Result: {0}", result.ToString());

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
                                Debug.LogMessage(LogEventLevel.Information, "Found existing file in loadedPlugins: {0} Deleting and moving new file to replace it", filePath);
                                File.Delete(filePath);
                            }

                            // Move the file
                            File.Move(tempFile.FullName, filePath);
                            Debug.LogMessage(LogEventLevel.Verbose, "Moved {0} to {1}", tempFile.FullName, filePath);
                        }
                        else
                        {
                            Debug.LogMessage(LogEventLevel.Information, "Skipping assembly: {0}.  There is already an assembly with that name loaded.", tempFile.FullName);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogMessage(LogEventLevel.Verbose, "Assembly {0} is not a custom assembly. Exception: {1}", tempFile.FullName, e);
                        continue; //catching any load issues and continuing. There will be exceptions loading Crestron .dlls from the cplz Probably should do something different here
                    }
                }

                // Delete the .cplz and the temp directory
                Directory.Delete(_tempDirectory, true);
                zfi.Delete();
            }

            Debug.LogMessage(LogEventLevel.Information, "Done with .cplz archives");
        }

        /// <summary>
        /// Attempts to load the assemblies from the loadedPlugins folder
        /// </summary>
        static void LoadPluginAssemblies()
        {
            Debug.LogMessage(LogEventLevel.Information, "Loading assemblies from loadedPlugins folder...");
            var pluginDi = new DirectoryInfo(_loadedPluginsDirectoryPath);
            var pluginFiles = pluginDi.GetFiles("*.dll");

            Debug.LogMessage(LogEventLevel.Verbose, "Found {0} plugin assemblies to load", pluginFiles.Length);

            foreach (var pluginFile in pluginFiles)
            {
                var loadedAssembly = LoadAssembly(pluginFile.FullName);

                LoadedPluginFolderAssemblies.Add(loadedAssembly);
            }

            Debug.LogMessage(LogEventLevel.Information, "All Plugins Loaded.");
        }

        /// <summary>
        /// Iterate the loaded assemblies and try to call the LoadPlugin method
        /// </summary>
        static void LoadCustomPluginTypes()
        {
            Debug.LogMessage(LogEventLevel.Information, "Loading Custom Plugin Types...");
            foreach (var loadedAssembly in LoadedPluginFolderAssemblies)
            {
                // iteratate this assembly's classes, looking for "LoadPlugin()" methods
                try
                {
                    var assy = loadedAssembly.Assembly;
                    Type[] types = { };
                    try
                    {
                        types = assy.GetTypes();
                        Debug.LogMessage(LogEventLevel.Debug, $"Got types for assembly {assy.GetName().Name}");
                    }
                    catch (TypeLoadException e)
                    {
                        Debug.LogMessage(LogEventLevel.Error, "Unable to get types for assembly {0}: {1}",
                            loadedAssembly.Name, e.Message);
                        Debug.LogMessage(LogEventLevel.Verbose, e.StackTrace);
                        continue;
                    }

                    foreach (var type in types)
                    {
                        try
                        {
                            if (typeof(IPluginDeviceFactory).IsAssignableFrom(type) && !type.IsAbstract)
                            {
                                var plugin =
                                    (IPluginDeviceFactory)Activator.CreateInstance(type);
                                LoadCustomPlugin(plugin, loadedAssembly);
                            }
                        }
                        catch (NotSupportedException)
                        {
                            //this happens for dlls that aren't PD dlls, like ports of Mono classes into S#. Swallowing.                               
                        }
                        catch (Exception e)
                        {
                            Debug.LogMessage(LogEventLevel.Error, "Load Plugin not found. {0}.{2} is not a plugin factory. Exception: {1}",
                                loadedAssembly.Name, e.Message, type.Name);
                            continue;
                        }

                    }
                }
                catch (Exception e)
                {
                    Debug.LogMessage(LogEventLevel.Information, "Error Loading assembly {0}: {1}",
                           loadedAssembly.Name, e.Message);
                    Debug.LogMessage(LogEventLevel.Verbose, "{0}", e.StackTrace);
                    continue;
                }
            }
            // plugin dll will be loaded.  Any classes in plugin should have a static constructor
            // that registers that class with the Core.DeviceFactory
            Debug.LogMessage(LogEventLevel.Information, "Done Loading Custom Plugin Types.");
        }

        /// <summary>
        /// Loads a custom plugin and performs a dependency check to ensure compatibility with the required Essentials
        /// framework version.
        /// </summary>
        /// <remarks>This method verifies that the plugin meets the minimum required Essentials framework version
        /// before loading it.  If the plugin fails the dependency check, it is skipped, and a log message is generated.  If
        /// the plugin passes the check, it is loaded, and its type factories are initialized.</remarks>
        /// <param name="pluginDeviceFactory">The plugin to be loaded, implementing the <see cref="IPluginDeviceFactory"/> interface.  If the plugin also
        /// implements <see cref="IPluginDevelopmentDeviceFactory"/>, additional checks for development versions are
        /// performed.</param>
        /// <param name="loadedAssembly">The assembly associated with the plugin being loaded. This is used for logging and tracking purposes.</param>
        static void LoadCustomPlugin(IPluginDeviceFactory pluginDeviceFactory, LoadedAssembly loadedAssembly)
        {
            var passed = pluginDeviceFactory is IPluginDevelopmentDeviceFactory developmentPlugin ? Global.IsRunningDevelopmentVersion
                (developmentPlugin.DevelopmentEssentialsFrameworkVersions, developmentPlugin.MinimumEssentialsFrameworkVersion)
                : Global.IsRunningMinimumVersionOrHigher(pluginDeviceFactory.MinimumEssentialsFrameworkVersion);

            if (!passed)
            {
                Debug.LogMessage(LogEventLevel.Information,
                    "\r\n********************\r\n\tPlugin indicates minimum Essentials version {0}.  Dependency check failed.  Skipping Plugin {1}\r\n********************",
                    pluginDeviceFactory.MinimumEssentialsFrameworkVersion, loadedAssembly.Name);
                return;
            }
            else
            {
                Debug.LogMessage(LogEventLevel.Information, "Passed plugin passed dependency check (required version {0})", pluginDeviceFactory.MinimumEssentialsFrameworkVersion);
            }

            Debug.LogMessage(LogEventLevel.Information, "Loading plugin factory: {0}", loadedAssembly.Name);

            LoadDeviceFactories(pluginDeviceFactory);

            if (!EssentialsPluginAssemblies.Contains(loadedAssembly))
                EssentialsPluginAssemblies.Add(loadedAssembly);
        }

        /// <summary>
        /// Loads device factories from the specified plugin device factory and registers them for use.
        /// </summary>
        /// <remarks>This method retrieves metadata from the provided <paramref name="deviceFactory"/>, including
        /// type names, descriptions, and configuration snippets, and registers the factory for each device type. The type
        /// names are converted to lowercase for registration.</remarks>
        /// <param name="deviceFactory">The plugin device factory that provides the device types, descriptions, and factory methods to be registered.</param>
        private static void LoadDeviceFactories(IPluginDeviceFactory deviceFactory)
        {
            foreach (var typeName in deviceFactory.TypeNames)
            {
                string description = deviceFactory.FactoryType.GetCustomAttributes(typeof(DescriptionAttribute), true) is DescriptionAttribute[] descriptionAttribute && descriptionAttribute.Length > 0
                    ? descriptionAttribute[0].Description
                    : "No description available";

                DeviceFactory.AddFactoryForType(typeName.ToLower(), description, deviceFactory.FactoryType, deviceFactory.BuildDevice);
            }
        }


        /// <summary>
        /// LoadPlugins method
        /// </summary>
        public static void LoadPlugins()
        {
            Debug.LogMessage(LogEventLevel.Information, "Attempting to Load Plugins from {_pluginDirectory}", _pluginDirectory);

            if (Directory.Exists(_pluginDirectory))
            {
                Debug.LogMessage(LogEventLevel.Information, "Plugins directory found, checking for plugins");

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
    /// Represents a LoadedAssembly
    /// </summary>
    public class LoadedAssembly
    {
        /// <summary>
        /// Gets the name of the assembly
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the version of the assembly
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; private set; }

        /// <summary>
        /// Gets the assembly
        /// </summary>
        [JsonIgnore]
        public Assembly Assembly { get; private set; }

        /// <summary>
        /// Initializes a new instance of the LoadedAssembly class
        /// </summary>
        /// <param name="name">The name of the assembly</param>
        /// <param name="version">The version of the assembly</param>
        /// <param name="assembly">The assembly</param>
        public LoadedAssembly(string name, string version, Assembly assembly)
        {
            Name = name;
            Version = version;
            Assembly = assembly;
        }

        /// <summary>
        /// SetAssembly method
        /// </summary>
        public void SetAssembly(Assembly assembly)
        {
            Assembly = assembly;
        }
    }
}