using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.DM
{
	/// <summary>
	/// Builds a controller for basic DM-RMCs (both 4K and non-4K) with Com and IR ports and no control functions
	/// 
	/// </summary>
	public class DmRmcX100CController : DmRmcControllerBase, IRoutingInputsOutputs, 
		IIROutputPorts, IComPorts, ICec
	{
		public DmRmc100C Rmc { get; private set; }

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
		public DmRmcX100CController(string key, string name, DmRmc100C rmc)
			: base(key, name, rmc)
		{
			Rmc = rmc;
			DmIn = new RoutingInputPort(DmPortName.DmIn, eRoutingSignalType.Audio | eRoutingSignalType.Video, 
				eRoutingPortConnectionType.DmCat, 0, this);
			HdmiOut = new RoutingOutputPort(DmPortName.HdmiOut, eRoutingSignalType.Audio | eRoutingSignalType.Video, 
				eRoutingPortConnectionType.Hdmi, null, this);

            // Set Ports for CEC
            HdmiOut.Port = Rmc; // Unique case, this class has no HdmiOutput port and ICec is implemented on the receiver class itself
		}

		public override bool CustomActivate()
		{
			// Base does register and sets up comm monitoring.
			return base.CustomActivate();
		}

	    public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
	    {
	        LinkDmRmcToApi(this, trilist, joinStart, joinMapKey, bridge);
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
		public Cec StreamCec { get { return Rmc.StreamCec; } }
		#endregion
	}
}