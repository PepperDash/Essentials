using System;
using Crestron.SimplSharpPro;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Devices;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Communications
{
	/// <summary>
	/// 
	/// </summary>
	public static class IRPortHelper
	{
		public static string IrDriverPathPrefix
		{
			get
			{
				return Global.FilePathPrefix + "IR" + Global.DirectorySeparator;
			}
		}

		/// <summary>
		/// Finds either the ControlSystem or a device controller that contains IR ports and
		/// returns a port from the hardware device
		/// </summary>
		/// <param name="propsToken"></param>
		/// <returns>IrPortConfig object.  The port and or filename will be empty/null 
		/// if valid values don't exist on config</returns>
		public static IrOutPortConfig GetIrPort(JToken propsToken)
		{
			var control = propsToken["control"];
			if (control == null)
				return null;
			if (control["method"].Value<string>() != "ir")
			{
				Debug.LogMessage(LogEventLevel.Information, "IRPortHelper called with non-IR properties");
				return null;
			}

			var port = new IrOutPortConfig();

			var portDevKey = control.Value<string>("controlPortDevKey");
			var portNum = control.Value<uint>("controlPortNumber");
			if (portDevKey == null || portNum == 0)
			{
				Debug.LogMessage(LogEventLevel.Debug, "WARNING: Properties is missing port device or port number");
				return port;
			}

			IIROutputPorts irDev = null;
			if (portDevKey.Equals("controlSystem", StringComparison.OrdinalIgnoreCase)
				|| portDevKey.Equals("processor", StringComparison.OrdinalIgnoreCase))
				irDev = Global.ControlSystem;
			else
				irDev = DeviceManager.GetDeviceForKey(portDevKey) as IIROutputPorts;

			if (irDev == null)
			{
				Debug.LogMessage(LogEventLevel.Debug, "[Config] Error, device with IR ports '{0}' not found", portDevKey);
				return port;
			}

			if (portNum <= irDev.NumberOfIROutputPorts) // success!
			{
				var file = IrDriverPathPrefix + control["irFile"].Value<string>();
				port.Port = irDev.IROutputPorts[portNum];
				port.FileName = file;
				return port; // new IrOutPortConfig { Port = irDev.IROutputPorts[portNum], FileName = file };
			}
			else
			{
				Debug.LogMessage(LogEventLevel.Debug, "[Config] Error, device '{0}' IR port {1} out of range",
					portDevKey, portNum);
				return port;
			}
		}

	    public static IROutputPort GetIrOutputPort(DeviceConfig dc)
	    {
	        var irControllerKey = dc.Key + "-ir";
	        if (dc.Properties == null)
	        {
	            Debug.LogMessage(LogEventLevel.Information, "[{0}] WARNING: Device config does not include properties.  IR will not function.", dc.Key);
	            return null;
	        }

	        var control = dc.Properties["control"];
	        if (control == null)
	        {
	            Debug.LogMessage(LogEventLevel.Information,
	                "WARNING: Device config does not include control properties.  IR will not function for {0}", dc.Key);
	            return null;
	        }

	        var portDevKey = control.Value<string>("controlPortDevKey");
	        var portNum = control.Value<uint>("controlPortNumber");
	        IIROutputPorts irDev = null;

	        if (portDevKey == null)
	        {
	            Debug.LogMessage(LogEventLevel.Information, "WARNING: control properties is missing ir device for {0}", dc.Key);
	            return null;
	        }

	        if (portNum == 0)
	        {
	            Debug.LogMessage(LogEventLevel.Information, "WARNING: control properties is missing ir port number for {0}", dc.Key);
	            return null;
	        }

	        if (portDevKey.Equals("controlSystem", StringComparison.OrdinalIgnoreCase)
	            || portDevKey.Equals("processor", StringComparison.OrdinalIgnoreCase))
	            irDev = Global.ControlSystem;
	        else
	            irDev = DeviceManager.GetDeviceForKey(portDevKey) as IIROutputPorts;

	        if (irDev == null)
	        {
	            Debug.LogMessage(LogEventLevel.Information, "WARNING: device with IR ports '{0}' not found", portDevKey);
	            return null;
	        }
	        if (portNum > irDev.NumberOfIROutputPorts)
	        {
	            Debug.LogMessage(LogEventLevel.Information, "WARNING: device '{0}' IR port {1} out of range",
	                portDevKey, portNum);
	            return null;
	        }


	        var port = irDev.IROutputPorts[portNum];

            

	        return port;
	    }

	    public static IrOutputPortController GetIrOutputPortController(DeviceConfig config)
	    {
            Debug.LogMessage(LogEventLevel.Debug, "Attempting to create new Ir Port Controller");

	        if (config == null)
	        {
	            return null;
	        }

            var postActivationFunc = new Func<DeviceConfig,IROutputPort> (GetIrOutputPort);
	        var irDevice = new IrOutputPortController(config.Key + "-ir", postActivationFunc, config);

	        return irDevice;
	    }

	    /*
        /// <summary>
        /// Returns a ready-to-go IrOutputPortController from a DeviceConfig object.
        /// </summary>	
        public static IrOutputPortController GetIrOutputPortController(DeviceConfig devConf)
        {
            var irControllerKey = devConf.Key + "-ir";
            if (devConf.Properties == null)
            {
                Debug.LogMessage(LogEventLevel.Information, "[{0}] WARNING: Device config does not include properties.  IR will not function.", devConf.Key);
                return new IrOutputPortController(irControllerKey, null, "");
            }

            var control = devConf.Properties["control"];
            if (control == null)
            {
                var c = new IrOutputPortController(irControllerKey, null, "");
                Debug.LogMessage(LogEventLevel.Information, c, "WARNING: Device config does not include control properties.  IR will not function");
                return c;
            }

            var portDevKey = control.Value<string>("controlPortDevKey");
            var portNum = control.Value<uint>("controlPortNumber");
            IIROutputPorts irDev = null;

            if (portDevKey == null)
            {
                var c = new IrOutputPortController(irControllerKey, null, "");
                Debug.LogMessage(LogEventLevel.Information, c, "WARNING: control properties is missing ir device");
                return c;
            }

            if (portNum == 0)
            {
                var c = new IrOutputPortController(irControllerKey, null, "");
                Debug.LogMessage(LogEventLevel.Information, c, "WARNING: control properties is missing ir port number");
                return c;
            } 

            if (portDevKey.Equals("controlSystem", StringComparison.OrdinalIgnoreCase)
                || portDevKey.Equals("processor", StringComparison.OrdinalIgnoreCase))
                irDev = Global.ControlSystem;
            else
                irDev = DeviceManager.GetDeviceForKey(portDevKey) as IIROutputPorts;

            if (irDev == null)
            {
                var c = new IrOutputPortController(irControllerKey, null, "");
                Debug.LogMessage(LogEventLevel.Information, c, "WARNING: device with IR ports '{0}' not found", portDevKey);
                return c;
            }

            if (portNum <= irDev.NumberOfIROutputPorts) // success!
                return new IrOutputPortController(irControllerKey, irDev.IROutputPorts[portNum],
                    IrDriverPathPrefix + control["irFile"].Value<string>());
            else
            {
                var c = new IrOutputPortController(irControllerKey, null, "");
                Debug.LogMessage(LogEventLevel.Information, c, "WARNING: device '{0}' IR port {1} out of range",
                    portDevKey, portNum);
                return c;
            }
        }*/
	}

	/// <summary>
	/// Wrapper to help in IR port creation
	/// </summary>
	public class IrOutPortConfig
	{
		[JsonProperty("port")]
		public IROutputPort Port { get; set; }
		
		[JsonProperty("fileName")]
		public string FileName { get; set; }

		[JsonProperty("useBridgeJoinMap")]
		public bool UseBridgeJoinMap { get; set; }

		public IrOutPortConfig()
		{
			FileName = "";			
		}
	}
}