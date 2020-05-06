using Crestron.SimplSharpPro;
//using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.DM
{
    using eVst = eX02VideoSourceType;
    using eAst = eX02AudioSourceType;

    public class DmTx4k100Controller : DmTxControllerBase, IRoutingInputsOutputs,
        IIROutputPorts, IComPorts, ICec
    {
        public DmTx4K100C1G Tx { get; private set; }

        public RoutingInputPort HdmiIn { get; private set; }
        public RoutingOutputPort DmOut { get; private set; }

        //public IntFeedback VideoSourceNumericFeedback { get; protected set; }
        //public IntFeedback AudioSourceNumericFeedback { get; protected set; }
        //public IntFeedback HdmiIn1HdcpCapabilityFeedback { get; protected set; }
        //public IntFeedback HdmiIn2HdcpCapabilityFeedback { get; protected set; }

        //public override IntFeedback HdcpSupportAllFeedback { get; protected set; }
        //public override ushort HdcpSupportCapability { get; protected set; }

        /// <summary>
        /// Helps get the "real" inputs, including when in Auto
        /// </summary>
        public eX02VideoSourceType ActualActiveVideoInput
        {
            get
                {
                    return eVst.Hdmi1;
                }
        }
        public RoutingPortCollection<RoutingInputPort> InputPorts
        {
            get
            {
                return new RoutingPortCollection<RoutingInputPort> 
				{ 
					HdmiIn				 
				};
            }
        }
        public RoutingPortCollection<RoutingOutputPort> OutputPorts
        {
            get
            {
                return new RoutingPortCollection<RoutingOutputPort> { DmOut };
            }
        }
        public DmTx4k100Controller(string key, string name, DmTx4K100C1G tx)
            : base(key, name, tx)
        {
            Tx = tx;

            HdmiIn = new RoutingInputPort(DmPortName.HdmiIn1,
                eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, eVst.Hdmi1, this);

            DmOut = new RoutingOutputPort(DmPortName.DmOut, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.DmCat, null, this);

            // Set Ports for CEC
            HdmiIn.Port = Tx;
        }


        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            LinkDmTxToApi(this, trilist, joinStart, joinMapKey, bridge);
        }

        #region IIROutputPorts Members
        public CrestronCollection<IROutputPort> IROutputPorts { get { return Tx.IROutputPorts; } }
        public int NumberOfIROutputPorts { get { return Tx.NumberOfIROutputPorts; } }
        #endregion

        #region IComPorts Members
        public CrestronCollection<ComPort> ComPorts { get { return Tx.ComPorts; } }
        public int NumberOfComPorts { get { return Tx.NumberOfComPorts; } }
        #endregion

        #region ICec Members
        public Cec StreamCec { get { return Tx.StreamCec; } }
        #endregion

        public override StringFeedback ActiveVideoInputFeedback { get; protected set; }
    }
}