using System;
using Crestron.SimplSharpPro.DM;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash_Essentials_DM.Chassis
{
	public class HdPsXxxAnalogAuxMixerController : IKeyed, IBasicVolumeWithFeedback
	{
		public string Key { get; private set; }

		public HdPsXxxAnalogAuxMixer Mixer { get; set; }

		public HdPsXxxAnalogAuxMixerController(string parent, uint mixer, HdPsXxx chassis)
		{
			Key = string.Format("{0}-analogMixer{1}", parent, mixer);

			Mixer = chassis.AnalogAuxiliaryMixer[mixer];

			VolumeLevelFeedback = new IntFeedback(VolumeFeedbackFunc);
			MuteFeedback = new BoolFeedback(MuteFeedbackFunc);
		}

		#region Volume

		public IntFeedback VolumeLevelFeedback { get; private set; }

		protected Func<int> VolumeFeedbackFunc
		{
			get { return () => Mixer.VolumeFeedback.UShortValue; }
		}

		public void SetVolume(ushort level)
		{
			Mixer.Volume.UShortValue = level;
		}

		public void VolumeUp(bool pressRelease)
		{
			if (pressRelease)
			{
				var remainingRatio = (65535 - Mixer.Volume.UShortValue)/65535;
				Mixer.Volume.CreateRamp(65535, 400);
			}
			else
			{
				Mixer.Volume.StopRamp();
			}
		}

		public void VolumeDown(bool pressRelease)
		{
			if (pressRelease)
			{
				var remainingRatio = Mixer.Volume.UShortValue/65535;
				Mixer.Volume.CreateRamp(0, (uint)(400 * remainingRatio));
			}
			else
			{
				Mixer.Volume.StopRamp();
			}
		}

		#endregion


		#region Mute

		private bool _isMuted;

		public BoolFeedback MuteFeedback { get; private set; }

		protected Func<bool> MuteFeedbackFunc
		{
			get { return () => _isMuted = Mixer.AuxiliaryMuteControl.MuteOnFeedback.BoolValue; }
		}

		public void MuteOn()
		{
			Mixer.AuxiliaryMuteControl.MuteOn();
		}

		public void MuteOff()
		{
			Mixer.AuxiliaryMuteControl.MuteOff();
		}

		public void MuteToggle()
		{
			if (_isMuted)
				MuteOff();
			else
				MuteOn();
		}

		#endregion
	}
}