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
            // Get the loaded assembly filenames
            var appDi = new DirectoryInfo(Global.ApplicationDirectoryPathPrefix);
            var assemblyFiles = appDi.GetFiles("*.dll");

            foreach (var file in assemblyFiles)
            {
                try
                {
                    LoadAssembly(file.FullName);
                }
                catch
                {
                    Debug.Console(2, "Assembly {0} is not a custom assembly", file.FullName);
                }
            }
        }

        /// <summary>
        /// Loads an assembly via Reflection and adds it to the list of loaded assemblies
        /// </summary>
        /// <param name="fileName"></param>
        static LoadedAssembly LoadAssembly(string fileName)
        {
            if (!CheckIfAssemblyExists(fileName))
            {

                var assembly = Assembly.LoadFrom(fileName);
                if (assembly != null)
                {
                    var assyVersion = GetAssemblyVersion(assembly);

                    var loadedAssembly = new LoadedAssembly(assembly.GetName().Name, assyVersion, assembly);
                    LoadedAssemblies.Add(loadedAssembly);
                    Debug.Console(0, Debug.ErrorLogLevel.Notice, "Loaded plugin file '{0}', version {1}", loadedAssembly.FileName, loadedAssembly.Version);
                    return loadedAssembly;
                }
                else
                {
                    Debug.Console(0, Debug.ErrorLogLevel.Notice, "Unable to load assembly: '{0}'", fileName);
                }
            }
            else
            {
                Debug.Console(0, Debug.ErrorLogLevel.Notice, "Skipping assembly: {0}.  There is already an assembly with that name loaded.", fileName);
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
            if (ver != null)
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
        public static bool CheckIfAssemblyExists(string filename)
        {
            Debug.Console(2, "Checking if assembly: {0} is loaded...", filename);
            var loadedAssembly = LoadedAssemblies.FirstOrDefault(s => s.FileName.Equals(filename));

            if (loadedAssembly != null)
            {
                return true;
            }
            else
            {
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
                Debug.Console(0, "{0} Version: {1}", assembly.FileName, assembly.Version);
            }
        }

         /// <summary>
        /// Loads plugins
        /// </summary>
        public static void LoadPlugins()
        {
            var dir = Global.FilePathPrefix + "plugins";
            var tempDir = dir + Global.DirectorySeparator + "temp";

            if (Directory.Exists(dir))
            {
                Debug.Console(0, Debug.ErrorLogLevel.Notice, "Plugins directory found, checking for factory plugins");
                var di = new DirectoryInfo(dir);
                var zFiles = di.GetFiles("*.cplz");

                foreach (var fi in zFiles)
                {
                    Directory.CreateDirectory(tempDir);
                    var tempDi = new DirectoryInfo(tempDir);

                    Debug.Console(0, "Found cplz: {0}. Unzipping into temp plugins directory", fi.Name);
                    var result = CrestronZIP.Unzip(fi.FullName, tempDi.FullName);
                    Debug.Console(0, "UnZip Result: {0}", result.ToString());

                    var files = tempDi.GetFiles("*.dll");

                    foreach (FileInfo file in files)
                    {
                        try
                        {
                            var pluginAssembly = LoadAssembly(fi.FullName);

                            if (pluginAssembly != null)
                            {
                                LoadedPluginFolderAssemblies.Add(pluginAssembly);
                            }
                        }
                        catch
                        {
                            Debug.Console(2, "Assembly {0} is not a custom assembly", file.FullName);
                            continue; //catching any load issues and continuing. There will be exceptions loading Crestron .dlls from the cplz Probably should do something different here
                        }
                        
                    }

                    // Delete the .cplz and the temp directory
                    fi.Delete();
                    Directory.Delete(tempDir);
                }

                // Iterate the loaded assemblies and try to call the LoadPlugin method
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
                                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
                                var loadPlugin = methods.FirstOrDefault(m => m.Name.Equals("LoadPlugin"));
                                if (loadPlugin != null)
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
                                                continue;
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

                                    Debug.Console(0, Debug.ErrorLogLevel.Notice, "Adding plugin: {0}", loadedAssembly.FileName);
                                    loadPlugin.Invoke(null, null);
                                }
                            }
                            catch
                            {
                                Debug.Console(2, "Load Plugin not found. {0} is not a plugin assembly", loadedAssembly.FileName);
                                continue;
                            }

                        }
                    }
                    catch
                    {
                        Debug.Console(2, "Assembly {0} is not a custom assembly. Types cannot be loaded.", loadedAssembly.FileName);
                        continue;
                    }
                }
                // plugin dll will be loaded.  Any classes in plugin should have a static constructor
                // that registers that class with the Core.DeviceFactory
            }
        }

    }

    /// <summary>
    /// Represents an assembly loaded at runtime and it's associated metadata
    /// </summary>
    public class LoadedAssembly
    {
        public string FileName { get; private set; }
        public string Version { get; private set; }
        public Assembly Assembly { get; private set; }

        public LoadedAssembly(string fileName, string version, Assembly assembly)
        {
            FileName = fileName;
            Version = version;
            Assembly = assembly;
        }
    }
}