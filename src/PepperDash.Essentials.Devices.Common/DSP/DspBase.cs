using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.Devices.Common.DSP
{
	public abstract class DspBase : EssentialsDevice, ILevelControls
	{
		public Dictionary<string,IBasicVolumeWithFeedback> LevelControlPoints { get; private set; }

        public Dictionary<string, DspControlPoint> DialerControlPoints { get; private set; }

        public Dictionary<string, DspControlPoint> SwitcherControlPoints { get; private set; }

		public DspBase(string key, string name) :
				base(key, name)
		{

			LevelControlPoints = new Dictionary<string, IBasicVolumeWithFeedback>();
			DialerControlPoints = new Dictionary<string, DspControlPoint>();
			SwitcherControlPoints = new Dictionary<string, DspControlPoint>();
	    }


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

	public abstract class DspControlPoint :IKeyed
	{
        /// <summary>
        /// Gets or sets the Key
        /// </summary>
        public string Key { get; }

        protected DspControlPoint(string key) => Key = key;
	}

    public abstract class DspLevelControlPoint :DspControlPoint, IBasicVolumeWithFeedback
    {
        /// <summary>
        /// Gets or sets the MuteFeedback
        /// </summary>
        public BoolFeedback MuteFeedback { get; }
        /// <summary>
        /// Gets or sets the VolumeLevelFeedback
        /// </summary>
        public IntFeedback VolumeLevelFeedback { get; }

        protected DspLevelControlPoint(string key, Func<bool> muteFeedbackFunc, Func<int> volumeLevelFeedbackFunc) : base(key)
        {
            MuteFeedback = new BoolFeedback(muteFeedbackFunc);
            VolumeLevelFeedback = new IntFeedback(volumeLevelFeedbackFunc);
        }

        public abstract void MuteOff();
        public abstract void MuteOn();
        public abstract void MuteToggle();
        public abstract void SetVolume(ushort level);
        public abstract void VolumeDown(bool pressRelease);
        public abstract void VolumeUp(bool pressRelease);
    }


    public abstract class DspDialerBase:DspControlPoint
	{
        protected DspDialerBase(string key) : base(key) { }
	}


	// Main program 
	// VTC 
	// ATC
	// Mics, unusual

}