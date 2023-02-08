extern alias Full;

using Crestron.SimplSharpPro;
//using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using Full.Newtonsoft.Json;

namespace PepperDash.Essentials.DM
{
    using eVst = eX02VideoSourceType;
    using eAst = eX02AudioSourceType;

    public class DmTx4k100Controller : BasicDmTxControllerBase, IRoutingInputsOutputs,
        IIROutputPorts, IComPorts, ICec
    {
        public DmTx4K100C1G Tx { get; private set; }

        public RoutingInputPort HdmiIn { get; private set; }
        public RoutingOutputPort DmOut { get; private set; }

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

            PreventRegistration = true;

            var parentDev = DeviceManager.GetDeviceForKey(key);
            var num = tx.DMInputOutput.Number;
            if (parentDev is DmpsRoutingController)
            {
                var dmps = parentDev as DmpsRoutingController;
                IsOnline.SetValueFunc(() => dmps.InputEndpointOnlineFeedbacks[num].BoolValue);
                dmps.InputEndpointOnlineFeedbacks[num].OutputChange += (o, a) => IsOnline.FireUpdate();
            }
            else if (parentDev is DmChassisController)
            {
                var controller = parentDev as DmChassisController;
                IsOnline.SetValueFunc(() => controller.InputEndpointOnlineFeedbacks[num].BoolValue);
                controller.InputEndpointOnlineFeedbacks[num].OutputChange += (o, a) => IsOnline.FireUpdate();
            }
        }

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new HDBaseTTxControllerJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<HDBaseTTxControllerJoinMap>(joinMapSerialized);

            this.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);
            trilist.StringInput[joinMap.Name.JoinNumber].StringValue = this.Name;
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
    }
}