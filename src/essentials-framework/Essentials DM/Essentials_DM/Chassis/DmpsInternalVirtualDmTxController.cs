using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;

using PepperDash.Core;
using PepperDash.Essentials.Core;


namespace PepperDash.Essentials.DM
{
    /// <summary>
    /// 
    /// </summary>
    public class DmpsInternalVirtualHdmiVgaInputController : Device, ITxRouting, IHasFeedback
    {
        public Card.Dmps3HdmiVgaInput InputCard { get; protected set; }

        public eHdcpCapabilityType HdcpSupportCapability { get; protected set; }
        public StringFeedback ActiveVideoInputFeedback { get; protected set; }

        public RoutingInputPortWithVideoStatuses HdmiIn { get; protected set; }
        public RoutingInputPortWithVideoStatuses VgaIn { get; protected set; }
        public RoutingInputPort AudioIn { get; protected set; }
        public RoutingInputPortWithVideoStatuses AnyVideoInput { get; protected set; }

        public RoutingOutputPort VirtualDmOut { get; protected set; }

        public IntFeedback VideoSourceNumericFeedback { get; protected set; }
        public IntFeedback AudioSourceNumericFeedback { get; protected set; }
        public IntFeedback HdmiInHdcpCapabilityFeedback { get; protected set; }

        /// <summary>
        /// Returns a list containing the Outputs that we want to expose.
        /// </summary>
        public FeedbackCollection<Feedback> Feedbacks { get; private set; }

        public void SetPortHdcpCapability(eHdcpCapabilityType hdcpMode, uint port) { }

        /// <summary>
        /// Helps get the "real" inputs, including when in Auto
        /// </summary>
        public eDmps3InputVideoSource ActualVideoInput
        {
            get
            {
                try
                {
                    if (InputCard.VideoSourceFeedback != eDmps3InputVideoSource.Auto)
                        return InputCard.VideoSourceFeedback;
                    else // auto
                    {
                        if (InputCard.HdmiInputPort.SyncDetectedFeedback.BoolValue)
                            return eDmps3InputVideoSource.Hdmi;
                        else if (InputCard.VgaInputPort.SyncDetectedFeedback.BoolValue)
                            return eDmps3InputVideoSource.Vga;
                        else
                            return eDmps3InputVideoSource.Bnc;
                    }
                }
                catch
                {
                    return eDmps3InputVideoSource.Bnc;
                }
            }
        }

        public virtual RoutingPortCollection<RoutingInputPort> InputPorts
        {
            get
            {
                return new RoutingPortCollection<RoutingInputPort> 
				{ 
					HdmiIn,
					VgaIn,
                    AudioIn,
					AnyVideoInput 
				};
            }
        }

        public RoutingPortCollection<RoutingOutputPort> OutputPorts
        {
            get
            {
                return new RoutingPortCollection<RoutingOutputPort> { VirtualDmOut };
            }
        }

        public DmpsInternalVirtualHdmiVgaInputController(string key, string name, DMInput inputCard)
            : base(key, name)
        {
            Feedbacks = new FeedbackCollection<Feedback>();

            if (inputCard is Card.Dmps3HdmiVgaInput)
            {
                InputCard = inputCard as Card.Dmps3HdmiVgaInput;
            
                HdmiIn = new RoutingInputPortWithVideoStatuses(DmPortName.HdmiIn, eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi,
                    eDmps3InputVideoSource.Hdmi, this, VideoStatusHelper.GetHdmiInputStatusFuncs(InputCard.HdmiInputPort));
                VgaIn = new RoutingInputPortWithVideoStatuses(DmPortName.VgaIn,
                    eRoutingSignalType.Video, eRoutingPortConnectionType.Vga, eDmps3InputVideoSource.Vga, this,
                    VideoStatusHelper.GetVgaInputStatusFuncs(InputCard.VgaInputPort));
                AudioIn = new RoutingInputPort(DmPortName.AudioIn, eRoutingSignalType.Audio, eRoutingPortConnectionType.LineAudio,
                    eDmps3InputAudioSource.Analog, this);

                if (InputCard.HdmiInputPort.HdcpSupportedLevelFeedback == eHdcpSupportedLevel.Hdcp2xSupport)
                    HdcpSupportCapability = eHdcpCapabilityType.Hdcp2_2Support;
                else if (InputCard.HdmiInputPort.HdcpSupportedLevelFeedback == eHdcpSupportedLevel.Hdcp1xSupport)
                    HdcpSupportCapability = eHdcpCapabilityType.Hdcp1xSupport;

                var combinedFuncs = new VideoStatusFuncsWrapper
                {
                    HdcpActiveFeedbackFunc = () =>
                        (ActualVideoInput == eDmps3InputVideoSource.Hdmi
                        && InputCard.HdmiInputPort.VideoAttributes.HdcpActiveFeedback.BoolValue),

                    HdcpStateFeedbackFunc = () =>
                    {
                        if (ActualVideoInput == eDmps3InputVideoSource.Hdmi)
                            return InputCard.HdmiInputPort.VideoAttributes.HdcpStateFeedback.ToString();
                        return "";
                    },

                    VideoResolutionFeedbackFunc = () =>
                    {
                        if (ActualVideoInput == eDmps3InputVideoSource.Hdmi)
                            return InputCard.HdmiInputPort.VideoAttributes.GetVideoResolutionString();
                        if (ActualVideoInput == eDmps3InputVideoSource.Vga)
                            return InputCard.VgaInputPort.VideoAttributes.GetVideoResolutionString();
                        return "";
                    },
                    VideoSyncFeedbackFunc = () =>
                        (ActualVideoInput == eDmps3InputVideoSource.Hdmi
                        && InputCard.HdmiInputPort.SyncDetectedFeedback.BoolValue)
                        || (ActualVideoInput == eDmps3InputVideoSource.Vga
                        && InputCard.VgaInputPort.SyncDetectedFeedback.BoolValue),

                    HasVideoStatusFunc = () =>
                        (ActualVideoInput == eDmps3InputVideoSource.Hdmi
                        && HdmiIn.VideoStatus.HasVideoStatusFeedback.BoolValue)
                        || (ActualVideoInput == eDmps3InputVideoSource.Vga
                        && VgaIn.VideoStatus.HasVideoStatusFeedback.BoolValue)
                };

                AnyVideoInput = new RoutingInputPortWithVideoStatuses(DmPortName.AnyVideoIn,
                    eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.None, eDmps3InputVideoSource.Auto, this, combinedFuncs);

                ActiveVideoInputFeedback = new StringFeedback("ActiveVideoInput", () => ActualVideoInput.ToString());

                VideoSourceNumericFeedback = new IntFeedback(() =>
                {
                    return (int)InputCard.VideoSourceFeedback;
                });
                AudioSourceNumericFeedback = new IntFeedback(() =>
                {
                    return (int)InputCard.AudioSourceFeedback;
                });

                HdmiInHdcpCapabilityFeedback = new IntFeedback("HdmiInHdcpCapability", () =>
                {
                    if (InputCard.HdmiInputPort.HdcpSupportOnFeedback.BoolValue)
                        return 1;
                    else
                        return 0;
                });

                // Set Ports for CEC
                HdmiIn.Port = InputCard.HdmiInputPort;

                VirtualDmOut = new RoutingOutputPort(DmPortName.DmOut, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                    eRoutingPortConnectionType.None, null, this);

                AddToFeedbackList(ActiveVideoInputFeedback, VideoSourceNumericFeedback, AudioSourceNumericFeedback,
                AnyVideoInput.VideoStatus.HasVideoStatusFeedback, AnyVideoInput.VideoStatus.HdcpActiveFeedback,
                AnyVideoInput.VideoStatus.HdcpStateFeedback, AnyVideoInput.VideoStatus.VideoResolutionFeedback,
                AnyVideoInput.VideoStatus.VideoSyncFeedback, HdmiInHdcpCapabilityFeedback);

                //AddPostActivationAction(() =>
                //{
                    // Link up all of these damned events to the various RoutingPorts via a helper handler
                    InputCard.HdmiInputPort.InputOutput.BaseDevice.BaseEvent += (o, a) => FowardInputStreamChange(HdmiIn, a.EventId);
                    InputCard.HdmiInputPort.VideoAttributes.AttributeChange += (o, a) => ForwardVideoAttributeChange(HdmiIn, a.EventId);

                    InputCard.VgaInputPort.InputOutput.BaseDevice.BaseEvent += (o, a) => FowardInputStreamChange(VgaIn, a.EventId);
                    InputCard.VgaInputPort.VideoAttributes.AttributeChange += (o, a) => ForwardVideoAttributeChange(VgaIn, a.EventId);
                //});

            }

        }

        /// <summary>
        /// Relays the input stream change to the appropriate RoutingInputPort.
        /// </summary>
        protected void FowardInputStreamChange(RoutingInputPortWithVideoStatuses inputPort, int eventId)
        {
            if (eventId == Crestron.SimplSharpPro.DM.DMInputEventIds.SourceSyncEventId)
            {
                inputPort.VideoStatus.VideoSyncFeedback.FireUpdate();
                AnyVideoInput.VideoStatus.VideoSyncFeedback.FireUpdate();
            }
        }

        /// <summary>
        /// Relays the VideoAttributes change to a RoutingInputPort
        /// </summary>
        protected void ForwardVideoAttributeChange(RoutingInputPortWithVideoStatuses inputPort, int eventId)
        {
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

        /// <summary>
        /// Adds feedback(s) to the list
        /// </summary>
        /// <param name="newFbs"></param>
        public void AddToFeedbackList(params Feedback[] newFbs)
        {
            foreach (var f in newFbs)
            {
                if (f != null)
                {
                    if (!Feedbacks.Contains(f))
                    {
                        Feedbacks.Add(f);
                    }
                }
            }
        }

        #region ITxRouting Members


        public virtual void ExecuteNumericSwitch(ushort input, ushort output, eRoutingSignalType type)
        {
            Debug.Console(2, this, "Executing Numeric Switch to input {0}.", input);

            switch (input)
            {
                case 0:
                    {
                        ExecuteSwitch(eDmps3InputVideoSource.Auto, null, type);
                        break;
                    }
                case 1:
                    {
                        ExecuteSwitch(HdmiIn.Selector, null, type);
                        break;
                    }
                case 2:
                    {
                        ExecuteSwitch(VgaIn.Selector, null, type);
                        break;
                    }
            }

        }

        #endregion

        #region IRouting Members

        public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType)
        {
            if ((signalType | eRoutingSignalType.Video) == eRoutingSignalType.Video)
                InputCard.VideoSource = (eDmps3InputVideoSource)inputSelector;
            if ((signalType | eRoutingSignalType.Audio) == eRoutingSignalType.Audio)
                InputCard.AudioSource = (eDmps3InputAudioSource)inputSelector;
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class DmpsInternalVirtualHdmiVgaBncInputController : DmpsInternalVirtualHdmiVgaInputController
    {
        public new Card.Dmps3HdmiVgaBncInput InputCard { get; private set; }

        public RoutingInputPortWithVideoStatuses BncIn { get; private set; }
        public RoutingInputPort SpdifIn { get; private set; }

        public override RoutingPortCollection<RoutingInputPort> InputPorts
        {
            get
            {
                return new RoutingPortCollection<RoutingInputPort> 
				{ 
					HdmiIn,
					VgaIn,
                    BncIn,
                    SpdifIn,
					AnyVideoInput 
				};
            }
        }

        public DmpsInternalVirtualHdmiVgaBncInputController(string key, string name, Card.Dmps3HdmiVgaBncInput inputCard)
            : base(key, name, inputCard)
        {
            InputCard = inputCard;

            HdmiIn = new RoutingInputPortWithVideoStatuses(DmPortName.HdmiIn, eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi,
                eDmps3InputVideoSource.Hdmi, this, VideoStatusHelper.GetHdmiInputStatusFuncs(InputCard.HdmiInputPort));
            VgaIn = new RoutingInputPortWithVideoStatuses(DmPortName.VgaIn,
                eRoutingSignalType.Video, eRoutingPortConnectionType.Vga, eDmps3InputVideoSource.Vga, this,
                VideoStatusHelper.GetVgaInputStatusFuncs(InputCard.VgaInputPort));
            BncIn = new RoutingInputPortWithVideoStatuses(DmPortName.BncIn, eRoutingSignalType.Video, eRoutingPortConnectionType.Component,
                eDmps3InputVideoSource.Bnc, this, VideoStatusHelper.GetBncInputStatusFuncs(InputCard.BncInputPort));
            SpdifIn = new RoutingInputPort(DmPortName.SpdifIn, eRoutingSignalType.Audio, eRoutingPortConnectionType.DigitalAudio,
                eDmps3InputAudioSource.Spdif, this);

            if (InputCard.HdmiInputPort.HdcpSupportedLevelFeedback == eHdcpSupportedLevel.Hdcp2xSupport)
                HdcpSupportCapability = eHdcpCapabilityType.Hdcp2_2Support;
            else if (InputCard.HdmiInputPort.HdcpSupportedLevelFeedback == eHdcpSupportedLevel.Hdcp1xSupport)
                HdcpSupportCapability = eHdcpCapabilityType.Hdcp1xSupport;

            var combinedFuncs = new VideoStatusFuncsWrapper
            {
                HdcpActiveFeedbackFunc = () =>
                    (ActualVideoInput == eDmps3InputVideoSource.Hdmi
                    && InputCard.HdmiInputPort.VideoAttributes.HdcpActiveFeedback.BoolValue),

                HdcpStateFeedbackFunc = () =>
                {
                    if (ActualVideoInput == eDmps3InputVideoSource.Hdmi)
                        return InputCard.HdmiInputPort.VideoAttributes.HdcpStateFeedback.ToString();
                    return "";
                },

                VideoResolutionFeedbackFunc = () =>
                {
                    if (ActualVideoInput == eDmps3InputVideoSource.Hdmi)
                        return InputCard.HdmiInputPort.VideoAttributes.GetVideoResolutionString();
                    if (ActualVideoInput == eDmps3InputVideoSource.Vga)
                        return InputCard.VgaInputPort.VideoAttributes.GetVideoResolutionString();
                    if (ActualVideoInput == eDmps3InputVideoSource.Bnc)
                        return InputCard.BncInputPort.VideoAttributes.GetVideoResolutionString();
                    return "";
                },
                VideoSyncFeedbackFunc = () =>
                    (ActualVideoInput == eDmps3InputVideoSource.Hdmi
                    && InputCard.HdmiInputPort.SyncDetectedFeedback.BoolValue)
                    || (ActualVideoInput == eDmps3InputVideoSource.Vga
                    && InputCard.VgaInputPort.SyncDetectedFeedback.BoolValue)
                    || (ActualVideoInput == eDmps3InputVideoSource.Bnc
                    && InputCard.BncInputPort.VideoDetectedFeedback.BoolValue),

                HasVideoStatusFunc = () =>
                    (ActualVideoInput == eDmps3InputVideoSource.Hdmi 
                    && HdmiIn.VideoStatus.HasVideoStatusFeedback.BoolValue)
                    || (ActualVideoInput == eDmps3InputVideoSource.Vga
                    && VgaIn.VideoStatus.HasVideoStatusFeedback.BoolValue)
                    || (ActualVideoInput == eDmps3InputVideoSource.Bnc
                    &&BncIn.VideoStatus.HasVideoStatusFeedback.BoolValue)
            };

            AnyVideoInput = new RoutingInputPortWithVideoStatuses(DmPortName.AnyVideoIn,
                eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.None, 0, this, combinedFuncs);

            ActiveVideoInputFeedback = new StringFeedback("ActiveVideoInput", () => ActualVideoInput.ToString());

            VideoSourceNumericFeedback = new IntFeedback(() =>
            {
                return (int)InputCard.VideoSourceFeedback;
            });
            AudioSourceNumericFeedback = new IntFeedback(() =>
            {
                return (int)InputCard.AudioSourceFeedback;
            });

            HdmiInHdcpCapabilityFeedback = new IntFeedback("HdmiInHdcpCapability", () =>
            {
                if (InputCard.HdmiInputPort.HdcpSupportOnFeedback.BoolValue)
                    return 1;
                else
                    return 0;
            });

            // Set Ports for CEC
            HdmiIn.Port = InputCard.HdmiInputPort;

            VirtualDmOut = new RoutingOutputPort(DmPortName.DmOut, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.None, null, this);

            AddToFeedbackList(ActiveVideoInputFeedback, VideoSourceNumericFeedback, AudioSourceNumericFeedback,
            AnyVideoInput.VideoStatus.HasVideoStatusFeedback, AnyVideoInput.VideoStatus.HdcpActiveFeedback,
            AnyVideoInput.VideoStatus.HdcpStateFeedback, AnyVideoInput.VideoStatus.VideoResolutionFeedback,
            AnyVideoInput.VideoStatus.VideoSyncFeedback, HdmiInHdcpCapabilityFeedback);

            //AddPostActivationAction(() =>
            //{
                // Link up all of these damned events to the various RoutingPorts via a helper handler
                InputCard.HdmiInputPort.InputOutput.BaseDevice.BaseEvent += (o, a) => FowardInputStreamChange(HdmiIn, a.EventId);
                InputCard.HdmiInputPort.VideoAttributes.AttributeChange += (o, a) => ForwardVideoAttributeChange(HdmiIn, a.EventId);

                InputCard.VgaInputPort.InputOutput.BaseDevice.BaseEvent += (o, a) => FowardInputStreamChange(VgaIn, a.EventId);
                InputCard.VgaInputPort.VideoAttributes.AttributeChange += (o, a) => ForwardVideoAttributeChange(VgaIn, a.EventId);

                InputCard.BncInputPort.InputOutput.BaseDevice.BaseEvent += (o, a) => FowardInputStreamChange(HdmiIn, a.EventId);
                InputCard.BncInputPort.VideoAttributes.AttributeChange += (o, a) => ForwardVideoAttributeChange(HdmiIn, a.EventId);
            //});

        }

        public override void ExecuteNumericSwitch(ushort input, ushort output, eRoutingSignalType type)
        {
            Debug.Console(2, this, "Executing Numeric Switch to input {0}.", input);

            switch (input)
            {
                case 0:
                    {
                        ExecuteSwitch(eDmps3InputVideoSource.Auto, null, type);
                        break;
                    }
                case 1:
                    {
                        ExecuteSwitch(HdmiIn.Selector, null, type);
                        break;
                    }
                case 2:
                    {
                        ExecuteSwitch(VgaIn.Selector, null, type);
                        break;
                    }
                case 3:
                    {
                        ExecuteSwitch(BncIn.Selector, null, type);
                        break;
                    }
            }
        }
    }
}