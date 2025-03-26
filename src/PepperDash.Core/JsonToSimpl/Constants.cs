using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Core.JsonToSimpl
{
	/// <summary>
	/// Constants for Simpl modules
	/// </summary>
	public class JsonToSimplConstants
	{
        /// <summary>
        /// 
        /// </summary>
		public const ushort BoolValueChange = 1;
        /// <summary>
        /// 
        /// </summary>
		public const ushort JsonIsValidBoolChange = 2;

        /// <summary>
        /// Reports the if the device is 3-series compatible
        /// </summary>
        public const ushort ProgramCompatibility3SeriesChange = 3;

        /// <summary>
        /// Reports the if the device is 4-series compatible
        /// </summary>
        public const ushort ProgramCompatibility4SeriesChange = 4;

        /// <summary>
        /// Reports the device platform enum value
        /// </summary>
        public const ushort DevicePlatformValueChange = 5;

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
		public const ushort FullPathToArrayChange = 202;
        /// <summary>
        /// 
        /// </summary>
		public const ushort ActualFilePathChange = 203;

        /// <summary>
        /// 
        /// </summary>
		public const ushort FilenameResolvedChange = 204;
        /// <summary>
        /// 
        /// </summary>
		public const ushort FilePathResolvedChange = 205;

        /// <summary>
        /// Reports the root directory change
        /// </summary>
        public const ushort RootDirectoryChange = 206;

        /// <summary>
        /// Reports the room ID change
        /// </summary>
        public const ushort RoomIdChange = 207;

        /// <summary>
        /// Reports the room name change
        /// </summary>
        public const ushort RoomNameChange = 208;
	}

	/// <summary>
	/// S+ values delegate
	/// </summary>
	public delegate void SPlusValuesDelegate();

	/// <summary>
	/// S+ values wrapper
	/// </summary>
	public class SPlusValueWrapper
	{
        /// <summary>
        /// 
        /// </summary>
		public SPlusType ValueType { get; private set; }
        /// <summary>
        /// 
        /// </summary>
		public ushort Index { get; private set; }
        /// <summary>
        /// 
        /// </summary>
		public ushort BoolUShortValue { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public string StringValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
		public SPlusValueWrapper() {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="index"></param>
		public SPlusValueWrapper(SPlusType type, ushort index)
		{
			ValueType = type;
			Index = index;
		}
	}

	/// <summary>
	/// S+ types enum
	/// </summary>
	public enum SPlusType
	{
        /// <summary>
        /// Digital
        /// </summary>
		Digital, 
        /// <summary>
        /// Analog
        /// </summary>
        Analog, 
        /// <summary>
        /// String
        /// </summary>
        String
	}
}