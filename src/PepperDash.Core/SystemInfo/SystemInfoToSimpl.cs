using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Core.SystemInfo
{
	/// <summary>
	/// System Info class
	/// </summary>
	public class SystemInfoToSimpl
	{
        /// <summary>
        /// Notifies of bool change
        /// </summary>
		public event EventHandler<BoolChangeEventArgs> BoolChange;
        /// <summary>
        /// Notifies of string change
        /// </summary>
		public event EventHandler<StringChangeEventArgs> StringChange;

        /// <summary>
        /// Notifies of processor change
        /// </summary>
		public event EventHandler<ProcessorChangeEventArgs> ProcessorChange;
        /// <summary>
        /// Notifies of ethernet change
        /// </summary>
		public event EventHandler<EthernetChangeEventArgs> EthernetChange;
        /// <summary>
        /// Notifies of control subnet change
        /// </summary>
		public event EventHandler<ControlSubnetChangeEventArgs> ControlSubnetChange;
        /// <summary>
        /// Notifies of program change
        /// </summary>
		public event EventHandler<ProgramChangeEventArgs> ProgramChange;

		/// <summary>
		/// Constructor
		/// </summary>
		public SystemInfoToSimpl()
		{

		}

		/// <summary>
		/// Gets the current processor info
		/// </summary>
		public void GetProcessorInfo()
		{
			OnBoolChange(true, 0, SystemInfoConstants.BusyBoolChange);

			try
			{
				var processor = new ProcessorInfo();
				processor.Model = InitialParametersClass.ControllerPromptName;
				processor.SerialNumber = CrestronEnvironment.SystemInfo.SerialNumber;
				processor.ModuleDirectory = InitialParametersClass.ProgramDirectory.ToString();
				processor.ProgramIdTag = InitialParametersClass.ProgramIDTag;
				processor.DevicePlatform = CrestronEnvironment.DevicePlatform.ToString();
				processor.OsVersion = CrestronEnvironment.OSVersion.Version.ToString();
				processor.RuntimeEnvironment = CrestronEnvironment.RuntimeEnvironment.ToString();
				processor.LocalTimeZone = CrestronEnvironment.GetTimeZone().Offset;

				// Does not return firmware version matching a "ver" command
				// returns the "ver -v" 'CAB' version
				// example return ver -v: 
				//		RMC3 Cntrl Eng [v1.503.3568.25373 (Oct 09 2018), #4001E302] @E-00107f4420f0
				//		Build: 14:05:46  Oct 09 2018 (3568.25373)
				//		Cab: 1.503.0070
				//		Applications:  1.0.6855.21351
				//		Updater: 1.4.24
				//		Bootloader: 1.22.00
				//		RMC3-SetupProgram: 1.003.0011
				//		IOPVersion: FPGA [v09] slot:7
				//		PUF: Unknown
				//Firmware = CrestronEnvironment.OSVersion.Firmware;
				//Firmware = InitialParametersClass.FirmwareVersion;

				// Use below logic to get actual firmware ver, not the 'CAB' returned by the above
				// matches console return of a "ver" and on SystemInfo page
				// example return ver: 
				//		RMC3 Cntrl Eng [v1.503.3568.25373 (Oct 09 2018), #4001E302] @E-00107f4420f0
				var response = "";
				CrestronConsole.SendControlSystemCommand("ver", ref response);
				processor.Firmware = ParseConsoleResponse(response, "Cntrl Eng", "[", "(");
				processor.FirmwareDate = ParseConsoleResponse(response, "Cntrl Eng", "(", ")");

				OnProcessorChange(processor, 0, SystemInfoConstants.ProcessorConfigChange);
			}
			catch (Exception e)
			{
				var msg = string.Format("GetProcessorInfo failed: {0}", e.Message);
				CrestronConsole.PrintLine(msg);
				//ErrorLog.Error(msg);
			}			

			OnBoolChange(false, 0, SystemInfoConstants.BusyBoolChange);
		}

		/// <summary>
		/// Gets the current ethernet info
		/// </summary>
		public void GetEthernetInfo()
		{
			OnBoolChange(true, 0, SystemInfoConstants.BusyBoolChange);

			var adapter = new EthernetInfo();

			try
			{
				// get lan adapter id
				var adapterId = CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetLANAdapter);

				// get lan adapter info
				var dhcpState = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_DHCP_STATE, adapterId);
				if (!string.IsNullOrEmpty(dhcpState))
					adapter.DhcpIsOn = (ushort)(dhcpState.ToLower().Contains("on") ? 1 : 0);

				adapter.Hostname = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_HOSTNAME, adapterId);
				adapter.MacAddress = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_MAC_ADDRESS, adapterId);
				adapter.IpAddress = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, adapterId);
				adapter.Subnet = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_MASK, adapterId);
				adapter.Gateway = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_ROUTER, adapterId);
				adapter.Domain = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_DOMAIN_NAME, adapterId);

				// returns comma seperate list of dns servers with trailing comma
				// example return: "8.8.8.8 (DHCP),8.8.4.4 (DHCP),"
				string dns = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_DNS_SERVER, adapterId);
				if (dns.Contains(","))
				{
					string[] dnsList = dns.Split(',');
					for (var i = 0; i < dnsList.Length; i++)
					{
						if(i == 0)
							adapter.Dns1 = !string.IsNullOrEmpty(dnsList[0]) ? dnsList[0] : "0.0.0.0";
						if(i == 1)
							adapter.Dns2 = !string.IsNullOrEmpty(dnsList[1]) ? dnsList[1] : "0.0.0.0";
						if(i == 2)
							adapter.Dns3 = !string.IsNullOrEmpty(dnsList[2]) ? dnsList[2] : "0.0.0.0";
					}					
				}
				else
				{
					adapter.Dns1 = !string.IsNullOrEmpty(dns) ? dns : "0.0.0.0";
					adapter.Dns2 = "0.0.0.0";
					adapter.Dns3 = "0.0.0.0";
				}

				OnEthernetInfoChange(adapter, 0, SystemInfoConstants.EthernetConfigChange);
			}
			catch (Exception e)
			{
				var msg = string.Format("GetEthernetInfo failed: {0}", e.Message);
				CrestronConsole.PrintLine(msg);
				//ErrorLog.Error(msg);
			}

			OnBoolChange(false, 0, SystemInfoConstants.BusyBoolChange);
		}

		/// <summary>
		/// Gets the current control subnet info
		/// </summary>
		public void GetControlSubnetInfo()
		{
			OnBoolChange(true, 0, SystemInfoConstants.BusyBoolChange);

			var adapter = new ControlSubnetInfo();

			try
			{
				// get cs adapter id
				var adapterId = CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetCSAdapter);
				if (!adapterId.Equals(EthernetAdapterType.EthernetUnknownAdapter))
				{
					adapter.Enabled = 1;
					adapter.IsInAutomaticMode = (ushort)(CrestronEthernetHelper.IsControlSubnetInAutomaticMode ? 1 : 0);
					adapter.MacAddress = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_MAC_ADDRESS, adapterId);
					adapter.IpAddress = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, adapterId);
					adapter.Subnet = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_MASK, adapterId);
					adapter.RouterPrefix = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CONTROL_SUBNET_ROUTER_PREFIX, adapterId);
				}
			}
			catch (Exception e)
			{
				adapter.Enabled = 0;
				adapter.IsInAutomaticMode = 0;
				adapter.MacAddress = "NA";
				adapter.IpAddress = "NA";
				adapter.Subnet = "NA";
				adapter.RouterPrefix = "NA";

				var msg = string.Format("GetControlSubnetInfo failed: {0}", e.Message);
				CrestronConsole.PrintLine(msg);
				//ErrorLog.Error(msg);
			}

			OnControlSubnetInfoChange(adapter, 0, SystemInfoConstants.ControlSubnetConfigChange);
			OnBoolChange(false, 0, SystemInfoConstants.BusyBoolChange);
		}

		/// <summary>
		/// Gets the program info by index
		/// </summary>
		/// <param name="index"></param>
		public void GetProgramInfoByIndex(ushort index)
		{
			if (index < 1 || index > 10)
				return;

			OnBoolChange(true, 0, SystemInfoConstants.BusyBoolChange);

			var program = new ProgramInfo();

			try
			{
				var response = "";
				CrestronConsole.SendControlSystemCommand(string.Format("progcomments:{0}", index), ref response);
				
				// no program loaded or running
				if (response.Contains("Bad or Incomplete Command"))
				{
					program.Name = "";
					program.System = "";
					program.Programmer = "";
					program.CompileTime = "";
					program.Database = "";
					program.Environment = "";
				}
				else
				{
					// SIMPL returns
					program.Name = ParseConsoleResponse(response, "Program File", ":", "\x0D");
					program.System = ParseConsoleResponse(response, "System Name", ":", "\x0D");
					program.ProgramIdTag = ParseConsoleResponse(response, "Friendly Name", ":", "\x0D");
					program.Programmer = ParseConsoleResponse(response, "Programmer", ":", "\x0D");
					program.CompileTime = ParseConsoleResponse(response, "Compiled On", ":", "\x0D");
					program.Database = ParseConsoleResponse(response, "CrestronDB", ":", "\x0D");
					program.Environment = ParseConsoleResponse(response, "Source Env", ":", "\x0D");

					// S# returns
					if (program.System.Length == 0)
						program.System = ParseConsoleResponse(response, "Application Name", ":", "\x0D");
					if (program.Database.Length == 0)
						program.Database = ParseConsoleResponse(response, "PlugInVersion", ":", "\x0D");
					if (program.Environment.Length == 0)
						program.Environment = ParseConsoleResponse(response, "Program Tool", ":", "\x0D");

				}
				
				OnProgramChange(program, index, SystemInfoConstants.ProgramConfigChange);
			}
			catch (Exception e)
			{
				var msg = string.Format("GetProgramInfoByIndex failed: {0}", e.Message);
				CrestronConsole.PrintLine(msg);
				//ErrorLog.Error(msg);
			}

			OnBoolChange(false, 0, SystemInfoConstants.BusyBoolChange);
		}

		/// <summary>
		/// Gets the processor uptime and passes it to S+
		/// </summary>
		public void RefreshProcessorUptime()
		{
			try
			{
				string response = "";
				CrestronConsole.SendControlSystemCommand("uptime", ref response);
				var uptime = ParseConsoleResponse(response, "running for", "running for", "\x0D");
				OnStringChange(uptime, 0, SystemInfoConstants.ProcessorUptimeChange);
			}
			catch (Exception e)
			{
				var msg = string.Format("RefreshProcessorUptime failed:\r{0}", e.Message);
				CrestronConsole.PrintLine(msg);
				//ErrorLog.Error(msg);
			}
		}

		/// <summary>
		/// Gets the program uptime, by index, and passes it to S+
		/// </summary>
		/// <param name="index"></param>
		public void RefreshProgramUptimeByIndex(int index)
		{
			try
			{
				string response = "";
				CrestronConsole.SendControlSystemCommand(string.Format("proguptime:{0}", index), ref response);
				string uptime = ParseConsoleResponse(response, "running for", "running for", "\x0D");
				OnStringChange(uptime, (ushort)index, SystemInfoConstants.ProgramUptimeChange);
			}
			catch (Exception e)
			{
				var msg = string.Format("RefreshProgramUptimebyIndex({0}) failed:\r{1}", index, e.Message);
				CrestronConsole.PrintLine(msg);
				//ErrorLog.Error(msg);
			}
		}

		/// <summary>
		/// Sends command to console, passes response back using string change event
		/// </summary>
		/// <param name="cmd"></param>
		public void SendConsoleCommand(string cmd)
		{
			if (string.IsNullOrEmpty(cmd))
				return;

			string response = "";
			CrestronConsole.SendControlSystemCommand(cmd, ref response);
			if (!string.IsNullOrEmpty(response))
			{
				if (response.EndsWith("\x0D\\x0A"))
					response.Trim('\n');

				OnStringChange(response, 0, SystemInfoConstants.ConsoleResponseChange);
			}
		}

		/// <summary>
		/// private method to parse console messages
		/// </summary>
        /// <param name="data"></param>
		/// <param name="line"></param>
        /// <param name="dataStart"></param>
        /// <param name="dataEnd"></param>
		/// <returns></returns>
		private string ParseConsoleResponse(string data, string line, string dataStart, string dataEnd)
		{
			var response = "";

			if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(line) || string.IsNullOrEmpty(dataStart) || string.IsNullOrEmpty(dataEnd))
				return response;

			try
			{
				var linePos = data.IndexOf(line);
				var startPos = data.IndexOf(dataStart, linePos) + dataStart.Length;
				var endPos = data.IndexOf(dataEnd, startPos);				
				response = data.Substring(startPos, endPos - startPos).Trim();
			}
			catch (Exception e)
			{
				var msg = string.Format("ParseConsoleResponse failed: {0}", e.Message);
				CrestronConsole.PrintLine(msg);
				//ErrorLog.Error(msg);
			}

			return response;
		}

		/// <summary>
		/// Protected boolean change event handler
		/// </summary>
		/// <param name="state"></param>
		/// <param name="index"></param>
		/// <param name="type"></param>
		protected void OnBoolChange(bool state, ushort index, ushort type)
		{
			var handler = BoolChange;
			if (handler != null)
			{
				var args = new BoolChangeEventArgs(state, type);
				args.Index = index;
				BoolChange(this, args);
			}
		}

		/// <summary>
		/// Protected string change event handler
		/// </summary>
		/// <param name="value"></param>
		/// <param name="index"></param>
		/// <param name="type"></param>
		protected void OnStringChange(string value, ushort index, ushort type)
		{
			var handler = StringChange;
			if (handler != null)
			{
				var args = new StringChangeEventArgs(value, type);
				args.Index = index;
				StringChange(this, args);
			}
		}

		/// <summary>
		/// Protected processor config change event handler
		/// </summary>
		/// <param name="processor"></param>
		/// <param name="index"></param>
		/// <param name="type"></param>
		protected void OnProcessorChange(ProcessorInfo processor, ushort index, ushort type)
		{
			var handler = ProcessorChange;
			if (handler != null)
			{
				var args = new ProcessorChangeEventArgs(processor, type);
				args.Index = index;
				ProcessorChange(this, args);
			}
		}

		/// <summary>
		/// Ethernet change event handler
		/// </summary>
		/// <param name="ethernet"></param>
		/// <param name="index"></param>
		/// <param name="type"></param>
		protected void OnEthernetInfoChange(EthernetInfo ethernet, ushort index, ushort type)
		{
			var handler = EthernetChange;
			if (handler != null)
			{
				var args = new EthernetChangeEventArgs(ethernet, type);
				args.Index = index;
				EthernetChange(this, args);
			}
		}

		/// <summary>
		/// Control Subnet change event handler
		/// </summary>
		/// <param name="ethernet"></param>
		/// <param name="index"></param>
		/// <param name="type"></param>
		protected void OnControlSubnetInfoChange(ControlSubnetInfo ethernet, ushort index, ushort type)
		{
			var handler = ControlSubnetChange;
			if (handler != null)
			{
				var args = new ControlSubnetChangeEventArgs(ethernet, type);
				args.Index = index;
				ControlSubnetChange(this, args);
			}
		}

		/// <summary>
		/// Program change event handler
		/// </summary>
		/// <param name="program"></param>
		/// <param name="index"></param>
		/// <param name="type"></param>
		protected void OnProgramChange(ProgramInfo program, ushort index, ushort type)
		{
			var handler = ProgramChange;

			if (handler != null)
			{
				var args = new ProgramChangeEventArgs(program, type);
				args.Index = index;
				ProgramChange(this, args);
			}
		}
	}
}