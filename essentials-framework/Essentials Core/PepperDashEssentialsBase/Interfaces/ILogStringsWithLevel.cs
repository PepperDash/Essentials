using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;

namespace PepperDash_Essentials_Core.Interfaces
{
	public interface ILogStringsWithLevel
	{
		/// <summary>
		/// Defines a class that is capable of logging a string with an int level
		/// </summary>
		void SendToLog(IKeyed device, Debug.ErrorLogLevel level,string logMessage);
	}

}