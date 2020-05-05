using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;

using PepperDash.Essentials.Core;


namespace PepperDash.Essentials.DM
{
    public class DmRmc4k100C1GController : DmHdBaseTControllerBase, IRoutingInputsOutputs,
        IIROutputPorts, IComPorts, ICec
    {
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

        public DmRmc4k100C1GController(string key, string name, DmRmc4K100C1G rmc)
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

        #region IIROutputPorts Members
        public CrestronCollection<IROutputPort> IROutputPorts { get { return (Rmc as DmRmc4K100C1G).IROutputPorts; } }
        public int NumberOfIROutputPorts { get { return (Rmc as DmRmc4K100C1G).NumberOfIROutputPorts; } }
        #endregion

        #region IComPorts Members
        public CrestronCollection<ComPort> ComPorts { get { return Rmc.ComPorts; } }
        public int NumberOfComPorts { get { return Rmc.NumberOfComPorts; } }
        #endregion

        #region ICec Members
        public Cec StreamCec { get { return (Rmc as DmRmc4K100C1G).StreamCec; } }
        #endregion
    }
}