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
    /// Guards <see cref="PluginReports"/> and <see cref="PluginFileNames"/>, which are written during
    /// startup and read later from the console and the Web API.
    /// </summary>
    private static readonly object PluginReportsLock = new();

    /// <summary>
    /// What happened to each plugin file during startup, keyed by file name.
    /// </summary>
    private static readonly Dictionary<string, PluginLoadReport> PluginReports = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The file each plugin-folder assembly was loaded from, so later stages can report against it.
    /// </summary>
    private static readonly Dictionary<LoadedAssembly, string> PluginFileNames = new();

    /// <summary>
    /// A snapshot of what happened to every plugin file during startup, including those that loaded
    /// cleanly. Each report is a copy, so it is safe to read while loading continues.
    /// </summary>
    public static IReadOnlyList<PluginLoadReport> PluginLoadReports
    {
        get
        {
            lock (PluginReportsLock)
            {
                return PluginReports.Values
                    .Select(r => r.Clone())
                    .OrderBy(r => r.FileName, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
        }
    }

    /// <summary>
    /// A snapshot of the plugin files that recorded at least one issue during startup.
    /// </summary>
    public static IReadOnlyList<PluginLoadReport> PluginLoadReportsWithIssues =>
        PluginLoadReports.Where(r => r.Issues.Count > 0).ToList();

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
    public static void AddLoadedAssembly(string name, Assembly assembly)
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
            var info = PluginAssemblyInspector.Inspect(filePath);
            var (isCompatible, reason) = CheckNet8Compatibility(info);
            return (isCompatible, reason, info.References.Select(r => r.Name).ToList());
        }
        catch (BadImageFormatException)
        {
            return (false, "Not a valid .NET assembly", []);
        }
        catch (Exception ex)
        {
            return (false, $"Error analyzing assembly: {ex.Message}", []);
        }
    }

    /// <summary>
    /// Checks already-read metadata for references to types that do not exist on .NET 8.
    /// </summary>
    private static (bool IsCompatible, string Reason) CheckNet8Compatibility(PluginBuildInfo info)
    {
        var incompatibleType = info.TypeReferences.FirstOrDefault(KnownIncompatibleTypes.Contains);

        return incompatibleType != null
            ? (false, $"Uses incompatible type: {incompatibleType}")
            : (true, null);
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
        // The caller has already checked .NET 8 compatibility from the file's metadata.
        string fileName = Path.GetFileName(filePath);

        try
        {
            var assembly = Assembly.LoadFrom(filePath);
            if (assembly != null)
            {
                var assyVersion = GetAssemblyVersion(assembly);

                // LoadFrom returns an already-loaded assembly with the same identity rather than loading
                // this file, even when it came from somewhere else (e.g. a different prerelease build
                // with the same assembly version). Report which binary is actually in use.
                var loadedFrom = string.IsNullOrEmpty(assembly.Location) ? null : Path.GetFullPath(assembly.Location);
                if (loadedFrom != null && !string.Equals(loadedFrom, Path.GetFullPath(filePath), StringComparison.Ordinal))
                {
                    RecordIssue(fileName, new PluginLoadIssue(PluginLoadIssueSeverity.Warning, PluginLoadIssueCategory.DuplicateAssembly,
                        $"An assembly with the same identity is already loaded from {loadedFrom}, so version {assyVersion} from there is used instead of this file.",
                        "Only one copy of an assembly can be loaded. Remove the duplicate, or make sure both copies are the same build.",
                        requestedBy));
                }

                var loadedAssembly = new LoadedAssembly(assembly.GetName().Name, assyVersion, assembly);
                LoadedAssemblies.Add(loadedAssembly);
                Debug.LogMessage(LogEventLevel.Information, "Loaded assembly '{0}', version {1}", loadedAssembly.Name, loadedAssembly.Version);
                return loadedAssembly;
            }
            else
            {
                RecordIssue(fileName, new PluginLoadIssue(PluginLoadIssueSeverity.Error, PluginLoadIssueCategory.LoadFailure,
                    "The assembly could not be loaded.", $"Assembly.LoadFrom returned no assembly for {filePath}.", requestedBy));
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
                var assyVersion = GetAssemblyVersion(existingAssembly);
                RecordIssue(fileName, new PluginLoadIssue(PluginLoadIssueSeverity.Warning, PluginLoadIssueCategory.DuplicateAssembly,
                    $"An assembly named {assemblyName} is already loaded, so version {assyVersion} is used instead of this file.",
                    "Two copies of the same assembly were found. Only one can be loaded, so anything that needs this file's version may fail at runtime.",
                    requestedBy));
                var loadedAssembly = new LoadedAssembly(existingAssembly.GetName().Name, assyVersion, existingAssembly);
                LoadedAssemblies.Add(loadedAssembly);
                return loadedAssembly;
            }
            
            RecordIssue(fileName, new PluginLoadIssue(PluginLoadIssueSeverity.Error, PluginLoadIssueCategory.LoadFailure,
                $"An assembly named {assemblyName} is already loaded, but it could not be found to reuse.", ex.Message, requestedBy));
            return null;
        }
        catch(Exception ex)
        {
            // Check if this might be a .NET Framework compatibility issue
            var isTypeLoadProblem = ex.Message.Contains("Could not load type") ||
                ex.Message.Contains("Unable to load one or more of the requested types");

            if (isTypeLoadProblem)
            {
                IncompatiblePlugins.Add(new IncompatiblePlugin(fileName, ex.Message, requestedBy));
            }

            RecordIssue(fileName, new PluginLoadIssue(PluginLoadIssueSeverity.Error,
                isTypeLoadProblem ? PluginLoadIssueCategory.TypeLoadFailure : PluginLoadIssueCategory.LoadFailure,
                isTypeLoadProblem
                    ? $"Could not load the assembly, likely a .NET 8 compatibility issue: {ex.Message}"
                    : $"Could not load the assembly: {ex.Message}",
                ex.ToString(), requestedBy));
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

        var issueCount = PluginLoadReportsWithIssues.Sum(r => r.Issues.Count);
        if (issueCount > 0)
        {
            CrestronConsole.ConsoleCommandResponse("Plugin load issues: {0}. Run 'pluginissues' for details." + CrestronEnvironment.NewLine, issueCount);
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
                    RecordProgramAssemblyDuplicate(pluginFile.Name, null);
                }
            }
            catch (Exception e)
            {
                RecordIssue(pluginFile.Name, new PluginLoadIssue(PluginLoadIssueSeverity.Warning, PluginLoadIssueCategory.PackageExtractFailure,
                    $"Could not move the file into the loadedAssemblies folder: {e.Message}",
                    "If an older copy is already in loadedAssemblies, that copy is loaded instead. " + e));
                continue;
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
            // Each archive is handled on its own, so one bad .cplz does not stop the others loading.
            try
            {
                Directory.CreateDirectory(TempDirectory);
                var tempDi = new DirectoryInfo(TempDirectory);

                Debug.LogMessage(LogEventLevel.Information, "Found cplz: {0}. Unzipping into temp plugins directory", zfi.FullName);
                var result = CrestronZIP.Unzip(zfi.FullName, tempDi.FullName);
                Debug.LogMessage(LogEventLevel.Information, "UnZip Result: {0}", result.ToString());

                if (result != CrestronZIP.ResultCode.ZR_OK)
                {
                    RecordIssue(zfi.Name, new PluginLoadIssue(PluginLoadIssueSeverity.Warning, PluginLoadIssueCategory.PackageExtractFailure,
                        $"Extracting the archive returned {result}.",
                        "Any files that were extracted are still loaded. Check the archive is not corrupt and re-deploy it if the plugin is missing."));
                }

                var tempFiles = tempDi.GetFiles("*.dll");

                if (tempFiles.Length == 0)
                {
                    RecordIssue(zfi.Name, new PluginLoadIssue(PluginLoadIssueSeverity.Error, PluginLoadIssueCategory.PackageExtractFailure,
                        "The archive contains no .dll files, so nothing was loaded from it."));
                }

                foreach (var tempFile in tempFiles)
                {
                    try
                    {
                        if (!CheckIfAssemblyLoaded(tempFile.Name))
                        {
                            string filePath = LoadedPluginsDirectoryPath + Global.DirectorySeparator + tempFile.Name;

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
                            RecordProgramAssemblyDuplicate(tempFile.Name, zfi.Name);
                        }
                    }
                    catch (Exception e)
                    {
                        RecordIssue(tempFile.Name, new PluginLoadIssue(PluginLoadIssueSeverity.Warning, PluginLoadIssueCategory.PackageExtractFailure,
                            $"Could not move the file out of {zfi.Name} into the loadedAssemblies folder: {e.Message}",
                            "If an older copy is already in loadedAssemblies, that copy is loaded instead. " + e, zfi.Name));
                    }
                }

                zfi.Delete();
            }
            catch (Exception e)
            {
                RecordIssue(zfi.Name, new PluginLoadIssue(PluginLoadIssueSeverity.Error, PluginLoadIssueCategory.PackageExtractFailure,
                    $"Could not extract the archive: {e.Message}", e.ToString()));
            }
            finally
            {
                try
                {
                    if (Directory.Exists(TempDirectory))
                        Directory.Delete(TempDirectory, true);
                }
                catch (Exception e)
                {
                    Debug.LogMessage(LogEventLevel.Warning, "Unable to delete temp plugins directory {0}: {1}", TempDirectory, e.Message);
                }
            }
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

        // First, read every assembly's metadata before loading any of them: check .NET 8 compatibility,
        // and compare how each was built against what is running.
        var availableAssemblies = GetAvailableAssemblyVersions();
        var assemblyCompatibility = new Dictionary<string, (bool IsCompatible, string Reason)>(StringComparer.OrdinalIgnoreCase);

        foreach (var pluginFile in pluginFiles)
        {
            string fileName = pluginFile.Name;

            lock (PluginReportsLock)
            {
                GetOrCreateReport(fileName);
            }

            try
            {
                var info = PluginAssemblyInspector.Inspect(pluginFile.FullName);

                lock (PluginReportsLock)
                {
                    var report = GetOrCreateReport(fileName);
                    report.AssemblyName = info.AssemblyName;
                    report.Version = info.DisplayVersion;

                    foreach (var pair in info.BuiltAgainst)
                    {
                        report.BuiltAgainst[pair.Key] = pair.Value;
                    }
                }

                assemblyCompatibility[fileName] = CheckNet8Compatibility(info);

                // Version mismatches are reported but never stop the plugin loading.
                foreach (var issue in PluginVersionChecker.Check(info, Global.AssemblyVersion, availableAssemblies))
                {
                    RecordIssue(fileName, issue);
                }
            }
            catch (BadImageFormatException)
            {
                assemblyCompatibility[fileName] = (false, "Not a valid .NET assembly");
            }
            catch (Exception ex)
            {
                assemblyCompatibility[fileName] = (false, $"Error analyzing assembly: {ex.Message}");
            }
        }
        
        // Now load compatible assemblies and track incompatible ones
        foreach (var pluginFile in pluginFiles)
        {
            string fileName = pluginFile.Name;
            var (isCompatible, reason) = assemblyCompatibility[fileName];
            
            if (!isCompatible)
            {
                IncompatiblePlugins.Add(new IncompatiblePlugin(fileName, reason, null));
                RecordIssue(fileName, new PluginLoadIssue(PluginLoadIssueSeverity.Error,
                    reason.StartsWith("Uses incompatible type", StringComparison.Ordinal)
                        ? PluginLoadIssueCategory.IncompatibleApi
                        : PluginLoadIssueCategory.LoadFailure,
                    $"Not compatible with .NET 8: {reason}",
                    "The assembly was not loaded."));
                continue;
            }
            
            // Try to load the assembly
            var loadedAssembly = LoadAssembly(pluginFile.FullName, null);
            
            if (loadedAssembly != null)
            {
                LoadedPluginFolderAssemblies.Add(loadedAssembly);

                lock (PluginReportsLock)
                {
                    PluginFileNames[loadedAssembly] = fileName;
                }
            }
        }

        Debug.LogMessage(LogEventLevel.Information, "All Plugins Loaded.");
    }

    /// <summary>
    /// Collects the assembly versions a plugin will actually run against, by simple name. Assemblies
    /// already in the AppDomain win, then the versions Essentials itself was compiled against (which
    /// covers Crestron SDK assemblies that have not been loaded yet), then the program directory.
    /// </summary>
    private static Dictionary<string, Version> GetAvailableAssemblyVersions()
    {
        var versions = new Dictionary<string, Version>(StringComparer.OrdinalIgnoreCase);

        void AddIfMissing(string name, Version version)
        {
            if (!string.IsNullOrEmpty(name) && version != null && !versions.ContainsKey(name))
                versions[name] = version;
        }

        var domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in domainAssemblies)
        {
            try
            {
                var name = assembly.GetName();
                AddIfMissing(name.Name, name.Version);
            }
            catch (Exception ex)
            {
                Debug.LogMessage(LogEventLevel.Verbose, "Unable to read the name of a loaded assembly: {0}", ex.Message);
            }
        }

        foreach (var assembly in domainAssemblies)
        {
            try
            {
                if (!PluginVersionChecker.EssentialsAssemblyNames.Contains(assembly.GetName().Name))
                    continue;

                foreach (var reference in assembly.GetReferencedAssemblies())
                {
                    AddIfMissing(reference.Name, reference.Version);
                }
            }
            catch (Exception ex)
            {
                Debug.LogMessage(LogEventLevel.Verbose, "Unable to read the references of a loaded assembly: {0}", ex.Message);
            }
        }

        try
        {
            foreach (var file in Directory.GetFiles(Global.ApplicationDirectoryPathPrefix, "*.dll"))
            {
                try
                {
                    var name = AssemblyName.GetAssemblyName(file);
                    AddIfMissing(name.Name, name.Version);
                }
                catch (Exception)
                {
                    // Native libraries in the program folder have no assembly name
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogMessage(LogEventLevel.Verbose, "Unable to read program directory assemblies: {0}", ex.Message);
        }

        return versions;
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

            var pluginFileName = FileNameFor(loadedAssembly);
            
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
                    MarkTypesFailedToLoad(pluginFileName);

                    var otherLoaderErrors = new List<string>();

                    // Check if any of the loader exceptions are due to missing assemblies
                    foreach (var loaderEx in e.LoaderExceptions.Where(x => x != null))
                    {
                        if (loaderEx is FileNotFoundException fileNotFoundEx && !string.IsNullOrEmpty(fileNotFoundEx.FileName))
                        {
                            string missingAssembly = fileNotFoundEx.FileName;

                            // Add to incompatible plugins with dependency information
                            IncompatiblePlugins.Add(new IncompatiblePlugin(
                                Path.GetFileName(missingAssembly), 
                                $"Missing dependency required by {loadedAssembly.Name}", 
                                loadedAssembly.Name));

                            RecordIssue(pluginFileName, new PluginLoadIssue(PluginLoadIssueSeverity.Error, PluginLoadIssueCategory.MissingDependency,
                                $"Missing dependency: {missingAssembly}",
                                "Deploy the missing assembly alongside the plugin, or include it in the plugin's .cplz. " + fileNotFoundEx.Message));
                        }
                        else
                        {
                            otherLoaderErrors.Add(loaderEx.Message);
                        }
                    }

                    if (otherLoaderErrors.Count > 0 || e.LoaderExceptions.All(x => x == null))
                    {
                        RecordIssue(pluginFileName, new PluginLoadIssue(PluginLoadIssueSeverity.Error, PluginLoadIssueCategory.TypeLoadFailure,
                            $"Could not read the assembly's types: {e.Message}",
                            string.Join(Environment.NewLine, otherLoaderErrors.Distinct())));
                    }

                    continue;
                }
                catch (TypeLoadException e)
                {
                    MarkTypesFailedToLoad(pluginFileName);

                    RecordIssue(pluginFileName, new PluginLoadIssue(PluginLoadIssueSeverity.Error, PluginLoadIssueCategory.TypeLoadFailure,
                        $"Could not read the assembly's types: {e.Message}", e.ToString()));
                    
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
                        if (!typeof(IPluginDeviceFactory).IsAssignableFrom(type) || type.IsAbstract)
                        {
                            continue;
                        }

                        lock (PluginReportsLock)
                        {
                            GetOrCreateReport(pluginFileName).FactoryTypesFound++;
                        }

                        var plugin = (IPluginDeviceFactory)Activator.CreateInstance(type);
                        LoadCustomPlugin(plugin, loadedAssembly);
                    }
                    catch (NotSupportedException ex)
                    {
                        // This happens for dlls that aren't PD dlls, like ports of Mono classes into S#.
                        RecordIssue(pluginFileName, new PluginLoadIssue(PluginLoadIssueSeverity.Warning, PluginLoadIssueCategory.FactoryFailure,
                            $"Plugin factory {type.FullName} is not supported on this platform: {ex.Message}", ex.ToString()));
                    }
                    catch (Exception ex)
                    {
                        // Activator.CreateInstance wraps anything the factory's constructor throws, so report the
                        // plugin's own exception rather than "Exception has been thrown by the target of an invocation"
                        var cause = ex is TargetInvocationException { InnerException: not null } wrapped ? wrapped.InnerException : ex;

                        RecordIssue(pluginFileName, new PluginLoadIssue(PluginLoadIssueSeverity.Error, PluginLoadIssueCategory.FactoryFailure,
                            $"Plugin factory {type.FullName} could not be loaded: {cause.GetType().Name}: {cause.Message}", ex.ToString()));
                        continue;
                    }
                }
            }
            catch (Exception e)
            {
                RecordIssue(pluginFileName, new PluginLoadIssue(PluginLoadIssueSeverity.Error, PluginLoadIssueCategory.LoadFailure,
                    $"Error loading the assembly's plugin types: {e.Message}", e.ToString()));
                
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
    /// <param name="deviceFactory">The plugin to be loaded, implementing the <see cref="IPluginDeviceFactory"/> interface.  If the plugin also
    /// implements <see cref="IPluginDevelopmentDeviceFactory"/>, additional checks for development versions are
    /// performed.</param>
    /// <param name="loadedAssembly">The assembly associated with the plugin being loaded. This is used for logging and tracking purposes.</param>
    private static void LoadCustomPlugin(IPluginDeviceFactory deviceFactory, LoadedAssembly loadedAssembly)
    {
        var developmentDeviceFactory = deviceFactory as IPluginDevelopmentDeviceFactory;

        var passed = developmentDeviceFactory != null ? Global.IsRunningDevelopmentVersion
            (developmentDeviceFactory.DevelopmentEssentialsFrameworkVersions, developmentDeviceFactory.MinimumEssentialsFrameworkVersion)
            : Global.IsRunningMinimumVersionOrHigher(deviceFactory.MinimumEssentialsFrameworkVersion);

        if (!passed)
        {
            RecordIssue(FileNameFor(loadedAssembly), PluginVersionChecker.DescribeFactoryRejection(
                deviceFactory.GetType().FullName,
                deviceFactory.MinimumEssentialsFrameworkVersion,
                developmentDeviceFactory?.DevelopmentEssentialsFrameworkVersions,
                Global.AssemblyVersion));
            return;
        }
        else
        {
            Debug.LogInformation("Passed plugin passed dependency check (required version {essentialsMinimumVersion})", deviceFactory.MinimumEssentialsFrameworkVersion);
        }

        Debug.LogInformation("Loading plugin: {pluginName}", loadedAssembly.Name);        

        LoadDeviceFactories(deviceFactory);

        if(!EssentialsPluginAssemblies.Contains(loadedAssembly))
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
            //Debug.LogMessage(LogEventLevel.Verbose, "Getting Description Attribute from class: '{0}'", typeof(T).FullName);
            var descriptionAttribute = deviceFactory.FactoryType.GetCustomAttributes(typeof(DescriptionAttribute), true) as DescriptionAttribute[];
            string description = descriptionAttribute[0].Description;
            var snippetAttribute = deviceFactory.FactoryType.GetCustomAttributes(typeof(ConfigSnippetAttribute), true) as ConfigSnippetAttribute[];
            DeviceFactory.AddFactoryForType(typeName.ToLower(), description, deviceFactory.FactoryType, deviceFactory.BuildDevice);
        }
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

            FinalizePluginStatuses();

            // Each issue was logged as it was found; this points at where to read them all together.
            var withIssues = PluginLoadReportsWithIssues;
            if (withIssues.Count > 0)
            {
                Debug.LogMessage(LogEventLevel.Warning,
                    "Plugin loading finished with {issueCount} issue(s) ({errorCount} error(s)) in {fileCount} file(s). Run 'pluginissues' at the console or GET /api/pluginLoadIssues for details.",
                    withIssues.Sum(r => r.Issues.Count),
                    withIssues.Sum(r => r.Issues.Count(i => i.Severity == PluginLoadIssueSeverity.Error)),
                    withIssues.Count);
            }
        }
    }

    /// <summary>
    /// Records an issue against a plugin file and logs it at the issue's severity.
    /// </summary>
    private static void RecordIssue(string fileName, PluginLoadIssue issue)
    {
        lock (PluginReportsLock)
        {
            GetOrCreateReport(fileName).AddIssue(issue);
        }

        var level = issue.Severity == PluginLoadIssueSeverity.Error ? LogEventLevel.Error : LogEventLevel.Warning;

        if (issue.TriggeredBy != "Direct load")
        {
            Debug.LogMessage(level, "Plugin {pluginFile}: [{category}] {message} (required by {triggeredBy})",
                fileName, issue.Category, issue.Message, issue.TriggeredBy);
        }
        else
        {
            Debug.LogMessage(level, "Plugin {pluginFile}: [{category}] {message}", fileName, issue.Category, issue.Message);
        }

        if (!string.IsNullOrEmpty(issue.Detail))
        {
            Debug.LogMessage(LogEventLevel.Debug, "Plugin {pluginFile} detail: {detail}", fileName, issue.Detail);
        }
    }

    /// <summary>
    /// Returns the report for a file, creating it if needed. Call while holding <see cref="PluginReportsLock"/>.
    /// </summary>
    private static PluginLoadReport GetOrCreateReport(string fileName)
    {
        if (!PluginReports.TryGetValue(fileName, out var report))
        {
            report = new PluginLoadReport(fileName);
            PluginReports[fileName] = report;
        }

        return report;
    }

    /// <summary>
    /// The plugin file an assembly was loaded from, falling back to its assembly name.
    /// </summary>
    private static string FileNameFor(LoadedAssembly loadedAssembly)
    {
        lock (PluginReportsLock)
        {
            return PluginFileNames.TryGetValue(loadedAssembly, out var fileName)
                ? fileName
                : loadedAssembly.Name + ".dll";
        }
    }

    /// <summary>
    /// Records a plugin file that was not used because the running program already provides an
    /// assembly of that name, e.g. a plugin that bundles its own PepperDashCore.dll.
    /// </summary>
    /// <param name="fileName">The skipped file.</param>
    /// <param name="archiveName">The .cplz it came from, or null for a DLL dropped straight into the plugins folder.</param>
    private static void RecordProgramAssemblyDuplicate(string fileName, string archiveName)
    {
        var programCopy = LoadedAssemblies.FirstOrDefault(a => a.Name.Equals(fileName));
        var version = string.IsNullOrEmpty(programCopy?.Version) ? "the program's copy" : $"version {programCopy.Version}";

        RecordIssue(fileName, new PluginLoadIssue(PluginLoadIssueSeverity.Warning, PluginLoadIssueCategory.DuplicateAssembly,
            archiveName != null
                ? $"The copy bundled in {archiveName} was ignored because the program already provides {fileName} ({version})."
                : $"Ignored because the program already provides {fileName} ({version}).",
            "Plugins should reference Essentials with ExcludeAssets=runtime rather than ship its assemblies. The plugin runs against the program's copy, so a bundled copy from a different build can cause MissingMethodException or TypeLoadException.",
            archiveName));

        lock (PluginReportsLock)
        {
            GetOrCreateReport(fileName).Status = PluginLoadStatus.Skipped;
        }
    }

    private static void MarkTypesFailedToLoad(string fileName)
    {
        lock (PluginReportsLock)
        {
            GetOrCreateReport(fileName).TypesFailedToLoad = true;
        }
    }

    /// <summary>
    /// Sets each plugin file's final status once every load stage has run.
    /// </summary>
    private static void FinalizePluginStatuses()
    {
        lock (PluginReportsLock)
        {
            foreach (var pair in PluginFileNames)
            {
                var report = GetOrCreateReport(pair.Value);

                if (EssentialsPluginAssemblies.Contains(pair.Key))
                    report.Status = PluginLoadStatus.Loaded;
                else if (report.TypesFailedToLoad)
                    report.Status = PluginLoadStatus.Failed;
                else if (report.FactoryTypesFound > 0)
                    report.Status = PluginLoadStatus.Skipped;
                else
                    report.Status = PluginLoadStatus.LoadedAsLibrary;
            }

            // Anything never loaded into the AppDomain failed somewhere earlier
            foreach (var report in PluginReports.Values.Where(r => r.Status == PluginLoadStatus.Pending))
            {
                report.Status = PluginLoadStatus.Failed;
            }
        }
    }
}
