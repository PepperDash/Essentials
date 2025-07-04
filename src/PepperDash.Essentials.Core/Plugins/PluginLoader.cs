using System;
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


namespace PepperDash.Essentials;

/// <summary>
/// Provides functionality for loading and managing plugins and assemblies in the application.
/// </summary>
/// <remarks>The <see cref="PluginLoader"/> class is responsible for discovering, loading, and managing assemblies
/// and plugins, including handling compatibility checks for .NET 8. It supports loading assemblies from  the program
/// directory, plugins folder, and .cplz archives. Additionally, it tracks incompatible plugins  and provides reporting
/// capabilities for loaded assemblies and their versions.</remarks>
public static class PluginLoader
{
    /// <summary>
    /// Gets the list of assemblies that have been loaded into the application.
    /// </summary>
    public static List<LoadedAssembly> LoadedAssemblies { get; private set; }

    /// <summary>
    /// Represents a collection of assemblies loaded from the plugin folder.
    /// </summary>
    /// <remarks>This field is used to store assemblies that have been dynamically loaded from a designated
    /// plugin folder. It is intended for internal use and should not be modified directly.</remarks>
    private static readonly List<LoadedAssembly> LoadedPluginFolderAssemblies;

    /// <summary>
    /// Gets the list of plugins that are incompatible with the current system or configuration.
    /// </summary>
    /// <remarks>This property provides information about plugins that are not supported or cannot function
    /// correctly in the current environment. Use this list to identify and handle incompatible plugins appropriately in
    /// your application logic.</remarks>
    public static List<IncompatiblePlugin> IncompatiblePlugins { get; private set; }

    /// <summary>
    /// Gets the loaded assembly that contains the core functionality of the application.
    /// </summary>
    public static LoadedAssembly EssentialsAssembly { get; private set; }

    /// <summary>
    /// Gets the loaded assembly information for the PepperDash Core library.
    /// </summary>
    public static LoadedAssembly PepperDashCoreAssembly { get; private set; }

    /// <summary>
    /// Gets the list of assemblies that are Essentials plugins loaded by the application.
    /// </summary>
    public static List<LoadedAssembly> EssentialsPluginAssemblies { get; private set; }

    /// <summary>
    /// Gets the directory path where plugins are stored.
    /// </summary>
    private static string PluginDirectory => Global.FilePathPrefix + "plugins";

    /// <summary>
    /// Gets the directory path where loaded plugin assemblies are stored.
    /// </summary>
    private static string LoadedPluginsDirectoryPath => PluginDirectory + Global.DirectorySeparator + "loadedAssemblies";

    /// <summary>
    /// Gets the path to the temporary directory used by the plugin.
    /// </summary>
    private static string TempDirectory => PluginDirectory + Global.DirectorySeparator + "temp";

    /// <summary>
    /// Represents a collection of fully qualified type names that are known to be incompatible with the current
    /// application or framework.
    /// </summary>
    /// <remarks>This collection contains the names of types that are deprecated, obsolete, or otherwise
    /// incompatible with the intended usage of the application. These types may represent security risks, unsupported
    /// features, or legacy APIs that should be avoided.</remarks>
    private static readonly HashSet<string> KnownIncompatibleTypes =
    [
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
    ];

    /// <summary>
    /// Initializes static members of the <see cref="PluginLoader"/> class.
    /// </summary>
    /// <remarks>This static constructor initializes the collections used to manage plugin assemblies and
    /// track incompatible plugins.</remarks>
    static PluginLoader()
    {
        LoadedAssemblies = [];
        LoadedPluginFolderAssemblies = [];
        EssentialsPluginAssemblies = [];
        IncompatiblePlugins = [];
    }

    /// <summary>
    /// Loads and registers assemblies from the application's directory that match specific naming patterns.
    /// </summary>
    /// <remarks>This method scans the application's directory for assemblies with filenames containing 
    /// "Essentials" or "PepperDash" and registers them in the <see cref="LoadedAssemblies"/> collection.  It also
    /// assigns specific assemblies to predefined properties, such as <see cref="EssentialsAssembly"/>  and <see
    /// cref="PepperDashCoreAssembly"/>, based on their names.   Debug messages are logged at various stages to provide
    /// detailed information about the process,  including the number of assemblies found and their versions. This
    /// method is intended to be used  during application initialization to ensure required assemblies are loaded and
    /// tracked.</remarks>
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
    /// Associates the specified assembly with the given name in the loaded assemblies collection.
    /// </summary>
    /// <remarks>If an assembly with the specified name already exists in the loaded assemblies collection, 
    /// this method updates its associated assembly. If no matching name is found, the method does nothing.</remarks>
    /// <param name="name">The name used to identify the assembly. This value is case-sensitive and must not be null or empty.</param>
    /// <param name="assembly">The assembly to associate with the specified name. This value must not be null.</param>
    public static void SetEssentialsAssembly(string name, Assembly assembly)
    {
        var loadedAssembly = LoadedAssemblies.FirstOrDefault(la => la.Name.Equals(name));

        loadedAssembly?.SetAssembly(assembly);
    }

    /// <summary>
    /// Determines whether a plugin assembly is compatible with .NET 8.
    /// </summary>
    /// <remarks>This method analyzes the provided assembly to determine compatibility with .NET 8 by checking
    /// for known incompatible types, inspecting custom attributes, and collecting assembly references. If the analysis
    /// encounters an error, the method returns <see langword="false"/> with an appropriate error message.</remarks>
    /// <param name="filePath">The file path to the plugin assembly to analyze.</param>
    /// <returns>A tuple containing the following: <list type="bullet"> <item> <description><see langword="true"/> if the plugin
    /// is compatible with .NET 8; otherwise, <see langword="false"/>.</description> </item> <item> <description>A
    /// string providing the reason for incompatibility, or <see langword="null"/> if the plugin is
    /// compatible.</description> </item> <item> <description>A list of assembly references found in the
    /// plugin.</description> </item> </list></returns>
    public static (bool IsCompatible, string Reason, List<string> References) IsPluginCompatibleWithNet8(string filePath)
    {
        try
        {
            List<string> referencedAssemblies = [];

            using FileStream fs = new(filePath, FileMode.Open,
                    FileAccess.Read, FileShare.ReadWrite);
            using PEReader peReader = new(fs);

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
        catch (Exception ex)
        {
            return (false, $"Error analyzing assembly: {ex.Message}", new List<string>());
        }
    }

    /// <summary>
    /// Loads an assembly from the specified file path and verifies its compatibility with .NET 8.
    /// </summary>
    /// <remarks>This method performs a compatibility check to ensure the assembly is compatible with .NET 8
    /// before attempting to load it. If the assembly is incompatible, it is added to the list of incompatible plugins,
    /// and a warning is logged. If the assembly is already loaded, the existing instance is returned instead of
    /// reloading it.  Exceptions during the load process are handled internally, and appropriate log messages are
    /// generated for issues such as: <list type="bullet"> <item><description>Incompatibility with .NET
    /// 8.</description></item> <item><description>File load conflicts (e.g., an assembly with the same name is already
    /// loaded).</description></item> <item><description>General errors, including potential .NET Framework
    /// compatibility issues.</description></item> </list></remarks>
    /// <param name="filePath">The full path to the assembly file to load. This cannot be null or empty.</param>
    /// <param name="requestedBy">An optional identifier for the entity requesting the load operation. This can be null.</param>
    /// <returns>A <see cref="LoadedAssembly"/> object representing the loaded assembly if the operation succeeds; otherwise,
    /// <see langword="null"/>.</returns>
    private static LoadedAssembly LoadAssembly(string filePath, string requestedBy = null)
    {
        try
        {
            // Check .NET 8 compatibility before loading
            var (isCompatible, reason, references) = IsPluginCompatibleWithNet8(filePath);
            if (!isCompatible)
            {
                string fileName = Path.GetFileName(filePath);
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
        catch(FileLoadException ex) when (ex.Message.Contains("Assembly with same name is already loaded"))
        {
            // Get the assembly name from the file path
            string assemblyName = Path.GetFileNameWithoutExtension(filePath);
            
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
            string fileName = Path.GetFileName(filePath);
            
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
    /// Retrieves the version information of the specified assembly.
    /// </summary>
    /// <remarks>This method first attempts to retrieve the version from the <see
    /// cref="AssemblyInformationalVersionAttribute"/>. If the attribute is not present, it falls back to the assembly's
    /// version as defined in its metadata.</remarks>
    /// <param name="assembly">The assembly from which to retrieve the version information. Cannot be <see langword="null"/>.</param>
    /// <returns>A string representing the version of the assembly. If the assembly has an <see
    /// cref="AssemblyInformationalVersionAttribute"/>, its <see
    /// cref="AssemblyInformationalVersionAttribute.InformationalVersion"/> value is returned. Otherwise, the assembly's
    /// version is returned in the format "Major.Minor.Build.Revision".</returns>
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
    /// Determines whether an assembly with the specified name is currently loaded.
    /// </summary>
    /// <remarks>This method performs a case-sensitive comparison to determine if the specified assembly is
    /// loaded. It logs verbose messages indicating the status of the check.</remarks>
    /// <param name="name">The name of the assembly to check. This value is case-sensitive.</param>
    /// <returns><see langword="true"/> if an assembly with the specified name is loaded; otherwise, <see langword="false"/>.</returns>
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
    /// Reports the versions of the Essentials framework, PepperDash Core, and loaded plugins to the console.
    /// </summary>
    /// <remarks>This method outputs version information for the Essentials framework, PepperDash Core, and
    /// all loaded Essentials plugins to the Crestron console. If any incompatible plugins are detected, their details
    /// are also reported, including the reason for incompatibility and the plugin that required them, if
    /// applicable.</remarks>
    /// <param name="command">The command string that triggered the version report. This parameter is not used directly by the method.</param>
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
    /// Moves .dll assemblies from the plugins folder to the loaded plugins directory.
    /// </summary>
    /// <remarks>This method scans the plugins folder for .dll files and moves them to the loaded plugins
    /// directory if they are not already loaded. If a file with the same name exists in the target directory, it is
    /// replaced. The method logs the process at various stages and handles exceptions for individual files to ensure
    /// the operation continues for other files.</remarks>
    private static void MoveDllAssemblies()
    {
        Debug.LogMessage(LogEventLevel.Information, "Looking for .dll assemblies from plugins folder...");

        var pluginDi = new DirectoryInfo(PluginDirectory);
        var pluginFiles = pluginDi.GetFiles("*.dll");

        if (pluginFiles.Length > 0)
        {
            if (!Directory.Exists(LoadedPluginsDirectoryPath))
            {
                Directory.CreateDirectory(LoadedPluginsDirectoryPath);
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

                    filePath = LoadedPluginsDirectoryPath + Global.DirectorySeparator + pluginFile.Name;

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
    /// Extracts and processes .cplz archive files found in the specified directories, moving their contents to the
    /// appropriate plugin directory.
    /// </summary>
    /// <remarks>This method searches for .cplz files in the plugin directory and user folder, extracts their
    /// contents, and moves any .dll files to the loaded plugins directory. If a .dll file with the same name already
    /// exists in the target directory, it is replaced with the new file. Temporary files and directories created during
    /// the process are cleaned up after the operation completes.</remarks>
    private static void UnzipAndMoveCplzArchives()
    {
        Debug.LogMessage(LogEventLevel.Information, "Looking for .cplz archives from plugins folder...");
        var di = new DirectoryInfo(PluginDirectory);
        var zFiles = di.GetFiles("*.cplz");

        //// Find cplz files at the root of the user folder. Makes development/testing easier for VC-4, and helps with mistakes by end users

        //var userDi = new DirectoryInfo(Global.FilePathPrefix);
        //var userZFiles = userDi.GetFiles("*.cplz");

        Debug.LogInformation("Checking {folder} for .cplz files", Global.FilePathPrefix);
        var cplzFiles = Directory.GetFiles(Global.FilePathPrefix, "*.cplz", SearchOption.AllDirectories)
            .Select(f => new FileInfo(f))
            .ToArray();

        if (cplzFiles.Length > 0)
        {
            if (!Directory.Exists(LoadedPluginsDirectoryPath))
            {
                Directory.CreateDirectory(LoadedPluginsDirectoryPath);
            }
        }

        foreach (var zfi in cplzFiles)
        {
            Directory.CreateDirectory(TempDirectory);
            var tempDi = new DirectoryInfo(TempDirectory);

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

                        filePath = LoadedPluginsDirectoryPath + Global.DirectorySeparator + tempFile.Name;

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
            Directory.Delete(TempDirectory, true);
            zfi.Delete();
        }

        Debug.LogMessage(LogEventLevel.Information, "Done with .cplz archives");
    }

    /// <summary>
    /// Loads plugin assemblies from the designated plugin directory, checks their compatibility with .NET 8,  and loads
    /// the compatible assemblies into the application.
    /// </summary>
    /// <remarks>This method scans the plugin directory for all `.dll` files, verifies their compatibility
    /// with .NET 8,  and attempts to load the compatible assemblies. Assemblies that are incompatible are logged as
    /// warnings  and added to a list of incompatible plugins for further inspection.</remarks>
    private static void LoadPluginAssemblies()
    {
        Debug.LogMessage(LogEventLevel.Information, "Loading assemblies from loadedPlugins folder...");
        var pluginDi = new DirectoryInfo(LoadedPluginsDirectoryPath);
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
            var (isCompatible, reason, _) = assemblyCompatibility[fileName];
            
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
    /// Loads and initializes custom plugin types from the assemblies in the plugin folder.
    /// </summary>
    /// <remarks>This method iterates through all loaded plugin assemblies, identifies types that implement 
    /// the <see cref="IPluginDeviceFactory"/> interface, and attempts to instantiate and load them.  Assemblies or
    /// types that cannot be loaded due to missing dependencies, type loading errors,  or other exceptions are logged,
    /// and incompatible plugins are tracked for further analysis.</remarks>
    private static void LoadCustomPluginTypes()
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
                Type[] types = [];
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
                        if (loaderEx is FileNotFoundException fileNotFoundEx)
                        {
                            string missingAssembly = fileNotFoundEx.FileName;
                            if (!string.IsNullOrEmpty(missingAssembly))
                            {
                                Debug.LogMessage(LogEventLevel.Warning, "Assembly {0} requires missing dependency: {1}", 
                                    loadedAssembly.Name, missingAssembly);
                                
                                // Add to incompatible plugins with dependency information
                                IncompatiblePlugins.Add(new IncompatiblePlugin(
                                    Path.GetFileName(missingAssembly), 
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
    /// Updates the triggering plugin information for incompatible plugins based on their dependencies.
    /// </summary>
    /// <remarks>This method iterates through a predefined list of incompatible plugins and updates their
    /// triggering plugin information if they were directly loaded and are found to be dependencies of other plugins.
    /// The update is performed for the first plugin that depends on the incompatible plugin.</remarks>
    /// <param name="pluginDependencies">A dictionary where the key is the name of a plugin and the value is a list of its dependencies. Each dependency
    /// is represented as a string, which may include additional metadata.</param>
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
    /// Loads a custom plugin and performs a dependency check to ensure compatibility with the required Essentials
    /// framework version.
    /// </summary>
    /// <remarks>This method verifies that the plugin meets the minimum required Essentials framework version
    /// before loading it.  If the plugin fails the dependency check, it is skipped, and a log message is generated.  If
    /// the plugin passes the check, it is loaded, and its type factories are initialized.</remarks>
    /// <param name="plugin">The plugin to be loaded, implementing the <see cref="IPluginDeviceFactory"/> interface.  If the plugin also
    /// implements <see cref="IPluginDevelopmentDeviceFactory"/>, additional checks for development versions are
    /// performed.</param>
    /// <param name="loadedAssembly">The assembly associated with the plugin being loaded. This is used for logging and tracking purposes.</param>
    private static void LoadCustomPlugin(IPluginDeviceFactory plugin, LoadedAssembly loadedAssembly)
    {
        var passed = plugin is IPluginDevelopmentDeviceFactory developmentPlugin ? Global.IsRunningDevelopmentVersion
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
    /// Loads plugins from the designated plugin directory, processes them, and integrates them into the application.
    /// </summary>
    /// <remarks>This method performs the following steps: <list type="bullet"> <item><description>Checks if
    /// the plugin directory exists.</description></item> <item><description>Processes any plugin files, including .dll
    /// and .cplz files, by moving or extracting them as needed.</description></item> <item><description>Loads
    /// assemblies from the processed plugins into the application domain.</description></item>
    /// <item><description>Identifies and reports any incompatible plugins, including the reason for
    /// incompatibility.</description></item> </list> Plugins that are successfully loaded are made available for use,
    /// while incompatible plugins are logged for review.</remarks>
    public static void LoadPlugins()
    {
        Debug.LogMessage(LogEventLevel.Information, "Attempting to Load Plugins from {_pluginDirectory}", PluginDirectory);

        if (Directory.Exists(PluginDirectory))
        {
            Debug.LogMessage(LogEventLevel.Information, "Plugins directory found, checking for plugins");

            // Deal with any .dll files
            MoveDllAssemblies();

            // Deal with any .cplz files
            UnzipAndMoveCplzArchives();

            if (Directory.Exists(LoadedPluginsDirectoryPath))
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
