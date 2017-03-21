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
	public class DmRmcScalerCController : DmRmcControllerBase, IRoutingInputsOutputs, 
		IIROutputPorts, IComPorts, ICec
	{
		public DmRmcScalerC Rmc { get; private set; }

		public RoutingInputPort DmIn { get; private set; }
		public RoutingOutputPort HdmiOut { get; private set; }

		public RoutingPortCollection<RoutingInputPort> InputPorts
		{
			get { return new RoutingPortCollection<RoutingInputPort> { DmIn }; }
		}

		public RoutingPortCollection<RoutingOutputPort> OutputPorts
		{
			get { return new RoutingPortCollection<RoutingOutputPort> { HdmiOut }; }
		}

		/// <summary>
		///  Make a Crestron RMC and put it in here
		/// </summary>
		public DmRmcScalerCController(string key, string name, DmRmcScalerC rmc)
			: base(key, name, rmc)
		{
			Rmc = rmc;
			DmIn = new RoutingInputPort(DmPortName.DmIn, eRoutingSignalType.AudioVideo, 
				eRoutingPortConnectionType.DmCat, 0, this);
			HdmiOut = new RoutingOutputPort(DmPortName.HdmiOut, eRoutingSignalType.AudioVideo, 
				eRoutingPortConnectionType.Hdmi, null, this);
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
	}
}