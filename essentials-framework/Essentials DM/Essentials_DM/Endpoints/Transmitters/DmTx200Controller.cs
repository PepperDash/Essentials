﻿using System;
using System.Linq;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.DM
{
    // using eVst = Crestron.SimplSharpPro.DeviceSupport.eX02VideoSourceType;

    /// <summary>
    /// Controller class for all DM-TX-201C/S/F transmitters
    /// </summary>
    [Description("Wrapper class for DM-TX-200-C")]
    public class DmTx200Controller : DmTxControllerBase, ITxRoutingWithFeedback, IHasFreeRun, IVgaBrightnessContrastControls
    {
        public DmTx200C2G Tx { get; private set; }

        public RoutingInputPortWithVideoStatuses HdmiInput { get; private set; }
        public RoutingInputPortWithVideoStatuses VgaInput { get; private set; }
        public RoutingOutputPort DmOutput { get; private set; }

        public override StringFeedback ActiveVideoInputFeedback { get; protected set; }
        public IntFeedback VideoSourceNumericFeedback { get; protected set; }
        public IntFeedback AudioSourceNumericFeedback { get; protected set; }
        public IntFeedback HdmiInHdcpCapabilityFeedback { get; protected set; } //actually state
        public BoolFeedback HdmiVideoSyncFeedback { get; protected set; }
        public BoolFeedback VgaVideoSyncFeedback { get; protected set; }

        public BoolFeedback FreeRunEnabledFeedback { get; protected set; }

        public IntFeedback VgaBrightnessFeedback { get; protected set; }
        public IntFeedback VgaContrastFeedback { get; protected set; }

        //IroutingNumericEvent
        public event EventHandler<RoutingNumericEventArgs> NumericSwitchChange;

        /// <summary>
        /// Raise an event when the status of a switch object changes.
        /// </summary>
        /// <param name="e">Arguments defined as IKeyName sender, output, input, and eRoutingSignalType</param>
        private void OnSwitchChange(RoutingNumericEventArgs e)
        {
            var newEvent = NumericSwitchChange;
            if (newEvent != null) newEvent(this, e);
        }


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
                if (Tx.HdmiInput.SyncDetectedFeedback.BoolValue)
                    return DmTx200Base.eSourceSelection.Digital;

                return Tx.VgaInput.SyncDetectedFeedback.BoolValue ? DmTx200Base.eSourceSelection.Analog : DmTx200Base.eSourceSelection.Disable;
            }
        }

        public RoutingPortCollection<RoutingInputPort> InputPorts
        {
            get
            {
                return new RoutingPortCollection<RoutingInputPort> 
				{ 
					HdmiInput, 
					VgaInput, 
					AnyVideoInput 
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
        public DmTx200Controller(string key, string name, DmTx200C2G tx, bool preventRegistration)
            : base(key, name, tx)
        {
            Tx = tx;
            PreventRegistration = preventRegistration;

            HdmiInput = new RoutingInputPortWithVideoStatuses(DmPortName.HdmiIn,
                eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi,
                DmTx200Base.eSourceSelection.Digital, this,
                VideoStatusHelper.GetHdmiInputStatusFuncs(tx.HdmiInput))
            {
                FeedbackMatchObject = DmTx200Base.eSourceSelection.Digital
            };

            VgaInput = new RoutingInputPortWithVideoStatuses(DmPortName.VgaIn,
                eRoutingSignalType.Video, eRoutingPortConnectionType.Vga, DmTx200Base.eSourceSelection.Analog, this,
                VideoStatusHelper.GetVgaInputStatusFuncs(tx.VgaInput))
            {
                FeedbackMatchObject = DmTx200Base.eSourceSelection.Analog
            };

            ActiveVideoInputFeedback = new StringFeedback("ActiveVideoInput",
                () => ActualActiveVideoInput.ToString());

            Tx.HdmiInput.InputStreamChange += InputStreamChangeEvent;
            Tx.VgaInput.InputStreamChange += VgaInputOnInputStreamChange;
            Tx.BaseEvent += Tx_BaseEvent;
            Tx.OnlineStatusChange += Tx_OnlineStatusChange;

            VideoSourceNumericFeedback = new IntFeedback(() => (int)Tx.VideoSourceFeedback);
            AudioSourceNumericFeedback = new IntFeedback(() => (int)Tx.AudioSourceFeedback);

            HdmiInHdcpCapabilityFeedback = new IntFeedback("HdmiInHdcpCapability", () => tx.HdmiInput.HdcpSupportOnFeedback.BoolValue ? 1 : 0);

            //setting this on the base class so that we can get it easily on the chassis.
            HdcpStateFeedback = HdmiInHdcpCapabilityFeedback;

            HdcpSupportCapability = eHdcpCapabilityType.HdcpAutoSupport;

            HdmiVideoSyncFeedback = new BoolFeedback(() => tx.HdmiInput.SyncDetectedFeedback.BoolValue);

            VgaVideoSyncFeedback = new BoolFeedback(() => tx.VgaInput.SyncDetectedFeedback.BoolValue);

            FreeRunEnabledFeedback = new BoolFeedback(() => tx.VgaInput.FreeRunFeedback == eDmFreeRunSetting.Enabled);

            VgaBrightnessFeedback = new IntFeedback(() => tx.VgaInput.VideoControls.BrightnessFeedback.UShortValue);
            VgaContrastFeedback = new IntFeedback(() => tx.VgaInput.VideoControls.ContrastFeedback.UShortValue);

            tx.VgaInput.VideoControls.ControlChange += VideoControls_ControlChange;


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

            DmOutput = new RoutingOutputPort(DmPortName.DmOut, eRoutingSignalType.Audio | eRoutingSignalType.Video, 
                eRoutingPortConnectionType.DmCat, null, this);

            AddToFeedbackList(ActiveVideoInputFeedback, VideoSourceNumericFeedback, AudioSourceNumericFeedback,
                AnyVideoInput.VideoStatus.HasVideoStatusFeedback, AnyVideoInput.VideoStatus.HdcpActiveFeedback,
                AnyVideoInput.VideoStatus.HdcpStateFeedback, AnyVideoInput.VideoStatus.VideoResolutionFeedback,
                AnyVideoInput.VideoStatus.VideoSyncFeedback, HdmiInHdcpCapabilityFeedback, HdmiVideoSyncFeedback,
                VgaVideoSyncFeedback);

            // Set Ports for CEC
            HdmiInput.Port = Tx.HdmiInput;
            VgaInput.Port = Tx.VgaInput;
            DmOutput.Port = Tx.DmOutput;
        }

        private void VgaInputOnInputStreamChange(EndpointInputStream inputStream, EndpointInputStreamEventArgs args)
        {
            switch (args.EventId)
            {
                case EndpointInputStreamEventIds.FreeRunFeedbackEventId:
                    FreeRunEnabledFeedback.FireUpdate();
                    break;
                case EndpointInputStreamEventIds.SyncDetectedFeedbackEventId:
                    VgaVideoSyncFeedback.FireUpdate();
                    break;
            }
        }

        void VideoControls_ControlChange(object sender, GenericEventArgs args)
        {
            var id = args.EventId;
            Debug.Console(2, this, "EventId {0}", args.EventId);

            switch (id)
            {
                case VideoControlsEventIds.BrightnessFeedbackEventId:
                    VgaBrightnessFeedback.FireUpdate();
                    break;
                case VideoControlsEventIds.ContrastFeedbackEventId:
                    VgaContrastFeedback.FireUpdate();
                    break;
            }
        }

        void Tx_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            var localVideoInputPort =
                InputPorts.FirstOrDefault(p => (DmTx200Base.eSourceSelection) p.Selector == Tx.VideoSourceFeedback);
            var localAudioInputPort =
                InputPorts.FirstOrDefault(p => (DmTx200Base.eSourceSelection) p.Selector == Tx.AudioSourceFeedback);


            ActiveVideoInputFeedback.FireUpdate();
            VideoSourceNumericFeedback.FireUpdate();
            AudioSourceNumericFeedback.FireUpdate();
            OnSwitchChange(new RoutingNumericEventArgs(1, VideoSourceNumericFeedback.UShortValue, OutputPorts.First(), localVideoInputPort, eRoutingSignalType.Video));
            OnSwitchChange(new RoutingNumericEventArgs(1, AudioSourceNumericFeedback.UShortValue, OutputPorts.First(), localAudioInputPort, eRoutingSignalType.Audio));
        }

        public override bool CustomActivate()
        {

            Tx.HdmiInput.InputStreamChange += (o, a) => FowardInputStreamChange(HdmiInput, a.EventId);
            Tx.HdmiInput.VideoAttributes.AttributeChange += (o, a) => FireVideoAttributeChange(HdmiInput, a.EventId);

            Tx.VgaInput.InputStreamChange += (o, a) => FowardInputStreamChange(VgaInput, a.EventId);
            Tx.VgaInput.VideoAttributes.AttributeChange += (o, a) => FireVideoAttributeChange(VgaInput, a.EventId);

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
            if (VgaVideoSyncFeedback != null)
            {
                VgaVideoSyncFeedback.LinkInputSig(trilist.BooleanInput[joinMap.Input2VideoSyncStatus.JoinNumber]);
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

        public void ExecuteNumericSwitch(ushort input, ushort output, eRoutingSignalType type)
        {
            Debug.Console(2, this, "Executing Numeric Switch to input {0}.", input);

            switch (input)
            {
                case 0:
                    {
                        ExecuteSwitch(DmTx200Base.eSourceSelection.Auto, null, type);
                        break;
                    }
                case 1:
                    {
                        ExecuteSwitch(HdmiInput.Selector, null, type);
                        break;
                    }
                case 2:
                    {
                        ExecuteSwitch(VgaInput.Selector, null, type);
                        break;
                    }
                case 3:
                    {
                        ExecuteSwitch(DmTx200Base.eSourceSelection.Disable, null, type);
                        break;
                    }
            }
        }

        public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType)
        {
            if ((signalType | eRoutingSignalType.Video) == eRoutingSignalType.Video)
                Tx.VideoSource = (DmTx200Base.eSourceSelection)inputSelector;
            if ((signalType | eRoutingSignalType.Audio) == eRoutingSignalType.Audio)
                Tx.AudioSource = (DmTx200Base.eSourceSelection)inputSelector;
        }

        void Tx_BaseEvent(GenericBase device, BaseEventArgs args)
        {
            var id = args.EventId;
            Debug.Console(2, this, "EventId {0}", args.EventId);

            switch (id)
            {
                case EndpointTransmitterBase.VideoSourceFeedbackEventId:
                    var localVideoInputPort = InputPorts.FirstOrDefault(p => (DmTx200Base.eSourceSelection)p.Selector == Tx.VideoSourceFeedback);
                    Debug.Console(2, this, "  Video Source: {0}", Tx.VideoSourceFeedback);
                    VideoSourceNumericFeedback.FireUpdate();
                    ActiveVideoInputFeedback.FireUpdate();
                    OnSwitchChange(new RoutingNumericEventArgs(1, VideoSourceNumericFeedback.UShortValue, OutputPorts.First(), localVideoInputPort, eRoutingSignalType.Video));
                    break;
                case EndpointTransmitterBase.AudioSourceFeedbackEventId:
                    var localInputAudioPort = InputPorts.FirstOrDefault(p => (DmTx200Base.eSourceSelection)p.Selector == Tx.AudioSourceFeedback);
                    Debug.Console(2, this, "  Audio Source: {0}", Tx.AudioSourceFeedback);
                    AudioSourceNumericFeedback.FireUpdate();
                    OnSwitchChange(new RoutingNumericEventArgs(1, AudioSourceNumericFeedback.UShortValue, OutputPorts.First(), localInputAudioPort, eRoutingSignalType.Audio));
                    break;
            }
        }

        void InputStreamChangeEvent(EndpointInputStream inputStream, EndpointInputStreamEventArgs args)
        {
            Debug.Console(2, "{0} event {1} stream {2}", Tx.ToString(), inputStream.ToString(), args.EventId.ToString());

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
            if (eventId != EndpointInputStreamEventIds.SyncDetectedFeedbackEventId)
            {
                return;
            }
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

    }
}