using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;

using PepperDash.Essentials.Core;


namespace PepperDash.Essentials.DM
{

    [Description("Wrapper Class for DM-RMC-4K-100-C-1G")]
    public class DmRmc4k100C1GController : DmHdBaseTControllerBase, IRoutingInputsOutputs,
        IIROutputPorts, IComPorts, ICec
    {
        private readonly DmRmc4K100C1G _rmc;
        public RoutingInputPort DmIn { get; private set; }
        public RoutingOutputPort HdmiOut { get; private set; }

        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

        public DmRmc4k100C1GController(string key, string name, DmRmc4K100C1G rmc)
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