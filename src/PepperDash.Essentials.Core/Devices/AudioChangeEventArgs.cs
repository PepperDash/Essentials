using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Essentials.Core.Interfaces;

namespace PepperDash.Essentials.Core
{
    public class AudioChangeEventArgs
	{
		public AudioChangeType ChangeType { get; private set; }
		public IBasicVolumeControls AudioDevice { get; private set; }

		public AudioChangeEventArgs(IBasicVolumeControls device, AudioChangeType changeType)
		{
			ChangeType = changeType;
			AudioDevice = device;
		}
	}
}