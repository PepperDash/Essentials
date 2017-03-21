using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.DSP
{
	public abstract class DspBase : Device
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
        //string Key { get; protected set; }
	}



	// Main program 
	// VTC 
	// ATC
	// Mics, unusual

    public interface IDspLevelControl : IBasicVolumeWithFeedback
    {
        /// <summary>
        /// In BiAmp: Instance Tag, QSC: Named Control, Polycom: 
        /// </summary>
        string ControlPointTag { get; }
        int Index1 { get; }
        int Index2 { get; }
        bool HasMute { get; }
        bool HasLevel { get; }
        bool AutomaticUnmuteOnVolumeUp { get; }
    }

    //public abstract class DspLevelControl : DspControlPoint, IBasicVolumeWithFeedback
    //{
    //    protected abstract Func<bool> MuteFeedbackFunc { get; }
    //    protected abstract Func<int> VolumeLevelFeedbackFunc { get; }

    //    public DspLevelControl(string id)
    //    {
    //        MuteFeedback = new BoolFeedback(MuteFeedbackFunc);
    //        VolumeLevelFeedback = new IntFeedback(VolumeLevelFeedbackFunc);
    //    }

    //    // Poll and listen for these
    //    // Max value
    //    // Min value
    //    #region IBasicVolumeWithFeedback Members

    //    public BoolFeedback MuteFeedback { get; private set; }

    //    public abstract void MuteOff();

    //    public abstract void MuteOn();

    //    public abstract void SetVolume(ushort level);

    //    public IntFeedback VolumeLevelFeedback { get; private set; }

    //    #endregion

    //    #region IBasicVolumeControls Members

    //    public abstract void MuteToggle();

    //    public abstract void VolumeDown(bool pressRelease);

    //    public abstract void VolumeUp(bool pressRelease);

    //    #endregion
    //}

	// Privacy mute
	public abstract class DspMuteControl : DspControlPoint
	{
		protected abstract Func<bool> MuteFeedbackFunc { get; }

		public DspMuteControl(string id)
		{
			MuteFeedback = new BoolFeedback(MuteFeedbackFunc);
		}

		public BoolFeedback MuteFeedback { get; private set; }

		public abstract void MuteOff();

		public abstract void MuteOn();
		
		public abstract void MuteToggle();
	}
}