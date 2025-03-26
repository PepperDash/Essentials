using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Core.JsonStandardObjects
{
	/*
	Convert JSON snippt to C#: http://json2csharp.com/#
	
	JSON Snippet:
	{
		"devices": [
			{
				"key": "deviceKey",
				"name": "deviceName",
				"type": "deviceType",
				"properties": {
					"deviceId": 1,
					"enabled": true,
					"control": {
						"method": "methodName",
						"controlPortDevKey": "deviceControlPortDevKey",
						"controlPortNumber": 1,
						"comParams": {
							"baudRate": 9600,
							"dataBits": 8,
							"stopBits": 1,
							"parity": "None",
							"protocol": "RS232",
							"hardwareHandshake": "None",
							"softwareHandshake": "None",
							"pacing": 0
						},
						"tcpSshProperties": {
							"address": "172.22.1.101",
							"port": 23,
							"username": "user01",
							"password": "password01",
							"autoReconnect": false,
							"autoReconnectIntervalMs": 10000
						}
					}
				}
			}
		]
	}
	*/
	/// <summary>
	/// Device communication parameter class
	/// </summary>
	public class ComParamsConfig
	{
        /// <summary>
        /// 
        /// </summary>
		public int baudRate { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public int dataBits { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public int stopBits { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string parity { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string protocol { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string hardwareHandshake { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string softwareHandshake { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public int pacing { get; set; }

		// convert properties for simpl
        /// <summary>
        /// 
        /// </summary>
		public ushort simplBaudRate { get { return Convert.ToUInt16(baudRate); } }
        /// <summary>
        /// 
        /// </summary>
		public ushort simplDataBits { get { return Convert.ToUInt16(dataBits); } }
        /// <summary>
        /// 
        /// </summary>
		public ushort simplStopBits { get { return Convert.ToUInt16(stopBits); } }
        /// <summary>
        /// 
        /// </summary>
		public ushort simplPacing { get { return Convert.ToUInt16(pacing); } }

		/// <summary>
		/// Constructor
		/// </summary>
		public ComParamsConfig()
		{

		}
	}

	/// <summary>
	/// Device TCP/SSH properties class
	/// </summary>
	public class TcpSshPropertiesConfig
	{
        /// <summary>
        /// 
        /// </summary>
		public string address { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public int port { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string username { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string password { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public bool autoReconnect { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public int autoReconnectIntervalMs { get; set; }

		// convert properties for simpl
        /// <summary>
        /// 
        /// </summary>
		public ushort simplPort { get { return Convert.ToUInt16(port); } }
        /// <summary>
        /// 
        /// </summary>
		public ushort simplAutoReconnect { get { return (ushort)(autoReconnect ? 1 : 0); } }
        /// <summary>
        /// 
        /// </summary>
		public ushort simplAutoReconnectIntervalMs { get { return Convert.ToUInt16(autoReconnectIntervalMs); } }

		/// <summary>
		/// Constructor
		/// </summary>
		public TcpSshPropertiesConfig()
		{

		}
	}

	/// <summary>
	/// Device control class
	/// </summary>
	public class ControlConfig
	{
        /// <summary>
        /// 
        /// </summary>
		public string method { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string controlPortDevKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public int controlPortNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public ComParamsConfig comParams { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public TcpSshPropertiesConfig tcpSshProperties { get; set; }

		// convert properties for simpl
        /// <summary>
        /// 
        /// </summary>
		public ushort simplControlPortNumber { get { return Convert.ToUInt16(controlPortNumber); } }

		/// <summary>
		/// Constructor
		/// </summary>
		public ControlConfig()
		{
			comParams = new ComParamsConfig();
			tcpSshProperties = new TcpSshPropertiesConfig();
		}
	}

	/// <summary>
	/// Device properties class
	/// </summary>
	public class PropertiesConfig
	{
        /// <summary>
        /// 
        /// </summary>
		public int deviceId { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public bool enabled { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public ControlConfig control { get; set; }

		// convert properties for simpl
        /// <summary>
        /// 
        /// </summary>
		public ushort simplDeviceId { get { return Convert.ToUInt16(deviceId); } }
        /// <summary>
        /// 
        /// </summary>
		public ushort simplEnabled { get { return (ushort)(enabled ? 1 : 0); } }

		/// <summary>
		/// Constructor
		/// </summary>
		public PropertiesConfig()
		{
			control = new ControlConfig();
		}
	}

	/// <summary>
	/// Root device class
	/// </summary>
	public class RootObject
	{
        /// <summary>
        /// The collection of devices
        /// </summary>
		public List<DeviceConfig> devices { get; set; }
	}
}