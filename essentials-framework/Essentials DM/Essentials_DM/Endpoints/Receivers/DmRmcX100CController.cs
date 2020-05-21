using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.DM
{
	/// <summary>
	/// Builds a controller for basic DM-RMCs (both 4K and non-4K) with Com and IR ports and no control functions
	/// 
	/// </summary>
    [Description("Wrapper Class for DM-RMC-4K-100-C & DM-RMC-100-C")]
    public class DmRmcX100CController : DmRmcControllerBase, IRoutingInputsOutputs, 
		IIROutputPorts, IComPorts, ICec
	{
	    private readonly DmRmc100C _rmc;

		public RoutingInputPort DmIn { get; protected set; }
		public RoutingOutputPort HdmiOut { get; protected set; }

	    public RoutingPortCollection<RoutingInputPort> InputPorts { get; protected set; }

	    public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; protected set; }

		/// <summary>
		///  Make a Crestron RMC and put it in here
		/// </summary>
		public DmRmcX100CController(string key, string name, DmRmc100C rmc)
			: base(key, name, rmc)
		{
			_rmc = rmc;
			DmIn = new RoutingInputPort(DmPortName.DmIn, eRoutingSignalType.AudioVideo, 
				eRoutingPortConnectionType.DmCat, 0, this);
			HdmiOut = new RoutingOutputPort(DmPortName.HdmiOut, eRoutingSignalType.AudioVideo, 
				eRoutingPortConnectionType.Hdmi, null, this) {Port = _rmc};

		    InputPorts = new RoutingPortCollection<RoutingInputPort> {DmIn};
		    OutputPorts = new RoutingPortCollection<RoutingOutputPort> {HdmiOut};
		}

	    public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
	    {
	        LinkDmRmcToApi(this, trilist, joinStart, joinMapKey, bridge);
	    }

	    #region IIROutputPorts Members
		public CrestronCollection<IROutputPort> IROutputPorts { get { return _rmc.IROutputPorts; } }
		public int NumberOfIROutputPorts { get { return _rmc.NumberOfIROutputPorts; } }
		#endregion

		#region IComPorts Members
		public CrestronCollection<ComPort> ComPorts { get { return _rmc.ComPorts; } }
		public int NumberOfComPorts { get { return _rmc.NumberOfComPorts; } }
		#endregion

		#region ICec Members
		public Cec StreamCec { get { return _rmc.StreamCec; } }
		#endregion
	}
}