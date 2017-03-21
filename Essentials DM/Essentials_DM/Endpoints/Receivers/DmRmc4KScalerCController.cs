using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;

using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.DM
{
	/// <summary>
	/// Builds a controller for basic DM-RMCs with Com and IR ports and no control functions
	/// 
	/// </summary>
	public class DmRmc4kScalerCController : DmRmcControllerBase, IRoutingInputsOutputs, IBasicVolumeWithFeedback,
		IIROutputPorts, IComPorts, ICec, IRelayPorts
	{
		public DmRmc4kScalerC Rmc { get; private set; }

		public RoutingInputPort DmIn { get; private set; }
		public RoutingOutputPort HdmiOut { get; private set; }
		public RoutingOutputPort BalancedAudioOut { get; private set; }

		public RoutingPortCollection<RoutingInputPort> InputPorts
		{
			get { return new RoutingPortCollection<RoutingInputPort> { DmIn }; }
		}

		public RoutingPortCollection<RoutingOutputPort> OutputPorts
		{
			get { return new RoutingPortCollection<RoutingOutputPort> { HdmiOut, BalancedAudioOut }; }
		}

		/// <summary>
		///  Make a Crestron RMC and put it in here
		/// </summary>
		public DmRmc4kScalerCController(string key, string name, DmRmc4kScalerC rmc)
			: base(key, name, rmc)
		{
			Rmc = rmc;
			DmIn = new RoutingInputPort(DmPortName.DmIn, eRoutingSignalType.AudioVideo, 
				eRoutingPortConnectionType.DmCat, 0, this);
			HdmiOut = new RoutingOutputPort(DmPortName.HdmiOut, eRoutingSignalType.AudioVideo, 
				eRoutingPortConnectionType.Hdmi, null, this);
			BalancedAudioOut = new RoutingOutputPort(DmPortName.BalancedAudioOut, eRoutingSignalType.Audio,
				eRoutingPortConnectionType.LineAudio, null, this);

			MuteFeedback = new BoolFeedback(() => false);
			VolumeLevelFeedback = new IntFeedback(CommonIntCue.MainVolumeLevelFeedback, () => 
				rmc.AudioOutput.VolumeFeedback.UShortValue);
		}

		public override bool CustomActivate()
		{
			// Base does register and sets up comm monitoring.
			return base.CustomActivate();
		}

		#region IIROutputPorts Members
		public CrestronCollection<IROutputPort> IROutputPorts { get { return Rmc.IROutputPorts; } }
		public int NumberOfIROutputPorts { get { return Rmc.NumberOfIROutputPorts; } }
		#endregion

		#region IComPorts Members
		public CrestronCollection<ComPort> ComPorts { get { return Rmc.ComPorts; } }
		public int NumberOfComPorts { get { return Rmc.NumberOfComPorts; } }
		#endregion

		#region ICec Members
		/// <summary>
		/// Gets the CEC stream directly from the HDMI port.
		/// </summary>
		public Cec StreamCec { get { return Rmc.HdmiOutput.StreamCec; } }
		#endregion

		#region IRelayPorts Members

		public int NumberOfRelayPorts
		{
			get { return Rmc.NumberOfRelayPorts; }
		}

		public CrestronCollection<Relay> RelayPorts
		{
			get { return Rmc.RelayPorts; }
		}

		#endregion

		#region IBasicVolumeWithFeedback Members

		public BoolFeedback MuteFeedback
		{
			get;
			private set;
		}

		/// <summary>
		/// Not implemented
		/// </summary>
		public void MuteOff()
		{	
		}

		/// <summary>
		/// Not implemented
		/// </summary>
		public void MuteOn()
		{
		}

		public void SetVolume(ushort level)
		{
			Rmc.AudioOutput.Volume.UShortValue = level;
		}

		public IntFeedback VolumeLevelFeedback
		{
			get;
			private set;
		}

		#endregion

		#region IBasicVolumeControls Members

		/// <summary>
		/// Not implemented
		/// </summary>
		public void MuteToggle()
		{
		}

		public void VolumeDown(bool pressRelease)
		{
			if (pressRelease)
				SigHelper.RampTimeScaled(Rmc.AudioOutput.Volume, 0, 4000);
			else
				Rmc.AudioOutput.Volume.StopRamp();
		}

		public void VolumeUp(bool pressRelease)
		{
			if (pressRelease)
				SigHelper.RampTimeScaled(Rmc.AudioOutput.Volume, 65535, 4000);
			else
				Rmc.AudioOutput.Volume.StopRamp();
		}

		#endregion
	}
}