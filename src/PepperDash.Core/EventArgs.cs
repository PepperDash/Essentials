using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Core
{
	/// <summary>
	/// Bool change event args
	/// </summary>
	public class BoolChangeEventArgs : EventArgs
	{
		/// <summary>
		/// Boolean state property
		/// </summary>
		public bool State { get; set; }
		
		/// <summary>
		/// Boolean ushort value property
		/// </summary>
		public ushort IntValue { get { return (ushort)(State ? 1 : 0); } }
		
		/// <summary>
		/// Boolean change event args type
		/// </summary>
		public ushort Type { get; set; }
		
		/// <summary>
		/// Boolean change event args index
		/// </summary>
		public ushort Index { get; set; }
		
		/// <summary>
		/// Constructor
		/// </summary>
		public BoolChangeEventArgs()
		{

		}
		
		/// <summary>
		/// Constructor overload
		/// </summary>
		/// <param name="state"></param>
		/// <param name="type"></param>
		public BoolChangeEventArgs(bool state, ushort type)
		{
			State = state;
			Type = type;
		}
		
		/// <summary>
		/// Constructor overload
		/// </summary>
		/// <param name="state"></param>
		/// <param name="type"></param>
		/// <param name="index"></param>
		public BoolChangeEventArgs(bool state, ushort type, ushort index)
		{
			State = state;
			Type = type;
			Index = index;
		}
	}

	/// <summary>
	/// Ushort change event args
	/// </summary>
	public class UshrtChangeEventArgs : EventArgs
	{
		/// <summary>
		/// Ushort change event args integer value
		/// </summary>
		public ushort IntValue { get; set; }
		
		/// <summary>
		/// Ushort change event args type
		/// </summary>
		public ushort Type { get; set; }
		
		/// <summary>
		/// Ushort change event args index
		/// </summary>
		public ushort Index { get; set; }
		
		/// <summary>
		/// Constructor
		/// </summary>
		public UshrtChangeEventArgs()
		{

		}

		/// <summary>
		/// Constructor overload
		/// </summary>
		/// <param name="intValue"></param>
		/// <param name="type"></param>
		public UshrtChangeEventArgs(ushort intValue, ushort type)
		{
			IntValue = intValue;
			Type = type;
		}

		/// <summary>
		/// Constructor overload
		/// </summary>
		/// <param name="intValue"></param>
		/// <param name="type"></param>
		/// <param name="index"></param>
		public UshrtChangeEventArgs(ushort intValue, ushort type, ushort index)
		{
			IntValue = intValue;
			Type = type;
			Index = index;
		}
	}

	/// <summary>
	/// String change event args
	/// </summary>
	public class StringChangeEventArgs : EventArgs
	{
		/// <summary>
		/// String change event args value
		/// </summary>
		public string StringValue { get; set; }

		/// <summary>
		/// String change event args type
		/// </summary>
		public ushort Type { get; set; }

		/// <summary>
		/// string change event args index
		/// </summary>
		public ushort Index { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public StringChangeEventArgs()
		{

		}

		/// <summary>
		/// Constructor overload
		/// </summary>
		/// <param name="stringValue"></param>
		/// <param name="type"></param>
		public StringChangeEventArgs(string stringValue, ushort type)
		{
			StringValue = stringValue;
			Type = type;
		}

		/// <summary>
		/// Constructor overload
		/// </summary>
		/// <param name="stringValue"></param>
		/// <param name="type"></param>
		/// <param name="index"></param>
		public StringChangeEventArgs(string stringValue, ushort type, ushort index)
		{
			StringValue = stringValue;
			Type = type;
			Index = index;
		}
	}	
}