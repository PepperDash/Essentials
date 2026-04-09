using System;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.Diagnostics;
using PepperDash.Core;
using PepperDash.Core.Abstractions;
using PepperDash.Core.Adapters;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Routing;
using System.Threading;
using Timeout = Crestron.SimplSharp.Timeout;
using Serilog.Events;
using System.Threading.Tasks;
using PepperDash.Essentials.Core.Web;

namespace PepperDash.Essentials;

/// <summary>
/// Represents the main control system for the application, providing initialization, configuration loading, and device
/// management functionality.
/// </summary>
/// <remarks>This class extends <see cref="CrestronControlSystem"/> and serves as the entry point for the control
/// system. It manages the initialization of devices, rooms, tie lines, and other system components. Additionally, it
/// provides methods for platform determination, configuration loading, and system teardown.</remarks>
public class ControlSystem : CrestronControlSystem, ILoadConfig
{
    private Timer startTimer;
    private ManualResetEventSlim initializeEvent;
    private const long StartupTime = 500;

    /// <summary>
    /// Initializes a new instance of the <see cref="ControlSystem"/> class, setting up the system's global state and
    /// dependencies.
    /// </summary>
    /// <remarks>This constructor configures the control system by initializing key components such as the
    /// device manager and secrets manager,  and sets global properties like the maximum number of user threads and the
    /// program initialization state.  It also adjusts the error log's minimum debug level based on the device
    /// platform.</remarks>
    public ControlSystem()
        : base()

    {
        try
        {
            // Register Crestron service adapters BEFORE the first reference to Debug,
            // so that Debug's static constructor uses these implementations instead of
            // calling the Crestron SDK statics directly.
            DebugServiceRegistration.Register(
                new CrestronEnvironmentAdapter(),
                new CrestronConsoleAdapter(),
                new CrestronDataStoreAdapter());

            Crestron.SimplSharpPro.CrestronThread.Thread.MaxNumberOfUserThreads = 400;

            Global.ControlSystem = this;
            DeviceManager.Initialize(this);
            SecretsManager.Initialize();
            SystemMonitor.ProgramInitialization.ProgramInitializationUnderUserControl = true;

            Debug.SetErrorLogMinimumDebugLevel(CrestronEnvironment.DevicePlatform == eDevicePlatform.Appliance ? LogEventLevel.Warning : LogEventLevel.Verbose);
        }
        catch (Exception e)
        {
            try
            {
                Debug.LogError(e, "FATAL INITIALIZE ERROR. System is in an inconsistent state");
            }
            catch
            {
                // Debug may not be initialized (e.g. its own static ctor failed); fall back to console.
                CrestronConsole.PrintLine($"FATAL INITIALIZE ERROR. System is in an inconsistent state\r\n{e.Message}\r\n{e.StackTrace}");
            }
        }
    }

    /// <summary>
    /// Initializes the control system and prepares it for operation.
    /// </summary>
    /// <remarks>This method ensures that all devices in the system are properly registered and initialized
    /// before the system is fully operational.  If the control system is of a DMPS type, the method waits for all
    /// devices to activate, allowing HD-BaseT DM endpoints to register  before completing initialization. For non-DMPS
    /// systems, initialization proceeds without waiting.</remarks>
    public override void InitializeSystem()
    {
        // If the control system is a DMPS type, we need to wait to exit this method until all devices have had time to activate
        // to allow any HD-BaseT DM endpoints to register first.
        bool preventInitializationComplete = Global.ControlSystemIsDmpsType;
        if (preventInitializationComplete)
        {
            Debug.LogMessage(LogEventLevel.Debug, "******************* Initializing System **********************");

            startTimer = new Timer(StartSystem, preventInitializationComplete, StartupTime, Timeout.Infinite);

            initializeEvent = new ManualResetEventSlim(false);

            DeviceManager.AllDevicesRegistered += (o, a) =>
            {
                initializeEvent.Set();
            };

            initializeEvent.Wait(30000);

            Debug.LogMessage(LogEventLevel.Debug, "******************* System Initialization Complete **********************");

            SystemMonitor.ProgramInitialization.ProgramInitializationComplete = true;
        }
        else
        {
            startTimer = new Timer(StartSystem, preventInitializationComplete, StartupTime, Timeout.Infinite);
        }
    }

    private void StartSystem(object preventInitialization)
    {
        DeterminePlatform();

        // Print .NET runtime version
        Debug.LogMessage(LogEventLevel.Information, "Running on .NET runtime version: {0}", System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);

        if (Debug.DoNotLoadConfigOnNextBoot)
        {
            CrestronConsole.AddNewConsoleCommand(s => Task.Run(() => GoWithLoad()), "go", "Loads configuration file",
                ConsoleAccessLevelEnum.AccessOperator);
        }

        CrestronConsole.AddNewConsoleCommand(PluginLoader.ReportAssemblyVersions, "reportversions", "Reports the versions of the loaded assemblies", ConsoleAccessLevelEnum.AccessOperator);

        CrestronConsole.AddNewConsoleCommand(Core.DeviceFactory.GetDeviceFactoryTypes, "gettypes", "Gets the device types that can be built. Accepts a filter string.", ConsoleAccessLevelEnum.AccessOperator);

        CrestronConsole.AddNewConsoleCommand(BridgeHelper.PrintJoinMap, "getjoinmap", "map(s) for bridge or device on bridge [brKey [devKey]]", ConsoleAccessLevelEnum.AccessOperator);

        CrestronConsole.AddNewConsoleCommand(BridgeHelper.JoinmapMarkdown, "getjoinmapmarkdown"
            , "generate markdown of map(s) for bridge or device on bridge [brKey [devKey]]", ConsoleAccessLevelEnum.AccessOperator);

        CrestronConsole.AddNewConsoleCommand(s => Debug.LogMessage(LogEventLevel.Information, "CONSOLE MESSAGE: {0}", s), "appdebugmessage", "Writes message to log", ConsoleAccessLevelEnum.AccessOperator);

        CrestronConsole.AddNewConsoleCommand(s =>
        {
            foreach (var tl in TieLineCollection.Default)
                CrestronConsole.ConsoleCommandResponse("  {0}{1}", tl, CrestronEnvironment.NewLine);
        },
        "listtielines", "Prints out all tie lines", ConsoleAccessLevelEnum.AccessOperator);

        CrestronConsole.AddNewConsoleCommand(s =>
        {
            CrestronConsole.ConsoleCommandResponse
                ("Current running configuration. This is the merged system and template configuration" + CrestronEnvironment.NewLine);
            CrestronConsole.ConsoleCommandResponse(Newtonsoft.Json.JsonConvert.SerializeObject
                (ConfigReader.ConfigObject, Newtonsoft.Json.Formatting.Indented));
        }, "showconfig", "Shows the current running merged config", ConsoleAccessLevelEnum.AccessOperator);

        CrestronConsole.AddNewConsoleCommand(s =>
            CrestronConsole.ConsoleCommandResponse(
            "This system can be found at the following URLs:{2}" +
            "System URL:   {0}{2}" +
            "Template URL: {1}{2}",
            ConfigReader.ConfigObject.SystemUrl,
            ConfigReader.ConfigObject.TemplateUrl,
            CrestronEnvironment.NewLine),
            "portalinfo",
            "Shows portal URLS from configuration",
            ConsoleAccessLevelEnum.AccessOperator);


        CrestronConsole.AddNewConsoleCommand(DeviceManager.GetRoutingPorts,
            "getroutingports", "Reports all routing ports, if any.  Requires a device key", ConsoleAccessLevelEnum.AccessOperator);

        DeviceManager.AddDevice(new EssentialsWebApi("essentialsWebApi", "Essentials Web API"));

        if (!Debug.DoNotLoadConfigOnNextBoot)
        {
            GoWithLoad();
            return;
        }

        if (!(bool)preventInitialization)
        {
            SystemMonitor.ProgramInitialization.ProgramInitializationComplete = true;
        }
    }

    /// <summary>
    /// Determines if the program is running on a processor (appliance) or server (VC-4).
    /// 
    /// Sets Global.FilePathPrefix and Global.ApplicationDirectoryPathPrefix based on platform
    /// </summary>
    public void DeterminePlatform()
    {
        try
        {
            Debug.LogMessage(LogEventLevel.Information, "Determining Platform...");

            string filePathPrefix;

            var dirSeparator = Global.DirectorySeparator;

            string directoryPrefix;

            directoryPrefix = Directory.GetApplicationRootDirectory();

            Global.SetAssemblyVersion(PluginLoader.GetAssemblyVersion(Assembly.GetExecutingAssembly()));

            if (CrestronEnvironment.DevicePlatform != eDevicePlatform.Server)   // Handles 3-series running Windows CE OS
            {
                string userFolder = "user";
                string nvramFolder = "nvram";

                Debug.LogMessage(LogEventLevel.Information, "Starting Essentials v{version:l} on {processorSeries:l} Appliance", Global.AssemblyVersion, "4-series");
                //Debug.LogMessage(LogEventLevel.Information, "Starting Essentials v{0} on {1} Appliance", Global.AssemblyVersion, is4series ? "4-series" : "3-series");

                // Check if User/ProgramX exists
                if (Directory.Exists(Global.ApplicationDirectoryPathPrefix + dirSeparator + userFolder
                    + dirSeparator + string.Format("program{0}", InitialParametersClass.ApplicationNumber)))
                {

                    Debug.LogMessage(LogEventLevel.Information, "{userFolder:l}/program{applicationNumber} directory found", userFolder, InitialParametersClass.ApplicationNumber);
                    filePathPrefix = directoryPrefix + dirSeparator + userFolder
                    + dirSeparator + string.Format("program{0}", InitialParametersClass.ApplicationNumber) + dirSeparator;
                }
                // Check if Nvram/Programx exists
                else if (Directory.Exists(directoryPrefix + dirSeparator + nvramFolder
                    + dirSeparator + string.Format("program{0}", InitialParametersClass.ApplicationNumber)))
                {
                    Debug.LogMessage(LogEventLevel.Information, "{nvramFolder:l}/program{applicationNumber} directory found", nvramFolder, InitialParametersClass.ApplicationNumber);

                    filePathPrefix = directoryPrefix + dirSeparator + nvramFolder
                    + dirSeparator + string.Format("program{0}", InitialParametersClass.ApplicationNumber) + dirSeparator;
                }
                // If neither exists, set path to User/ProgramX
                else
                {
                    Debug.LogMessage(LogEventLevel.Information, "{userFolder:l}/program{applicationNumber} directory found", userFolder, InitialParametersClass.ApplicationNumber);

                    filePathPrefix = directoryPrefix + dirSeparator + userFolder
                    + dirSeparator + string.Format("program{0}", InitialParametersClass.ApplicationNumber) + dirSeparator;
                }
            }
            else   // Handles Linux OS (Virtual Control)
            {
                //Debug.SetDebugLevel(2);
                Debug.LogMessage(LogEventLevel.Information, "Starting Essentials v{version:l} on Virtual Control Server", Global.AssemblyVersion);

                // Set path to User/
                filePathPrefix = directoryPrefix + dirSeparator + "User" + dirSeparator;
            }

            Global.SetFilePathPrefix(filePathPrefix);
        }
        catch (Exception e)
        {
            Debug.LogMessage(e, "Unable to determine platform due to exception");
        }
    }

    /// <summary>
    /// Begins the process of loading resources including plugins and configuration data
    /// </summary>
    public void GoWithLoad()
    {
        try
        {
            Debug.SetDoNotLoadConfigOnNextBoot(false);

            PluginLoader.AddProgramAssemblies();

            _ = new Core.DeviceFactory();

            LoadAssets(Global.ApplicationDirectoryPathPrefix, Global.FilePathPrefix);

            Debug.LogMessage(LogEventLevel.Information, "Starting Essentials load from configuration");

            var filesReady = SetupFilesystem();
            if (filesReady)
            {
                Debug.LogMessage(LogEventLevel.Information, "Checking for plugins");
                PluginLoader.LoadPlugins();

                Debug.LogMessage(LogEventLevel.Information, "Folder structure verified. Loading config...");
                if (!ConfigReader.LoadConfig2() || ConfigReader.ConfigObject == null)
                {
                    Debug.LogMessage(LogEventLevel.Warning, "Unable to load config file. Please ensure a valid config file is present and restart the program.");
                    // return;
                }

                Load();
                Debug.LogMessage(LogEventLevel.Information, "Essentials load complete");
            }
            else
            {
                Debug.LogMessage(LogEventLevel.Information,
                    @"----------------------------------------------
                        ------------------------------------------------
                        ------------------------------------------------
                        Essentials file structure setup completed.
                        Please load config, sgd and ir files and
                        restart program.
                        ------------------------------------------------
                        ------------------------------------------------
                        ------------------------------------------------");
            }

        }
        catch (Exception e)
        {
            Debug.LogMessage(e, "FATAL INITIALIZE ERROR. System is in an inconsistent state");
        }
        finally
        {
            // Notify the OS that the program intitialization has completed
            SystemMonitor.ProgramInitialization.ProgramInitializationComplete = true;
        }

    }



    /// <summary>
    /// Verifies filesystem is set up. IR, SGD, and programX folders
    /// </summary>
    bool SetupFilesystem()
    {
        Debug.LogMessage(LogEventLevel.Information, "Verifying and/or creating folder structure");
        var configDir = Global.FilePathPrefix;

        Debug.LogMessage(LogEventLevel.Information, "FilePathPrefix: {filePathPrefix:l}", configDir);
        var configExists = Directory.Exists(configDir);
        if (!configExists)
            Directory.Create(configDir);

        var irDir = Global.FilePathPrefix + "ir";
        if (!Directory.Exists(irDir))
            Directory.Create(irDir);

        var sgdDir = Global.FilePathPrefix + "sgd";
        if (!Directory.Exists(sgdDir))
            Directory.Create(sgdDir);

        var pluginDir = Global.FilePathPrefix + "plugins";
        if (!Directory.Exists(pluginDir))
            Directory.Create(pluginDir);

        var joinmapDir = Global.FilePathPrefix + "joinmaps";
        if (!Directory.Exists(joinmapDir))
            Directory.Create(joinmapDir);

        return configExists;
    }

    /// <summary>
    /// TearDown method
    /// </summary>
    public void TearDown()
    {
        Debug.LogMessage(LogEventLevel.Information, "Tearing down existing system");
        DeviceManager.DeactivateAll();

        TieLineCollection.Default.Clear();

        foreach (var key in DeviceManager.GetDevices())
            DeviceManager.RemoveDevice(key);

        Debug.LogMessage(LogEventLevel.Information, "Tear down COMPLETE");
    }


    /// <summary>
    /// Load method
    /// </summary>
    void Load()
    {
        LoadDevices();
        LoadRooms();

        DeviceManager.ActivateAll();

        LoadTieLines();

        /*var mobileControl = GetMobileControlDevice();

		    if (mobileControl == null) return;

        mobileControl.LinkSystemMonitorToAppServer();*/

    }

    /// <summary>
    /// Reads all devices from config and adds them to DeviceManager
    /// </summary>
    public void LoadDevices()
    {

        // Build the processor wrapper class
        DeviceManager.AddDevice(new Core.Devices.CrestronProcessor("processor"));

        DeviceManager.AddDevice(new RoutingFeedbackManager($"routingFeedbackManager", "Routing Feedback Manager"));

        // Add global System Monitor device
        if (CrestronEnvironment.DevicePlatform == eDevicePlatform.Appliance)
        {
            DeviceManager.AddDevice(
                new Core.Monitoring.SystemMonitorController("systemMonitor"));
        }

        if (ConfigReader.ConfigObject is not null)
        {
            Debug.LogMessage(LogEventLevel.Warning, "LoadDevices: ConfigObject is null. Cannot load devices.");

            foreach (var devConf in ConfigReader.ConfigObject.Devices)
            {
                IKeyed newDev = null;

                try
                {
                    Debug.LogMessage(LogEventLevel.Information, "Creating device '{deviceKey:l}', type '{deviceType:l}'", devConf.Key, devConf.Type);
                    // Skip this to prevent unnecessary warnings
                    if (devConf.Key == "processor")
                    {
                        var prompt = Global.ControlSystem.ControllerPrompt;

                        var typeMatch = string.Equals(devConf.Type, prompt, StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(devConf.Type, prompt.Replace("-", ""), StringComparison.OrdinalIgnoreCase);

                        if (!typeMatch)
                            Debug.LogMessage(LogEventLevel.Information,
                                "WARNING: Config file defines processor type as '{deviceType:l}' but actual processor is '{processorType:l}'!  Some ports may not be available",
                                devConf.Type.ToUpper(), Global.ControlSystem.ControllerPrompt.ToUpper());


                        continue;
                    }


                    if (newDev == null)
                        newDev = Core.DeviceFactory.GetDevice(devConf);

                    if (newDev != null)
                        DeviceManager.AddDevice(newDev);
                    else
                        Debug.LogMessage(LogEventLevel.Information, "ERROR: Cannot load unknown device type '{deviceType:l}', key '{deviceKey:l}'.", devConf.Type, devConf.Key);
                }
                catch (Exception e)
                {
                    Debug.LogMessage(e, "ERROR: Creating device {deviceKey:l}. Skipping device.", args: new[] { devConf.Key });
                }
            }

        }
        Debug.LogMessage(LogEventLevel.Information, "All Devices Loaded.");

    }


    /// <summary>
    /// Helper method to load tie lines.  This should run after devices have loaded
    /// </summary>
    public void LoadTieLines()
    {
        // In the future, we can't necessarily just clear here because devices
        // might be making their own internal sources/tie lines

        var tlc = TieLineCollection.Default;

        if (ConfigReader.ConfigObject?.TieLines == null)
        {
            return;
        }

        foreach (var tieLineConfig in ConfigReader.ConfigObject.TieLines)
        {
            var newTL = tieLineConfig.GetTieLine();
            if (newTL != null)
                tlc.Add(newTL);
        }

        Debug.LogMessage(LogEventLevel.Information, "All Tie Lines Loaded.");

    }

    /// <summary>
    /// Reads all rooms from config and adds them to DeviceManager
    /// </summary>
    public void LoadRooms()
    {
        if (ConfigReader.ConfigObject?.Rooms == null)
        {
            Debug.LogMessage(LogEventLevel.Information, "Notice: Configuration contains no rooms - Is this intentional?  This may be a valid configuration.");
            return;
        }

        foreach (var roomConfig in ConfigReader.ConfigObject.Rooms)
        {
            try
            {
                var room = Core.DeviceFactory.GetDevice(roomConfig);

                if (room == null)
                {
                    Debug.LogWarning("ERROR: Cannot load unknown room type '{roomType:l}', key '{roomKey:l}'.", roomConfig.Type, roomConfig.Key);
                    continue;
                }

                DeviceManager.AddDevice(room);
            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Exception loading room {roomKey}:{roomType}", null, roomConfig.Key, roomConfig.Type);
                continue;
            }
        }

        Debug.LogMessage(LogEventLevel.Information, "All Rooms Loaded.");


    }


    internal static void LoadAssets(string applicationDirectoryPath, string filePathPrefix) =>
        AssetLoader.Load(applicationDirectoryPath, filePathPrefix);
}

