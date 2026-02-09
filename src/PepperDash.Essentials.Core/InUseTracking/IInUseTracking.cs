using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Defines a class that uses an InUseTracker
	/// </summary>
	public interface IInUseTracking
	{
		/// <summary>
		/// Gets the InUseTracker
		/// </summary>
		InUseTracking InUseTracker { get; }
	}
}