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
    public class DmTx4kz100Controller : DmTxControllerBase, IRoutingInputsOutputs, IHasFeedback,
        IIROutputPorts, IComPorts, ICec
    {
        public DmTx4kz100C1G Tx { get; private set; }

        public RoutingInputPortWithVideoStatuses HdmiInput { get; private set; }
        public RoutingOutputPort DmOutput { get; private set; }

        public override StringFeedback ActiveVideoInputFeedback { get; protected set; }
        public IntFeedback HdmiInHdcpCapabilityFeedback { get; protected set; }
        public BoolFeedback HdmiVideoSyncFeedback { get; protected set; }

        /// <summary>
        /// Helps get the "real" inputs, including when in Auto
        /// </summary>
        public DmTx200Base.eSourceSelection ActualActiveVideoInput
        {
            get
            {
                if (Tx.VideoSourceFeedback == DmTx200Base.eSourceSelection.Digital ||
                        Tx.VideoSourceFeedback == DmTx200Base.eSourceSelection.Analog ||
                        Tx.VideoSourceFeedback == DmTx200Base.eSourceSelection.Disable)
                    return Tx.VideoSourceFeedback;
                else // auto
                {
                    if (Tx.HdmiInput.SyncDetectedFeedback.BoolValue)
                        return DmTx200Base.eSourceSelection.Digital;
                    else if (Tx.VgaInput.SyncDetectedFeedback.BoolValue)
                        return DmTx200Base.eSourceSelection.Analog;
                    else
                        return DmTx200Base.eSourceSelection.Disable;
                }
            }
        }

        public RoutingPortCollection<RoutingInputPort> InputPorts
        {
            get
            {
                return new RoutingPortCollection<RoutingInputPort> 
				{ 
					HdmiInput
                };
            }
        }

        public RoutingPortCollection<RoutingOutputPort> OutputPorts
        {
            get
            {
                return new RoutingPortCollection<RoutingOutputPort> { DmOutput };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="tx"></param>
        public DmTx4kz100Controller(string key, string name, DmTx4kz100C1G tx)
            : base(key, name, tx)
        {
            Tx = tx;

            HdmiInput = new RoutingInputPortWithVideoStatuses(DmPortName.HdmiIn,
                eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, DmTx200Base.eSourceSelection.Digital, this,
                VideoStatusHelper.GetHdmiInputStatusFuncs(tx.HdmiInput));

            ActiveVideoInputFeedback = new StringFeedback("ActiveVideoInput",
                () => ActualActiveVideoInput.ToString());

            Tx.HdmiInput.InputStreamChange += InputStreamChangeEvent;
            Tx.BaseEvent += Tx_BaseEvent;
            Tx.OnlineStatusChange += Tx_OnlineStatusChange;


            HdmiInHdcpCapabilityFeedback = new IntFeedback("HdmiInHdcpCapability", () =>
            {
                if (tx.HdmiInput.HdcpSupportOnFeedback.BoolValue)
                    return 1;
                else
                    return 0;
            });

            HdcpSupportCapability = eHdcpCapabilityType.HdcpAutoSupport;

            HdmiVideoSyncFeedback = new BoolFeedback(() => (bool)tx.HdmiInput.SyncDetectedFeedback.BoolValue);


            var combinedFuncs = new VideoStatusFuncsWrapper
            {
                HdcpActiveFeedbackFunc = () =>
                    (ActualActiveVideoInput == DmTx200Base.eSourceSelection.Digital
                    && tx.HdmiInput.VideoAttributes.HdcpActiveFeedback.BoolValue),

                HdcpStateFeedbackFunc = () => ActualActiveVideoInput == DmTx200Base.eSourceSelection.Digital ? tx.HdmiInput.VideoAttributes.HdcpStateFeedback.ToString() : "",

                VideoResolutionFeedbackFunc = () =>
                {
                    if (ActualActiveVideoInput == DmTx200Base.eSourceSelection.Digital)
                        return tx.HdmiInput.VideoAttributes.GetVideoResolutionString();
                    return ActualActiveVideoInput == DmTx200Base.eSourceSelection.Analog ? tx.VgaInput.VideoAttributes.GetVideoResolutionString() : "";
                },
                VideoSyncFeedbackFunc = () =>
                (ActualActiveVideoInput == DmTx200Base.eSourceSelection.Digital
                && tx.HdmiInput.SyncDetectedFeedback.BoolValue)
                || (ActualActiveVideoInput == DmTx200Base.eSourceSelection.Analog
                && tx.VgaInput.SyncDetectedFeedback.BoolValue)
                || (ActualActiveVideoInput == DmTx200Base.eSourceSelection.Auto
                && (tx.VgaInput.SyncDetectedFeedback.BoolValue || tx.HdmiInput.SyncDetectedFeedback.BoolValue))

            };

            AnyVideoInput = new RoutingInputPortWithVideoStatuses(DmPortName.AnyVideoIn,
    eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.None, 0, this, combinedFuncs);

            DmOutput = new RoutingOutputPort(DmPortName.DmOut, eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.DmCat, null, this);

            AddToFeedbackList(ActiveVideoInputFeedback,  
                AnyVideoInput.VideoStatus.HasVideoStatusFeedback, AnyVideoInput.VideoStatus.HdcpActiveFeedback,
                AnyVideoInput.VideoStatus.HdcpStateFeedback, AnyVideoInput.VideoStatus.VideoResolutionFeedback,
                AnyVideoInput.VideoStatus.VideoSyncFeedback, HdmiInHdcpCapabilityFeedback, HdmiVideoSyncFeedback
                );

            // Set Ports for CEC
            HdmiInput.Port = Tx.HdmiInput;
            DmOutput.Port = Tx.DmOutput;
        }


        void Tx_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            ActiveVideoInputFeedback.FireUpdate();
            HdmiVideoSyncFeedback.FireUpdate();
        }

        public override bool CustomActivate()
        {

            Tx.HdmiInput.InputStreamChange += (o, a) => FowardInputStreamChange(HdmiInput, a.EventId);
            Tx.HdmiInput.VideoAttributes.AttributeChange += (o, a) => FireVideoAttributeChange(HdmiInput, a.EventId);

            // Base does register and sets up comm monitoring.
            return base.CustomActivate();
        }

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = GetDmTxJoinMap(joinStart, joinMapKey);

            if (HdmiVideoSyncFeedback != null)
            {
                HdmiVideoSyncFeedback.LinkInputSig(trilist.BooleanInput[joinMap.Input1VideoSyncStatus.JoinNumber]);
            }

            LinkDmTxToApi(this, trilist, joinMap, bridge);
        }

        /// <summary>
        /// Enables or disables free run
        /// </summary>
        /// <param name="enable"></param>
        public void SetFreeRunEnabled(bool enable)
        {
            Tx.VgaInput.FreeRun = enable ? eDmFreeRunSetting.Enabled : eDmFreeRunSetting.Disabled;
        }

        /// <summary>
        /// Sets the VGA brightness level
        /// </summary>
        /// <param name="level"></param>
        public void SetVgaBrightness(ushort level)
        {
            Tx.VgaInput.VideoControls.Brightness.UShortValue = level;
        }

        /// <summary>
        /// Sets the VGA contrast level
        /// </summary>
        /// <param name="level"></param>
        public void SetVgaContrast(ushort level)
        {
            Tx.VgaInput.VideoControls.Contrast.UShortValue = level;
        }


        void Tx_BaseEvent(GenericBase device, BaseEventArgs args)
        {
            var id = args.EventId;
            Debug.Console(2, this, "EventId {0}", args.EventId);

            switch (id)
            {
                case EndpointTransmitterBase.VideoSourceFeedbackEventId:
                    Debug.Console(2, this, "  Video Source: {0}", Tx.VideoSourceFeedback);
                    ActiveVideoInputFeedback.FireUpdate();
                    break;
                case EndpointTransmitterBase.AudioSourceFeedbackEventId:
                    Debug.Console(2, this, "  Audio Source: {0}", Tx.AudioSourceFeedback);
                    break;
            }
        }

        void InputStreamChangeEvent(EndpointInputStream inputStream, EndpointInputStreamEventArgs args)
        {
            Debug.Console(2, "{0} event {1} stream {2}", this.Tx.ToString(), inputStream.ToString(), args.EventId.ToString());

            switch (args.EventId)
            {
                case EndpointInputStreamEventIds.HdcpSupportOffFeedbackEventId:
                    HdmiInHdcpCapabilityFeedback.FireUpdate();
                    break;
                case EndpointInputStreamEventIds.HdcpSupportOnFeedbackEventId:
                    HdmiInHdcpCapabilityFeedback.FireUpdate();
                    break;
                case EndpointInputStreamEventIds.SyncDetectedFeedbackEventId:
                    HdmiVideoSyncFeedback.FireUpdate();
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
        void FireVideoAttributeChange(RoutingInputPortWithVideoStatuses inputPort, int eventId)
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

        #region ICec Members
        /// <summary>
        /// Gets the CEC stream directly from the HDMI port.
        /// </summary>
        public Cec StreamCec { get { return Tx.HdmiInput.StreamCec; } }
        #endregion


    }
}