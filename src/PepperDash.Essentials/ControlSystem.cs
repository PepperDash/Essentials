
using System;
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
using PepperDash.Essentials.Core.Config.Essentials;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.Plugins;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Core.Secrets;
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

            Debug.SetErrorLogMinimumDebugLevel(CrestronEnvironment.DevicePlatform == eDevicePlatform.Appliance ? LogEventLevel.Warning : LogEventLevel.Verbose);

            // AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
        }

        private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name).Name;
            if (assemblyName == "PepperDash_Core")
            {
                return Assembly.LoadFrom("PepperDashCore.dll");
            }

            if (assemblyName == "PepperDash_Essentials_Core")
            {
                return Assembly.LoadFrom("PepperDash.Essentials.Core.dll");
            }

            if (assemblyName == "Essentials Devices Common")
            {
                return Assembly.LoadFrom("PepperDash.Essentials.Devices.Common.dll");
            }

            return null;
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
            Debug.SetErrorLogMinimumDebugLevel(LogEventLevel.Verbose);

            DeterminePlatform();

            if (Debug.DoNotLoadConfigOnNextBoot)
            {
                CrestronConsole.AddNewConsoleCommand(s => CrestronInvoke.BeginInvoke((o) => GoWithLoad()), "go", "Loads configuration file",
                    ConsoleAccessLevelEnum.AccessOperator);
            }

            CrestronConsole.AddNewConsoleCommand(PluginLoader.ReportAssemblyVersions, "reportversions", "Reports the versions of the loaded assemblies", ConsoleAccessLevelEnum.AccessOperator);

            CrestronConsole.AddNewConsoleCommand(Core.Factory.DeviceFactory.GetDeviceFactoryTypes, "gettypes", "Gets the device types that can be built. Accepts a filter string.", ConsoleAccessLevelEnum.AccessOperator);

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
                        newDev = Core.Factory.DeviceFactory.GetDevice(devConf);

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
                    var room = Core.Factory.DeviceFactory.GetDevice(roomConfig);

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
    }
}
