using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;


namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Abstract base class for TriList handler bridges
	/// </summary>
	public abstract class HandlerBridge
	{
  /// <summary>
  /// Gets or sets the IsAttached
  /// </summary>
		public bool IsAttached { get; protected set; }

		/// <summary>
		/// Attaches the handler to the panel's user objects
		/// </summary>
		public abstract void AttachToTriListOutputs(bool sendUpdate);

		/// <summary>
		/// Removes the handler from the panel's user objects
		/// </summary>
		public abstract void DetachFromTriListOutputs();
	}
}