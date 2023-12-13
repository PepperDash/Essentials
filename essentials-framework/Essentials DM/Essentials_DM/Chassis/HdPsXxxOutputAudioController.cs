using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DM;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash_Essentials_DM.Chassis
{
	public class HdPsXxxOutputAudioController : IKeyed,
		IHasVolumeControlWithFeedback, IHasMuteControlWithFeedback // || IBasicVolumeWithFeedback
	{
		public string Key { get; private set; }

		public HdPsXxxOutputPort Port { get; private set; }

		public HdPsXxxOutputAudioController(string parent, uint output, HdPsXxx chassis)
		{
			Key = string.Format("{0}-audioOut{1}", parent, output);

			Port = chassis.HdmiDmLiteOutputs[output].OutputPort;

			VolumeLevelFeedback = new IntFeedback(() => VolumeLevel);
			MuteFeedback = new BoolFeedback(() => IsMuted);

			//if(Port.AudioOutput.Volume != null)
			//    VolumeLevel = Port.AudioOutput.VolumeFeedback.UShortValue;

			IsMuted = Port.MuteOnFeedback.BoolValue;
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
			set
			{
				var level = value;
				Debug.Console(1, this, "VolumeLevel: value-'{0}', level-'{1}'", value, level);
				
				// ScaleWithLimits(inputValue, InputUpperBound, InputLowerBound, OutputUpperBound, OutputLowerBound)
				_volumeLevel = CrestronEnvironment.ScaleWithLimits(level, DeviceLevelMax, DeviceLevelMin, CrestronLevelMax, CrestronLevelMin);

				Debug.Console(2, this, "VolumeFeedback: level-'{0}', scaled-'{1}'", level, _volumeLevel);

				VolumeLevelFeedback.FireUpdate();
			}
		}

		public IntFeedback VolumeLevelFeedback { get; private set; }

		public void SetVolume(ushort level)
		{
			// ScaleWithLimits(inputValue, InputUpperBound, InputLowerBound, OutputUpperBound, OutputLowerBound)
			var scaled = CrestronEnvironment.ScaleWithLimits(level, 
				CrestronLevelMax, CrestronLevelMin, 
				DeviceLevelMax, DeviceLevelMin);

			Debug.Console(1, this, "SetVolume: level-'{0}', scaled-'{1}'", level, scaled);

			Port.AudioOutput.Volume.ShortValue = (short)scaled;
		}

		public void VolumeUp(bool pressRelease)
		{
			if (pressRelease)
			{
				Port.AudioOutput.Volume.CreateSignedRamp(DeviceLevelMax, RampTime);
			}
			else
			{
				Port.AudioOutput.Volume.StopRamp();
			}
		}

		public void VolumeDown(bool pressRelease)
		{
			if (pressRelease)
			{
				Port.AudioOutput.Volume.CreateSignedRamp(DeviceLevelMin, RampTime);
			}
			else
			{
				Port.AudioOutput.Volume.StopRamp();
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
			Port.MuteOn();
		}

		public void MuteOff()
		{
			Port.MuteOff();
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