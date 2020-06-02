using Crestron.SimplSharpPro;
//using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.DM
{
    using eVst = eX02VideoSourceType;
    using eAst = eX02AudioSourceType;


    [Description("Wrapper class for DM-TX-4K-Z-302-C")]
    public class DmTx4kz302CController : DmTxControllerBase, ITxRouting, IHasFeedback,
        IIROutputPorts, IComPorts
    {
        public DmTx4kz302C Tx { get; private set; }

        public RoutingInputPortWithVideoStatuses HdmiIn1 { get; private set; }
        public RoutingInputPortWithVideoStatuses HdmiIn2 { get; private set; }
        public RoutingInputPortWithVideoStatuses DisplayPortIn { get; private set; }
        public RoutingOutputPort DmOut { get; private set; }
        public RoutingOutputPort HdmiLoopOut { get; private set; }

        public override StringFeedback ActiveVideoInputFeedback { get; protected set; }
        public IntFeedback VideoSourceNumericFeedback { get; protected set; }
        public IntFeedback AudioSourceNumericFeedback { get; protected set; }
        public IntFeedback HdmiIn1HdcpCapabilityFeedback { get; protected set; }
        public IntFeedback HdmiIn2HdcpCapabilityFeedback { get; protected set; }
        public BoolFeedback Hdmi1VideoSyncFeedback { get; protected set; }
        public BoolFeedback Hdmi2VideoSyncFeedback { get; protected set; }
        public BoolFeedback DisplayPortVideoSyncFeedback { get; protected set; }

        //public override IntFeedback HdcpSupportAllFeedback { get; protected set; }
        //public override ushort HdcpSupportCapability { get; protected set; }

        /// <summary>
        /// Helps get the "real" inputs, including when in Auto
        /// </summary>
        public eX02VideoSourceType ActualActiveVideoInput
        {
            get
            {
                if (Tx.VideoSourceFeedback != eVst.Auto)
                    return Tx.VideoSourceFeedback;
                if (Tx.HdmiInputs[1].SyncDetectedFeedback.BoolValue)
                    return eVst.Hdmi1;
                if (Tx.HdmiInputs[2].SyncDetectedFeedback.BoolValue)
                    return eVst.Hdmi2;

                return Tx.DisplayPortInput.SyncDetectedFeedback.BoolValue ? eVst.Vga : eVst.AllDisabled;
            }
        }
        public RoutingPortCollection<RoutingInputPort> InputPorts
        {
            get
            {
                return new RoutingPortCollection<RoutingInputPort> 
				{ 
					HdmiIn1,
					HdmiIn2,
					DisplayPortIn, 
					AnyVideoInput 
				};
            }
        }
        public RoutingPortCollection<RoutingOutputPort> OutputPorts
        {
            get
            {
                return new RoutingPortCollection<RoutingOutputPort> { DmOut, HdmiLoopOut };
            }
        }
        public DmTx4kz302CController(string key, string name, DmTx4kz302C tx)
            : base(key, name, tx)
        {
            Tx = tx;

            HdmiIn1 = new RoutingInputPortWithVideoStatuses(DmPortName.HdmiIn1,
                eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, eVst.Hdmi1, this,
                VideoStatusHelper.GetHdmiInputStatusFuncs(tx.HdmiInputs[1]));
            HdmiIn2 = new RoutingInputPortWithVideoStatuses(DmPortName.HdmiIn2,
                eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, eVst.Hdmi2, this,
                VideoStatusHelper.GetHdmiInputStatusFuncs(tx.HdmiInputs[2]));
            DisplayPortIn = new RoutingInputPortWithVideoStatuses(DmPortName.VgaIn,
                eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DisplayPort, eVst.DisplayPort, this,
                VideoStatusHelper.GetDisplayPortInputStatusFuncs(tx.DisplayPortInput));
            ActiveVideoInputFeedback = new StringFeedback("ActiveVideoInput",
                () => ActualActiveVideoInput.ToString());

            Tx.HdmiInputs[1].InputStreamChange += InputStreamChangeEvent;
            Tx.HdmiInputs[2].InputStreamChange += InputStreamChangeEvent;
            Tx.DisplayPortInput.InputStreamChange += DisplayPortInputStreamChange;
            Tx.BaseEvent += Tx_BaseEvent;

            VideoSourceNumericFeedback = new IntFeedback(() => (int)Tx.VideoSourceFeedback);
            AudioSourceNumericFeedback = new IntFeedback(() => (int)Tx.VideoSourceFeedback);

            HdmiIn1HdcpCapabilityFeedback = new IntFeedback("HdmiIn1HdcpCapability", () => (int)tx.HdmiInputs[1].HdcpCapabilityFeedback);

            HdmiIn2HdcpCapabilityFeedback = new IntFeedback("HdmiIn2HdcpCapability", () => (int)tx.HdmiInputs[2].HdcpCapabilityFeedback);

            HdcpSupportCapability = eHdcpCapabilityType.Hdcp2_2Support;

            Hdmi1VideoSyncFeedback = new BoolFeedback(() => (bool)tx.HdmiInputs[1].SyncDetectedFeedback.BoolValue);

            Hdmi2VideoSyncFeedback = new BoolFeedback(() => (bool)tx.HdmiInputs[2].SyncDetectedFeedback.BoolValue);

            DisplayPortVideoSyncFeedback = new BoolFeedback(() => (bool)tx.DisplayPortInput.SyncDetectedFeedback.BoolValue);


            var combinedFuncs = new VideoStatusFuncsWrapper
            {
                HdcpActiveFeedbackFunc = () =>
                    (ActualActiveVideoInput == eVst.Hdmi1
                    && tx.HdmiInputs[1].VideoAttributes.HdcpActiveFeedback.BoolValue)
                    || (ActualActiveVideoInput == eVst.Hdmi2
                    && tx.HdmiInputs[2].VideoAttributes.HdcpActiveFeedback.BoolValue),

                HdcpStateFeedbackFunc = () =>
                {
                    if (ActualActiveVideoInput == eVst.Hdmi1)
                        return tx.HdmiInputs[1].VideoAttributes.HdcpStateFeedback.ToString();
                    return ActualActiveVideoInput == eVst.Hdmi2 ? tx.HdmiInputs[2].VideoAttributes.HdcpStateFeedback.ToString() : "";
                },

                VideoResolutionFeedbackFunc = () =>
                {
                    if (ActualActiveVideoInput == eVst.Hdmi1)
                        return tx.HdmiInputs[1].VideoAttributes.GetVideoResolutionString();
                    if (ActualActiveVideoInput == eVst.Hdmi2)
                        return tx.HdmiInputs[2].VideoAttributes.GetVideoResolutionString();
                    return ActualActiveVideoInput == eVst.Vga ? tx.DisplayPortInput.VideoAttributes.GetVideoResolutionString() : "";
                },
                VideoSyncFeedbackFunc = () =>
                    (ActualActiveVideoInput == eVst.Hdmi1
                    && tx.HdmiInputs[1].SyncDetectedFeedback.BoolValue)
                    || (ActualActiveVideoInput == eVst.Hdmi2
                    && tx.HdmiInputs[2].SyncDetectedFeedback.BoolValue)
                    || (ActualActiveVideoInput == eVst.Vga
                    && tx.DisplayPortInput.SyncDetectedFeedback.BoolValue)

            };

            AnyVideoInput = new RoutingInputPortWithVideoStatuses(DmPortName.AnyVideoIn,
                eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.None, 0, this, combinedFuncs);

            DmOut = new RoutingOutputPort(DmPortName.DmOut, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.DmCat, null, this);
            HdmiLoopOut = new RoutingOutputPort(DmPortName.HdmiLoopOut, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, null, this);


            AddToFeedbackList(ActiveVideoInputFeedback, VideoSourceNumericFeedback, AudioSourceNumericFeedback,
                AnyVideoInput.VideoStatus.HasVideoStatusFeedback, AnyVideoInput.VideoStatus.HdcpActiveFeedback,
                AnyVideoInput.VideoStatus.HdcpStateFeedback, AnyVideoInput.VideoStatus.VideoResolutionFeedback,
                AnyVideoInput.VideoStatus.VideoSyncFeedback, HdmiIn1HdcpCapabilityFeedback, HdmiIn2HdcpCapabilityFeedback,
                Hdmi1VideoSyncFeedback, Hdmi2VideoSyncFeedback, DisplayPortVideoSyncFeedback);

            // Set Ports for CEC
            HdmiIn1.Port = Tx.HdmiInputs[1];
            HdmiIn2.Port = Tx.HdmiInputs[2];
            HdmiLoopOut.Port = Tx.HdmiOutput;
            DmOut.Port = Tx.DmOutput;
        }

        void DisplayPortInputStreamChange(EndpointInputStream inputStream, EndpointInputStreamEventArgs args)
        {
            Debug.Console(2, "{0} event {1} stream {2}", Tx.ToString(), inputStream.ToString(), args.EventId.ToString());

            switch (args.EventId)
            {
                case EndpointInputStreamEventIds.SyncDetectedFeedbackEventId:
                    DisplayPortVideoSyncFeedback.FireUpdate();
                    break;
            }
        }



        public override bool CustomActivate()
        {
            // Link up all of these damned events to the various RoutingPorts via a helper handler
            Tx.HdmiInputs[1].InputStreamChange += (o, a) => FowardInputStreamChange(HdmiIn1, a.EventId);
            Tx.HdmiInputs[1].VideoAttributes.AttributeChange += (o, a) => ForwardVideoAttributeChange(HdmiIn1, a.EventId);

            Tx.HdmiInputs[2].InputStreamChange += (o, a) => FowardInputStreamChange(HdmiIn2, a.EventId);
            Tx.HdmiInputs[2].VideoAttributes.AttributeChange += (o, a) => ForwardVideoAttributeChange(HdmiIn2, a.EventId);

            Tx.DisplayPortInput.InputStreamChange += (o, a) => FowardInputStreamChange(DisplayPortIn, a.EventId);
            Tx.DisplayPortInput.VideoAttributes.AttributeChange += (o, a) => ForwardVideoAttributeChange(DisplayPortIn, a.EventId);

            // Base does register and sets up comm monitoring.
            return base.CustomActivate();
        }

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = GetDmTxJoinMap(joinStart, joinMapKey);

            if (Hdmi1VideoSyncFeedback != null)
            {
                Hdmi1VideoSyncFeedback.LinkInputSig(trilist.BooleanInput[joinMap.Input1VideoSyncStatus.JoinNumber]);
            }
            if (Hdmi2VideoSyncFeedback != null)
            {
                Hdmi2VideoSyncFeedback.LinkInputSig(trilist.BooleanInput[joinMap.Input2VideoSyncStatus.JoinNumber]);
            }
            if (DisplayPortVideoSyncFeedback != null)
            {
                DisplayPortVideoSyncFeedback.LinkInputSig(trilist.BooleanInput[joinMap.Input3VideoSyncStatus.JoinNumber]);
            }

            LinkDmTxToApi(this, trilist, joinMap, bridge);
        }

        public void ExecuteNumericSwitch(ushort input, ushort output, eRoutingSignalType type)
        {
            Debug.Console(2, this, "Executing Numeric Switch to input {0}.", input);

                switch (input)
                {
                    case 0:
                        {
                            ExecuteSwitch(eVst.Auto, null, eRoutingSignalType.Audio | eRoutingSignalType.Video);
                            break;
                        }
                    case 1:
                        {
                            ExecuteSwitch(HdmiIn1.Selector, null, eRoutingSignalType.Audio | eRoutingSignalType.Video);
                            break;
                        }
                    case 2:
                        {
                            ExecuteSwitch(HdmiIn2.Selector, null, eRoutingSignalType.Audio | eRoutingSignalType.Video);
                            break;
                        }
                    case 3:
                        {
                            ExecuteSwitch(DisplayPortIn.Selector, null, eRoutingSignalType.Audio | eRoutingSignalType.Video);
                            break;
                        }
                    case 4:
                        {
                            ExecuteSwitch(eVst.AllDisabled, null, eRoutingSignalType.Audio | eRoutingSignalType.Video);
                            break;
                        }
                }
            

        }

        public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType)
        {
            if ((signalType | eRoutingSignalType.Video) == eRoutingSignalType.Video)
                Tx.VideoSource = (eVst)inputSelector;

            // NOTE:  It's possible that this particular TX model may not like the AudioSource property being set.  
            // The SIMPL definition only shows a single analog for AudioVideo Source
            if ((signalType | eRoutingSignalType.Audio) == eRoutingSignalType.Audio)
                //it doesn't
                Debug.Console(2, this, "Unable to execute audio-only switch for tx {0}", Key);
                //Tx.AudioSource = (eAst)inputSelector;
        }

        void InputStreamChangeEvent(EndpointInputStream inputStream, EndpointInputStreamEventArgs args)
        {
            Debug.Console(2, "{0} event {1} stream {2}", Tx.ToString(), inputStream.ToString(), args.EventId.ToString());

            switch (args.EventId)
            {
                case EndpointInputStreamEventIds.HdcpSupportOffFeedbackEventId:
                    if (inputStream == Tx.HdmiInputs[1]) HdmiIn1HdcpCapabilityFeedback.FireUpdate();
                    if (inputStream == Tx.HdmiInputs[2]) HdmiIn2HdcpCapabilityFeedback.FireUpdate();
                    break;
                case EndpointInputStreamEventIds.HdcpSupportOnFeedbackEventId:
                    if (inputStream == Tx.HdmiInputs[1]) HdmiIn1HdcpCapabilityFeedback.FireUpdate();
                    if (inputStream == Tx.HdmiInputs[2]) HdmiIn2HdcpCapabilityFeedback.FireUpdate();
                    break;
                case EndpointInputStreamEventIds.SyncDetectedFeedbackEventId:
                    if (inputStream == Tx.HdmiInputs[1]) Hdmi1VideoSyncFeedback.FireUpdate();
                    if (inputStream == Tx.HdmiInputs[2]) Hdmi2VideoSyncFeedback.FireUpdate();
                    break;
            }
        }

        void Tx_BaseEvent(GenericBase device, BaseEventArgs args)
        {
            var id = args.EventId;
            switch (id)
            {
                case EndpointTransmitterBase.VideoSourceFeedbackEventId:
                    Debug.Console(2, this, "  Video Source: {0}", Tx.VideoSourceFeedback);
                    VideoSourceNumericFeedback.FireUpdate();
                    ActiveVideoInputFeedback.FireUpdate();
                    break;
                case EndpointTransmitterBase.AudioSourceFeedbackEventId:
                    Debug.Console(2, this, "  Audio Source: {0}", Tx.AudioSourceFeedback);
                    AudioSourceNumericFeedback.FireUpdate();
                    break;
            }
        }

        /// <summary>
        /// Relays the input stream change to the appropriate RoutingInputPort.
        /// </summary>
        void FowardInputStreamChange(RoutingInputPortWithVideoStatuses inputPort, int eventId)
        {
            if (eventId != EndpointInputStreamEventIds.SyncDetectedFeedbackEventId) return;
            inputPort.VideoStatus.VideoSyncFeedback.FireUpdate();
            AnyVideoInput.VideoStatus.VideoSyncFeedback.FireUpdate();
        }

        /// <summary>
        /// Relays the VideoAttributes change to a RoutingInputPort
        /// </summary>
        void ForwardVideoAttributeChange(RoutingInputPortWithVideoStatuses inputPort, int eventId)
        {
            //// LOCATION: Crestron.SimplSharpPro.DM.VideoAttributeEventIds
            //Debug.Console(2, this, "VideoAttributes_AttributeChange event id={0} from {1}",
            //    args.EventId, (sender as VideoAttributesEnhanced).Owner.GetType());
            switch (eventId)
            {
                case VideoAttributeEventIds.HdcpActiveFeedbackEventId:
                    inputPort.VideoStatus.HdcpActiveFeedback.FireUpdate();
                    AnyVideoInput.VideoStatus.HdcpActiveFeedback.FireUpdate();
                    break;
                case VideoAttributeEventIds.HdcpStateFeedbackEventId:
                    inputPort.VideoStatus.HdcpStateFeedback.FireUpdate();
                    AnyVideoInput.VideoStatus.HdcpStateFeedback.FireUpdate();
                    break;
                case VideoAttributeEventIds.HorizontalResolutionFeedbackEventId:
                case VideoAttributeEventIds.VerticalResolutionFeedbackEventId:
                    inputPort.VideoStatus.VideoResolutionFeedback.FireUpdate();
                    AnyVideoInput.VideoStatus.VideoResolutionFeedback.FireUpdate();
                    break;
                case VideoAttributeEventIds.FramesPerSecondFeedbackEventId:
                    inputPort.VideoStatus.VideoResolutionFeedback.FireUpdate();
                    AnyVideoInput.VideoStatus.VideoResolutionFeedback.FireUpdate();
                    break;
            }
        }


        #region IIROutputPorts Members
        public CrestronCollection<IROutputPort> IROutputPorts { get { return Tx.IROutputPorts; } }
        public int NumberOfIROutputPorts { get { return Tx.NumberOfIROutputPorts; } }
        #endregion

        #region IComPorts Members
        public CrestronCollection<ComPort> ComPorts { get { return Tx.ComPorts; } }
        public int NumberOfComPorts { get { return Tx.NumberOfComPorts; } }
        #endregion
    }
}