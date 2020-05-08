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
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.DM
{
    /// <summary>
    /// Represent both a transmitter and receiver pair of the HD-MD-400-C-E / HD-MD-300-C-E / HD-MD-200-C-E kits
    /// </summary>
    [Description("Wrapper class for all HD-MD variants")]
    public class HdMdxxxCEController : CrestronGenericBridgeableBaseDevice, IRouting//, IComPorts
    {
        /// <summary>
        /////  DmLite Ports
        ///// </summary>
        //public RoutingOutputPort ToRx { get; private set; }
        //public RoutingInputPort FromTx { get; private set; }

        public RoutingOutputPort HdmiOut { get; private set; }

        public HdMdxxxCE TxRxPair { get; private set; }

        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

        /// <summary>
        /// The value of the current video source for the HDMI output on the receiver
        /// </summary>
        public IntFeedback VideoSourceFeedback { get; private set; }

        /// <summary>
        /// Indicates if Auto Route is on on the transmitter
        /// </summary>
        public BoolFeedback AutoRouteOnFeedback { get; private set; }

        /// <summary>
        /// Indicates if Priority Routing is on on the transmitter
        /// </summary>
        public BoolFeedback PriorityRoutingOnFeedback { get; private set; }

        /// <summary>
        /// INdicates if the On Screen Display is enabled 
        /// </summary>
        public BoolFeedback InputOnScreenDisplayEnabledFeedback { get; private set; }

        /// <summary>
        /// Indicates if video sync is detected on each of the inputs
        /// </summary>
        public Dictionary<uint, BoolFeedback> SyncDetectedFeedbacks { get; private set; }

        /// <summary>
        /// Indicates if the remote end device is detected
        /// </summary>
        public BoolFeedback RemoteEndDetectedFeedback { get; private set; }

        public RoutingPortCollection<RoutingOutputPort> OutputPorts
        {
            get { return new RoutingPortCollection<RoutingOutputPort> { HdmiOut }; }
        }

        public HdMdxxxCEController(string key, string name, HdMdxxxCE txRxPair)
            :base(key, name, txRxPair)
        {

            TxRxPair = txRxPair;

            RemoteEndDetectedFeedback = new BoolFeedback(() => TxRxPair.RemoteEndDetectedOnFeedback.BoolValue);

            AutoRouteOnFeedback = new BoolFeedback(() => TxRxPair.TransmitterAutoModeOnFeedback.BoolValue);

            PriorityRoutingOnFeedback = new BoolFeedback(() => TxRxPair.PriorityRoutingOnFeedback.BoolValue);

            InputOnScreenDisplayEnabledFeedback = new BoolFeedback(() => TxRxPair.OnScreenDisplayEnabledFeedback.BoolValue);

            InputPorts = new RoutingPortCollection<RoutingInputPort>();

            SyncDetectedFeedbacks = new Dictionary<uint, BoolFeedback>();

            // Add the HDMI input port on the receiver
            InputPorts.Add(new RoutingInputPort(DmPortName.Hdmi, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, 1, this));

            SyncDetectedFeedbacks.Add(1, new BoolFeedback( () => TxRxPair.HdmiInputs[1].VideoDetectedFeedback.BoolValue));

            if(txRxPair is HdMd400CE)
            {
                InputPorts.Add(new RoutingInputPort(DmPortName.HdmiIn1, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, 2, this));
                SyncDetectedFeedbacks.Add(2, new BoolFeedback(() => TxRxPair.HdmiInputs[2].VideoDetectedFeedback.BoolValue));

                InputPorts.Add(new RoutingInputPort(DmPortName.HdmiIn2, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, 3, this));
                SyncDetectedFeedbacks.Add(3, new BoolFeedback(() => TxRxPair.HdmiInputs[3].VideoDetectedFeedback.BoolValue));

                InputPorts.Add(new RoutingInputPort(DmPortName.VgaIn, eRoutingSignalType.Video | eRoutingSignalType.Audio,
                    eRoutingPortConnectionType.Vga, 4, this));
                SyncDetectedFeedbacks.Add(4, new BoolFeedback(() => TxRxPair.VgaInputs[1].VideoDetectedFeedback.BoolValue));

                // Set Ports for CEC
                InputPorts[DmPortName.HdmiIn1].Port = TxRxPair.HdmiInputs[1];
                InputPorts[DmPortName.HdmiIn2].Port = TxRxPair.HdmiInputs[2];
            }
            else if (txRxPair is HdMd300CE)
            {
                InputPorts.Add(new RoutingInputPort(DmPortName.HdmiIn, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                    eRoutingPortConnectionType.Hdmi, 2, this));
                SyncDetectedFeedbacks.Add(2, new BoolFeedback(() => TxRxPair.HdmiInputs[2].VideoDetectedFeedback.BoolValue));

                InputPorts.Add(new RoutingInputPort(DmPortName.VgaIn, eRoutingSignalType.Video | eRoutingSignalType.Audio,
                    eRoutingPortConnectionType.Vga, 3, this));
                SyncDetectedFeedbacks.Add(3, new BoolFeedback(() => TxRxPair.VgaInputs[1].VideoDetectedFeedback.BoolValue));

                // Set Ports for CEC
                InputPorts[DmPortName.HdmiIn].Port = TxRxPair.HdmiInputs[1];
            }
            else if (txRxPair is HdMd200CE || txRxPair is HdMd200C1GE)
            {
                InputPorts.Add(new RoutingInputPort(DmPortName.HdmiIn, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                    eRoutingPortConnectionType.Hdmi, 2, this));
                SyncDetectedFeedbacks.Add(2, new BoolFeedback(() => TxRxPair.HdmiInputs[2].VideoDetectedFeedback.BoolValue));

                // Set Ports for CEC
                InputPorts[DmPortName.HdmiIn].Port = TxRxPair.HdmiInputs[1];
            }

            //ToRx = new RoutingOutputPort(DmPortName.ToTx, eRoutingSignalType.Audio | eRoutingSignalType.Video,
            //    eRoutingPortConnectionType.DmCat, null, this);

            //FromTx = new RoutingInputPort(DmPortName.FromTx, eRoutingSignalType.Audio | eRoutingSignalType.Video,
            //    eRoutingPortConnectionType.DmCat, null, this);

            HdmiOut = new RoutingOutputPort(DmPortName.HdmiOut, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, null, this);

            OutputPorts[DmPortName.HdmiOut].Port = TxRxPair.HdmiOutputs[1];

            TxRxPair.DMInputChange += new DMInputEventHandler(TxRxPair_DMInputChange);
            TxRxPair.DMOutputChange += new DMOutputEventHandler(TxRxPair_DMOutputChange);
            TxRxPair.DMSystemChange += new DMSystemEventHandler(TxRxPair_DMSystemChange);

            VideoSourceFeedback = new IntFeedback(() => (int)TxRxPair.HdmiOutputs[1].VideoOutFeedback.Number);
        }

        void TxRxPair_DMSystemChange(Switch device, DMSystemEventArgs args)
        {
            if (args.EventId == DMSystemEventIds.RemoteEndDetectedEventId)
                RemoteEndDetectedFeedback.FireUpdate();
            else if (args.EventId == DMSystemEventIds.TransmitterAutoModeOnEventId)
                AutoRouteOnFeedback.FireUpdate();
            else if (args.EventId == DMSystemEventIds.PriorityRoutingOnEventId)
                PriorityRoutingOnFeedback.FireUpdate();
            else if (args.EventId == DMSystemEventIds.OnScreenDisplayEnabledEventId)
                InputOnScreenDisplayEnabledFeedback.FireUpdate();
        }

        void TxRxPair_DMOutputChange(Switch device, DMOutputEventArgs args)
        {
            if (args.EventId == DMOutputEventIds.VideoOutEventId)
                VideoSourceFeedback.FireUpdate();
        }

        void TxRxPair_DMInputChange(Switch device, DMInputEventArgs args)
        {
            if (args.EventId == DMInputEventIds.VideoDetectedEventId)
                SyncDetectedFeedbacks[args.Number].FireUpdate();
        }

        public void AutoRouteOn()
        {
            TxRxPair.TransmitterAutoModeOn();
        }

        public void AutoRouteOff()
        {
            TxRxPair.TransmitterAutoModeOff();
        }

        public void PriorityRouteOn()
        {
            TxRxPair.PriorityRoutingOn();
        }

        public void PriorityRouteOff()
        {
            TxRxPair.PriorityRoutingOff();
        }

        public void OnScreenDisplayEnable()
        {
            TxRxPair.OnScreenDisplayEnabled();
        }

        public void OnScreenDisplayDisable()
        {
            TxRxPair.OnScreenDisplayDisabled();
        }

        public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType)
        {
            var number = Convert.ToUInt32(inputSelector); // Cast can sometimes fail

            var input = number == 0 ? null : TxRxPair.Inputs[number];

            TxRxPair.HdmiOutputs[1].VideoOut = input;
        }

        // This device has a different class for com ports which will make it hard to implement IComPorts....

        //#region IComPorts Members
        //public CrestronCollection<ComPort> ComPorts { get { return TxRxPair.ComPorts as CrestronCollection<ComPort>; } }
        //public int NumberOfComPorts { get { return 1; } }
        //#endregion
        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new HdMdxxxCEControllerJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<HdMdxxxCEControllerJoinMap>(joinMapSerialized);

            bridge.AddJoinMap(Key, joinMap);

            Debug.Console(1, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);
            RemoteEndDetectedFeedback.LinkInputSig(trilist.BooleanInput[joinMap.RemoteEndDetected.JoinNumber]);

            trilist.SetSigTrueAction(joinMap.AutoRouteOn.JoinNumber, AutoRouteOn);
            AutoRouteOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.AutoRouteOn.JoinNumber]);
            trilist.SetSigTrueAction(joinMap.AutoRouteOff.JoinNumber, AutoRouteOff);
            AutoRouteOnFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.AutoRouteOff.JoinNumber]);

            trilist.SetSigTrueAction(joinMap.PriorityRoutingOn.JoinNumber, PriorityRouteOn);
            PriorityRoutingOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PriorityRoutingOn.JoinNumber]);
            trilist.SetSigTrueAction(joinMap.PriorityRoutingOff.JoinNumber, PriorityRouteOff);
            PriorityRoutingOnFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.PriorityRoutingOff.JoinNumber]);

            trilist.SetSigTrueAction(joinMap.InputOnScreenDisplayEnabled.JoinNumber, OnScreenDisplayEnable);
            InputOnScreenDisplayEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.InputOnScreenDisplayEnabled.JoinNumber]);
            trilist.SetSigTrueAction(joinMap.AutoRouteOff.JoinNumber, OnScreenDisplayDisable);
            InputOnScreenDisplayEnabledFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.InputOnScreenDisplayDisabled.JoinNumber]);

            trilist.SetUShortSigAction(joinMap.VideoSource.JoinNumber, (i) => ExecuteSwitch(i, null, eRoutingSignalType.Video | eRoutingSignalType.Audio));
            VideoSourceFeedback.LinkInputSig(trilist.UShortInput[joinMap.VideoSource.JoinNumber]);

            trilist.UShortInput[joinMap.SourceCount.JoinNumber].UShortValue = (ushort)InputPorts.Count;

            foreach (var input in InputPorts)
            {
                var number = Convert.ToUInt16(input.Selector);
                var numberJoin = (UInt16)(number - 1);
                SyncDetectedFeedbacks[number].LinkInputSig(trilist.BooleanInput[joinMap.SyncDetected.JoinNumber + numberJoin]);
                trilist.StringInput[joinMap.SourceNames.JoinNumber + numberJoin].StringValue = input.Key;
            }
        }
    }

    public class HdMdxxxCEPropertiesConfig
    {
        public ControlPropertiesConfig Control { get; set; }
    }

    public class HdMdxxxCEControllerFactory : EssentialsDeviceFactory<HdMdxxxCEController>
    {
        public HdMdxxxCEControllerFactory()
        {
            TypeNames = new List<string>() { "hdmd400ce", "hdmd300ce", "hdmd200ce", "hdmd200c1ge"};
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            var typeName = dc.Type.ToLower();
            var key = dc.Key;
            var name = dc.Name;

            Debug.Console(1, "Factory Attempting to create new HD-MD Device");

            var props = JsonConvert.DeserializeObject
                <PepperDash.Essentials.DM.HdMdxxxCEPropertiesConfig>(dc.Properties.ToString());

            if (typeName.Equals("hdmd400ce"))
                return new PepperDash.Essentials.DM.HdMdxxxCEController(key, name,
                    new HdMd400CE(props.Control.IpIdInt, props.Control.TcpSshProperties.Address, Global.ControlSystem));
            else if (typeName.Equals("hdmd300ce"))
                return new PepperDash.Essentials.DM.HdMdxxxCEController(key, name,
                    new HdMd300CE(props.Control.IpIdInt, props.Control.TcpSshProperties.Address, Global.ControlSystem));
            else if (typeName.Equals("hdmd200ce"))
                return new PepperDash.Essentials.DM.HdMdxxxCEController(key, name,
                    new HdMd200CE(props.Control.IpIdInt, props.Control.TcpSshProperties.Address, Global.ControlSystem));
            else if (typeName.Equals("hdmd200c1ge"))
                return new PepperDash.Essentials.DM.HdMdxxxCEController(key, name,
                    new HdMd200C1GE(props.Control.IpIdInt, props.Control.TcpSshProperties.Address, Global.ControlSystem));
            else
                return null;
        }
    }

}