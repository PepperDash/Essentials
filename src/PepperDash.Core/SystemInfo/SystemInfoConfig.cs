using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Core.SystemInfo
{
	/// <summary>
	/// Processor info class
	/// </summary>
	public class ProcessorInfo
	{
        /// <summary>
        /// 
        /// </summary>
		public string Model { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string SerialNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string Firmware { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string FirmwareDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string OsVersion { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string RuntimeEnvironment { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string DevicePlatform { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string ModuleDirectory { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string LocalTimeZone { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string ProgramIdTag { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public ProcessorInfo()
		{
			
		}
	}

	/// <summary>
	/// Ethernet info class
	/// </summary>
	public class EthernetInfo
	{
        /// <summary>
        /// 
        /// </summary>
		public ushort DhcpIsOn { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string Hostname { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string MacAddress { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string IpAddress { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string Subnet { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string Gateway { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string Dns1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string Dns2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string Dns3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string Domain { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public EthernetInfo()
		{
			
		}
	}

	/// <summary>
	/// Control subnet info class
	/// </summary>
	public class ControlSubnetInfo
	{
        /// <summary>
        /// 
        /// </summary>
		public ushort Enabled { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public ushort IsInAutomaticMode { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string MacAddress { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string IpAddress { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string Subnet { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string RouterPrefix { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public ControlSubnetInfo()
		{
		
		}
	}

	/// <summary>
	/// Program info class
	/// </summary>
	public class ProgramInfo
	{
        /// <summary>
        /// 
        /// </summary>
		public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string Header { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string System { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string ProgramIdTag { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string CompileTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string Database { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string Environment { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string Programmer { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public ProgramInfo()
		{
			
		}
	}
}