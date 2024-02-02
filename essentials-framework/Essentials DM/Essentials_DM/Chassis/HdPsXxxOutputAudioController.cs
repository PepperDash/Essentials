using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DM;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash_Essentials_DM.Chassis
{
	public class HdPsXxxOutputAudioController : IKeyed,
		IHasVolumeControlWithFeedback, IHasMuteControlWithFeedback
	{
		public string Key { get; private set; }

		private readonly HdPsXxxHdmiDmLiteOutputMixer _mixer;	// volume/volumeFeedback
		private readonly HdPsXxxOutputPort _port;				// mute/muteFeedback

		public HdPsXxxOutputAudioController(string parent, uint output, HdPsXxx chassis)
		{
			Key = string.Format("{0}-audioOut{1}", parent, output);

			_port = chassis.HdmiDmLiteOutputs[output].OutputPort;
			_mixer = chassis.HdmiDmLiteOutputs[output].Mixer;

			chassis.DMOutputChange += ChassisOnDmOutputChange;

			VolumeLevelFeedback = new IntFeedback(() => VolumeLevel);
			MuteFeedback = new BoolFeedback(() => IsMuted);
		}

		private void ChassisOnDmOutputChange(Switch device, DMOutputEventArgs args)
		{
			switch (args.EventId)
			{
				case (DMOutputEventIds.VolumeEventId):
					{
						Debug.Console(2, this, "HdPsXxxOutputAudioController: {0} > Index-{1}, Number-{3}, EventId-{2} - AudioMute/UnmuteEventId",
							device.ToString(), args.Index, args.EventId, args.Number);

						VolumeLevel = _mixer.VolumeFeedback.ShortValue;

						break;
					}
				case DMOutputEventIds.MuteOnEventId:
				case DMOutputEventIds.MuteOffEventId:
					{
						Debug.Console(2, this, "HdPsXxxOutputAudioController: {0} > Index-{1}, Number-{3}, EventId-{2} - MuteOnEventId/MuteOffEventId",
							device.ToString(), args.Index, args.EventId, args.Number);
						
						IsMuted = _port.MuteOnFeedback.BoolValue;
						
						break;
					}
				default:
					{
						Debug.Console(1, this, "HdPsXxxOutputAudioController: {0} > Index-{1}, Number-{3}, EventId-{2} - unhandled eventId", 
							device.ToString(), args.Index, args.EventId, args.Number);
						break;
					}
			}
		}

		#region Volume

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

				Debug.Console(2, this, "VolumeFeedback: level-'{0}', scaled-'{1}'", level, _volumeLevel);

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
			_port.MuteOn();
		}

		public void MuteOff()
		{
			_port.MuteOff();
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