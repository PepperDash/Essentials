

using System;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// 
	/// </summary>
	public class CommFactory
	{
		/// <summary>
		/// GetControlPropertiesConfig method
		/// </summary>
		/// <param name="deviceConfig">The Device config object</param>
		/// <returns>EssentialsControlPropertiesConfig object</returns>
		public static EssentialsControlPropertiesConfig GetControlPropertiesConfig(DeviceConfig deviceConfig)
		{
			try
			{
				return JsonConvert.DeserializeObject<EssentialsControlPropertiesConfig>
					(deviceConfig.Properties["control"].ToString());
				//Debug.LogMessage(LogEventLevel.Verbose, "Control TEST: {0}", JsonConvert.SerializeObject(controlConfig));
			}
			catch (Exception e)
			{

				Debug.LogMessage(LogEventLevel.Information, "ERROR: [{0}] Control properties deserialize failed:\r{1}", deviceConfig.Key, e);
				return null;
			}
		}


		/// <summary>
		/// Returns a comm method of either com port, TCP, SSH, and puts this into the DeviceManager
		/// </summary>
		/// <param name="deviceConfig">The Device config object</param>
		/// <summary>
		/// CreateCommForDevice method
		/// </summary>
		public static IBasicCommunication CreateCommForDevice(DeviceConfig deviceConfig)
		{
			EssentialsControlPropertiesConfig controlConfig = GetControlPropertiesConfig(deviceConfig);
			if (controlConfig == null)
				return null;

			IBasicCommunication comm = null;
			try
			{
				var c = controlConfig.TcpSshProperties;
				switch (controlConfig.Method)
				{
					case eControlMethod.Com:
						comm = new ComPortController(deviceConfig.Key + "-com", GetComPort, controlConfig.ComParams.Value, controlConfig);
						break;
					case eControlMethod.ComBridge:
						comm = new CommBridge(deviceConfig.Key + "-simpl", deviceConfig.Name + " Simpl");
						break;
					case eControlMethod.Cec:
						comm = new CecPortController(deviceConfig.Key + "-cec", GetCecPort, controlConfig);
						break;
					case eControlMethod.IR:
						break;
					case eControlMethod.Ssh:
						{
							var ssh = new GenericSshClient(deviceConfig.Key + "-ssh", c.Address, c.Port, c.Username, c.Password)
							{
								AutoReconnect = c.AutoReconnect,
								DisableEcho = c.DisableSshEcho
							};
							if (ssh.AutoReconnect)
								ssh.AutoReconnectIntervalMs = c.AutoReconnectIntervalMs;
							comm = ssh;
							break;
						}
					case eControlMethod.Tcpip:
						{
							var tcp = new GenericTcpIpClient(deviceConfig.Key + "-tcp", c.Address, c.Port, c.BufferSize)
							{
								AutoReconnect = c.AutoReconnect
							};
							if (tcp.AutoReconnect)
								tcp.AutoReconnectIntervalMs = c.AutoReconnectIntervalMs;
							comm = tcp;
							break;
						}
					case eControlMethod.Udp:
						{
							var udp = new GenericUdpServer(deviceConfig.Key + "-udp", c.Address, c.Port, c.BufferSize);
							comm = udp;
							break;
						}
					case eControlMethod.Telnet:
						break;
					case eControlMethod.SecureTcpIp:
						{
							var secureTcp = new GenericSecureTcpIpClient(deviceConfig.Key + "-secureTcp", c.Address, c.Port, c.BufferSize)
							{
								AutoReconnect = c.AutoReconnect
							};
							if (secureTcp.AutoReconnect)
								secureTcp.AutoReconnectIntervalMs = c.AutoReconnectIntervalMs;
							comm = secureTcp;
							break;
						}
					default:
						break;
				}
			}
			catch (Exception e)
			{
				Debug.LogMessage(LogEventLevel.Information, "Cannot create communication from JSON:\r{0}\r\rException:\r{1}",
					deviceConfig.Properties.ToString(), e);
			}

			// put it in the device manager if it's the right flavor
			if (comm is Device comDev)
				DeviceManager.AddDevice(comDev);
			return comm;
		}

		/// <summary>
		/// GetComPort method
		/// </summary>
		public static ComPort GetComPort(EssentialsControlPropertiesConfig config)
		{
			var comPar = config.ComParams;
			var dev = GetIComPortsDeviceFromManagedDevice(config.ControlPortDevKey);
			if (dev != null && config.ControlPortNumber <= dev.NumberOfComPorts)
				return dev.ComPorts[config.ControlPortNumber.Value];
			Debug.LogMessage(LogEventLevel.Information, "GetComPort: Device '{0}' does not have com port {1}", config.ControlPortDevKey, config.ControlPortNumber);
			return null;
		}

		/// <summary>
		///  Gets an ICec port from a RoutingInput or RoutingOutput on a device
		/// </summary>
		/// <param name="config"></param>
		/// <returns></returns>
		/// <summary>
		/// GetCecPort method
		/// </summary>
		public static ICec GetCecPort(ControlPropertiesConfig config)
		{
			try
			{
				var dev = DeviceManager.GetDeviceForKey(config.ControlPortDevKey);

				Debug.LogMessage(LogEventLevel.Information, "GetCecPort: device '{0}' {1}", config.ControlPortDevKey, dev == null
					? "is not valid, failed to get cec port"
					: "found in device manager, attempting to get cec port");

				if (dev == null)
					return null;

				if (String.IsNullOrEmpty(config.ControlPortName))
				{
					Debug.LogMessage(LogEventLevel.Information, "GetCecPort: '{0}' - Configuration missing 'ControlPortName'", config.ControlPortDevKey);
					return null;
				}


				var inputsOutputs = dev as IRoutingInputsOutputs;
				if (inputsOutputs == null)
				{
					Debug.LogMessage(LogEventLevel.Information, "GetCecPort: Device '{0}' does not support IRoutingInputsOutputs, failed to get CEC port called '{1}'",
						config.ControlPortDevKey, config.ControlPortName);

					return null;
				}

				var inputPort = inputsOutputs.InputPorts[config.ControlPortName];
				if (inputPort != null && inputPort.Port is ICec)
					return inputPort.Port as ICec;


				var outputPort = inputsOutputs.OutputPorts[config.ControlPortName];
				if (outputPort != null && outputPort.Port is ICec)
					return outputPort.Port as ICec;
			}
			catch (Exception ex)
			{
				Debug.LogMessage(LogEventLevel.Debug, "GetCecPort Exception Message: {0}", ex.Message);
				Debug.LogMessage(LogEventLevel.Verbose, "GetCecPort Exception StackTrace: {0}", ex.StackTrace);
				if (ex.InnerException != null)
					Debug.LogMessage(LogEventLevel.Information, "GetCecPort Exception InnerException: {0}", ex.InnerException);
			}

			Debug.LogMessage(LogEventLevel.Information, "GetCecPort: Device '{0}' does not have a CEC port called '{1}'",
					config.ControlPortDevKey, config.ControlPortName);

			return null;
		}

		/// <summary>
		/// Helper to grab the IComPorts device for this PortDeviceKey. Key "controlSystem" will
		/// return the ControlSystem object from the Global class.
		/// </summary>
		/// <returns>IComPorts device or null if the device is not found or does not implement IComPorts</returns>
		/// <summary>
		/// GetIComPortsDeviceFromManagedDevice method
		/// </summary>
		public static IComPorts GetIComPortsDeviceFromManagedDevice(string ComPortDevKey)
		{
			if ((ComPortDevKey.Equals("controlSystem", System.StringComparison.OrdinalIgnoreCase)
				|| ComPortDevKey.Equals("processor", System.StringComparison.OrdinalIgnoreCase))
				&& Global.ControlSystem is IComPorts)
				return Global.ControlSystem;
			else
			{
				var dev = DeviceManager.GetDeviceForKey(ComPortDevKey) as IComPorts;
				if (dev == null)
					Debug.LogMessage(LogEventLevel.Information, "ComPortConfig: Cannot find com port device '{0}'", ComPortDevKey);
				return dev;
			}
		}
	}

	/// <summary>
	/// Represents a EssentialsControlPropertiesConfig
	/// </summary>
	public class EssentialsControlPropertiesConfig :
			ControlPropertiesConfig
	{
		/// <summary>
		/// Gets or sets the ComParams
		/// </summary>
		[JsonProperty("comParams", NullValueHandling = NullValueHandling.Ignore)]
		[JsonConverter(typeof(ComSpecJsonConverter))]
		public ComPort.ComPortSpec? ComParams { get; set; }

		/// <summary>
		/// Gets or sets the CresnetId
		/// </summary>
		[JsonProperty("cresnetId", NullValueHandling = NullValueHandling.Ignore)]
		public string CresnetId { get; set; }

		/// <summary>
		/// Attempts to provide uint conversion of string CresnetId
		/// </summary>
		[JsonIgnore]
		public uint CresnetIdInt
		{
			get
			{
				try
				{
					return Convert.ToUInt32(CresnetId, 16);
				}
				catch (Exception)
				{
					throw new FormatException(string.Format("ERROR:Unable to convert Cresnet ID: {0} to hex.  Error:\n{1}", CresnetId));
				}
			}
		}

		/// <summary>
		/// Gets or sets the InfinetId
		/// </summary>
		[JsonProperty("infinetId", NullValueHandling = NullValueHandling.Ignore)]
		public string InfinetId { get; set; }

		/// <summary>
		/// Attepmts to provide uiont conversion of string InifinetId
		/// </summary>
		[JsonIgnore]
		public uint InfinetIdInt
		{
			get
			{
				try
				{
					return Convert.ToUInt32(InfinetId, 16);
				}
				catch (Exception)
				{
					throw new FormatException(string.Format("ERROR:Unable to conver Infinet ID: {0} to hex.  Error:\n{1}", InfinetId));
				}
			}
		}
	}

	/// <summary>
	/// Represents a IrControlSpec
	/// </summary>
	public class IrControlSpec
	{
		/// <summary>
		/// Gets or sets the PortDeviceKey
		/// </summary>
		public string PortDeviceKey { get; set; }
		/// <summary>
		/// Gets or sets the PortNumber
		/// </summary>
		public uint PortNumber { get; set; }
		/// <summary>
		/// Gets or sets the File
		/// </summary>
		public string File { get; set; }
	}
}