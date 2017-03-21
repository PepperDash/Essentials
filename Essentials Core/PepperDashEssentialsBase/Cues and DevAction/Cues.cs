using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;


namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Encapsulates a string-named, joined and typed command to a device
	/// </summary>
	[Obsolete()]
	public class DevAction
	{
		public Cue Cue { get; private set; }
		public Action<object> Action { get; private set; }


		public DevAction(Cue cue, Action<object> action)
		{
			Cue = cue;
			Action = action;
		}
	}

	public enum eCueType
	{
		Bool, Int, String, Void, Other
	}


    /// <summary>
    /// The Cue class is a container to represent a name / join number / type for simplifying
    /// commands coming into devices.
    /// </summary>
	public class Cue
	{
		public string Name { get; private set; }
		public uint Number { get; private set; }
		public eCueType Type { get; private set; }

		public Cue(string name, uint join, eCueType type)
		{
			Name = name;
			Number = join;
			Type = type;
		}

		/// <summary>
		/// Override that prints out the cue's data
		/// </summary>
		public override string ToString()
		{
			return string.Format("{0} Cue '{1}'-{2}", Type, Name, Number);
		}

		///// <summary>
		///// Returns a new Cue with JoinNumber offset
		///// </summary>
		//public Cue GetOffsetCopy(uint offset)
		//{
		//    return new Cue(Name, Number + offset, Type);
		//}

		/// <summary>
		/// Helper method to create a Cue of Bool type
		/// </summary>
		/// <returns>Cue</returns>
		public static Cue BoolCue(string name, uint join)
		{
			return new Cue(name, join, eCueType.Bool);
		}

		/// <summary>
		/// Helper method to create a Cue of ushort type
		/// </summary>
		/// <returns>Cue</returns>		
		public static Cue UShortCue(string name, uint join)
		{
			return new Cue(name, join, eCueType.Int);
		}

		/// <summary>
		/// Helper method to create a Cue of string type
		/// </summary>
		/// <returns>Cue</returns>
		public static Cue StringCue(string name, uint join)
		{
			return new Cue(name, join, eCueType.String);
		}

		public static readonly Cue DefaultBoolCue = new Cue("-none-", 0, eCueType.Bool);
		public static readonly Cue DefaultIntCue = new Cue("-none-", 0, eCueType.Int);
		public static readonly Cue DefaultStringCue = new Cue("-none-", 0, eCueType.String);
	}
}