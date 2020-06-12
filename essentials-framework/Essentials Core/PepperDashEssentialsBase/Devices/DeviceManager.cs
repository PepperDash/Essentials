using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
	public static class DeviceManager
	{
	    private static readonly CCriticalSection DeviceCriticalSection = new CCriticalSection();
	    private static readonly CEvent AllowAddDevicesCEvent = new CEvent(false, true);
		//public static List<Device> Devices { get { return _Devices; } }
		//static List<Device> _Devices = new List<Device>();

		static readonly Dictionary<string, IKeyed> Devices = new Dictionary<string, IKeyed>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Returns a copy of all the devices in a list
		/// </summary>
		public static List<IKeyed> AllDevices { get { return new List<IKeyed>(Devices.Values); } }

	    public static bool AddDeviceEnabled;

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
			CrestronConsole.AddNewConsoleCommand(s => CrestronConsole.ConsoleCommandResponse(DeviceJsonApi.GetProperties(s)), "devprops", "", ConsoleAccessLevelEnum.AccessOperator);
			CrestronConsole.AddNewConsoleCommand(s => CrestronConsole.ConsoleCommandResponse(DeviceJsonApi.GetMethods(s)), "devmethods", "", ConsoleAccessLevelEnum.AccessOperator);
			CrestronConsole.AddNewConsoleCommand(s => CrestronConsole.ConsoleCommandResponse(DeviceJsonApi.GetApiMethods(s)), "apimethods", "", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(SimulateComReceiveOnDevice, "devsimreceive",
                "Simulates incoming data on a com device", ConsoleAccessLevelEnum.AccessOperator);

            CrestronConsole.AddNewConsoleCommand(s => SetDeviceStreamDebugging(s), "setdevicestreamdebug", "set comm debug [deviceKey] [off/rx/tx/both] ([minutes])", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(s => DisableAllDeviceStreamDebugging(), "disableallstreamdebug", "disables stream debugging on all devices", ConsoleAccessLevelEnum.AccessOperator);
        }

		/// <summary>
		/// Calls activate steps on all Device class items
		/// </summary>
		public static void ActivateAll()
		{
		    try
		    {
		        DeviceCriticalSection.Enter();
                AddDeviceEnabled = false;
		        // PreActivate all devices
		        foreach (var d in Devices.Values)
		        {
		            try
		            {
		                if (d is Device)
		                    (d as Device).PreActivate();
		            }
		            catch (Exception e)
		            {
		                Debug.Console(0, d, "ERROR: Device PreActivation failure:\r{0}", e);
		            }
		        }

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
		                Debug.Console(0, d, "ERROR: Device Activation failure:\r{0}", e);
		            }
		        }

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
		                Debug.Console(0, d, "ERROR: Device PostActivation failure:\r{0}", e);
		            }
		        }
		    }
		    finally
		    {
                DeviceCriticalSection.Leave();
		    }
		}

		/// <summary>
		/// Calls activate on all Device class items
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

		//static void ListMethods(string devKey)
		//{
		//    var dev = GetDeviceForKey(devKey);
		//    if(dev != null)
		//    {
		//        var type = dev.GetType().GetCType();
		//        var methods = type.GetMethods(BindingFlags.Public|BindingFlags.Instance);
		//        var sb = new StringBuilder();
		//        sb.AppendLine(string.Format("{2} methods on [{0}] ({1}):", dev.Key, type.Name, methods.Length));
		//        foreach (var m in methods)
		//        {
		//            sb.Append(string.Format("{0}(", m.Name));
		//            var pars = m.GetParameters();
		//            foreach (var p in pars)
		//                sb.Append(string.Format("({1}){0} ", p.Name, p.ParameterType.Name));
		//            sb.AppendLine(")");
		//        }
		//        CrestronConsole.ConsoleCommandResponse(sb.ToString());
		//    }
		//}

	    private static void ListDevices(string s)
	    {
	        Debug.Console(0, "{0} Devices registered with Device Manager:", Devices.Count);
	        var sorted = Devices.Values.ToList();
	        sorted.Sort((a, b) => a.Key.CompareTo(b.Key));

	        foreach (var d in sorted)
	        {
	            var name = d is IKeyName ? (d as IKeyName).Name : "---";
	            Debug.Console(0, "  [{0}] {1}", d.Key, name);
	        }
	    }

	    private static void ListDeviceFeedbacks(string devKey)
	    {
	        var dev = GetDeviceForKey(devKey);
	        if (dev == null)
	        {
	            Debug.Console(0, "Device '{0}' not found", devKey);
	            return;
	        }
	        var statusDev = dev as IHasFeedback;
	        if (statusDev == null)
	        {
	            Debug.Console(0, "Device '{0}' does not have visible feedbacks", devKey);
	            return;
	        }
	        statusDev.DumpFeedbacksToConsole(true);
	    }

	    //static void ListDeviceCommands(string devKey)
        //{
        //    var dev = GetDeviceForKey(devKey);
        //    if (dev == null)
        //    {
        //        Debug.Console(0, "Device '{0}' not found", devKey);
        //        return;
        //    }
        //    Debug.Console(0, "This needs to be reworked.  Stay tuned.", devKey);
        //}

	    private static void ListDeviceCommStatuses(string input)
	    {
	        var sb = new StringBuilder();
	        foreach (var dev in Devices.Values.OfType<ICommunicationMonitor>())
	        {
	            sb.Append(string.Format("{0}: {1}\r", dev,
	                dev.CommunicationMonitor.Status));
	        }
	        CrestronConsole.ConsoleCommandResponse(sb.ToString());
	    }


	    //static void DoDeviceCommand(string command)
        //{
        //    Debug.Console(0, "Not yet implemented.  Stay tuned");
        //}

		public static void AddDevice(IKeyed newDev)
		{
		    try
		    {
		        if (!DeviceCriticalSection.TryEnter())
		        {
		            Debug.Console(0, Debug.ErrorLogLevel.Error, "Currently unable to add devices to Device Manager. Please try again");
		            return;
		        }
		        // Check for device with same key
		        //var existingDevice = _Devices.FirstOrDefault(d => d.Key.Equals(newDev.Key, StringComparison.OrdinalIgnoreCase));
		        ////// If it exists, remove or warn??
		        //if (existingDevice != null)

		        if (!AddDeviceEnabled)
		        {
		            Debug.Console(0, Debug.ErrorLogLevel.Error, "All devices have been activated. Adding new devices is not allowed.");
		            return;
		        }

		        if (Devices.ContainsKey(newDev.Key))
		        {
		            Debug.Console(0, newDev, "WARNING: A device with this key already exists.  Not added to manager");
		            return;
		        }
		        Devices.Add(newDev.Key, newDev);
		        //if (!(_Devices.Contains(newDev)))
		        //    _Devices.Add(newDev);
		    }
		    finally
		    {
		        DeviceCriticalSection.Leave();
		    }
		}

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
		            Debug.Console(0, "Device manager: Device '{0}' does not exist in manager.  Cannot remove", newDev.Key);
		    }
		    finally
		    {
		        DeviceCriticalSection.Leave();
		    }
		}

		public static IEnumerable<string> GetDeviceKeys()
		{
		        //return _Devices.Select(d => d.Key).ToList();
		        return Devices.Keys;
		}

		public static IEnumerable<IKeyed> GetDevices()
		{
		        //return _Devices.Select(d => d.Key).ToList();
		        return Devices.Values;
		}

		public static IKeyed GetDeviceForKey(string key)
		{
		        //return _Devices.FirstOrDefault(d => d.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
		        if (key != null && Devices.ContainsKey(key))
		            return Devices[key];

		        return null;
		}

        /// <summary>
        /// Console handler that simulates com port data receive 
        /// </summary>
        /// <param name="s"></param>
        public static void SimulateComReceiveOnDevice(string s)
        {
            // devcomsim:1 xyzabc
            var match = Regex.Match(s, @"(\S*)\s*(.*)");
            if (match.Groups.Count < 3)
            {
                CrestronConsole.ConsoleCommandResponse("  Format: devsimreceive:P <device key> <string to send>");
                return;
            }
            //Debug.Console(2, "**** {0} - {1} ****", match.Groups[1].Value, match.Groups[2].Value);

            var com = GetDeviceForKey(match.Groups[1].Value) as ComPortController;
            if (com == null)
            {
                CrestronConsole.ConsoleCommandResponse("'{0}' is not a comm port device", match.Groups[1].Value);
                return;
            }
            com.SimulateReceive(match.Groups[2].Value);
        }

        /// <summary>
        /// Attempts to set the debug level of a device
        /// </summary>
        /// <param name="s"></param>
        public static void SetDeviceStreamDebugging(string s)
        {
            var args = s.Split(' ');

            var deviceKey = args[0];
            var setting = args[1];

            var timeout= String.Empty;

            if (args.Length >= 3)
            {
                timeout = args[2];
            }

            var device = GetDeviceForKey(deviceKey) as IStreamDebugging;

            if (device == null)
            {
                Debug.Console(0, "Unable to get device with key: {0}", deviceKey);
                return;
            }

            eStreamDebuggingSetting debugSetting;

            try
            {
                debugSetting = (eStreamDebuggingSetting)Enum.Parse(typeof(eStreamDebuggingSetting), setting, true);
            }
            catch
            {
                Debug.Console(0, "Unable to convert setting value.  Please use off/rx/tx/both");
                return;
            }

            if (!string.IsNullOrEmpty(timeout))
            {
                try
                {
                    var min = Convert.ToUInt32(timeout);

                    device.StreamDebugging.SetDebuggingWithSpecificTimeout(debugSetting, min);
                    Debug.Console(0, "Device: '{0}' debug level set to {1) for {2} minutes", deviceKey, debugSetting, min);

                }
                catch (Exception e)
                {
                    Debug.Console(0, "Unable to convert minutes or settings value.  Please use an integer value for minutes. Errro: {0}", e);
                }
            }
            else
            {
                device.StreamDebugging.SetDebuggingWithDefaultTimeout(debugSetting);
                Debug.Console(0, "Device: '{0}' debug level set to {1) for default time (30 minutes)", deviceKey, debugSetting);
            }
        }

        /// <summary>
        /// Sets stream debugging settings to off for all devices
        /// </summary>
        public static void DisableAllDeviceStreamDebugging()
        {
            foreach (var device in AllDevices)
            {
                var streamDevice = device as IStreamDebugging;

                if (streamDevice != null)
                {
                    streamDevice.StreamDebugging.SetDebuggingWithDefaultTimeout(eStreamDebuggingSetting.Off);
                }
            }
        }
	}
}