using Crestron.SimplSharp.Ssh;
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

        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

        public HDBaseTRxController(string key, string name, HDRx3CB rmc)
            : base(key, name, rmc)
        {
            DmIn = new RoutingInputPort(DmPortName.DmIn, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.DmCat, 0, this);
            HDBaseTSink = new RoutingOutputPort(DmPortName.HdmiOut, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.Hdmi, null, this) {Port = Rmc};

            InputPorts = new RoutingPortCollection<RoutingInputPort> {DmIn};
            OutputPorts = new RoutingPortCollection<RoutingOutputPort> {HDBaseTSink};
        }

        #region IComPorts Members
        public CrestronCollection<ComPort> ComPorts { get { return Rmc.ComPorts; } }
        public int NumberOfComPorts { get { return Rmc.NumberOfComPorts; } }
        #endregion
    }
}