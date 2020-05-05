using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;

using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.DM
{
    public class HDBaseTRxController : DmHdBaseTControllerBase, IRoutingInputsOutputs,
             IComPorts
    {
        public RoutingInputPort DmIn { get; private set; }
        public RoutingOutputPort HDBaseTSink { get; private set; }

        public RoutingPortCollection<RoutingInputPort> InputPorts
        {
            get { return new RoutingPortCollection<RoutingInputPort> { DmIn }; }
        }

        public RoutingPortCollection<RoutingOutputPort> OutputPorts
        {
            get { return new RoutingPortCollection<RoutingOutputPort> { HDBaseTSink }; }
        }

        public HDBaseTRxController(string key, string name, HDRx3CB rmc)
            : base(key, name, rmc)
        {
            Rmc = rmc;
            DmIn = new RoutingInputPort(DmPortName.DmIn, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.DmCat, 0, this);
            HDBaseTSink = new RoutingOutputPort(DmPortName.HdmiOut, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, null, this);

            // Set Ports for CEC
            HDBaseTSink.Port = Rmc; // Unique case, this class has no HdmiOutput port and ICec is implemented on the receiver class itself
        }

        #region IComPorts Members
        public CrestronCollection<ComPort> ComPorts { get { return Rmc.ComPorts; } }
        public int NumberOfComPorts { get { return Rmc.NumberOfComPorts; } }
        #endregion
    }
}