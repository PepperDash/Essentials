using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
//using Crestron.SimplSharpPro.DeviceSupport;
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
	using eVst = DmTx401C.eSourceSelection;

    [Description("Wrapper class for DM-TX-401-C")]
    public class DmTx401CController : DmTxControllerBase, ITxRouting, IIROutputPorts, IComPorts, IHasFreeRun, IVgaBrightnessContrastControls
	{
		public DmTx401C Tx { get; private set; }

		public RoutingInputPortWithVideoStatuses HdmiIn { get; private set; }
		public RoutingInputPortWithVideoStatuses DisplayPortIn { get; private set; }
		public RoutingInputPortWithVideoStatuses VgaIn { get; private set; }
		public RoutingInputPortWithVideoStatuses CompositeIn { get; private set; }
		public RoutingOutputPort DmOut { get; private set; }

		public override StringFeedback ActiveVideoInputFeedback { get; protected set; }
        public IntFeedback VideoSourceNumericFeedback { get; protected set; }
        public IntFeedback AudioSourceNumericFeedback { get; protected set; }
        public IntFeedback HdmiInHdcpCapabilityFeedback { get; protected set; }
        public BoolFeedback DisplayPortVideoSyncFeedback { get; protected set; }
        public BoolFeedback HdmiVideoSyncFeedback { get; protected set; }
        public BoolFeedback VgaVideoSyncFeedback { get; protected set; }

        public BoolFeedback FreeRunEnabledFeedback { get; protected set; }

        public IntFeedback VgaBrightnessFeedback { get; protected set; }
        public IntFeedback VgaContrastFeedback { get; protected set; }

		/// <summary>
		/// Helps get the "real" inputs, including when in Auto
		/// </summary>
		public BaseDmTx401.eSourceSelection ActualVideoInput
		{
			get
			{
				if (Tx.VideoSourceFeedback != BaseDmTx401.eSourceSelection.Auto)
					return Tx.VideoSourceFeedback;
				else // auto
				{
					if (Tx.HdmiInput.SyncDetectedFeedback.BoolValue)
						return BaseDmTx401.eSourceSelection.HDMI;
					else if (Tx.VgaInput.SyncDetectedFeedback.BoolValue)
						return BaseDmTx401.eSourceSelection.VGA;
					else if (Tx.DisplayPortInput.SyncDetectedFeedback.BoolValue)
						return BaseDmTx401.eSourceSelection.DisplayPort;
					else if (Tx.CvbsInput.SyncDetectedFeedback.BoolValue)
						return BaseDmTx401.eSourceSelection.Composite;
					else
						return BaseDmTx401.eSourceSelection.Disabled;
				}
			}
		}

		public RoutingPortCollection<RoutingInputPort> InputPorts
		{
			get
			{
				return new RoutingPortCollection<RoutingInputPort> 
				{ 
					HdmiIn,
					DisplayPortIn,
					VgaIn, 
					CompositeIn,
					AnyVideoInput 
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="name"></param>
		/// <param name="tx"></param>
		public DmTx401CController(string key, string name, DmTx401C tx)
			: base(key, name, tx)
		{
			Tx = tx;

			HdmiIn = new RoutingInputPortWithVideoStatuses(DmPortName.HdmiIn,
                eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, eVst.HDMI, this,
				VideoStatusHelper.GetHdmiInputStatusFuncs(tx.HdmiInput));
			DisplayPortIn = new RoutingInputPortWithVideoStatuses(DmPortName.DisplayPortIn,
                eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, eVst.DisplayPort, this,
				VideoStatusHelper.GetDisplayPortInputStatusFuncs(tx.DisplayPortInput));
			VgaIn = new RoutingInputPortWithVideoStatuses(DmPortName.VgaIn,
                eRoutingSignalType.Video, eRoutingPortConnectionType.Vga, eVst.VGA, this, 
				VideoStatusHelper.GetVgaInputStatusFuncs(tx.VgaInput));
			CompositeIn = new RoutingInputPortWithVideoStatuses(DmPortName.CompositeIn,
                eRoutingSignalType.Video, eRoutingPortConnectionType.Composite, eVst.Composite, this,
				VideoStatusHelper.GetVgaInputStatusFuncs(tx.VgaInput));

            Tx.HdmiInput.InputStreamChange += HdmiInputStreamChangeEvent;
            Tx.DisplayPortInput.InputStreamChange += DisplayPortInputStreamChangeEvent;
            Tx.BaseEvent += Tx_BaseEvent;
            Tx.VgaInput.InputStreamChange += VgaInputOnInputStreamChange;
            tx.VgaInput.VideoControls.ControlChange += VideoControls_ControlChange;


			ActiveVideoInputFeedback = new StringFeedback("ActiveVideoInput",
				() => ActualVideoInput.ToString());

            VideoSourceNumericFeedback = new IntFeedback(() => (int)Tx.VideoSourceFeedback);

            AudioSourceNumericFeedback = new IntFeedback(() => (int)Tx.AudioSourceFeedback);

            HdmiInHdcpCapabilityFeedback = new IntFeedback("HdmiInHdcpCapability", () => tx.HdmiInput.HdcpSupportOnFeedback.BoolValue ? 1 : 0);

            HdcpSupportCapability = eHdcpCapabilityType.HdcpAutoSupport;

            DisplayPortVideoSyncFeedback = new BoolFeedback("DisplayPortVideoSync", () => (bool)tx.DisplayPortInput.SyncDetectedFeedback.BoolValue);

            HdmiVideoSyncFeedback = new BoolFeedback(() => (bool)tx.HdmiInput.SyncDetectedFeedback.BoolValue);

            VgaVideoSyncFeedback = new BoolFeedback(() => (bool)tx.VgaInput.SyncDetectedFeedback.BoolValue);

            FreeRunEnabledFeedback = new BoolFeedback(() => tx.VgaInput.FreeRunFeedback == eDmFreeRunSetting.Enabled);

            VgaBrightnessFeedback = new IntFeedback(() => tx.VgaInput.VideoControls.BrightnessFeedback.UShortValue);

            VgaContrastFeedback = new IntFeedback(() => tx.VgaInput.VideoControls.ContrastFeedback.UShortValue);


			var combinedFuncs = new VideoStatusFuncsWrapper
			{
				HdcpActiveFeedbackFunc = () =>
					(ActualVideoInput == eVst.HDMI
					&& tx.HdmiInput.VideoAttributes.HdcpActiveFeedback.BoolValue)
					|| (ActualVideoInput == eVst.DisplayPort
					&& tx.DisplayPortInput.VideoAttributes.HdcpActiveFeedback.BoolValue),

				HdcpStateFeedbackFunc = () =>
				{
					if (ActualVideoInput == eVst.HDMI)
						return tx.HdmiInput.VideoAttributes.HdcpStateFeedback.ToString();
					if (ActualVideoInput == eVst.DisplayPort)
						return tx.DisplayPortInput.VideoAttributes.HdcpStateFeedback.ToString();
					return "";
				},

				VideoResolutionFeedbackFunc = () =>
				{
					if (ActualVideoInput == eVst.HDMI)
						return tx.HdmiInput.VideoAttributes.GetVideoResolutionString();
					if (ActualVideoInput == eVst.DisplayPort)
						return tx.DisplayPortInput.VideoAttributes.GetVideoResolutionString();
					if (ActualVideoInput == eVst.VGA)
						return tx.VgaInput.VideoAttributes.GetVideoResolutionString();
					if (ActualVideoInput == eVst.Composite)
						return tx.CvbsInput.VideoAttributes.GetVideoResolutionString();
					return "";
				},
				VideoSyncFeedbackFunc = () =>
					(ActualVideoInput == eVst.HDMI
					&& tx.HdmiInput.SyncDetectedFeedback.BoolValue)
					|| (ActualVideoInput == eVst.DisplayPort
					&& tx.DisplayPortInput.SyncDetectedFeedback.BoolValue)
					|| (ActualVideoInput == eVst.VGA
					&& tx.VgaInput.SyncDetectedFeedback.BoolValue)
					|| (ActualVideoInput == eVst.Composite
					&& tx.CvbsInput.SyncDetectedFeedback.BoolValue)
			};

            AnyVideoInput = new RoutingInputPortWithVideoStatuses(DmPortName.AnyVideoIn,
                eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.None, 0, this, combinedFuncs);

            DmOut = new RoutingOutputPort(DmPortName.DmOut, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.DmCat, null, this);

            AddToFeedbackList(ActiveVideoInputFeedback, VideoSourceNumericFeedback, AudioSourceNumericFeedback,
                AnyVideoInput.VideoStatus.HasVideoStatusFeedback, AnyVideoInput.VideoStatus.HdcpActiveFeedback,
                AnyVideoInput.VideoStatus.HdcpStateFeedback, AnyVideoInput.VideoStatus.VideoResolutionFeedback,
                AnyVideoInput.VideoStatus.VideoSyncFeedback, HdmiInHdcpCapabilityFeedback, DisplayPortVideoSyncFeedback,
                HdmiVideoSyncFeedback, VgaVideoSyncFeedback);

            // Set Ports for CEC
            DisplayPortIn.Port = Tx.DisplayPortInput;
            HdmiIn.Port = Tx.HdmiInput;
            DmOut.Port = Tx.DmOutput;
		}

		public override bool CustomActivate()
		{
			// Link up all of these damned events to the various RoutingPorts via a helper handler
			Tx.HdmiInput.InputStreamChange += (o, a) => FowardInputStreamChange(HdmiIn, a.EventId);
			Tx.HdmiInput.VideoAttributes.AttributeChange += (o, a) => ForwardVideoAttributeChange(HdmiIn, a.EventId);

			Tx.DisplayPortInput.InputStreamChange += (o, a) => FowardInputStreamChange(DisplayPortIn, a.EventId);
			Tx.DisplayPortInput.VideoAttributes.AttributeChange += (o, a) => ForwardVideoAttributeChange(DisplayPortIn, a.EventId);

			Tx.VgaInput.InputStreamChange += (o, a) => FowardInputStreamChange(VgaIn, a.EventId);
			Tx.VgaInput.VideoAttributes.AttributeChange += (o, a) => ForwardVideoAttributeChange(VgaIn, a.EventId);

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

        public void ExecuteNumericSwitch(ushort input, ushort output, eRoutingSignalType type)
        {
            Debug.Console(2, this, "Executing Numeric Switch to input {0}.", input);

            switch (input)
            {
                case 0:
                    {
                        ExecuteSwitch(eVst.Auto, null, type);
                        break;
                    }
                case 1:
                    {
                        ExecuteSwitch(DisplayPortIn.Selector, null, type);
                        break;
                    }
                case 2:
                    {
                        ExecuteSwitch(HdmiIn.Selector, null, type);
                        break;
                    }
                case 3:
                    {
                        ExecuteSwitch(VgaIn.Selector, null, type);
                        break;
                    }
                case 4:
                    {
                        ExecuteSwitch(CompositeIn.Selector, null, type);
                        break;
                    }
                case 5:
                    {
                        ExecuteSwitch(eVst.Disabled, null, type);
                        break;
                    }
            }
            
        }

        public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType)
        {
            if ((signalType | eRoutingSignalType.Video) == eRoutingSignalType.Video)
                Tx.VideoSource = (eVst)inputSelector;
            if ((signalType | eRoutingSignalType.Audio) == eRoutingSignalType.Audio)
                Tx.AudioSource = (eVst)inputSelector;
        }

        void Tx_BaseEvent(GenericBase device, BaseEventArgs args)
        {
            var id = args.EventId;
            Debug.Console(2, this, "EventId {0}", args.EventId);

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

        void HdmiInputStreamChangeEvent(EndpointInputStream inputStream, EndpointInputStreamEventArgs args)
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

        void DisplayPortInputStreamChangeEvent(EndpointInputStream inputStream, EndpointInputStreamEventArgs args)
        {
            Debug.Console(2, "{0} event {1} stream {2}", Tx.ToString(), inputStream.ToString(), args.EventId.ToString());

            switch (args.EventId)
            {
                case EndpointInputStreamEventIds.SyncDetectedFeedbackEventId:
                    DisplayPortVideoSyncFeedback.FireUpdate();
                    break;
            }
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