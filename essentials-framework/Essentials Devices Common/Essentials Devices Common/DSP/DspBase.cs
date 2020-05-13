using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.DSP
{
	public abstract class DspBase : EssentialsDevice
	{
		public Dictionary<string, DspControlPoint> LevelControlPoints { get; private set; }

        public Dictionary<string, DspControlPoint> DialerControlPoints { get; private set; }

        public Dictionary<string, DspControlPoint> SwitcherControlPoints { get; private set; }

		public abstract void RunPreset(string name);

		public DspBase(string key, string name) :
			base(key, name) { }


		// in audio call feedback

		// VOIP
		// Phone dialer
	}

	// Fusion
	// Privacy state
	// Online state
	// level/mutes ?
	
	// AC Log call stats
	
		// Typical presets:
		// call default preset to restore levels and mutes

	public abstract class DspControlPoint 
	{
         string Key { get; set; }
	}


	public class DspDialerBase
	{

	}


	// Main program 
	// VTC 
	// ATC
	// Mics, unusual

}