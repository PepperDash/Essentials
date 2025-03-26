using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Core.SystemInfo
{
	/// <summary>
	/// Constants 
	/// </summary>
	public class SystemInfoConstants
	{
        /// <summary>
        /// 
        /// </summary>
		public const ushort BoolValueChange = 1;
		/// <summary>
		/// 
		/// </summary>
        public const ushort CompleteBoolChange = 2;
		/// <summary>
		/// 
		/// </summary>
        public const ushort BusyBoolChange = 3;
        
        /// <summary>
        /// 
        /// </summary>
		public const ushort UshortValueChange = 101;

        /// <summary>
        /// 
        /// </summary>
		public const ushort StringValueChange = 201;
		/// <summary>
		/// 
		/// </summary>
        public const ushort ConsoleResponseChange = 202;
		/// <summary>
		/// 
		/// </summary>
        public const ushort ProcessorUptimeChange = 203;
		/// <summary>
		/// 
		/// </summary>
        public const ushort ProgramUptimeChange = 204;

        /// <summary>
        /// 
        /// </summary>
		public const ushort ObjectChange = 301;
		/// <summary>
		/// 
		/// </summary>
        public const ushort ProcessorConfigChange = 302;
		/// <summary>
		/// 
		/// </summary>
        public const ushort EthernetConfigChange = 303;
		/// <summary>
		/// 
		/// </summary>
        public const ushort ControlSubnetConfigChange = 304;
		/// <summary>
		/// 
		/// </summary>
        public const ushort ProgramConfigChange = 305;
	}

	/// <summary>
	/// Processor Change Event Args Class
	/// </summary>
	public class ProcessorChangeEventArgs : EventArgs
	{
        /// <summary>
        /// 
        /// </summary>
		public ProcessorInfo Processor { get; set; }
		/// <summary>
		/// 
		/// </summary>
        public ushort Type { get; set; }
		/// <summary>
		/// 
		/// </summary>
        public ushort Index { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public ProcessorChangeEventArgs()
		{

		}

		/// <summary>
		/// Constructor overload
		/// </summary>
		public ProcessorChangeEventArgs(ProcessorInfo processor, ushort type)
		{
			Processor = processor;
			Type = type;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public ProcessorChangeEventArgs(ProcessorInfo processor, ushort type, ushort index)
		{
			Processor = processor;
			Type = type;
			Index = index;
		}
	}

	/// <summary>
	/// Ethernet Change Event Args Class
	/// </summary>
	public class EthernetChangeEventArgs : EventArgs
	{
        /// <summary>
        /// 
        /// </summary>
		public EthernetInfo Adapter { get; set; }
		/// <summary>
		/// 
		/// </summary>
        public ushort Type { get; set; }
		/// <summary>
		/// 
		/// </summary>
        public ushort Index { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public EthernetChangeEventArgs()
		{

		}

		/// <summary>
		/// Constructor overload
		/// </summary>
        /// <param name="ethernet"></param>
		/// <param name="type"></param>
		public EthernetChangeEventArgs(EthernetInfo ethernet, ushort type)
		{
			Adapter = ethernet;
			Type = type;
		}

		/// <summary>
		/// Constructor overload
		/// </summary>
        /// <param name="ethernet"></param>
		/// <param name="type"></param>
        /// <param name="index"></param>
		public EthernetChangeEventArgs(EthernetInfo ethernet, ushort type, ushort index)
		{
			Adapter = ethernet;
			Type = type;
			Index = index;
		}
	}

	/// <summary>
	/// Control Subnet Chage Event Args Class
	/// </summary>
	public class ControlSubnetChangeEventArgs : EventArgs
	{
        /// <summary>
        /// 
        /// </summary>
		public ControlSubnetInfo Adapter { get; set; }
		/// <summary>
		/// 
		/// </summary>
        public ushort Type { get; set; }
		/// <summary>
		/// 
		/// </summary>
        public ushort Index { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public ControlSubnetChangeEventArgs()
		{

		}

		/// <summary>
		/// Constructor overload
		/// </summary>
		public ControlSubnetChangeEventArgs(ControlSubnetInfo controlSubnet, ushort type)
		{
			Adapter = controlSubnet;
			Type = type;
		}

		/// <summary>
		/// Constructor overload
		/// </summary>
		public ControlSubnetChangeEventArgs(ControlSubnetInfo controlSubnet, ushort type, ushort index)
		{
			Adapter = controlSubnet;
			Type = type;
			Index = index;
		}
	}

	/// <summary>
	/// Program Change Event Args Class
	/// </summary>
	public class ProgramChangeEventArgs : EventArgs
	{
        /// <summary>
        /// 
        /// </summary>
		public ProgramInfo Program { get; set; }
		/// <summary>
		/// 
		/// </summary>
        public ushort Type { get; set; }
		/// <summary>
		/// 
		/// </summary>
        public ushort Index { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public ProgramChangeEventArgs()
		{

		}

		/// <summary>
		/// Constructor overload
		/// </summary>
        /// <param name="program"></param>
		/// <param name="type"></param>
		public ProgramChangeEventArgs(ProgramInfo program, ushort type)
		{
			Program = program;
			Type = type;
		}

		/// <summary>
		/// Constructor overload
		/// </summary>
        /// <param name="program"></param>
		/// <param name="type"></param>
        /// <param name="index"></param>
		public ProgramChangeEventArgs(ProgramInfo program, ushort type, ushort index)
		{
			Program = program;
			Type = type;
			Index = index;
		}
	}
}