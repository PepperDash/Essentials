﻿using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using System.Reflection;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Reflection.Metadata;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using Serilog.Events;
using Newtonsoft.Json;
using CrestronIO = Crestron.SimplSharp.CrestronIO;
using SystemIO = System.IO;

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
        /// List of plugins that were found to be incompatible with .NET 8
        /// </summary>
        public static List<IncompatiblePlugin> IncompatiblePlugins { get; private set; }

        public static LoadedAssembly EssentialsAssembly { get; private set; }

        public static LoadedAssembly PepperDashCoreAssembly { get; private set; }

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

        // Known incompatible types in .NET 8
        private static readonly HashSet<string> KnownIncompatibleTypes = new HashSet<string>
        {
            "System.Net.ICertificatePolicy",
            "System.Security.Cryptography.SHA1CryptoServiceProvider",
            "System.Web.HttpUtility",
            "System.Configuration.ConfigurationManager",
            "System.Web.Services.Protocols.SoapHttpClientProtocol",
            "System.Runtime.Remoting",
            "System.EnterpriseServices",
            "System.Runtime.Serialization.Formatters.Binary.BinaryFormatter",
            "System.Security.SecurityManager",
            "System.Security.Permissions.FileIOPermission",
            "System.AppDomain.CreateDomain"
        };

        static PluginLoader()
        {
            LoadedAssemblies = new List<LoadedAssembly>();
            LoadedPluginFolderAssemblies = new List<LoadedAssembly>();
            EssentialsPluginAssemblies = new List<LoadedAssembly>();
            IncompatiblePlugins = new List<IncompatiblePlugin>();
        }

        /// <summary>
        /// Retrieves all the loaded assemblies from the program directory
        /// </summary>
        public static void AddProgramAssemblies()
        {
            Debug.LogMessage(LogEventLevel.Verbose, "Getting Assemblies loaded with Essentials");
            // Get the loaded assembly filenames
            var appDi = new SystemIO.DirectoryInfo(Global.ApplicationDirectoryPathPrefix);
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

        public static void SetEssentialsAssembly(string name, Assembly assembly)
        {
            var loadedAssembly = LoadedAssemblies.FirstOrDefault(la => la.Name.Equals(name));

            if (loadedAssembly != null)
            {
                loadedAssembly.SetAssembly(assembly);
            }
        }

        /// <summary>
        /// Checks if a plugin is compatible with .NET 8 by examining its metadata
        /// </summary>
        /// <param name="filePath">Path to the plugin assembly</param>
        /// <returns>Tuple with compatibility result, reason if incompatible, and referenced assemblies</returns>
        public static (bool IsCompatible, string Reason, List<string> References) IsPluginCompatibleWithNet8(string filePath)
        {
            try
            {
                List<string> referencedAssemblies = new List<string>();
                
                using (SystemIO.FileStream fs = new SystemIO.FileStream(filePath, SystemIO.FileMode.Open, 
                    SystemIO.FileAccess.Read, SystemIO.FileShare.ReadWrite))
                using (PEReader peReader = new PEReader(fs))
                {
                    if (!peReader.HasMetadata)
                        return (false, "Not a valid .NET assembly", referencedAssemblies);

                    MetadataReader metadataReader = peReader.GetMetadataReader();
                    
                    // Collect assembly references
                    foreach (var assemblyRefHandle in metadataReader.AssemblyReferences)
                    {
                        var assemblyRef = metadataReader.GetAssemblyReference(assemblyRefHandle);
                        string assemblyName = metadataReader.GetString(assemblyRef.Name);
                        referencedAssemblies.Add(assemblyName);
                    }

                    // Check for references to known incompatible types
                    foreach (var typeRefHandle in metadataReader.TypeReferences)
                    {
                        var typeRef = metadataReader.GetTypeReference(typeRefHandle);
                        string typeNamespace = metadataReader.GetString(typeRef.Namespace);
                        string typeName = metadataReader.GetString(typeRef.Name);

                        string fullTypeName = $"{typeNamespace}.{typeName}";
                        if (KnownIncompatibleTypes.Contains(fullTypeName))
                        {
                            return (false, $"Uses incompatible type: {fullTypeName}", referencedAssemblies);
                        }
                    }

                    // Check for explicit .NET 8 compatibility attribute
                    bool hasNet8Attribute = false;
                    foreach (var customAttributeHandle in metadataReader.GetAssemblyDefinition().GetCustomAttributes())
                    {
                        var customAttribute = metadataReader.GetCustomAttribute(customAttributeHandle);
                        var ctorHandle = customAttribute.Constructor;

                        if (ctorHandle.Kind == HandleKind.MemberReference)
                        {
                            var memberRef = metadataReader.GetMemberReference((MemberReferenceHandle)ctorHandle);
                            var typeRef = metadataReader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);

                            string typeName = metadataReader.GetString(typeRef.Name);
                            if (typeName == "Net8CompatibleAttribute" || typeName == "TargetFrameworkAttribute")
                            {
                                hasNet8Attribute = true;
                                break;
                            }
                        }
                    }

                    if (hasNet8Attribute)
                    {
                        return (true, null, referencedAssemblies);
                    }

                    // If we can't determine incompatibility, assume it's compatible
                    return (true, null, referencedAssemblies);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error analyzing assembly: {ex.Message}", new List<string>());
            }
        }

        /// <summary>
        /// Loads an assembly via Reflection and adds it to the list of loaded assemblies
        /// </summary>
        /// <param name="filePath">Path to the assembly file</param>
        /// <param name="requestedBy">Name of the plugin requesting this assembly (null for direct loads)</param>
        static LoadedAssembly LoadAssembly(string filePath, string requestedBy = null)
        {
            try
            {
                // Check .NET 8 compatibility before loading
                var (isCompatible, reason, references) = IsPluginCompatibleWithNet8(filePath);
                if (!isCompatible)
                {
                    string fileName = CrestronIO.Path.GetFileName(filePath);
                    Debug.LogMessage(LogEventLevel.Warning, "Assembly '{0}' is not compatible with .NET 8: {1}", fileName, reason);
                    
                    var incompatiblePlugin = new IncompatiblePlugin(fileName, reason, requestedBy);
                    IncompatiblePlugins.Add(incompatiblePlugin);
                    return null;
                }

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
            catch(SystemIO.FileLoadException ex) when (ex.Message.Contains("Assembly with same name is already loaded"))
            {
                // Get the assembly name from the file path
                string assemblyName = CrestronIO.Path.GetFileNameWithoutExtension(filePath);
                
                // Try to find the already loaded assembly
                var existingAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase));
                    
                if (existingAssembly != null)
                {
                    Debug.LogMessage(LogEventLevel.Information, "Assembly '{0}' is already loaded, using existing instance", assemblyName);
                    var assyVersion = GetAssemblyVersion(existingAssembly);
                    var loadedAssembly = new LoadedAssembly(existingAssembly.GetName().Name, assyVersion, existingAssembly);
                    LoadedAssemblies.Add(loadedAssembly);
                    return loadedAssembly;
                }
                
                Debug.LogMessage(LogEventLevel.Warning, "Assembly with same name already loaded but couldn't find it: {0}", filePath);
                return null;
            }
            catch(Exception ex)
            {
                string fileName = CrestronIO.Path.GetFileName(filePath);
                
                // Check if this might be a .NET Framework compatibility issue
                if (ex.Message.Contains("Could not load type") || 
                    ex.Message.Contains("Unable to load one or more of the requested types"))
                {
                    Debug.LogMessage(LogEventLevel.Error, "Error loading assembly {0}: Likely .NET 8 compatibility issue: {1}", 
                        fileName, ex.Message);
                    IncompatiblePlugins.Add(new IncompatiblePlugin(fileName, ex.Message, requestedBy));
                }
                else
                {
                    Debug.LogMessage(ex, "Error loading assembly from {path}", null, filePath);
                }
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
        /// <param name="filename"></param>
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

            if (IncompatiblePlugins.Count > 0)
            {
                CrestronConsole.ConsoleCommandResponse("Incompatible Plugins:" + CrestronEnvironment.NewLine);
                foreach (var plugin in IncompatiblePlugins)
                {
                    if (plugin.TriggeredBy != "Direct load")
                    {
                        CrestronConsole.ConsoleCommandResponse("{0}: {1} (Required by: {2})" + CrestronEnvironment.NewLine, 
                            plugin.Name, plugin.Reason, plugin.TriggeredBy);
                    }
                    else
                    {
                        CrestronConsole.ConsoleCommandResponse("{0}: {1}" + CrestronEnvironment.NewLine, 
                            plugin.Name, plugin.Reason);
                    }
                }
            }
        }
        
        /// <summary>
        /// Moves any .dll assemblies not already loaded from the plugins folder to loadedPlugins folder
        /// </summary>
        static void MoveDllAssemblies()
        {
            Debug.LogMessage(LogEventLevel.Information, "Looking for .dll assemblies from plugins folder...");

            var pluginDi = new SystemIO.DirectoryInfo(_pluginDirectory);
            var pluginFiles = pluginDi.GetFiles("*.dll");

            if (pluginFiles.Length > 0)
            {
                if (!SystemIO.Directory.Exists(_loadedPluginsDirectoryPath))
                {
                    SystemIO.Directory.CreateDirectory(_loadedPluginsDirectoryPath);
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
                        if (SystemIO.File.Exists(filePath))
                        {
                            Debug.LogMessage(LogEventLevel.Information, "Found existing file in loadedPlugins: {0} Deleting and moving new file to replace it", filePath);
                            SystemIO.File.Delete(filePath);
                        }

                        // Move the file
                        SystemIO.File.Move(pluginFile.FullName, filePath);
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
            Debug.LogMessage(LogEventLevel.Information, "Looking for .cplz archives from plugins folder...");
            var di = new SystemIO.DirectoryInfo(_pluginDirectory);
            var zFiles = di.GetFiles("*.cplz");

            if (zFiles.Length > 0)
            {
                if (!SystemIO.Directory.Exists(_loadedPluginsDirectoryPath))
                {
                    SystemIO.Directory.CreateDirectory(_loadedPluginsDirectoryPath);
                }
            }

            foreach (var zfi in zFiles)
            {
                SystemIO.Directory.CreateDirectory(_tempDirectory);
                var tempDi = new SystemIO.DirectoryInfo(_tempDirectory);

                Debug.LogMessage(LogEventLevel.Information, "Found cplz: {0}. Unzipping into temp plugins directory", zfi.Name);
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
                            if (SystemIO.File.Exists(filePath))
                            {
                                Debug.LogMessage(LogEventLevel.Information, "Found existing file in loadedPlugins: {0} Deleting and moving new file to replace it", filePath);
                                SystemIO.File.Delete(filePath);
                            }

                            // Move the file
                            SystemIO.File.Move(tempFile.FullName, filePath);
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
                SystemIO.Directory.Delete(_tempDirectory, true);
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
            var pluginDi = new CrestronIO.DirectoryInfo(_loadedPluginsDirectoryPath);
            var pluginFiles = pluginDi.GetFiles("*.dll");

            Debug.LogMessage(LogEventLevel.Verbose, "Found {0} plugin assemblies to load", pluginFiles.Length);

            // First, check compatibility of all assemblies before loading any
            var assemblyCompatibility = new Dictionary<string, (bool IsCompatible, string Reason, List<string> References)>();
            
            foreach (var pluginFile in pluginFiles)
            {
                string fileName = pluginFile.Name;
                assemblyCompatibility[fileName] = IsPluginCompatibleWithNet8(pluginFile.FullName);
            }
            
            // Now load compatible assemblies and track incompatible ones
            foreach (var pluginFile in pluginFiles)
            {
                string fileName = pluginFile.Name;
                var (isCompatible, reason, references) = assemblyCompatibility[fileName];
                
                if (!isCompatible)
                {
                    Debug.LogMessage(LogEventLevel.Warning, "Assembly '{0}' is not compatible with .NET 8: {1}", fileName, reason);
                    IncompatiblePlugins.Add(new IncompatiblePlugin(fileName, reason, null));
                    continue;
                }
                
                // Try to load the assembly
                var loadedAssembly = LoadAssembly(pluginFile.FullName, null);
                
                if (loadedAssembly != null)
                {
                    LoadedPluginFolderAssemblies.Add(loadedAssembly);
                }
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
                // Skip if assembly is null (can happen if we had loading issues)
                if (loadedAssembly == null || loadedAssembly.Assembly == null)
                    continue;
                
                // iteratate this assembly's classes, looking for "LoadPlugin()" methods
                try
                {
                    var assy = loadedAssembly.Assembly;
                    Type[] types = {};
                    try
                    {
                        types = assy.GetTypes();
                        Debug.LogMessage(LogEventLevel.Debug, $"Got types for assembly {assy.GetName().Name}");
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        Debug.LogMessage(LogEventLevel.Error, "Unable to get types for assembly {0}: {1}",
                            loadedAssembly.Name, e.Message);
                        
                        // Check if any of the loader exceptions are due to missing assemblies
                        foreach (var loaderEx in e.LoaderExceptions)
                        {
                            if (loaderEx is SystemIO.FileNotFoundException fileNotFoundEx)
                            {
                                string missingAssembly = fileNotFoundEx.FileName;
                                if (!string.IsNullOrEmpty(missingAssembly))
                                {
                                    Debug.LogMessage(LogEventLevel.Warning, "Assembly {0} requires missing dependency: {1}", 
                                        loadedAssembly.Name, missingAssembly);
                                    
                                    // Add to incompatible plugins with dependency information
                                    IncompatiblePlugins.Add(new IncompatiblePlugin(
                                        CrestronIO.Path.GetFileName(missingAssembly), 
                                        $"Missing dependency required by {loadedAssembly.Name}", 
                                        loadedAssembly.Name));
                                }
                            }
                        }
                        
                        Debug.LogMessage(LogEventLevel.Verbose, e.StackTrace);
                        continue;
                    }
                    catch (TypeLoadException e)
                    {
                        Debug.LogMessage(LogEventLevel.Error, "Unable to get types for assembly {0}: {1}",
                            loadedAssembly.Name, e.Message);
                        Debug.LogMessage(LogEventLevel.Verbose, e.StackTrace);
                        
                        // Add to incompatible plugins if this is likely a .NET 8 compatibility issue
                        if (e.Message.Contains("Could not load type") || 
                            e.Message.Contains("Unable to load one or more of the requested types"))
                        {
                            IncompatiblePlugins.Add(new IncompatiblePlugin(loadedAssembly.Name, 
                                $"Type loading error: {e.Message}", 
                                null));
                        }
                        
                        continue;
                    }

                    foreach (var type in types)
                    {
                        try
                        {                    
                            if (typeof (IPluginDeviceFactory).IsAssignableFrom(type) && !type.IsAbstract)
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
                    
                    // Add to incompatible plugins if this is likely a .NET 8 compatibility issue
                    if (e.Message.Contains("Could not load type") || 
                        e.Message.Contains("Unable to load one or more of the requested types"))
                    {
                        IncompatiblePlugins.Add(new IncompatiblePlugin(loadedAssembly.Name, 
                            $"Assembly loading error: {e.Message}", 
                            null));
                    }
                    
                    continue;
                }
            }
            
            // Update incompatible plugins with dependency information
            var pluginDependencies = new Dictionary<string, List<string>>();
            // Populate pluginDependencies with relevant data
            // Example: pluginDependencies["PluginA"] = new List<string> { "Dependency1", "Dependency2" };
            UpdateIncompatiblePluginDependencies(pluginDependencies);
            
            // plugin dll will be loaded.  Any classes in plugin should have a static constructor
            // that registers that class with the Core.DeviceFactory
            Debug.LogMessage(LogEventLevel.Information, "Done Loading Custom Plugin Types.");
        }

        /// <summary>
        /// Updates incompatible plugins with information about which plugins depend on them
        /// </summary>
        private static void UpdateIncompatiblePluginDependencies(Dictionary<string, List<string>> pluginDependencies)
        {
            // For each incompatible plugin
            foreach (var incompatiblePlugin in IncompatiblePlugins)
            {
                // If it already has a requestedBy, skip it
                if (incompatiblePlugin.TriggeredBy != "Direct load")
                    continue;
                    
                // Find plugins that depend on this incompatible plugin
                foreach (var plugin in pluginDependencies)
                {
                    string pluginName = plugin.Key;
                    List<string> dependencies = plugin.Value;
                    
                    // If this plugin depends on the incompatible plugin
                    if (dependencies.Contains(incompatiblePlugin.Name) || 
                        dependencies.Any(d => d.StartsWith(incompatiblePlugin.Name + ",")))
                    {
                        incompatiblePlugin.UpdateTriggeringPlugin(pluginName);
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Loads a
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="loadedAssembly"></param>
        static void LoadCustomPlugin(IPluginDeviceFactory plugin, LoadedAssembly loadedAssembly)
        {
            var developmentPlugin = plugin as IPluginDevelopmentDeviceFactory;

            var passed = developmentPlugin != null ? Global.IsRunningDevelopmentVersion
                (developmentPlugin.DevelopmentEssentialsFrameworkVersions, developmentPlugin.MinimumEssentialsFrameworkVersion) 
                : Global.IsRunningMinimumVersionOrHigher(plugin.MinimumEssentialsFrameworkVersion);

            if (!passed)
            {
                Debug.LogMessage(LogEventLevel.Information,
                    "\r\n********************\r\n\tPlugin indicates minimum Essentials version {0}.  Dependency check failed.  Skipping Plugin {1}\r\n********************",
                    plugin.MinimumEssentialsFrameworkVersion, loadedAssembly.Name);
                return;
            }
            else
            {
                Debug.LogMessage(LogEventLevel.Information, "Passed plugin passed dependency check (required version {0})", plugin.MinimumEssentialsFrameworkVersion);
            }

            Debug.LogMessage(LogEventLevel.Information, "Loading plugin: {0}", loadedAssembly.Name);
            plugin.LoadTypeFactories();

            if(!EssentialsPluginAssemblies.Contains(loadedAssembly))
                EssentialsPluginAssemblies.Add(loadedAssembly);
        }

        /// <summary>
        /// Loads a a custom plugin via the legacy method
        /// </summary>
        /// <param name="type"></param>
        /// <param name="loadPlugin"></param>
        static void LoadCustomLegacyPlugin(Type type, MethodInfo loadPlugin, LoadedAssembly loadedAssembly)
        {
            Debug.LogMessage(LogEventLevel.Verbose, "LoadPlugin method found in {0}", type.Name);

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

            var minimumVersion = fields.FirstOrDefault(p => p.Name.Equals("MinimumEssentialsFrameworkVersion"));
            if (minimumVersion != null)
            {
                Debug.LogMessage(LogEventLevel.Verbose, "MinimumEssentialsFrameworkVersion found");

                var minimumVersionString = minimumVersion.GetValue(null) as string;

                if (!string.IsNullOrEmpty(minimumVersionString))
                {
                    var passed = Global.IsRunningMinimumVersionOrHigher(minimumVersionString);

                    if (!passed)
                    {
                        Debug.LogMessage(LogEventLevel.Information, "Plugin indicates minimum Essentials version {0}.  Dependency check failed.  Skipping Plugin", minimumVersionString);
                        return;
                    }
                    else
                    {
                        Debug.LogMessage(LogEventLevel.Information, "Passed plugin passed dependency check (required version {0})", minimumVersionString);
                    }
                }
                else
                {
                    Debug.LogMessage(LogEventLevel.Information, "MinimumEssentialsFrameworkVersion found but not set.  Loading plugin, but your mileage may vary.");
                }
            }
            else
            {
                Debug.LogMessage(LogEventLevel.Information, "MinimumEssentialsFrameworkVersion not found.  Loading plugin, but your mileage may vary.");
            }

            Debug.LogMessage(LogEventLevel.Information, "Loading legacy plugin: {0}", loadedAssembly.Name);
            loadPlugin.Invoke(null, null);
        }

        /// <summary>
        /// Loads plugins
        /// </summary>
        public static void LoadPlugins()
        {
            Debug.LogMessage(LogEventLevel.Information, "Attempting to Load Plugins from {_pluginDirectory}", _pluginDirectory);

            if (SystemIO.Directory.Exists(_pluginDirectory))
            {
                Debug.LogMessage(LogEventLevel.Information, "Plugins directory found, checking for plugins");

                // Deal with any .dll files
                MoveDllAssemblies();

                // Deal with any .cplz files
                UnzipAndMoveCplzArchives();

                if (SystemIO.Directory.Exists(_loadedPluginsDirectoryPath))
                {
                    // Load the assemblies from the loadedPlugins folder into the AppDomain
                    LoadPluginAssemblies();

                    // Load the types from any custom plugin assemblies
                    LoadCustomPluginTypes();
                }
                
                // Report on incompatible plugins
                if (IncompatiblePlugins.Count > 0)
                {
                    Debug.LogMessage(LogEventLevel.Warning, "Found {0} incompatible plugins:", IncompatiblePlugins.Count);
                    foreach (var plugin in IncompatiblePlugins)
                    {
                        if (plugin.TriggeredBy != "Direct load")
                        {
                            Debug.LogMessage(LogEventLevel.Warning, "  - {0}: {1} (Required by: {2})", 
                                plugin.Name, plugin.Reason, plugin.TriggeredBy);
                        }
                        else
                        {
                            Debug.LogMessage(LogEventLevel.Warning, "  - {0}: {1}", 
                                plugin.Name, plugin.Reason);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Represents an assembly loaded at runtime and it's associated metadata
    /// </summary>
    public class LoadedAssembly
    {
        [JsonProperty("name")]
        public string Name { get; private set; }
        [JsonProperty("version")]
        public string Version { get; private set; }
        [JsonIgnore]
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
    
    /// <summary>
    /// Represents a plugin that was found to be incompatible with .NET 8
    /// </summary>
    public class IncompatiblePlugin
    {
        [JsonProperty("name")]
        public string Name { get; private set; }
        
        [JsonProperty("reason")]
        public string Reason { get; private set; }
        
        [JsonProperty("triggeredBy")]
        public string TriggeredBy { get; private set; }
        
        public IncompatiblePlugin(string name, string reason, string triggeredBy = null)
        {
            Name = name;
            Reason = reason;
            TriggeredBy = triggeredBy ?? "Direct load";
        }
        
        /// <summary>
        /// Updates the plugin that triggered this incompatibility
        /// </summary>
        /// <param name="triggeringPlugin">Name of the plugin that requires this incompatible plugin</param>
        public void UpdateTriggeringPlugin(string triggeringPlugin)
        {
            if (!string.IsNullOrEmpty(triggeringPlugin))
            {
                TriggeredBy = triggeringPlugin;
            }
        }
    }
    
    /// <summary>
    /// Attribute to explicitly mark a plugin as .NET 8 compatible
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class Net8CompatibleAttribute : Attribute
    {
        public bool IsCompatible { get; }
        
        public Net8CompatibleAttribute(bool isCompatible = true)
        {
            IsCompatible = isCompatible;
        }
    }
}