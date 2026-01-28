using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// 
	/// </summary>
	public class VolumeDeviceChangeEventArgs : EventArgs
	{
		/// <summary>
		/// The old device
		/// </summary>
		public IBasicVolumeControls OldDev { get; private set; }

		/// <summary>
		/// The new device
		/// </summary>
		public IBasicVolumeControls NewDev { get; private set; }

		/// <summary>
		/// The type of change
		/// </summary>
		public ChangeType Type { get; private set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="oldDev">the old device</param>
		/// <param name="newDev">the new device</param>
		/// <param name="type">the type of change</param>
		public VolumeDeviceChangeEventArgs(IBasicVolumeControls oldDev, IBasicVolumeControls newDev, ChangeType type)
		{
			OldDev = oldDev;
			NewDev = newDev;
			Type = type;
		}
	}

 /// <summary>
 /// Enumeration of ChangeType values
 /// </summary>
	public enum ChangeType
	{
		/// <summary>
		/// Will change
		/// </summary>
		WillChange, 
		
		/// <summary>
		/// Did change
		/// </summary>
		DidChange
	}
}