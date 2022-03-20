using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.DM.Config;

namespace PepperDash.Essentials.DM
{
    // using eVst = Crestron.SimplSharpPro.DeviceSupport.eX02VideoSourceType;

    /// <summary>
    /// Controller class for all DM-TX-201C/S/F transmitters
    /// </summary>
    [Description("Wrapper class for DM-TX-4K-Z-100-C")]
    public class DmTx4kz100Controller : BasicDmTxControllerBase, IRoutingInputsOutputs, IHasFeedback,
        IIROutputPorts, IComPorts, ICec
    {
        public DmTx4kz100C1G Tx { get; private set; }

       public RoutingInputPort HdmiIn { get; private set; }
        public RoutingOutputPort DmOut { get; private set; }

        /// <summary>
        /// Helps get the "real" inputs, including when in Auto
        /// </summary>
        public eX02VideoSourceType ActualActiveVideoInput
        {
            get
                {
                    return eX02VideoSourceType.Hdmi1;
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
        public DmTx4kz100Controller(string key, string name, DmTx4kz100C1G tx)
            : base(key, name, tx)
        {
            Tx = tx;

            HdmiIn = new RoutingInputPort(DmPortName.HdmiIn1,
                eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, eX02VideoSourceType.Hdmi1, this);

            DmOut = new RoutingOutputPort(DmPortName.DmOut, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.DmCat, null, this);

            // Set Ports for CEC
            HdmiIn.Port = Tx;

            PreventRegistration = true;
        }

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            Debug.Console(1, this, "No properties to link. Skipping device {0}", Name);
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
        public Cec StreamCec { get { return Tx.HdmiInput.StreamCec; } }
        #endregion
    }
}