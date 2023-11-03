using System;
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

			VolumeLevelFeedback = new IntFeedback(VolumeFeedbackFunc);
			MuteFeedback = new BoolFeedback(MuteFeedbackFunc);
		}

		#region Volume

		public IntFeedback VolumeLevelFeedback { get; private set; }

		protected Func<int> VolumeFeedbackFunc
		{
			get { return () => Port.AudioOutput.VolumeFeedback.UShortValue; }
		}

		public void SetVolume(ushort level)
		{
			Port.AudioOutput.Volume.UShortValue = level;
		}

		public void VolumeUp(bool pressRelease)
		{
			if (pressRelease)
			{
				var remainingRatio = (65535 - Port.AudioOutput.Volume.UShortValue)/65535;
				Port.AudioOutput.Volume.CreateRamp(65535, 400);
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
				var remainingRatio = Port.AudioOutput.Volume.UShortValue/65535;
				Port.AudioOutput.Volume.CreateRamp(0, (uint)(400 * remainingRatio));
			}
			else
			{
				Port.AudioOutput.Volume.StopRamp();
			}
		}

		#endregion


		#region Mute

		private bool _isMuted;

		public BoolFeedback MuteFeedback { get; private set; }

		protected Func<bool> MuteFeedbackFunc
		{			
			get { return () => _isMuted = Port.MuteOnFeedback.BoolValue; }
		}

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
			if (_isMuted)
				MuteOff();
			else
				MuteOn();
		}

		#endregion
	}
}