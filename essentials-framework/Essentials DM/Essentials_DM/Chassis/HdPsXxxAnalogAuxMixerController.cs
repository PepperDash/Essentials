using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash_Essentials_DM.Chassis
{
	public class HdPsXxxAnalogAuxMixerController : IKeyed,
		IHasVolumeControlWithFeedback, IHasMuteControlWithFeedback
	{
		public string Key { get; private set; }

		private readonly HdPsXxxAnalogAuxMixer _mixer;

		public HdPsXxxAnalogAuxMixerController(string parent, uint mixer, HdPsXxx chassis)
		{
			Key = string.Format("{0}-analogMixer{1}", parent, mixer);

			_mixer = chassis.AnalogAuxiliaryMixer[mixer];

			_mixer.AuxMixerPropertyChange += OnAuxMixerPropertyChange;
			_mixer.AuxiliaryMuteControl.MuteAndVolumeControlPropertyChange += OnMuteAndVolumeControlPropertyChange;

			VolumeLevelFeedback = new IntFeedback(() => VolumeLevel);
			MuteFeedback = new BoolFeedback(() => IsMuted);
		}

		#region Volume

		private void OnAuxMixerPropertyChange(object sender, GenericEventArgs args)
		{
			Debug.Console(2, this, "OnAuxMixerPropertyChange: {0} > Index-{1}, EventId-{2}", sender.ToString(), args.Index, args.EventId);

			switch (args.EventId)
			{
				case MuteAndVolumeContorlEventIds.VolumeFeedbackEventId:
					{
						VolumeLevel = _mixer.VolumeFeedback.ShortValue;
						break;
					}
				case MuteAndVolumeContorlEventIds.MuteOnEventId:
				case MuteAndVolumeContorlEventIds.MuteOffEventId:
					{
						IsMuted = _mixer.AuxiliaryMuteControl.MuteOnFeedback.BoolValue;
						break;
					}
				default:
					{
						Debug.Console(1, this, "OnAuxMixerPropertyChange: {0} > Index-{1}, EventId-{2} - unhandled eventId", sender.ToString(), args.Index, args.EventId);
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
			private set
			{
				var level = value;
				
				_volumeLevel = CrestronEnvironment.ScaleWithLimits(level, DeviceLevelMax, DeviceLevelMin, CrestronLevelMax, CrestronLevelMin);
				
				Debug.Console(1, this, "VolumeFeedback: level-'{0}', scaled-'{1}'", level, _volumeLevel);

				VolumeLevelFeedback.FireUpdate();
			}
		}

		public IntFeedback VolumeLevelFeedback { get; private set; }

		public void SetVolume(ushort level)
		{
			var levelScaled = CrestronEnvironment.ScaleWithLimits(level, CrestronLevelMax, CrestronLevelMin, DeviceLevelMax, DeviceLevelMin);

			Debug.Console(1, this, "SetVolume: level-'{0}', levelScaled-'{1}'", level, levelScaled);
			
			_mixer.Volume.ShortValue = (short)levelScaled;
		}

		public void VolumeUp(bool pressRelease)
		{
			if (pressRelease)
			{
				_mixer.Volume.CreateSignedRamp(DeviceLevelMax, RampTime);
			}
			else
			{
				_mixer.Volume.StopRamp();
			}
		}

		public void VolumeDown(bool pressRelease)
		{
			if (pressRelease)
			{
				_mixer.Volume.CreateSignedRamp(DeviceLevelMin, RampTime);
			}
			else
			{
				_mixer.Volume.StopRamp();
			}
		}

		#endregion




		#region Mute

		private void OnMuteAndVolumeControlPropertyChange(MuteControl device, GenericEventArgs args)
		{
			Debug.Console(2, this, "OnMuteAndVolumeControlPropertyChange: {0} > Index-{1}, EventId-{2}", device.ToString(), args.Index, args.EventId);

			switch (args.EventId)
			{
				case MuteAndVolumeContorlEventIds.VolumeFeedbackEventId:
					{
						VolumeLevel = _mixer.VolumeFeedback.ShortValue;
						break;
					}
				case MuteAndVolumeContorlEventIds.MuteOnEventId:
				case MuteAndVolumeContorlEventIds.MuteOffEventId:
					{
						IsMuted = _mixer.AuxiliaryMuteControl.MuteOnFeedback.BoolValue;
						break;
					}
				default:
				{
					Debug.Console(1, this, "OnMuteAndVolumeControlPropertyChange: {0} > Index-{1}, EventId-{2} - unhandled eventId", device.ToString(), args.Index, args.EventId);
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
			_mixer.AuxiliaryMuteControl.MuteOn();
		}

		public void MuteOff()
		{
			_mixer.AuxiliaryMuteControl.MuteOff();
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