using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Core.GenericRESTfulCommunications
{
	/// <summary>
	/// Constants
	/// </summary>
    public class GenericRESTfulConstants
    {
		/// <summary>
		/// Generic boolean change
		/// </summary>
		public const ushort BoolValueChange = 1;
		/// <summary>
		/// Generic Ushort change
		/// </summary>
		public const ushort UshrtValueChange = 101;
		/// <summary>
		/// Response Code Ushort change
		/// </summary>
		public const ushort ResponseCodeChange = 102;
		/// <summary>
		/// Generic String chagne 
		/// </summary>
		public const ushort StringValueChange = 201;
		/// <summary>
		/// Response string change
		/// </summary>
		public const ushort ResponseStringChange = 202;
		/// <summary>
		/// Error string change
		/// </summary>
		public const ushort ErrorStringChange = 203;
    }
}