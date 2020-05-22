using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// 
	/// </summary>
	public class CommFactory
	{
		public static EssentialsControlPropertiesConfig GetControlPropertiesConfig(DeviceConfig deviceConfig)
		{
			try
			{
				return JsonConvert.DeserializeObject<EssentialsControlPropertiesConfig>
					(deviceConfig.Properties["control"].ToString());
				//Debug.Console(2, "Control TEST: {0}", JsonConvert.SerializeObject(controlConfig));
			}
			catch (Exception e)
			{

				Debug.Console(0, "ERROR: [{0}] Control properties deserialize failed:\r{1}", deviceConfig.Key, e);
				return null;
			}
		}


		/// <summary>
		/// Returns a comm method of either com port, TCP, SSH, and puts this into the DeviceManager
		/// </summary>
		/// <param name="deviceConfig">The Device config object</param>
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
						comm = new ComPortController(deviceConfig.Key + "-com", GetComPort, controlConfig.ComParams, controlConfig);
						break;
                    case eControlMethod.Cec:
                        comm = new CecPortController(deviceConfig.Key + "-cec", GetCecPort, controlConfig);
                        break;
					case eControlMethod.IR:
						break;
                    case eControlMethod.Ssh:
                        {
                            var ssh = new GenericSshClient(deviceConfig.Key + "-ssh", c.Address, c.Port, c.Username, c.Password);
                            ssh.AutoReconnect = c.AutoReconnect;
                            if(ssh.AutoReconnect)
                                ssh.AutoReconnectIntervalMs = c.AutoReconnectIntervalMs;
                            comm = ssh;
                            break;
                        }
                    case eControlMethod.Tcpip:
                        {
                            var tcp = new GenericTcpIpClient(deviceConfig.Key + "-tcp", c.Address, c.Port, c.BufferSize);
                            tcp.AutoReconnect = c.AutoReconnect;
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
					default:
						break;
				}				
			}
			catch (Exception e)
			{
				Debug.Console(0, "Cannot create communication from JSON:\r{0}\r\rException:\r{1}",
					deviceConfig.Properties.ToString(), e);
			}

			// put it in the device manager if it's the right flavor
			var comDev = comm as Device;
			if (comDev != null)
				DeviceManager.AddDevice(comDev);
			return comm;
		}

		public static ComPort GetComPort(EssentialsControlPropertiesConfig config)
		{
			var comPar = config.ComParams;
			var dev = GetIComPortsDeviceFromManagedDevice(config.ControlPortDevKey);
			if (dev != null && config.ControlPortNumber <= dev.NumberOfComPorts)
				return dev.ComPorts[config.ControlPortNumber];
			Debug.Console(0, "GetComPort: Device '{0}' does not have com port {1}", config.ControlPortDevKey, config.ControlPortNumber);
			return null;
		}

        /// <summary>
        ///  Gets an ICec port from a RoutingInput or RoutingOutput on a device
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static ICec GetCecPort(ControlPropertiesConfig config)
        {
            var dev = DeviceManager.GetDeviceForKey(config.ControlPortDevKey);

            if (dev != null)
            {          
                var inputPort = (dev as IRoutingInputsOutputs).InputPorts[config.ControlPortName];

                if (inputPort != null)
                    if (inputPort.Port is ICec)
                        return inputPort.Port as ICec;

                var outputPort = (dev as IRoutingInputsOutputs).OutputPorts[config.ControlPortName];

                if (outputPort != null)
                    if (outputPort.Port is ICec)
                        return outputPort.Port as ICec;
            }
            Debug.Console(0, "GetCecPort: Device '{0}' does not have a CEC port called: '{1}'", config.ControlPortDevKey, config.ControlPortName);
            return null;
        }

		/// <summary>
		/// Helper to grab the IComPorts device for this PortDeviceKey. Key "controlSystem" will
		/// return the ControlSystem object from the Global class.
		/// </summary>
		/// <returns>IComPorts device or null if the device is not found or does not implement IComPorts</returns>
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
					Debug.Console(0, "ComPortConfig: Cannot find com port device '{0}'", ComPortDevKey);
				return dev;
			}
		}
	}

    /// <summary>
    /// 
    /// </summary>
    public class EssentialsControlPropertiesConfig : 
        PepperDash.Core.ControlPropertiesConfig
    {

        [JsonConverter(typeof(ComSpecJsonConverter))]
        public ComPort.ComPortSpec ComParams { get; set; }

		public string CresnetId { get; set; }

        /// <summary>
        /// Attempts to provide uint conversion of string CresnetId
        /// </summary>
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

        public string InfinetId { get; set; }

        /// <summary>
        /// Attepmts to provide uiont conversion of string InifinetId
        /// </summary>
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

    public class IrControlSpec
    {
        public string PortDeviceKey { get; set; }
        public uint PortNumber { get; set; }
        public string File { get; set; }
    }
}