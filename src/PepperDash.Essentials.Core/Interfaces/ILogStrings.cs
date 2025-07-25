using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.Interfaces
{
 /// <summary>
 /// Defines the contract for ILogStrings
 /// </summary>
	public interface ILogStrings : IKeyed
	{
		/// <summary>
		/// Defines a class that is capable of logging a string
		/// </summary>
		void SendToLog(IKeyed device, string logMessage);
	}
}