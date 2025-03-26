using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Core.PasswordManagement
{
	/// <summary>
	/// Constants
	/// </summary>
	public class PasswordManagementConstants
	{
		/// <summary>
		/// Generic boolean value change constant
		/// </summary>
		public const ushort BoolValueChange = 1;
		/// <summary>
		/// Evaluated boolean change constant
		/// </summary>
		public const ushort PasswordInitializedChange = 2;
		/// <summary>
		/// Update busy change const
		/// </summary>
		public const ushort PasswordUpdateBusyChange = 3;
		/// <summary>
		/// Password is valid change constant
		/// </summary>
		public const ushort PasswordValidationChange = 4;
		/// <summary>
		/// Password LED change constant
		/// </summary>
		public const ushort PasswordLedFeedbackChange = 5;

		/// <summary>
		/// Generic ushort value change constant
		/// </summary>
		public const ushort UshrtValueChange = 101;
		/// <summary>
		/// Password count
		/// </summary>
		public const ushort PasswordManagerCountChange = 102;
		/// <summary>
		/// Password selecte index change constant
		/// </summary>
		public const ushort PasswordSelectIndexChange = 103;
		/// <summary>
		/// Password length
		/// </summary>
		public const ushort PasswordLengthChange = 104;
		
		/// <summary>
		/// Generic string value change constant
		/// </summary>
		public const ushort StringValueChange = 201;		
	}
}