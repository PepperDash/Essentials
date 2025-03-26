using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Core.JsonStandardObjects
{
	/// <summary>
	/// Constants for simpl modules
	/// </summary>
	public class JsonStandardDeviceConstants
	{
		/// <summary>
		/// Json object evaluated constant
		/// </summary>
		public const ushort JsonObjectEvaluated = 2;

		/// <summary>
		/// Json object changed constant
		/// </summary>
		public const ushort JsonObjectChanged = 104;
	}

	/// <summary>
	/// 
	/// </summary>
	public class DeviceChangeEventArgs : EventArgs
	{
		/// <summary>
		/// Device change event args object
		/// </summary>
		public DeviceConfig Device { get; set; }

		/// <summary>
		/// Device change event args type
		/// </summary>
		public ushort Type { get; set; }

		/// <summary>
		/// Device change event args index
		/// </summary>
		public ushort Index { get; set; }

		/// <summary>
		/// Default constructor
		/// </summary>
		public DeviceChangeEventArgs()
		{

		}

		/// <summary>
		/// Constructor overload
		/// </summary>
		/// <param name="device"></param>
		/// <param name="type"></param>
		public DeviceChangeEventArgs(DeviceConfig device, ushort type)
		{
			Device = device;
			Type = type;
		}

		/// <summary>
		/// Constructor overload
		/// </summary>
		/// <param name="device"></param>
		/// <param name="type"></param>
		/// <param name="index"></param>
		public DeviceChangeEventArgs(DeviceConfig device, ushort type, ushort index)
		{
			Device = device;
			Type = type;
			Index = index;
		}
	}
}