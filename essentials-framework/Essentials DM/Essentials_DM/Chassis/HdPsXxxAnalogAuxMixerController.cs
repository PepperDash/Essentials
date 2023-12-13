using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash_Essentials_DM.Chassis
{
	public class HdPsXxxAnalogAuxMixerController : IKeyed,
		IHasVolumeControlWithFeedback, IHasMuteControlWithFeedback // || IBasicVolumeWithFeedback
	{
		public string Key { get; private set; }

		public HdPsXxxAnalogAuxMixer Mixer { get; private set; }

		public HdPsXxxAnalogAuxMixerController(string parent, uint mixer, HdPsXxx chassis)
		{
			Key = string.Format("{0}-analogMixer{1}", parent, mixer);

			Mixer = chassis.AnalogAuxiliaryMixer[mixer];

			Mixer.AuxMixerPropertyChange += OnAuxMixerPropertyChange;
			Mixer.AuxiliaryMuteControl.MuteAndVolumeControlPropertyChange += OnMuteAndVolumeControlPropertyChange;

			VolumeLevelFeedback = new IntFeedback(() => VolumeLevel);
			MuteFeedback = new BoolFeedback(() => IsMuted);

			VolumeLevel = Mixer.VolumeFeedback.ShortValue;
			IsMuted = Mixer.AuxiliaryMuteControl.MuteOnFeedback.BoolValue;
		}

		#region Volume

		private void OnAuxMixerPropertyChange(object sender, GenericEventArgs args)
		{
			Debug.Console(2, this, "AuxMixerPropertyChange: {0} > Index-{1}, EventId-{2}", sender.GetType().ToString(), args.Index, args.EventId);

			switch (args.EventId)
			{
				case (3):
					{
						VolumeLevel = Mixer.VolumeFeedback.ShortValue;
						break;
					}
			}
		}

		private const ushort CrestronLevelMin = 0;
		private const ushort CrestronLevelMax = 65535;

		private const int DeviceLevelMin = -800;
		private const int DeviceLevelMax = 200;

		private const int RampTime = 5000;

		private int _volumeLevel;

		public int VolumeLevel
		{
			get { return _volumeLevel; }
			set
			{
				var level = value;
				
				// ScaleWithLimits(inputValue, InputUpperBound, InputLowerBound, OutputUpperBound, OutputLowerBound)
				_volumeLevel = CrestronEnvironment.ScaleWithLimits(level, DeviceLevelMax, DeviceLevelMin, CrestronLevelMax, CrestronLevelMin);

				Debug.Console(1, this, "VolumeFeedback: level-'{0}', scaled-'{1}'", level, _volumeLevel);

				VolumeLevelFeedback.FireUpdate();
			}
		}

		public IntFeedback VolumeLevelFeedback { get; private set; }
		
		public void SetVolume(ushort level)
		{
			// ScaleWithLimits(inputValue, InputUpperBound, InputLowerBound, OutputUpperBound, OutputLowerBound)
			var scaled = CrestronEnvironment.ScaleWithLimits(level, CrestronLevelMax, CrestronLevelMin, DeviceLevelMax, DeviceLevelMin);

			Debug.Console(1, this, "SetVolume: level-'{0}', scaled-'{1}'", level, scaled);

			Mixer.Volume.ShortValue = (short)scaled;
		}

		public void VolumeUp(bool pressRelease)
		{
			if (pressRelease)
			{
				Mixer.Volume.CreateSignedRamp(DeviceLevelMax, RampTime);
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
				//var remainingRatio = Mixer.Volume.UShortValue/CrestronLevelMax;
				//Mixer.Volume.CreateRamp(CrestronLevelMin, (uint)(RampTime * remainingRatio));

				Mixer.Volume.CreateSignedRamp(DeviceLevelMin, RampTime);
			}
			else
			{
				Mixer.Volume.StopRamp();
			}
		}

		#endregion




		#region Mute

		private void OnMuteAndVolumeControlPropertyChange(MuteControl device, GenericEventArgs args)
		{
			Debug.Console(2, this, "OnMuteAndVolumeControlPropertyChange: {0} > Index-{1}, EventId-{2}", device.ToString(), args.Index, args.EventId);

			switch (args.EventId)
			{
				case (1):
				case (2):
					{
						IsMuted = Mixer.AuxiliaryMuteControl.MuteOnFeedback.BoolValue;
						break;
					}
			}
		}

		private bool _isMuted;

		public bool IsMuted
		{
			get { return _isMuted; }
			set
			{
				_isMuted = value;

				Debug.Console(1, this, "IsMuted: _isMuted-'{0}'", _isMuted);

				MuteFeedback.FireUpdate();
			}
		}

		public BoolFeedback MuteFeedback { get; private set; }

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
			if (IsMuted)
				MuteOff();
			else
				MuteOn();
		}

		#endregion
	}
}