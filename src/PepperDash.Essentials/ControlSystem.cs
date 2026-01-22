using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.Diagnostics;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Core.Web;
using Serilog.Events;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Main control system class that inherits from CrestronControlSystem and manages program lifecycle
    /// </summary>
    public class ControlSystem : CrestronControlSystem, ILoadConfig
    {
        HttpLogoServer LogoServer;

        private CTimer _startTimer;
        private CEvent _initializeEvent;
        private const long StartupTime = 500;

        /// <summary>
        /// Initializes a new instance of the ControlSystem class
        /// </summary>
        public ControlSystem()
            : base()
        {
            Thread.MaxNumberOfUserThreads = 400;
            Global.ControlSystem = this;
            DeviceManager.Initialize(this);
            SecretsManager.Initialize();
            SystemMonitor.ProgramInitialization.ProgramInitializationUnderUserControl = true;
        }

        /// <summary>
        /// InitializeSystem method
        /// </summary>
        /// <inheritdoc />
        public override void InitializeSystem()
        {
            // If the control system is a DMPS type, we need to wait to exit this method until all devices have had time to activate
            // to allow any HD-BaseT DM endpoints to register first.
            bool preventInitializationComplete = Global.ControlSystemIsDmpsType;
            if (preventInitializationComplete)
            {
                Debug.LogMessage(LogEventLevel.Debug, "******************* InitializeSystem() Entering **********************");

                _startTimer = new CTimer(StartSystem, preventInitializationComplete, StartupTime);
                _initializeEvent = new CEvent(true, false);
                DeviceManager.AllDevicesRegistered += (o, a) =>
                {
                    _initializeEvent.Set();
                };
                _initializeEvent.Wait(30000);
                Debug.LogMessage(LogEventLevel.Debug, "******************* InitializeSystem() Exiting **********************");

                SystemMonitor.ProgramInitialization.ProgramInitializationComplete = true;
            }
            else
            {
                _startTimer = new CTimer(StartSystem, preventInitializationComplete, StartupTime);
            }
        }

        private void StartSystem(object preventInitialization)
        {
            DeterminePlatform();

            if (Debug.DoNotLoadConfigOnNextBoot)
            {
                CrestronConsole.AddNewConsoleCommand(s => CrestronInvoke.BeginInvoke((o) => GoWithLoad()), "go", "Loads configuration file",
                    ConsoleAccessLevelEnum.AccessOperator);
            }

            CrestronConsole.AddNewConsoleCommand(PluginLoader.ReportAssemblyVersions, "reportversions", "Reports the versions of the loaded assemblies", ConsoleAccessLevelEnum.AccessOperator);

            CrestronConsole.AddNewConsoleCommand(Core.DeviceFactory.GetDeviceFactoryTypes, "gettypes", "Gets the device types that can be built. Accepts a filter string.", ConsoleAccessLevelEnum.AccessOperator);

            CrestronConsole.AddNewConsoleCommand(BridgeHelper.PrintJoinMap, "getjoinmap", "map(s) for bridge or device on bridge [brKey [devKey]]", ConsoleAccessLevelEnum.AccessOperator);

            CrestronConsole.AddNewConsoleCommand(BridgeHelper.JoinmapMarkdown, "getjoinmapmarkdown"
                , "generate markdown of map(s) for bridge or device on bridge [brKey [devKey]]", ConsoleAccessLevelEnum.AccessOperator);

            CrestronConsole.AddNewConsoleCommand(s => Debug.LogMessage(LogEventLevel.Information, "CONSOLE MESSAGE: {0}", s), "appdebugmessage", "Writes message to log", ConsoleAccessLevelEnum.AccessOperator);

            CrestronConsole.AddNewConsoleCommand(ListTieLines,
            "listtielines", "Prints out all tie lines. Usage: listtielines [signaltype]", ConsoleAccessLevelEnum.AccessOperator);

            CrestronConsole.AddNewConsoleCommand(VisualizeRoutes, "visualizeroutes",
                "Visualizes routes by signal type",
                ConsoleAccessLevelEnum.AccessOperator);

            CrestronConsole.AddNewConsoleCommand(VisualizeCurrentRoutes, "visualizecurrentroutes",
                "Visualizes current active routes from DefaultCollection",
                ConsoleAccessLevelEnum.AccessOperator);

            CrestronConsole.AddNewConsoleCommand(s =>
            {
                CrestronConsole.ConsoleCommandResponse
                    ("Current running configuration. This is the merged system and template configuration" + CrestronEnvironment.NewLine);
                CrestronConsole.ConsoleCommandResponse(Newtonsoft.Json.JsonConvert.SerializeObject
                    (ConfigReader.ConfigObject, Newtonsoft.Json.Formatting.Indented).Replace(Environment.NewLine, "\r\n"));
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
        /// DeterminePlatform method
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
                    string userFolder;
                    string nvramFolder;
                    bool is4series = false;

                    if (eCrestronSeries.Series4 == (Global.ProcessorSeries & eCrestronSeries.Series4)) // Handle 4-series
                    {
                        is4series = true;
                        // Set path to user/
                        userFolder = "user";
                        nvramFolder = "nvram";
                    }
                    else
                    {
                        userFolder = "User";
                        nvramFolder = "Nvram";
                    }

                    Debug.LogMessage(LogEventLevel.Information, "Starting Essentials v{version:l} on {processorSeries:l} Appliance", Global.AssemblyVersion, is4series ? "4-series" : "3-series");
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
        /// GoWithLoad method
        /// </summary>
        public void GoWithLoad()
        {
            try
            {
                Debug.SetDoNotLoadConfigOnNextBoot(false);

                PluginLoader.AddProgramAssemblies();

                _ = new Core.DeviceFactory();
                // _ = new Devices.Common.DeviceFactory();
                // _ = new DeviceFactory();

                // _ = new ProcessorExtensionDeviceFactory();
                // _ = new MobileControlFactory();

                LoadAssets();

                Debug.LogMessage(LogEventLevel.Information, "Starting Essentials load from configuration");

                var filesReady = SetupFilesystem();
                if (filesReady)
                {
                    Debug.LogMessage(LogEventLevel.Information, "Checking for plugins");
                    PluginLoader.LoadPlugins();

                    Debug.LogMessage(LogEventLevel.Information, "Folder structure verified. Loading config...");
                    if (!ConfigReader.LoadConfig2())
                    {
                        Debug.LogMessage(LogEventLevel.Information, "Essentials Load complete with errors");
                        return;
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
        /// 
        /// </summary>
        void Load()
        {
            LoadDevices();
            LoadRooms();
            LoadLogoServer();

            DeviceManager.ActivateAll();

            LoadTieLines();

            /*var mobileControl = GetMobileControlDevice();

		    if (mobileControl == null) return;

            mobileControl.LinkSystemMonitorToAppServer();*/

        }

        /// <summary>
        /// LoadDevices method
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
            Debug.LogMessage(LogEventLevel.Information, "All Devices Loaded.");

        }


        /// <summary>
        /// LoadTieLines method
        /// </summary>
        public void LoadTieLines()
        {
            // In the future, we can't necessarily just clear here because devices
            // might be making their own internal sources/tie lines

            var tlc = TieLineCollection.Default;

            if (ConfigReader.ConfigObject.TieLines == null)
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

            Extensions.MapDestinationsToSources();

            Debug.LogMessage(LogEventLevel.Information, "All Routes Mapped.");
        }



        /// <summary>
        /// Visualizes routes in a tree format for better understanding of signal paths
        /// </summary>
        private void ListTieLines(string args)
        {
            try
            {
                if (args.Contains("?"))
                {
                    CrestronConsole.ConsoleCommandResponse("Usage: listtielines [signaltype]\r\n");
                    CrestronConsole.ConsoleCommandResponse("Signal types: Audio, Video, SecondaryAudio, AudioVideo, UsbInput, UsbOutput\r\n");
                    return;
                }

                eRoutingSignalType? signalTypeFilter = null;
                if (!string.IsNullOrEmpty(args))
                {
                    eRoutingSignalType parsedType;
                    if (Enum.TryParse(args.Trim(), true, out parsedType))
                    {
                        signalTypeFilter = parsedType;
                    }
                    else
                    {
                        CrestronConsole.ConsoleCommandResponse("Invalid signal type: {0}\r\n", args.Trim());
                        CrestronConsole.ConsoleCommandResponse("Valid types: Audio, Video, SecondaryAudio, AudioVideo, UsbInput, UsbOutput\r\n");
                        return;
                    }
                }

                var tielines = signalTypeFilter.HasValue
                    ? TieLineCollection.Default.Where(tl => tl.Type.HasFlag(signalTypeFilter.Value))
                    : TieLineCollection.Default;

                var count = 0;
                foreach (var tl in tielines)
                {
                    CrestronConsole.ConsoleCommandResponse("  {0}{1}", tl, CrestronEnvironment.NewLine);
                    count++;
                }

                CrestronConsole.ConsoleCommandResponse("\r\nTotal: {0} tieline{1}{2}", count, count == 1 ? "" : "s", CrestronEnvironment.NewLine);
            }
            catch (Exception ex)
            {
                CrestronConsole.ConsoleCommandResponse("Error listing tielines: {0}\r\n", ex.Message);
            }
        }

        private void VisualizeRoutes(string args)
        {
            try
            {
                if (args.Contains("?"))
                {
                    CrestronConsole.ConsoleCommandResponse("Usage: visualizeroutes [signaltype] [-s source] [-d destination]\r\n");
                    CrestronConsole.ConsoleCommandResponse("  signaltype: Audio, Video, AudioVideo, etc.\r\n");
                    CrestronConsole.ConsoleCommandResponse("  -s: Filter by source key (partial match)\r\n");
                    CrestronConsole.ConsoleCommandResponse("  -d: Filter by destination key (partial match)\r\n");
                    return;
                }

                ParseRouteFilters(args, out eRoutingSignalType? signalTypeFilter, out string sourceFilter, out string destFilter);

                CrestronConsole.ConsoleCommandResponse("\r\n+===========================================================================+\r\n");
                CrestronConsole.ConsoleCommandResponse("|                         ROUTE VISUALIZATION                               |\r\n");
                CrestronConsole.ConsoleCommandResponse("+===========================================================================+\r\n\r\n");

                foreach (var descriptorCollection in Extensions.RouteDescriptors.Where(kv => kv.Value.Descriptors.Count() > 0))
                {
                    // Filter by signal type if specified
                    if (signalTypeFilter.HasValue && descriptorCollection.Key != signalTypeFilter.Value)
                        continue;

                    CrestronConsole.ConsoleCommandResponse("\r\n+--- Signal Type: {0} ({1} routes) ---\r\n",
                        descriptorCollection.Key,
                        descriptorCollection.Value.Descriptors.Count());

                    foreach (var descriptor in descriptorCollection.Value.Descriptors)
                    {
                        // Filter by source/dest if specified
                        if (sourceFilter != null && !descriptor.Source.Key.ToLower().Contains(sourceFilter))
                            continue;
                        if (destFilter != null && !descriptor.Destination.Key.ToLower().Contains(destFilter))
                            continue;

                        VisualizeRouteDescriptor(descriptor);
                    }
                }

                CrestronConsole.ConsoleCommandResponse("\r\n");
            }
            catch (Exception ex)
            {
                CrestronConsole.ConsoleCommandResponse("Error visualizing routes: {0}\r\n", ex.Message);
            }
        }

        private void VisualizeCurrentRoutes(string args)
        {
            try
            {
                if (args.Contains("?"))
                {
                    CrestronConsole.ConsoleCommandResponse("Usage: visualizecurrentroutes [signaltype] [-s source] [-d destination]\r\n");
                    CrestronConsole.ConsoleCommandResponse("  signaltype: Audio, Video, AudioVideo, etc.\r\n");
                    CrestronConsole.ConsoleCommandResponse("  -s: Filter by source key (partial match)\r\n");
                    CrestronConsole.ConsoleCommandResponse("  -d: Filter by destination key (partial match)\r\n");
                    return;
                }

                ParseRouteFilters(args, out eRoutingSignalType? signalTypeFilter, out string sourceFilter, out string destFilter);

                CrestronConsole.ConsoleCommandResponse("\r\n+===========================================================================+\r\n");
                CrestronConsole.ConsoleCommandResponse("|                    CURRENT ROUTES VISUALIZATION                            |\r\n");
                CrestronConsole.ConsoleCommandResponse("+===========================================================================+\r\n\r\n");

                var hasRoutes = false;

                // Get all descriptors from DefaultCollection
                var allDescriptors = RouteDescriptorCollection.DefaultCollection.Descriptors;

                // Group by signal type
                var groupedByType = allDescriptors.GroupBy(d => d.SignalType);

                foreach (var group in groupedByType)
                {
                    var signalType = group.Key;

                    // Filter by signal type if specified
                    if (signalTypeFilter.HasValue && signalType != signalTypeFilter.Value)
                        continue;

                    var filteredDescriptors = group.Where(d =>
                    {
                        if (sourceFilter != null && !d.Source.Key.ToLower().Contains(sourceFilter))
                            return false;
                        if (destFilter != null && !d.Destination.Key.ToLower().Contains(destFilter))
                            return false;
                        return true;
                    }).ToList();

                    if (filteredDescriptors.Count == 0)
                        continue;

                    hasRoutes = true;
                    CrestronConsole.ConsoleCommandResponse("\r\n+--- Signal Type: {0} ({1} routes) ---\r\n",
                        signalType,
                        filteredDescriptors.Count);

                    foreach (var descriptor in filteredDescriptors)
                    {
                        VisualizeRouteDescriptor(descriptor);
                    }
                }

                if (!hasRoutes)
                {
                    CrestronConsole.ConsoleCommandResponse("\r\nNo active routes found in current state.\r\n");
                }

                CrestronConsole.ConsoleCommandResponse("\r\n");
            }
            catch (Exception ex)
            {
                CrestronConsole.ConsoleCommandResponse("Error visualizing current state: {0}\r\n", ex.Message);
            }
        }

        /// <summary>
        /// Parses route filter arguments from command line
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <param name="signalTypeFilter">Parsed signal type filter (if any)</param>
        /// <param name="sourceFilter">Parsed source filter (if any)</param>
        /// <param name="destFilter">Parsed destination filter (if any)</param>
        private void ParseRouteFilters(string args, out eRoutingSignalType? signalTypeFilter, out string sourceFilter, out string destFilter)
        {
            signalTypeFilter = null;
            sourceFilter = null;
            destFilter = null;

            if (string.IsNullOrEmpty(args))
                return;

            var parts = args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];

                // Check for flags
                if (part == "-s" && i + 1 < parts.Length)
                {
                    sourceFilter = parts[++i].ToLower();
                }
                else if (part == "-d" && i + 1 < parts.Length)
                {
                    destFilter = parts[++i].ToLower();
                }
                // Try to parse as signal type if not a flag and no signal type set yet
                else if (!part.StartsWith("-") && !signalTypeFilter.HasValue)
                {
                    if (Enum.TryParse(part, true, out eRoutingSignalType parsedType))
                    {
                        signalTypeFilter = parsedType;
                    }
                }
            }
        }

        /// <summary>
        /// Visualizes a single route descriptor in a tree format
        /// </summary>
        private void VisualizeRouteDescriptor(RouteDescriptor descriptor)
        {
            CrestronConsole.ConsoleCommandResponse("|\r\n");
            CrestronConsole.ConsoleCommandResponse("|-- {0} --> {1}\r\n",
                descriptor.Source.Key,
                descriptor.Destination.Key);

            if (descriptor.Routes == null || descriptor.Routes.Count == 0)
            {
                CrestronConsole.ConsoleCommandResponse("|   +-- (No switching steps)\r\n");
                return;
            }

            for (int i = 0; i < descriptor.Routes.Count; i++)
            {
                var route = descriptor.Routes[i];
                var isLast = i == descriptor.Routes.Count - 1;
                var prefix = isLast ? "+" : "|";
                var continuation = isLast ? " " : "|";

                if (route.SwitchingDevice != null)
                {
                    CrestronConsole.ConsoleCommandResponse("|   {0}-- [{1}] {2}\r\n",
                        prefix,
                        route.SwitchingDevice.Key,
                        GetSwitchDescription(route));

                    // Add visual connection line for non-last items
                    if (!isLast)
                        CrestronConsole.ConsoleCommandResponse("|   {0}      |\r\n", continuation);
                }
                else
                {
                    CrestronConsole.ConsoleCommandResponse("|   {0}-- {1}\r\n", prefix, route.ToString());
                }
            }
        }

        /// <summary>
        /// Gets a readable description of the switching operation
        /// </summary>
        private string GetSwitchDescription(RouteSwitchDescriptor route)
        {
            if (route.OutputPort != null && route.InputPort != null)
            {
                return string.Format("{0} -> {1}", route.OutputPort.Key, route.InputPort.Key);
            }
            else if (route.InputPort != null)
            {
                return string.Format("-> {0}", route.InputPort.Key);
            }
            else
            {
                return "(passthrough)";
            }
        }

        /// <summary>
        /// LoadRooms method
        /// </summary>
        public void LoadRooms()
        {
            if (ConfigReader.ConfigObject.Rooms == null)
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

        /// <summary>
        /// Fires up a logo server if not already running
        /// </summary>
        void LoadLogoServer()
        {
            if (ConfigReader.ConfigObject.Rooms == null)
            {
                Debug.LogMessage(LogEventLevel.Information, "No rooms configured. Bypassing Logo server startup.");
                return;
            }

            if (
                !ConfigReader.ConfigObject.Rooms.Any(
                    CheckRoomConfig))
            {
                Debug.LogMessage(LogEventLevel.Information, "No rooms configured to use system Logo server. Bypassing Logo server startup");
                return;
            }

            try
            {
                LogoServer = new HttpLogoServer(8080, Global.DirectorySeparator + "html" + Global.DirectorySeparator + "logo");
            }
            catch (Exception)
            {
                Debug.LogMessage(LogEventLevel.Information, "NOTICE: Logo server cannot be started. Likely already running in another program");
            }
        }

        private bool CheckRoomConfig(DeviceConfig c)
        {
            string logoDark = null;
            string logoLight = null;
            string logo = null;

            try
            {
                if (c.Properties["logoDark"] != null)
                {
                    logoDark = c.Properties["logoDark"].Value<string>("type");
                }

                if (c.Properties["logoLight"] != null)
                {
                    logoLight = c.Properties["logoLight"].Value<string>("type");
                }

                if (c.Properties["logo"] != null)
                {
                    logo = c.Properties["logo"].Value<string>("type");
                }

                return ((logoDark != null && logoDark == "system") ||
                        (logoLight != null && logoLight == "system") || (logo != null && logo == "system"));
            }
            catch
            {
                Debug.LogMessage(LogEventLevel.Information, "Unable to find logo information in any room config");
                return false;
            }
        }

        private static void LoadAssets()
        {
            var applicationDirectory = new DirectoryInfo(Global.ApplicationDirectoryPathPrefix);
            Debug.LogMessage(LogEventLevel.Information, "Searching: {applicationDirectory:l} for embedded assets - {Destination}", applicationDirectory.FullName, Global.FilePathPrefix);

            var zipFiles = applicationDirectory.GetFiles("assets*.zip");

            if (zipFiles.Length > 1)
            {
                throw new Exception("Multiple assets zip files found. Cannot continue.");
            }

            if (zipFiles.Length == 1)
            {
                var zipFile = zipFiles[0];
                var assetsRoot = System.IO.Path.GetFullPath(Global.FilePathPrefix);
                if (!assetsRoot.EndsWith(Path.DirectorySeparatorChar.ToString()) && !assetsRoot.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
                {
                    assetsRoot += Path.DirectorySeparatorChar;
                }
                Debug.LogMessage(LogEventLevel.Information, "Found assets zip file: {zipFile:l}... Unzipping...", zipFile.FullName);
                using (var archive = ZipFile.OpenRead(zipFile.FullName))
                {
                    foreach (var entry in archive.Entries)
                    {
                        var destinationPath = Path.Combine(Global.FilePathPrefix, entry.FullName);
                        var fullDest = System.IO.Path.GetFullPath(destinationPath);
                        if (!fullDest.StartsWith(assetsRoot, StringComparison.OrdinalIgnoreCase))
                            throw new InvalidOperationException($"Entry '{entry.FullName}' is trying to extract outside of the target directory.");

                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            Directory.CreateDirectory(destinationPath);
                            continue;
                        }

                        // If a directory exists where a file should go, delete it
                        if (Directory.Exists(destinationPath))
                            Directory.Delete(destinationPath, true);

                        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                        entry.ExtractToFile(destinationPath, true);
                        Debug.LogMessage(LogEventLevel.Information, "Extracted: {entry:l} to {Destination}", entry.FullName, destinationPath);
                    }
                }
            }

            // cleaning up zip files
            foreach (var file in zipFiles)
            {
                File.Delete(file.FullName);
            }

            var htmlZipFiles = applicationDirectory.GetFiles("htmlassets*.zip");

            if (htmlZipFiles.Length > 1)
            {
                throw new Exception("Multiple htmlassets zip files found in application directory. Please ensure only one htmlassets*.zip file is present and retry.");
            }


            if (htmlZipFiles.Length == 1)
            {
                var htmlZipFile = htmlZipFiles[0];
                var programDir = new DirectoryInfo(Global.FilePathPrefix.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                var userOrNvramDir = programDir.Parent;
                var rootDir = userOrNvramDir?.Parent;
                if (rootDir == null)
                {
                    throw new Exception($"Unable to determine root directory for html extraction. Current path: {Global.FilePathPrefix}");
                }
                var htmlDir = Path.Combine(rootDir.FullName, "html");
                var htmlRoot = System.IO.Path.GetFullPath(htmlDir);
                if (!htmlRoot.EndsWith(Path.DirectorySeparatorChar.ToString()) &&
                    !htmlRoot.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
                {
                    htmlRoot += Path.DirectorySeparatorChar;
                }
                Debug.LogMessage(LogEventLevel.Information, "Found htmlassets zip file: {zipFile:l}... Unzipping...", htmlZipFile.FullName);
                using (var archive = ZipFile.OpenRead(htmlZipFile.FullName))
                {
                    foreach (var entry in archive.Entries)
                    {
                        var destinationPath = Path.Combine(htmlDir, entry.FullName);
                        var fullDest = System.IO.Path.GetFullPath(destinationPath);
                        if (!fullDest.StartsWith(htmlRoot, StringComparison.OrdinalIgnoreCase))
                            throw new InvalidOperationException($"Entry '{entry.FullName}' is trying to extract outside of the target directory.");

                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            Directory.CreateDirectory(destinationPath);
                            continue;
                        }

                        // Only delete the file if it exists and is a file, not a directory
                        if (File.Exists(destinationPath))
                            File.Delete(destinationPath);

                        var parentDir = Path.GetDirectoryName(destinationPath);
                        if (!string.IsNullOrEmpty(parentDir))
                            Directory.CreateDirectory(parentDir);

                        entry.ExtractToFile(destinationPath, true);
                        Debug.LogMessage(LogEventLevel.Information, "Extracted: {entry:l} to {Destination}", entry.FullName, destinationPath);
                    }
                }
            }

            // cleaning up html zip files
            foreach (var file in htmlZipFiles)
            {
                File.Delete(file.FullName);
            }

            var jsonFiles = applicationDirectory.GetFiles("*configurationFile*.json");

            if (jsonFiles.Length > 1)
            {
                Debug.LogError("Multiple configuration files found in application directory: {@jsonFiles}", jsonFiles.Select(f => f.FullName).ToArray());
                throw new Exception("Multiple configuration files found. Cannot continue.");
            }

            if (jsonFiles.Length == 1)
            {
                var jsonFile = jsonFiles[0];
                var finalPath = Path.Combine(Global.FilePathPrefix, jsonFile.Name);
                Debug.LogMessage(LogEventLevel.Information, "Found configuration file: {jsonFile:l}... Moving to: {Destination}", jsonFile.FullName, finalPath);

                if (File.Exists(finalPath))
                {
                    Debug.LogMessage(LogEventLevel.Information, "Removing existing configuration file: {Destination}", finalPath);
                    File.Delete(finalPath);
                }

                jsonFile.MoveTo(finalPath);
            }
        }
    }
}
