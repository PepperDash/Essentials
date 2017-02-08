using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices;
using PepperDash.Core;

namespace PepperDash.Essentials
{
	public static class FactoryHelper
	{
		public static string IrDriverPathPrefix = @"\NVRAM\IR\";

		public static void HandleUnknownType(JToken devToken, string type)
		{
			Debug.Console(0, "[Config] ERROR: Type '{0}' not found in group '{1}'", type,
				devToken.Value<string>("group"));
		}

		public static void HandleDeviceCreationError(JToken devToken, Exception e)
		{
			Debug.Console(0, "[Config] ERROR creating device [{0}]: \r{1}",
				devToken["key"].Value<string>(), e);
			Debug.Console(0, "Relevant config:\r{0}", devToken.ToString(Newtonsoft.Json.Formatting.Indented));
		}

		/// <summary>
		/// Finds either the ControlSystem or a device controller that contains IR ports and
		/// returns a port from the hardware device
		/// </summary>
		/// <param name="propsToken"></param>
		/// <returns>Crestron IrPort or null if device doesn't have IR or is not found</returns>
		public static IrOutPortConfig GetIrPort(JToken propsToken)
		{
			var irSpec = propsToken["control"]["irSpec"];
			var portDevKey = irSpec.Value<string>("portDeviceKey");
			var portNum = irSpec.Value<uint>("portNumber");
			IIROutputPorts irDev = null;
			if (portDevKey.Equals("controlSystem", StringComparison.OrdinalIgnoreCase)
				|| portDevKey.Equals("processor", StringComparison.OrdinalIgnoreCase))
				irDev = Global.ControlSystem;
			else
				irDev = DeviceManager.GetDeviceForKey(portDevKey) as IIROutputPorts;

			if (irDev == null)
			{
				Debug.Console(0, "[Config] Error, device with IR ports '{0}' not found", portDevKey);
				return null;
			}

			if (portNum <= irDev.NumberOfIROutputPorts) // success!
			{
				var file = IrDriverPathPrefix + irSpec["file"].Value<string>();
				return new IrOutPortConfig { Port = irDev.IROutputPorts[portNum], FileName = file };
			}
			else
			{
				Debug.Console(0, "[Config] Error, device '{0}' IR port {1} out of range",
					portDevKey, portNum);
				return null;
			}
		}


		/// <summary>
		/// Finds either the ControlSystem or a device controller that contains com ports and
		/// returns a port from the hardware device
		/// </summary>
		/// <param name="propsToken">The Properties token from the device's config</param>
		/// <returns>Crestron ComPort or null if device doesn't have IR or is not found</returns>
		public static ComPort GetComPort(JToken propsToken)
		{
			var portDevKey = propsToken.Value<string>("comPortDevice");
			var portNum = propsToken.Value<uint>("comPortNumber");
			IComPorts comDev = null;
			if (portDevKey.Equals("controlSystem", StringComparison.OrdinalIgnoreCase))
				comDev = Global.ControlSystem;
			else
				comDev = DeviceManager.GetDeviceForKey(portDevKey) as IComPorts;

			if (comDev == null)
			{
				Debug.Console(0, "[Config] Error, device with com ports '{0}' not found", portDevKey);
				return null;
			}

			if (portNum <= comDev.NumberOfComPorts) // success!
				return comDev.ComPorts[portNum];
			else
			{
				Debug.Console(0, "[Config] Error, device '{0}' com port {1} out of range",
					portDevKey, portNum);
				return null;
			}
		}

		/// <summary>
		/// Returns the key if it exists or converts the name into a key
		/// </summary>
		public static string KeyOrConvertName(string key, string name)
		{
			if (string.IsNullOrEmpty(key))
				return name.Replace(' ', '-');
			return key;
		}
	}

	/// <summary>
	/// Wrapper to help in IR port creation
	/// </summary>
	public class IrOutPortConfig
	{
		public IROutputPort Port { get; set; }
		public string FileName { get; set; }
	}
}