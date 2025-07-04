using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core;

	/// <summary>
	/// 
	/// </summary>
	public class VolumeDeviceChangeEventArgs : EventArgs
	{
		public IBasicVolumeControls OldDev { get; private set; }
		public IBasicVolumeControls NewDev { get; private set; }
		public ChangeType Type { get; private set; }

		public VolumeDeviceChangeEventArgs(IBasicVolumeControls oldDev, IBasicVolumeControls newDev, ChangeType type)
		{
			OldDev = oldDev;
			NewDev = newDev;
			Type = type;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public enum ChangeType
	{
		WillChange, DidChange
	}