using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.EthernetCommunication;
using Crestron.SimplSharpPro.UI;
using Crestron.SimplSharp.Reflection;

using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
	public static class DeviceManager
	{
	    private static readonly CCriticalSection DeviceCriticalSection = new CCriticalSection();
		//public static List<Device> Devices { get { return _Devices; } }
		//static List<Device> _Devices = new List<Device>();

		static Dictionary<string, IKeyed> Devices = new Dictionary<string, IKeyed>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Returns a copy of all the devices in a list
		/// </summary>
		public static List<IKeyed> AllDevices { get { return new List<IKeyed>(Devices.Values); } }

		public static void Initialize(CrestronControlSystem cs)
		{
			CrestronConsole.AddNewConsoleCommand(ListDeviceCommStatuses, "devcommstatus", "Lists the communication status of all devices",
				ConsoleAccessLevelEnum.AccessOperator);
			CrestronConsole.AddNewConsoleCommand(ListDeviceFeedbacks, "devfb", "Lists current feedbacks", 
				ConsoleAccessLevelEnum.AccessOperator);
			CrestronConsole.AddNewConsoleCommand(ListDevices, "devlist", "Lists current managed devices", 
				ConsoleAccessLevelEnum.AccessOperator);
			CrestronConsole.AddNewConsoleCommand(DeviceJsonApi.DoDeviceActionWithJson, "devjson", "", 
				ConsoleAccessLevelEnum.AccessOperator);
			CrestronConsole.AddNewConsoleCommand(s =>
			{
				CrestronConsole.ConsoleCommandResponse(DeviceJsonApi.GetProperties(s));
			}, "devprops", "", ConsoleAccessLevelEnum.AccessOperator);
			CrestronConsole.AddNewConsoleCommand(s =>
			{
				CrestronConsole.ConsoleCommandResponse(DeviceJsonApi.GetMethods(s));
			}, "devmethods", "", ConsoleAccessLevelEnum.AccessOperator);
			CrestronConsole.AddNewConsoleCommand(s =>
			{
				CrestronConsole.ConsoleCommandResponse(DeviceJsonApi.GetApiMethods(s));
			}, "apimethods", "", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(SimulateComReceiveOnDevice, "devsimreceive",
                "Simulates incoming data on a com device", ConsoleAccessLevelEnum.AccessOperator);
		}

		/// <summary>
		/// Calls activate steps on all Device class items
		/// </summary>
		public static void ActivateAll()
		{
		    try
		    {
		        DeviceCriticalSection.Enter();
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
		        foreach (var d in Devices.Values)
		        {
		            if (d is Device)
		                (d as Device).Deactivate();
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

		static void ListDevices(string s)
		{
		    try
		    {
                DeviceCriticalSection.Enter();
		        Debug.Console(0, "{0} Devices registered with Device Manager:", Devices.Count);
		        var sorted = Devices.Values.ToList();
		        sorted.Sort((a, b) => a.Key.CompareTo(b.Key));

		        foreach (var d in sorted)
		        {
		            var name = d is IKeyName ? (d as IKeyName).Name : "---";
		            Debug.Console(0, "  [{0}] {1}", d.Key, name);
		        }
		    }
            finally {DeviceCriticalSection.Leave();}
		}

		static void ListDeviceFeedbacks(string devKey)
		{
		    try
		    {
                DeviceCriticalSection.Enter();
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
		    finally
		    {
		        DeviceCriticalSection.Leave();
		    }
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

		static void ListDeviceCommStatuses(string input)
		{
		    try
		    {
		        DeviceCriticalSection.Enter();

		        var sb = new StringBuilder();
		        foreach (var dev in Devices.Values)
		        {
		            if (dev is ICommunicationMonitor)
		                sb.Append(string.Format("{0}: {1}\r", dev.Key,
		                    (dev as ICommunicationMonitor).CommunicationMonitor.Status));
		        }
		        CrestronConsole.ConsoleCommandResponse(sb.ToString());
		    }
		    finally
		    {
		        DeviceCriticalSection.Leave();
		    }
		}


        //static void DoDeviceCommand(string command)
        //{
        //    Debug.Console(0, "Not yet implemented.  Stay tuned");
        //}

		public static void AddDevice(IKeyed newDev)
		{
		    try
		    {
                DeviceCriticalSection.Enter();
		        // Check for device with same key
		        //var existingDevice = _Devices.FirstOrDefault(d => d.Key.Equals(newDev.Key, StringComparison.OrdinalIgnoreCase));
		        ////// If it exists, remove or warn??
		        //if (existingDevice != null)
		        if (Devices.ContainsKey(newDev.Key))
		        {
		            Debug.Console(0, newDev, "WARNING: A device with this key already exists.  Not added to manager");
		            return;
		        }
		        Devices.Add(newDev.Key, newDev);
		        //if (!(_Devices.Contains(newDev)))
		        //    _Devices.Add(newDev);
		    }
            finally {DeviceCriticalSection.Leave();}
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
		    try
		    {
                DeviceCriticalSection.Enter();
		        //return _Devices.Select(d => d.Key).ToList();
		        return Devices.Keys;
		    }
		    finally
		    {
		        DeviceCriticalSection.Leave();
		    }
		}

		public static IEnumerable<IKeyed> GetDevices()
		{
		    try
		    {
                DeviceCriticalSection.Enter();
		        //return _Devices.Select(d => d.Key).ToList();
		        return Devices.Values;
		    }
            finally {DeviceCriticalSection.Leave();}
		}

		public static IKeyed GetDeviceForKey(string key)
		{
		    try
		    {
                DeviceCriticalSection.Enter();
		        //return _Devices.FirstOrDefault(d => d.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
		        if (key != null && Devices.ContainsKey(key))
		            return Devices[key];

		        return null;
		    }
		    finally
		    {
		        DeviceCriticalSection.Leave();
		    }
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

            ComPortController com = GetDeviceForKey(match.Groups[1].Value) as ComPortController;
            if (com == null)
            {
                CrestronConsole.ConsoleCommandResponse("'{0}' is not a comm port device", match.Groups[1].Value);
                return;
            }
            com.SimulateReceive(match.Groups[2].Value);
        }
	}
}