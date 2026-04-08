extern alias NewtonsoftJson;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Timers;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronDataStore;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.CrestronLogger;
using Formatting = NewtonsoftJson::Newtonsoft.Json.Formatting;
using JsonConvert = NewtonsoftJson::Newtonsoft.Json.JsonConvert;
using PdCore = PepperDash.Core.Abstractions;
using PepperDash.Core.Logging;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;
using Serilog.Templates;

namespace PepperDash.Core;

/// <summary>
/// Contains debug commands for use in various situations
/// </summary>
public static class Debug
{
    private static readonly string LevelStoreKey = "ConsoleDebugLevel";
    private static readonly string WebSocketLevelStoreKey = "WebsocketDebugLevel";
    private static readonly string ErrorLogLevelStoreKey = "ErrorLogDebugLevel";
    private static readonly string FileLevelStoreKey = "FileDebugLevel";
    private static readonly string DoNotLoadOnNextBootKey = "DoNotLoadOnNextBoot";

    private static readonly Dictionary<uint, LogEventLevel> _logLevels = new Dictionary<uint, LogEventLevel>()
    {
        {0, LogEventLevel.Information },
        {3, LogEventLevel.Warning },
        {4, LogEventLevel.Error },
        {5, LogEventLevel.Fatal },
        {1, LogEventLevel.Debug },
        {2, LogEventLevel.Verbose },
    };

    private static ILogger _logger;

    // Injected service abstractions. Populated by DebugServiceRegistration.Register()
    // before the Debug static constructor runs. Null when running on hardware without
    // pre-registration (the static ctor falls back to the Crestron SDK directly).
    private static PdCore.ICrestronEnvironment _environment;
    private static PdCore.ICrestronConsole _console;
    private static PdCore.ICrestronDataStore _dataStore;

    private static readonly LoggingLevelSwitch consoleLoggingLevelSwitch;

    private static readonly LoggingLevelSwitch websocketLoggingLevelSwitch;

    private static readonly LoggingLevelSwitch errorLogLevelSwitch;

    private static readonly LoggingLevelSwitch fileLoggingLevelSwitch;

    /// <summary>
    /// The minimum log level for messages to be sent to the console sink
    /// </summary>
    public static LogEventLevel WebsocketMinimumLogLevel
    {
        get { return websocketLoggingLevelSwitch.MinimumLevel; }
    }

    private static readonly DebugWebsocketSink websocketSink;

    /// <summary>
    /// The DebugWebsocketSink instance used for sending log messages to connected websocket clients.  
    /// This is exposed publicly in case there is a need to call methods on the sink directly, such as SendMessageToClients.  
    /// For general logging purposes, use the LogMessage and LogError methods in this class which will send messages to all configured sinks including the websocket sink.
    /// </summary>
    public static DebugWebsocketSink WebsocketSink
    {
        get { return websocketSink; }
    }

    /// <summary>
    /// Describes the folder location where a given program stores it's debug level memory. By default, the
    /// file written will be named appNdebug where N is 1-10.
    /// </summary>
    public static string OldFilePathPrefix = @"\nvram\debug\";

    /// <summary>
    /// Describes the new folder location where a given program stores it's debug level memory. By default, the
    /// file written will be named appNdebug where N is 1-10.
    /// </summary>
    public static string NewFilePathPrefix = @"\user\debug\";

    /// <summary>
    /// The name of the file containing the current debug settings.
    /// </summary>
    public static string FileName = "app0Debug.json"; // default; updated in static ctor using _environment.ApplicationNumber

    /// <summary>
    /// Debug level to set for a given program.
    /// </summary>
    public static int Level { get; private set; }

    /// <summary>
    /// When this is true, the configuration file will NOT be loaded until triggered by either a console command or a signal
    /// </summary>
    public static bool DoNotLoadConfigOnNextBoot { get; private set; }

    private static DebugContextCollection _contexts;

    private const int SaveTimeoutMs = 30000;

    /// <summary>
    /// Indicates whether the code is running on an appliance or not. Used to determine file paths and other appliance vs server differences
    /// </summary>
    public static bool IsRunningOnAppliance; // set in static constructor

    /// <summary>
    /// Version for the currently loaded PepperDashCore dll
    /// </summary>
    public static string PepperDashCoreVersion { get; private set; }

    // private static Timer _saveTimer;


    private const int defaultConsoleDebugTimeoutMin = 120;

    private static Timer consoleDebugTimer;

    /// <summary>
    /// When true, the IncludedExcludedKeys dict will contain keys to include. 
    /// When false (default), IncludedExcludedKeys will contain keys to exclude.
    /// </summary>
    private static bool _excludeAllMode;

    //static bool ExcludeNoKeyMessages;

    private static readonly Dictionary<string, object> IncludedExcludedKeys;

    private static readonly LoggerConfiguration _defaultLoggerConfiguration;

    private static LoggerConfiguration _loggerConfiguration;

    /// <summary>
    /// The default logger configuration used by the Debug class. Can be used as a base for creating custom logger configurations. 
    /// If changes are made to this configuration after initialization, call ResetLoggerConfiguration to have those changes reflected in the logger.
    /// </summary>
    public static LoggerConfiguration LoggerConfiguration => _loggerConfiguration;

    static Debug()
    {
        try
        {
            // Pick up services pre-registered by the composition root (or test setup).
            // If null, fall back to direct Crestron SDK calls for production hardware.
            _environment = PdCore.DebugServiceRegistration.Environment;
            _console = PdCore.DebugServiceRegistration.Console;
            _dataStore = PdCore.DebugServiceRegistration.DataStore;

            IsRunningOnAppliance = _environment?.DevicePlatform == PdCore.DevicePlatform.Appliance;

            // Update FileName now that _environment is available (avoids Crestron SDK ref in field initializer).
            FileName = $"app{_environment?.ApplicationNumber ?? 0}Debug.json";

            _dataStore?.InitStore();

            consoleDebugTimer = new Timer(defaultConsoleDebugTimeoutMin * 60000) { AutoReset = false };
            consoleDebugTimer.Elapsed += (s, e) =>
            {
                SetDebugLevel(LogEventLevel.Information);
                _console?.ConsoleCommandResponse($"Console debug level reset to {LogEventLevel.Information} after timeout of {defaultConsoleDebugTimeoutMin} minutes");
            };

            var defaultConsoleLevel = GetStoredLogEventLevel(LevelStoreKey);
            var defaultWebsocketLevel = GetStoredLogEventLevel(WebSocketLevelStoreKey);
            var defaultErrorLogLevel = GetStoredLogEventLevel(ErrorLogLevelStoreKey);
            var defaultFileLogLevel = GetStoredLogEventLevel(FileLevelStoreKey);

            consoleLoggingLevelSwitch = new LoggingLevelSwitch(initialMinimumLevel: defaultConsoleLevel);
            websocketLoggingLevelSwitch = new LoggingLevelSwitch(initialMinimumLevel: defaultWebsocketLevel);
            errorLogLevelSwitch = new LoggingLevelSwitch(initialMinimumLevel: defaultErrorLogLevel);
            fileLoggingLevelSwitch = new LoggingLevelSwitch(initialMinimumLevel: defaultFileLogLevel);

            websocketSink = TryCreateWebsocketSink();

            var appRoot = _environment?.GetApplicationRootDirectory()
                ?? System.IO.Path.GetTempPath();
            var sep = System.IO.Path.DirectorySeparatorChar;
            var appNum = _environment?.ApplicationNumber ?? 0;
            var roomId = _environment?.RoomId ?? 0;

            var logFilePath = IsRunningOnAppliance
                ? $"{appRoot}{sep}user{sep}debug{sep}app{appNum}{sep}global-log.log"
                : $"{appRoot}{sep}user{sep}debug{sep}room{roomId}{sep}global-log.log";

            _console?.PrintLine($"Saving log files to {logFilePath}");

            // Build the base Serilog pipeline — sinks that require the Crestron SDK are
            // added conditionally so the logger remains usable in test environments.
            _defaultLoggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Sink(new DebugConsoleSink(new ExpressionTemplate("[{@t:yyyy-MM-dd HH:mm:ss.fff}][{@l:u4}][{App}]{#if Key is not null}[{Key}]{#end} {@m}{#if @x is not null}\r\n{@x}{#end}")), levelSwitch: consoleLoggingLevelSwitch)
                .WriteTo.File(new RenderedCompactJsonFormatter(), logFilePath,
                    rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: LogEventLevel.Debug,
                    retainedFileCountLimit: IsRunningOnAppliance ? 30 : 60,
                    levelSwitch: fileLoggingLevelSwitch
                );

            // Websocket sink is null when DebugWebsocketSink failed to construct (e.g. test env).
            if (websocketSink != null)
                _defaultLoggerConfiguration.WriteTo.Sink(websocketSink, levelSwitch: websocketLoggingLevelSwitch);

            // Add Crestron-specific enricher and error-log sink only on real hardware.
            if (_environment?.IsHardwareRuntime == true)
            {
                var errorLogTemplate = IsRunningOnAppliance
                    ? "{@t:fff}ms [{@l:u4}]{#if Key is not null}[{Key}]{#end} {@m}{#if @x is not null}\r\n{@x}{#end}"
                    : "[{@t:yyyy-MM-dd HH:mm:ss.fff}][{@l:u4}][{App}]{#if Key is not null}[{Key}]{#end} {@m}{#if @x is not null}\r\n{@x}{#end}";

                _defaultLoggerConfiguration
                    .Enrich.With(new CrestronEnricher())
                    .WriteTo.Sink(new DebugErrorLogSink(new ExpressionTemplate(errorLogTemplate)), levelSwitch: errorLogLevelSwitch);
            }

            _loggerConfiguration = _defaultLoggerConfiguration;
            _logger = _loggerConfiguration.CreateLogger();

            GetVersion();

            string msg = IsRunningOnAppliance
                ? $"[App {appNum}] Using PepperDash_Core v{PepperDashCoreVersion}"
                : $"[Room {roomId}] Using PepperDash_Core v{PepperDashCoreVersion}";

            _console?.PrintLine(msg);
            LogMessage(LogEventLevel.Information, msg);

            IncludedExcludedKeys = new Dictionary<string, object>();

            if (_environment?.RuntimeEnvironment == PdCore.RuntimeEnvironment.SimplSharpPro)
            {
                _console?.AddNewConsoleCommand(SetDoNotLoadOnNextBootFromConsole, "donotloadonnextboot",
                    "donotloadonnextboot:P [true/false]: Should the application load on next boot",
                    PdCore.ConsoleAccessLevel.AccessOperator);

                _console?.AddNewConsoleCommand(SetDebugFromConsole, "appdebug",
                    "appdebug:P [0-5]: Sets the application's console debug message level",
                    PdCore.ConsoleAccessLevel.AccessOperator);

                _console?.AddNewConsoleCommand(ShowDebugLog, "appdebuglog",
                    "appdebuglog:P [all] Use \"all\" for full log.",
                    PdCore.ConsoleAccessLevel.AccessOperator);

                _console?.AddNewConsoleCommand(s => CrestronLogger.Clear(false), "appdebugclear",
                    "appdebugclear:P Clears the current custom log",
                    PdCore.ConsoleAccessLevel.AccessOperator);

                _console?.AddNewConsoleCommand(SetDebugFilterFromConsole, "appdebugfilter",
                    "appdebugfilter [params]",
                    PdCore.ConsoleAccessLevel.AccessOperator);
            }

            DoNotLoadConfigOnNextBoot = GetDoNotLoadOnNextBoot();

            if (DoNotLoadConfigOnNextBoot)
                _console?.PrintLine($"Program {appNum} will not load config after next boot.  Use console command go:{appNum} to load the config manually");

            consoleLoggingLevelSwitch.MinimumLevelChanged += (sender, args) =>
            {
                LogMessage(LogEventLevel.Information, "Console debug level set to {minimumLevel}", consoleLoggingLevelSwitch.MinimumLevel);
            };
        }
        catch (Exception ex)
        {
            // _logger may not have been initialized yet — do not call LogError here.
            // _console may also be null; fall back to CrestronConsole as last resort.
            // IMPORTANT: this catch block must not throw — any exception escaping a static
            // constructor permanently faults the type, making the entire class unusable.
            try { _console?.PrintLine($"Exception in Debug static constructor: {ex.Message}\r\n{ex.StackTrace}"); }
            catch
            {
                try { CrestronConsole.PrintLine($"Exception in Debug static constructor: {ex.Message}\r\n{ex.StackTrace}"); }
                catch { /* CrestronConsole unavailable (test/dev env) — swallow to keep type initializer healthy */ }
            }
        }
        finally
        {
            // Guarantee _logger is never null — all Debug.Log* calls are safe even if the
            // ctor failed partway through (e.g. on a dev machine without Crestron hardware).
            _logger ??= new LoggerConfiguration().MinimumLevel.Fatal().CreateLogger();
        }
    }

    /// <summary>Creates the WebSocket sink, returning null if construction fails in a test/dev environment.</summary>
    private static DebugWebsocketSink? TryCreateWebsocketSink()
    {
        try
        {
            return new DebugWebsocketSink(new JsonFormatter(renderMessage: true));
        }
        catch
        {
            _console?.PrintLine("DebugWebsocketSink could not be created in this environment; websocket logging disabled.");
            return null;
        }
    }

    private static bool GetDoNotLoadOnNextBoot()
    {
        if (_dataStore == null) return false;

        if (!_dataStore.TryGetLocalBool(DoNotLoadOnNextBootKey, out var doNotLoad))
        {
            LogError("Error retrieving DoNotLoadOnNextBoot value");
            return false;
        }

        return doNotLoad;
    }

    /// <summary>
    /// Updates the LoggerConfiguration used by the Debug class.  
    /// This allows for changing logger settings such as sinks and output templates.  
    /// After calling this method, the new configuration will be used for all subsequent log messages.
    /// </summary>
    /// <param name="config"></param>
    public static void UpdateLoggerConfiguration(LoggerConfiguration config)
    {
        _loggerConfiguration = config;

        _logger = config.CreateLogger();
    }

    /// <summary>
    /// Resets the LoggerConfiguration to the default configuration defined in this class.
    /// </summary>
    public static void ResetLoggerConfiguration()
    {
        _loggerConfiguration = _defaultLoggerConfiguration;

        _logger = _loggerConfiguration.CreateLogger();
    }

    private static LogEventLevel GetStoredLogEventLevel(string levelStoreKey)
    {
        try
        {
            if (_dataStore == null) return LogEventLevel.Information;

            if (!_dataStore.TryGetLocalInt(levelStoreKey, out int logLevel))
            {
                _console?.Print($"Unable to retrieve stored log level for {levelStoreKey}. Setting level to {LogEventLevel.Information}\r\n");
                return LogEventLevel.Information;
            }

            if (logLevel < 0 || logLevel > 5)
            {
                _console?.PrintLine($"Stored Log level not valid for {levelStoreKey}: {logLevel}. Setting level to {LogEventLevel.Information}");
                return LogEventLevel.Information;
            }

            return (LogEventLevel)logLevel;
        }
        catch (Exception ex)
        {
            _console?.PrintLine($"Exception retrieving log level for {levelStoreKey}: {ex.Message}");
            return LogEventLevel.Information;
        }
    }

    private static void GetVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var ver =
            assembly
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);

        if (ver != null && ver.Length > 0)
        {
            if (ver[0] is AssemblyInformationalVersionAttribute verAttribute)
            {
                PepperDashCoreVersion = verAttribute.InformationalVersion;
            }
        }
        else
        {
            var version = assembly.GetName().Version;
            PepperDashCoreVersion = version.ToString();
        }
    }

    // /// <summary>
    // /// Used to save memory when shutting down
    // /// </summary>
    // /// <param name="programEventType"></param>
    // static void CrestronEnvironment_ProgramStatusEventHandler(eProgramStatusEventType programEventType)
    // {

    //     if (programEventType == eProgramStatusEventType.Stopping)
    //     {
    //         Log.CloseAndFlush();

    //         if (_saveTimer != null)
    //         {
    //             _saveTimer.Stop();
    //             _saveTimer = null;
    //         }
    //         LogMessage(LogEventLevel.Information, "Saving debug settings");
    //         // SaveMemory();
    //     }
    // }

    /// <summary>
    /// Callback for console command
    /// </summary>
    /// <param name="levelString"></param>
    public static void SetDebugFromConsole(string levelString)
    {
        try
        {
            if (levelString.Trim() == "?")
            {
                _console?.ConsoleCommandResponse(
                
                "Used to set the minimum level of debug messages to be printed to the console:\r\n" +
                "[LogLevel] [TimeoutInMinutes]\r\n" +
                 "If TimeoutInMinutes is not provided, it will default to 120 minutes.  If provided, the level will reset to Information after the timeout period elapses.\r\n" +
                 "LogLevel can be either a number from 0-5 or a log level name.  If using a number, the mapping is as follows:\r\n" +
                $"{_logLevels[0]} = 0\r\n" +
                $"{_logLevels[1]} = 1\r\n" +
                $"{_logLevels[2]} = 2\r\n" +
                $"{_logLevels[3]} = 3\r\n" +
                $"{_logLevels[4]} = 4\r\n" +
                $"{_logLevels[5]} = 5");
                return;
            }

            if (string.IsNullOrEmpty(levelString.Trim()))
            {
                _console?.ConsoleCommandResponse($"AppDebug level = {consoleLoggingLevelSwitch.MinimumLevel}");
                return;
            }

            // split on space to allow for potential future addition of timeout parameter without breaking existing command usage
            var parts = Regex.Split(levelString.Trim(), @"\s+");
            levelString = parts[0];

            if (parts.Length > 1 && long.TryParse(parts[1], out var timeout))
            {
                timeout = Math.Max(timeout, 1); // enforce minimum timeout of 1 minute
                consoleDebugTimer.Interval = timeout * 60000;
            }

            // first try to parse as int for backward compatibility with existing usage of numeric levels

            if (int.TryParse(levelString, out var levelInt))
            {
                if (levelInt < 0 || levelInt > 5)
                {
                    _console?.ConsoleCommandResponse($"Error: Unable to parse {levelString} to valid log level. If using a number, value must be between 0-5");
                    return;
                }
                SetDebugLevel((uint)levelInt);
                return;
            }

            // make this parse attempt case-insensitive to allow for more flexible command usage
            if (Enum.TryParse<LogEventLevel>(levelString, true, out var levelEnum))
            {
                SetDebugLevel(levelEnum);
                return;
            }

            _console?.ConsoleCommandResponse($"Error: Unable to parse {levelString} to valid log level");
        }
        catch
        {
            _console?.ConsoleCommandResponse("Usage: appdebug:P [0-5]");
        }
    }

    /// <summary>
    /// Sets the debug level
    /// </summary>
    /// <param name="level"> Valid values 0-5</param>
    /// <param name="timeout"> Timeout in minutes</param>
    public static void SetDebugLevel(uint level, int timeout = defaultConsoleDebugTimeoutMin)
    {
        if (!_logLevels.TryGetValue(level, out var logLevel))
        {
            logLevel = LogEventLevel.Information;

            _console?.ConsoleCommandResponse($"{level} not valid. Setting level to {logLevel}");

            SetDebugLevel(logLevel, timeout);
        }

        SetDebugLevel(logLevel, timeout);
    }

    /// <summary>
    /// Sets the debug level 
    /// </summary>
    /// <param name="level"> The log level to set</param>
    /// <param name="timeout"> Timeout in minutes</param>
    public static void SetDebugLevel(LogEventLevel level, int timeout = defaultConsoleDebugTimeoutMin)
    {
        consoleDebugTimer.Stop();
        consoleDebugTimer.Interval = timeout * 60000;
        consoleDebugTimer.Start();

        consoleLoggingLevelSwitch.MinimumLevel = level;

        var appNum = _environment?.ApplicationNumber ?? 0;
        _console?.ConsoleCommandResponse($"[Application {appNum}], Debug level set to {consoleLoggingLevelSwitch.MinimumLevel}\r\n");
        _console?.ConsoleCommandResponse($"Storing level {level}:{(int)level}");

        if (_dataStore != null && !_dataStore.SetLocalInt(LevelStoreKey, (int)level))
            _console?.PrintLine($"Error saving console debug level setting");
        else
            _console?.ConsoleCommandResponse($"Store result: {(int)level}");
    }

    /// <summary>
    /// Sets the debug level for the websocket sink
    /// </summary>
    /// <param name="level"></param>
    public static void SetWebSocketMinimumDebugLevel(LogEventLevel level)
    {
        websocketLoggingLevelSwitch.MinimumLevel = level;

        if (_dataStore != null && !_dataStore.SetLocalUint(WebSocketLevelStoreKey, (uint)level))
            LogMessage(LogEventLevel.Information, "Error saving websocket debug level setting");

        LogMessage(LogEventLevel.Information, "Websocket debug level set to {0}", websocketLoggingLevelSwitch.MinimumLevel);
    }


    /// <summary>
    /// Sets the minimum debug level for the error log sink
    /// </summary>
    /// <param name="level"></param>
    public static void SetErrorLogMinimumDebugLevel(LogEventLevel level)
    {
        errorLogLevelSwitch.MinimumLevel = level;

        if (_dataStore != null && !_dataStore.SetLocalUint(ErrorLogLevelStoreKey, (uint)level))
            LogMessage(LogEventLevel.Information, "Error saving Error Log debug level setting");

        LogMessage(LogEventLevel.Information, "Error log debug level set to {0}", errorLogLevelSwitch.MinimumLevel);
    }

    /// <summary>
    /// Sets the minimum debug level for the file sink
    /// </summary>
    public static void SetFileMinimumDebugLevel(LogEventLevel level)
    {
        fileLoggingLevelSwitch.MinimumLevel = level;

        if (_dataStore != null && !_dataStore.SetLocalUint(FileLevelStoreKey, (uint)level))
            LogMessage(LogEventLevel.Information, "Error saving File debug level setting");

        LogMessage(LogEventLevel.Information, "File debug level set to {0}", fileLoggingLevelSwitch.MinimumLevel);
    }

    /// <summary>
    /// Callback for console command
    /// </summary>
    /// <param name="stateString"></param>
    public static void SetDoNotLoadOnNextBootFromConsole(string stateString)
    {
        try
        {
            if (string.IsNullOrEmpty(stateString.Trim()))
            {
                _console?.ConsoleCommandResponse($"DoNotLoadOnNextBoot = {DoNotLoadConfigOnNextBoot}");
                return;
            }

            SetDoNotLoadConfigOnNextBoot(bool.Parse(stateString));
        }
        catch
        {
            _console?.ConsoleCommandResponse("Usage: donotloadonnextboot:P [true/false]");
        }
    }

    /// <summary>
    /// Callback for console command
    /// </summary>
    /// <param name="items"></param>
    public static void SetDebugFilterFromConsole(string items)
    {
        var str = items.Trim();
        if (str == "?")
        {
            CrestronConsole.ConsoleCommandResponse("Usage:\r APPDEBUGFILTER key1 key2 key3....\r " +
                "+all: at beginning puts filter into 'default include' mode\r" +
                "      All keys that follow will be excluded from output.\r" +
                "-all: at beginning puts filter into 'default exclude all' mode.\r" +
                "      All keys that follow will be the only keys that are shown\r" +
                "+nokey: Enables messages with no key (default)\r" +
                "-nokey: Disables messages with no key.\r" +
                "(nokey settings are independent of all other settings)");
            return;
        }
        var keys = Regex.Split(str, @"\s*");
        foreach (var keyToken in keys)
        {
            var lkey = keyToken.ToLower();
            if (lkey == "+all")
            {
                IncludedExcludedKeys.Clear();
                _excludeAllMode = false;
            }
            else if (lkey == "-all")
            {
                IncludedExcludedKeys.Clear();
                _excludeAllMode = true;
            }
            //else if (lkey == "+nokey")
            //{
            //    ExcludeNoKeyMessages = false;
            //}
            //else if (lkey == "-nokey")
            //{
            //    ExcludeNoKeyMessages = true;
            //}
            else
            {
                string key;
                if (lkey.StartsWith("-"))
                {
                    key = lkey.Substring(1);
                    // if in exclude all mode, we need to remove this from the inclusions
                    if (_excludeAllMode)
                    {
                        if (IncludedExcludedKeys.ContainsKey(key))
                            IncludedExcludedKeys.Remove(key);
                    }
                    // otherwise include all mode, add to the exclusions
                    else
                    {
                        IncludedExcludedKeys[key] = new object();
                    }
                }
                else if (lkey.StartsWith("+"))
                {
                    key = lkey.Substring(1);
                    // if in exclude all mode, we need to add this as inclusion
                    if (_excludeAllMode)
                    {

                        IncludedExcludedKeys[key] = new object();
                    }
                    // otherwise include all mode, remove this from exclusions
                    else
                    {
                        if (IncludedExcludedKeys.ContainsKey(key))
                            IncludedExcludedKeys.Remove(key);
                    }
                }
            }
        }
    }




    /// <summary>
    /// sets the settings for a device or creates a new entry
    /// </summary>
    /// <param name="deviceKey"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static void SetDeviceDebugSettings(string deviceKey, object settings)
    {
        _contexts.SetDebugSettingsForKey(deviceKey, settings);
        // SaveMemoryOnTimeout();
    }

    /// <summary>
    /// Gets the device settings for a device by key or returns null
    /// </summary>
    /// <param name="deviceKey"></param>
    /// <returns></returns>
    public static object GetDeviceDebugSettingsForKey(string deviceKey)
    {
        return _contexts.GetDebugSettingsForKey(deviceKey);
    }

    /// <summary>
    /// Sets the flag to prevent application starting on next boot
    /// </summary>
    /// <param name="state"></param>
    public static void SetDoNotLoadConfigOnNextBoot(bool state)
    {
        DoNotLoadConfigOnNextBoot = state;

        if (_dataStore != null && !_dataStore.SetLocalBool(DoNotLoadOnNextBootKey, state))
            LogError("Error saving DoNotLoadConfigOnNextBoot setting");

        LogInformation("Do Not Load Config on Next Boot set to {state}", DoNotLoadConfigOnNextBoot);
    }

    /// <summary>
    /// 
    /// </summary>
    public static void ShowDebugLog(string s)
    {
        if (_environment == null) return; // CrestronLogger not available in test environments
        var loglist = CrestronLogger.PrintTheLog(s.ToLower() == "all");
        foreach (var l in loglist)
            _console?.ConsoleCommandResponse(l + CrestronEnvironment.NewLine);
    }

    /// <summary>
    /// Log an Exception as an Error
    /// </summary>
    /// <param name="ex">Exception to log</param>
    /// <param name="message">Message template</param>
    /// <param name="device">Optional IKeyed device. If provided, the Key of the device will be added to the log message</param>
    /// <param name="args">Args to put into message template</param>
    public static void LogMessage(Exception ex, string message, IKeyed device = null, params object[] args)
    {
        using (LogContext.PushProperty("Key", device?.Key))
        {
            _logger.Error(ex, message, args);
        }
    }

    /// <summary>
    /// Log a message
    /// </summary>
    /// <param name="level">Level to log at</param>
    /// <param name="message">Message template</param>
    /// <param name="device">Optional IKeyed device. If provided, the Key of the device will be added to the log message</param>
    /// <param name="args">Args to put into message template</param>
    public static void LogMessage(LogEventLevel level, string message, IKeyed device = null, params object[] args)
    {
        using (LogContext.PushProperty("Key", device?.Key))
        {
            _logger.Write(level, message, args);
        }
    }

    /// <summary>
    /// Log a message with at the specified level.
    /// </summary>
    public static void LogMessage(LogEventLevel level, string message, params object[] args)
    {
        _logger.Write(level, message, args);
    }

    /// <summary>
    /// Log a message with at the specified level and exception.
    /// </summary>
    /// <param name="level"></param>
    /// <param name="ex"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void LogMessage(LogEventLevel level, Exception ex, string message, params object[] args)
    {
        _logger.Write(level, ex, message, args);
    }

    /// <summary>
    /// Log a message with at the specified level and device context.
    /// </summary> <param name="level"></param>
    /// <param name="keyed"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void LogMessage(LogEventLevel level, IKeyed keyed, string message, params object[] args)
    {
        LogMessage(level, message, keyed, args);
    }

    /// <summary>
    /// Log a message with at the specified level, exception, and device context.
    /// </summary>
    /// <param name="level"></param>
    /// <param name="ex"></param>
    /// <param name="device"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void LogMessage(LogEventLevel level, Exception ex, IKeyed device, string message, params object[] args)
    {
        using (LogContext.PushProperty("Key", device?.Key))
        {
            _logger.Write(level, ex, message, args);
        }
    }

    #region Explicit methods for logging levels

    /// <summary>
    /// Log a message with Verbose level and device context.
    /// </summary>
    /// <param name="keyed"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void LogVerbose(IKeyed keyed, string message, params object[] args)
    {
        using (LogContext.PushProperty("Key", keyed?.Key))
        {
            _logger.Write(LogEventLevel.Verbose, message, args);
        }
    }

    /// <summary>
    /// Log a message with Verbose level, exception, and device context.
    /// </summary>
    public static void LogVerbose(Exception ex, IKeyed keyed, string message, params object[] args)
    {
        using (LogContext.PushProperty("Key", keyed?.Key))
        {
            _logger.Write(LogEventLevel.Verbose, ex, message, args);
        }
    }

    /// <summary>
    /// Log a message with Verbose level.
    /// </summary>
    public static void LogVerbose(string message, params object[] args)
    {
        _logger.Write(LogEventLevel.Verbose, message, args);
    }

    /// <summary>
    /// Log a message with Verbose level and exception.
    /// </summary>
    public static void LogVerbose(Exception ex, string message, params object[] args)
    {
        _logger.Write(LogEventLevel.Verbose, ex, null, message, args);
    }

    /// <summary>
    /// Log a message with Debug level and device context.
    /// </summary>
    public static void LogDebug(IKeyed keyed, string message, params object[] args)
    {
        using (LogContext.PushProperty("Key", keyed?.Key))
        {
            _logger.Write(LogEventLevel.Debug, message, args);
        }
    }

    /// <summary>
    /// Log a message with Debug level, exception, and device context.
    /// </summary>
    public static void LogDebug(Exception ex, IKeyed keyed, string message, params object[] args)
    {
        using (LogContext.PushProperty("Key", keyed?.Key))
        {
            _logger.Write(LogEventLevel.Debug, ex, message, args);
        }
    }

    /// <summary>
    /// Log a message with Debug level.
    /// </summary>
    public static void LogDebug(string message, params object[] args)
    {
        _logger.Write(LogEventLevel.Debug, message, args);
    }

    /// <summary>
    /// Log a message with Debug level and exception.
    /// </summary>
    public static void LogDebug(Exception ex, string message, params object[] args)
    {
        _logger.Write(LogEventLevel.Debug, ex, null, message, args);
    }

    /// <summary>
    /// Log a message with Information level and device context.
    /// </summary>
    public static void LogInformation(IKeyed keyed, string message, params object[] args)
    {
        using (LogContext.PushProperty("Key", keyed?.Key))
        {
            _logger.Write(LogEventLevel.Information, message, args);
        }
    }

    /// <summary>
    /// Log a message with Information level, exception, and device context.
    /// </summary>
    public static void LogInformation(Exception ex, IKeyed keyed, string message, params object[] args)
    {
        using (LogContext.PushProperty("Key", keyed?.Key))
        {
            _logger.Write(LogEventLevel.Information, ex, message, args);
        }
    }

    /// <summary>
    /// Log a message with Information level.
    /// </summary>
    public static void LogInformation(string message, params object[] args)
    {
        _logger.Write(LogEventLevel.Information, message, args);
    }

    /// <summary>
    /// Log a message with Information level and exception.
    /// </summary>
    public static void LogInformation(Exception ex, string message, params object[] args)
    {
        _logger.Write(LogEventLevel.Information, ex, null, message, args);
    }

    /// <summary>
    /// Log a message with Warning level and device context.
    /// </summary>
    public static void LogWarning(IKeyed keyed, string message, params object[] args)
    {
        using (LogContext.PushProperty("Key", keyed?.Key))
        {
            _logger.Write(LogEventLevel.Warning, message, args);
        }
    }

    /// <summary>
    /// Log a message with Warning level, exception, and device context.
    /// </summary>
    public static void LogWarning(Exception ex, IKeyed keyed, string message, params object[] args)
    {
        using (LogContext.PushProperty("Key", keyed?.Key))
        {
            _logger.Write(LogEventLevel.Warning, ex, message, args);
        }
    }

    /// <summary>
    /// Log a message with Warning level.
    /// </summary>
    public static void LogWarning(string message, params object[] args)
    {
        _logger.Write(LogEventLevel.Warning, message, args);
    }

    /// <summary>
    /// Log a message with Warning level and exception.
    /// </summary>
    public static void LogWarning(Exception ex, string message, params object[] args)
    {
        _logger.Write(LogEventLevel.Warning, ex, null, message, args);
    }

    /// <summary>
    /// Log a message with Error level and device context.
    /// </summary>
    public static void LogError(IKeyed keyed, string message, params object[] args)
    {
        using (LogContext.PushProperty("Key", keyed?.Key))
        {
            _logger.Write(LogEventLevel.Error, message, args);
        }
    }

    /// <summary>
    /// Log a message with Error level, exception, and device context.
    /// </summary>
    public static void LogError(Exception ex, IKeyed keyed, string message, params object[] args)
    {
        using (LogContext.PushProperty("Key", keyed?.Key))
        {
            _logger.Write(LogEventLevel.Error, ex, message, args);
        }
    }

    /// <summary>
    /// Log a message with Error level.
    /// </summary>
    public static void LogError(string message, params object[] args)
    {
        _logger.Write(LogEventLevel.Error, message, args);
    }

    /// <summary>
    /// Log a message with Error level and exception.
    /// </summary>
    public static void LogError(Exception ex, string message, params object[] args)
    {
        _logger.Write(LogEventLevel.Error, ex, null, message, args);
    }

    /// <summary>
    /// Log a message with Fatal level and device context.
    /// </summary>
    public static void LogFatal(IKeyed keyed, string message, params object[] args)
    {
        using (LogContext.PushProperty("Key", keyed?.Key))
        {
            _logger.Write(LogEventLevel.Fatal, message, args);
        }
    }

    /// <summary>
    /// Log a message with Fatal level, exception, and device context.
    /// </summary>
    public static void LogFatal(Exception ex, IKeyed keyed, string message, params object[] args)
    {
        using (LogContext.PushProperty("Key", keyed?.Key))
        {
            _logger.Write(LogEventLevel.Fatal, ex, message, args);
        }
    }

    /// <summary>
    /// Log a message with Fatal level.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void LogFatal(string message, params object[] args)
    {
        _logger.Write(LogEventLevel.Fatal, message, args);
    }

    /// <summary>
    /// Log a message with Fatal level and exception.
    /// </summary>
    public static void LogFatal(Exception ex, string message, params object[] args)
    {
        _logger.Write(LogEventLevel.Fatal, ex, null, message, args);
    }

    #endregion


    private static void LogMessage(uint level, string format, params object[] items)
    {
        if (!_logLevels.ContainsKey(level)) return;

        var logLevel = _logLevels[level];

        LogMessage(logLevel, format, items);
    }

    private static void LogMessage(uint level, IKeyed keyed, string format, params object[] items)
    {
        if (!_logLevels.ContainsKey(level)) return;

        var logLevel = _logLevels[level];

        LogMessage(logLevel, keyed, format, items);
    }


    // /// <summary>
    // /// Writes the memory object after timeout
    // /// </summary>
    // static void SaveMemoryOnTimeout()
    // {
    //     LogInformation("Saving debug settings");
    //     if (_saveTimer == null)
    //     {
    //         _saveTimer = new Timer(SaveTimeoutMs) { AutoReset = false };
    //         _saveTimer.Elapsed += (s, e) =>
    //         {
    //             _saveTimer = null;
    //             SaveMemory();
    //         };
    //         _saveTimer.Start();
    //     }
    //     else
    //     {
    //         _saveTimer.Stop();
    //         _saveTimer.Interval = SaveTimeoutMs;
    //         _saveTimer.Start();
    //     }
    // }

    // /// <summary>
    // /// Writes the memory - use SaveMemoryOnTimeout
    // /// </summary>
    // static void SaveMemory()
    // {
    //     //var dir = @"\NVRAM\debug";
    //     //if (!Directory.Exists(dir))
    //     //    Directory.Create(dir);

    //     try
    //     {
    //         var fileName = GetMemoryFileName();

    //         LogInformation("Loading debug settings file from {fileName}", fileName);

    //         using (var sw = new StreamWriter(fileName))
    //         {
    //             var json = JsonConvert.SerializeObject(_contexts);
    //             sw.Write(json);
    //             sw.Flush();
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         LogError("Exception saving debug settings: {message}", ex);
    //         return;
    //     }
    // }

    // /// <summary>
    // /// 
    // /// </summary>
    // static void LoadMemory()
    // {
    //     var file = GetMemoryFileName();
    //     if (File.Exists(file))
    //     {
    //         using (var sr = new StreamReader(file))
    //         {
    //             var json = sr.ReadToEnd();
    //             _contexts = JsonConvert.DeserializeObject<DebugContextCollection>(json);

    //             if (_contexts != null)
    //             {
    //                 LogMessage(LogEventLevel.Debug, "Debug memory restored from file");
    //                 return;
    //             }
    //         }
    //     }

    //     _contexts = new DebugContextCollection();
    // }

    /// <summary>
    /// Helper to get the file path for this app's debug memory
    /// </summary>
    static string GetMemoryFileName()
    {
        if (CrestronEnvironment.DevicePlatform == eDevicePlatform.Appliance)
        {
            // CheckForMigration();
            return string.Format(@"\user\debugSettings\program{0}", InitialParametersClass.ApplicationNumber);
        }

        return string.Format("{0}{1}user{1}debugSettings{1}{2}.json", Directory.GetApplicationRootDirectory(), Path.DirectorySeparatorChar, InitialParametersClass.RoomId);
    }
}