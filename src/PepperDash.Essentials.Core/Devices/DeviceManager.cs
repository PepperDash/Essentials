using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using PepperDash.Core;
using Serilog.Events;


namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Manages the devices in the system
    /// </summary>
    public static class DeviceManager
    {
        /// <summary>
        /// Raised when all devices have been activated
        /// </summary>
        public static event EventHandler<EventArgs> AllDevicesActivated;

        /// <summary>
        /// Raised when all devices have been registered
        /// </summary>
        public static event EventHandler<EventArgs> AllDevicesRegistered;

        /// <summary>
        /// Raised when all devices have been initialized
        /// </summary>
        public static event EventHandler<EventArgs> AllDevicesInitialized;

        private static readonly CCriticalSection DeviceCriticalSection = new CCriticalSection();

        private static readonly CEvent AllowAddDevicesCEvent = new CEvent(false, true);

        private static readonly Dictionary<string, IKeyed> Devices = new Dictionary<string, IKeyed>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets the AllDevices
        /// </summary>
        public static List<IKeyed> AllDevices { get { return new List<IKeyed>(Devices.Values); } }

        /// <summary>
        /// Gets or sets the AddDeviceEnabled
        /// </summary>
        public static bool AddDeviceEnabled;

        /// <summary>
        /// Initialize method
        /// </summary>
        public static void Initialize(CrestronControlSystem cs)
        {
            AddDeviceEnabled = true;
            CrestronConsole.AddNewConsoleCommand(ListDeviceCommStatuses, "devcommstatus", "Lists the communication status of all devices",
                ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(ListDeviceFeedbacks, "devfb", "Lists current feedbacks",
                ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(ListDevices, "devlist", "Lists current managed devices",
                ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(DeviceJsonApi.DoDeviceActionWithJson, "devjson", "",
                ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(s => CrestronConsole.ConsoleCommandResponse(DeviceJsonApi.GetProperties(s).Replace(Environment.NewLine, "\r\n")), "devprops", "", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(s => CrestronConsole.ConsoleCommandResponse(DeviceJsonApi.GetMethods(s).Replace(Environment.NewLine, "\r\n")), "devmethods", "", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(s => CrestronConsole.ConsoleCommandResponse(DeviceJsonApi.GetApiMethods(s).Replace(Environment.NewLine, "\r\n")), "apimethods", "", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(SimulateComReceiveOnDevice, "devsimreceive",
                "Simulates incoming data on a com device", ConsoleAccessLevelEnum.AccessOperator);

            CrestronConsole.AddNewConsoleCommand(SetDeviceStreamDebugging, "setdevicestreamdebug", "set comm debug [deviceKey] [off/rx/tx/both] ([minutes])", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(s => DisableAllDeviceStreamDebugging(), "disableallstreamdebug", "disables stream debugging on all devices", ConsoleAccessLevelEnum.AccessOperator);
        }

        /// <summary>
        /// ActivateAll method
        /// </summary>
        public static void ActivateAll()
        {
            try
            {
                OnAllDevicesRegistered();

                DeviceCriticalSection.Enter();
                AddDeviceEnabled = false;
                // PreActivate all devices
                Debug.LogMessage(LogEventLevel.Information, "****PreActivation starting...****");
                foreach (var d in Devices.Values)
                {
                    try
                    {
                        if (d is Device)
                            (d as Device).PreActivate();
                    }
                    catch (Exception e)
                    {
                        Debug.LogMessage(LogEventLevel.Information, d, "ERROR: Device {1} PreActivation failure: {0}", e.Message, d.Key);
                        Debug.LogMessage(LogEventLevel.Debug, d, "Stack Trace: {0}", e.StackTrace);
                    }
                }
                Debug.LogMessage(LogEventLevel.Information, "****PreActivation complete****");
                Debug.LogMessage(LogEventLevel.Information, "****Activation starting...****");

                // Activate all devices
                foreach (var d in Devices.Values)
                {
                    try
                    {
                        if (d is Device)
                            (d as Device).Activate();
                    }
                    catch (Exception e)
                    {
                        Debug.LogMessage(LogEventLevel.Information, d, "ERROR: Device {1} Activation failure: {0}", e.Message, d.Key);
                        Debug.LogMessage(LogEventLevel.Debug, d, "Stack Trace: {0}", e.StackTrace);
                    }
                }

                Debug.LogMessage(LogEventLevel.Information, "****Activation complete****");
                Debug.LogMessage(LogEventLevel.Information, "****PostActivation starting...****");

                // PostActivate all devices
                foreach (var d in Devices.Values)
                {
                    try
                    {
                        if (d is Device)
                            (d as Device).PostActivate();
                    }
                    catch (Exception e)
                    {
                        Debug.LogMessage(LogEventLevel.Information, d, "ERROR: Device {1} PostActivation failure: {0}", e.Message, d.Key);
                        Debug.LogMessage(LogEventLevel.Debug, d, "Stack Trace: {0}", e.StackTrace);
                    }
                }

                Debug.LogMessage(LogEventLevel.Information, "****PostActivation complete****");

                OnAllDevicesActivated();
            }
            finally
            {
                DeviceCriticalSection.Leave();
            }
        }

        private static void DeviceManager_Initialized(object sender, EventArgs e)
        {
            var allInitialized = Devices.Values.OfType<EssentialsDevice>().All(d => d.IsInitialized);

            if (allInitialized)
            {
                Debug.LogMessage(LogEventLevel.Information, "****All Devices Initalized****");

                OnAllDevicesInitialized();
            }
        }

        private static void OnAllDevicesActivated()
        {
            var handler = AllDevicesActivated;
            if (handler != null)
            {
                handler(null, new EventArgs());
            }
        }

        private static void OnAllDevicesRegistered()
        {
            var handler = AllDevicesRegistered;
            if (handler != null)
            {
                handler(null, new EventArgs());
            }
        }

        private static void OnAllDevicesInitialized()
        {
            var handler = AllDevicesInitialized;
            if (handler != null)
            {
                handler(null, new EventArgs());
            }
        }

        /// <summary>
        /// DeactivateAll method
        /// </summary>
        public static void DeactivateAll()
        {
            try
            {
                DeviceCriticalSection.Enter();
                foreach (var d in Devices.Values.OfType<Device>())
                {
                    d.Deactivate();
                }
            }
            finally
            {
                DeviceCriticalSection.Leave();
            }
        }

        private static void ListDevices(string s)
        {
            CrestronConsole.ConsoleCommandResponse($"{Devices.Count} Devices registered with Device Manager:\r\n");

            var sorted = Devices.Values.ToList();
            sorted.Sort((a, b) => a.Key.CompareTo(b.Key));

            foreach (var d in sorted)
            {
                var name = d is IKeyName ? (d as IKeyName).Name : "---";
                CrestronConsole.ConsoleCommandResponse($"  [{d.Key}] {name}\r\n");
            }
        }

        private static void ListDeviceFeedbacks(string devKey)
        {
            var dev = GetDeviceForKey(devKey);
            if (dev == null)
            {
                CrestronConsole.ConsoleCommandResponse($"Device '{devKey}' not found\r\n");
                return;
            }
            if (!(dev is IHasFeedback statusDev))
            {
                CrestronConsole.ConsoleCommandResponse($"Device '{devKey}' does not have visible feedbacks\r\n");
                return;
            }
            statusDev.DumpFeedbacksToConsole(true);
        }

        private static void ListDeviceCommStatuses(string input)
        {

            foreach (var dev in Devices.Values.OfType<ICommunicationMonitor>())
            {
                CrestronConsole.ConsoleCommandResponse($"{dev}: {dev.CommunicationMonitor.Status}\r\n");
            }
        }

        /// <summary>
        /// AddDevice method
        /// </summary>
        public static void AddDevice(IKeyed newDev)
        {
            try
            {
                if (!DeviceCriticalSection.TryEnter())
                {
                    Debug.LogMessage(LogEventLevel.Information, "Currently unable to add devices to Device Manager. Please try again");
                    return;
                }
                // Check for device with same key
                //var existingDevice = _Devices.FirstOrDefault(d => d.Key.Equals(newDev.Key, StringComparison.OrdinalIgnoreCase));
                ////// If it exists, remove or warn??
                //if (existingDevice != null)

                if (!AddDeviceEnabled)
                {
                    Debug.LogMessage(LogEventLevel.Information, "All devices have been activated. Adding new devices is not allowed.");
                    return;
                }

                if (Devices.ContainsKey(newDev.Key))
                {
                    Debug.LogMessage(LogEventLevel.Information, newDev, "WARNING: A device with this key already exists.  Not added to manager");
                    return;
                }
                Devices.Add(newDev.Key, newDev);
                //if (!(_Devices.Contains(newDev)))
                //    _Devices.Add(newDev);

                if (newDev is EssentialsDevice essentialsDev)
                    essentialsDev.Initialized += DeviceManager_Initialized;
            }
            finally
            {
                DeviceCriticalSection.Leave();
            }
        }

        /// <summary>
        /// AddDevice method
        /// </summary>
        public static void AddDevice(IEnumerable<IKeyed> devicesToAdd)
        {
            try
            {
                if (!DeviceCriticalSection.TryEnter())
                {
                    Debug.LogMessage(LogEventLevel.Information,
                        "Currently unable to add devices to Device Manager. Please try again");
                    return;
                }
                if (!AddDeviceEnabled)
                {
                    Debug.LogMessage(LogEventLevel.Information,
                        "All devices have been activated. Adding new devices is not allowed.");
                    return;
                }

                foreach (var dev in devicesToAdd)
                {
                    try
                    {
                        Devices.Add(dev.Key, dev);
                    }
                    catch (ArgumentException ex)
                    {
                        Debug.LogMessage(LogEventLevel.Information, "Error adding device with key {0} to Device Manager: {1}\r\nStack Trace: {2}",
                            dev.Key, ex.Message, ex.StackTrace);
                    }
                }
            }
            finally
            {
                DeviceCriticalSection.Leave();
            }
        }

        /// <summary>
        /// RemoveDevice method
        /// </summary>
        public static void RemoveDevice(IKeyed newDev)
        {
            try
            {
                DeviceCriticalSection.Enter();
                if (newDev == null)
                    return;
                if (Devices.ContainsKey(newDev.Key))
                    Devices.Remove(newDev.Key);
                //if (_Devices.Contains(newDev))
                //    _Devices.Remove(newDev);
                else
                    Debug.LogMessage(LogEventLevel.Information, "Device manager: Device '{0}' does not exist in manager.  Cannot remove", newDev.Key);
            }
            finally
            {
                DeviceCriticalSection.Leave();
            }
        }

        /// <summary>
        /// GetDeviceKeys method
        /// </summary>
        public static IEnumerable<string> GetDeviceKeys()
        {
            //return _Devices.Select(d => d.Key).ToList();
            return Devices.Keys;
        }

        /// <summary>
        /// GetDevices method
        /// </summary>
        public static IEnumerable<IKeyed> GetDevices()
        {
            //return _Devices.Select(d => d.Key).ToList();
            return Devices.Values;
        }

        /// <summary>
        /// GetDeviceForKey method
        /// </summary>
        public static IKeyed GetDeviceForKey(string key)
        {
            //return _Devices.FirstOrDefault(d => d.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (key != null && Devices.ContainsKey(key))
                return Devices[key];

            return null;
        }

        /// <summary>
        /// GetDeviceForKey method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static T GetDeviceForKey<T>(string key)
        {
            //return _Devices.FirstOrDefault(d => d.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (key == null || !Devices.ContainsKey(key))
                return default;

            if (!(Devices[key] is T))
            {
                Debug.LogMessage(LogEventLevel.Error, "Device with key '{0}' is not of type '{1}'", key, typeof(T).Name);
                return default;
            }

            return (T)Devices[key];
        }

        /// <summary>
        /// Console handler that simulates com port data receive 
        /// </summary>
        /// <param name="s"></param>
        /// <summary>
        /// SimulateComReceiveOnDevice method
        /// </summary>
        public static void SimulateComReceiveOnDevice(string s)
        {
            // devcomsim:1 xyzabc
            var match = Regex.Match(s, @"(\S*)\s*(.*)");
            if (match.Groups.Count < 3)
            {
                CrestronConsole.ConsoleCommandResponse("  Format: devsimreceive:P <device key> <string to send>");
                return;
            }
            //Debug.LogMessage(LogEventLevel.Verbose, "**** {0} - {1} ****", match.Groups[1].Value, match.Groups[2].Value);

            if (!(GetDeviceForKey(match.Groups[1].Value) is ComPortController com))
            {
                CrestronConsole.ConsoleCommandResponse("'{0}' is not a comm port device", match.Groups[1].Value);
                return;
            }
            com.SimulateReceive(match.Groups[2].Value);
        }

        /// <summary>
        /// Prints a list of routing inputs and outputs by device key.
        /// </summary>
        /// <param name="s">Device key from which to report data</param>
        /// <summary>
        /// GetRoutingPorts method
        /// </summary>
        public static void GetRoutingPorts(string s)
        {
            var device = GetDeviceForKey(s);

            if (device == null) return;
            var inputPorts = ((device as IRoutingInputs) != null) ? (device as IRoutingInputs).InputPorts : null;
            var outputPorts = ((device as IRoutingOutputs) != null) ? (device as IRoutingOutputs).OutputPorts : null;
            if (inputPorts != null)
            {
                CrestronConsole.ConsoleCommandResponse("Device {0} has {1} Input Ports:{2}", s, inputPorts.Count, CrestronEnvironment.NewLine);
                foreach (var routingInputPort in inputPorts)
                {
                    CrestronConsole.ConsoleCommandResponse("{0}{1}", routingInputPort.Key, CrestronEnvironment.NewLine);
                }
            }
            if (outputPorts == null) return;
            CrestronConsole.ConsoleCommandResponse("Device {0} has {1} Output Ports:{2}", s, outputPorts.Count, CrestronEnvironment.NewLine);
            foreach (var routingOutputPort in outputPorts)
            {
                CrestronConsole.ConsoleCommandResponse("{0}{1}", routingOutputPort.Key, CrestronEnvironment.NewLine);
            }
        }

        /// <summary>
        /// Attempts to set the debug level of a device
        /// </summary>
        /// <param name="s"></param>
        /// <summary>
        /// SetDeviceStreamDebugging method
        /// </summary>
        public static void SetDeviceStreamDebugging(string s)
        {
            if (String.IsNullOrEmpty(s) || s.Contains("?"))
            {
                CrestronConsole.ConsoleCommandResponse(
                    "SETDEVICESTREAMDEBUG [{deviceKey}] [OFF |TX | RX | BOTH] [timeOutInMinutes]\r\n" +
                    "    {deviceKey} [OFF | TX | RX | BOTH] - Device to set stream debugging on, and which setting to use\r\n" +
                    "    timeOutInMinutes - Set timeout for stream debugging. Default is 30 minutes");
                return;
            }

            var args = s.Split(' ');

            var deviceKey = args[0];
            var setting = args[1];

            var timeout = String.Empty;

            if (args.Length >= 3)
            {
                timeout = args[2];
            }


            if (!(GetDeviceForKey(deviceKey) is IStreamDebugging device))
            {
                CrestronConsole.ConsoleCommandResponse("Unable to get device with key: {0}", deviceKey);
                return;
            }

            eStreamDebuggingSetting debugSetting;

            try
            {
                debugSetting = (eStreamDebuggingSetting)Enum.Parse(typeof(eStreamDebuggingSetting), setting, true);
            }
            catch
            {
                CrestronConsole.ConsoleCommandResponse("Unable to convert setting value.  Please use off/rx/tx/both");
                return;
            }

            if (!string.IsNullOrEmpty(timeout))
            {
                try
                {
                    var min = Convert.ToUInt32(timeout);

                    device.StreamDebugging.SetDebuggingWithSpecificTimeout(debugSetting, min);
                    CrestronConsole.ConsoleCommandResponse("Device: '{0}' debug level set to {1} for {2} minutes", deviceKey, debugSetting, min);

                }
                catch (Exception e)
                {
                    CrestronConsole.ConsoleCommandResponse("Unable to convert minutes or settings value.  Please use an integer value for minutes. Error: {0}", e);
                }
            }
            else
            {
                device.StreamDebugging.SetDebuggingWithDefaultTimeout(debugSetting);
                CrestronConsole.ConsoleCommandResponse("Device: '{0}' debug level set to {1} for default time (30 minutes)", deviceKey, debugSetting);
            }
        }

        /// <summary>
        /// DisableAllDeviceStreamDebugging method
        /// </summary>
        public static void DisableAllDeviceStreamDebugging()
        {
            foreach (var device in AllDevices)
            {
                if (device is IStreamDebugging streamDevice)
                {
                    streamDevice.StreamDebugging.SetDebuggingWithDefaultTimeout(eStreamDebuggingSetting.Off);
                }
            }
        }
    }
}