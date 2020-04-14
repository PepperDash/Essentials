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
    public class DmTx201XController : DmTxControllerBase, ITxRouting, IHasFeedback, IHasFreeRun, IVgaBrightnessContrastControls
	{
		public DmTx201S Tx { get; private set; } // uses the 201S class as it is the base class for the 201C

		public RoutingInputPortWithVideoStatuses HdmiInput { get; private set; }
		public RoutingInputPortWithVideoStatuses VgaInput { get; private set; }
		public RoutingOutputPort DmOutput { get; private set; }
        public RoutingOutputPort HdmiLoopOut { get; private set; }

        public override StringFeedback ActiveVideoInputFeedback { get; protected set; }
        public IntFeedback VideoSourceNumericFeedback { get; protected set; }
        public IntFeedback AudioSourceNumericFeedback { get; protected set; }
        public IntFeedback HdmiInHdcpCapabilityFeedback { get; protected set; }

        public BoolFeedback FreeRunEnabledFeedback { get; protected set; }

        public IntFeedback VgaBrightnessFeedback { get; protected set; }
        public IntFeedback VgaContrastFeedback { get; protected set; }

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
				return new RoutingPortCollection<RoutingOutputPort> { DmOutput, HdmiLoopOut };
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="name"></param>
		/// <param name="tx"></param>
		public DmTx201XController(string key, string name, DmTx201S tx)
			: base(key, name, tx)
		{
			Tx = tx;

			HdmiInput = new RoutingInputPortWithVideoStatuses(DmPortName.HdmiIn,
				eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, DmTx200Base.eSourceSelection.Digital, this,
				VideoStatusHelper.GetHdmiInputStatusFuncs(tx.HdmiInput));
			VgaInput = new RoutingInputPortWithVideoStatuses(DmPortName.VgaIn,
				eRoutingSignalType.Video, eRoutingPortConnectionType.Vga, DmTx200Base.eSourceSelection.Analog, this,
				VideoStatusHelper.GetVgaInputStatusFuncs(tx.VgaInput));

			ActiveVideoInputFeedback = new StringFeedback("ActiveVideoInput",
				() => ActualActiveVideoInput.ToString());

            Tx.HdmiInput.InputStreamChange += new EndpointInputStreamChangeEventHandler(InputStreamChangeEvent);
            Tx.BaseEvent += Tx_BaseEvent;
            Tx.OnlineStatusChange += new OnlineStatusChangeEventHandler(Tx_OnlineStatusChange);

            VideoSourceNumericFeedback = new IntFeedback(() => 
                {
                    return (int)Tx.VideoSourceFeedback;
                });
            AudioSourceNumericFeedback = new IntFeedback(() =>
                {
                    return (int)Tx.AudioSourceFeedback;
                });

            HdmiInHdcpCapabilityFeedback = new IntFeedback("HdmiInHdcpCapability", () =>
                {
                    if (tx.HdmiInput.HdcpSupportOnFeedback.BoolValue)
                        return 1;
                    else
                        return 0;
                });

            FreeRunEnabledFeedback = new BoolFeedback(() => tx.VgaInput.FreeRunFeedback == eDmFreeRunSetting.Enabled);

            VgaBrightnessFeedback = new IntFeedback(() => tx.VgaInput.VideoControls.BrightnessFeedback.UShortValue);
            VgaContrastFeedback = new IntFeedback(() => tx.VgaInput.VideoControls.ContrastFeedback.UShortValue);

            tx.VgaInput.VideoControls.ControlChange += new Crestron.SimplSharpPro.DeviceSupport.GenericEventHandler(VideoControls_ControlChange);

            HdcpSupportCapability = eHdcpCapabilityType.HdcpAutoSupport;

			var combinedFuncs = new VideoStatusFuncsWrapper
			{
				HdcpActiveFeedbackFunc = () =>
					(ActualActiveVideoInput == DmTx200Base.eSourceSelection.Digital
					&& tx.HdmiInput.VideoAttributes.HdcpActiveFeedback.BoolValue),

				HdcpStateFeedbackFunc = () =>
				{
					if (ActualActiveVideoInput == DmTx200Base.eSourceSelection.Digital)
						return tx.HdmiInput.VideoAttributes.HdcpStateFeedback.ToString();
					return "";
				},

				VideoResolutionFeedbackFunc = () =>
				{
					if (ActualActiveVideoInput == DmTx200Base.eSourceSelection.Digital)
						return tx.HdmiInput.VideoAttributes.GetVideoResolutionString();
					if (ActualActiveVideoInput == DmTx200Base.eSourceSelection.Analog)
						return tx.VgaInput.VideoAttributes.GetVideoResolutionString();
					return "";
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
			HdmiLoopOut = new RoutingOutputPort(DmPortName.HdmiLoopOut, eRoutingSignalType.Audio | eRoutingSignalType.Video,
				eRoutingPortConnectionType.Hdmi, null, this);

            AddToFeedbackList(ActiveVideoInputFeedback, VideoSourceNumericFeedback, AudioSourceNumericFeedback,
                AnyVideoInput.VideoStatus.HasVideoStatusFeedback, AnyVideoInput.VideoStatus.HdcpActiveFeedback,
                AnyVideoInput.VideoStatus.HdcpStateFeedback, AnyVideoInput.VideoStatus.VideoResolutionFeedback,
                AnyVideoInput.VideoStatus.VideoSyncFeedback, HdmiInHdcpCapabilityFeedback);

            // Set Ports for CEC
            HdmiInput.Port = Tx.HdmiInput;
            VgaInput.Port = Tx.VgaInput;
            HdmiLoopOut.Port = Tx.HdmiOutput;
            DmOutput.Port = Tx.DmOutput;
		}

        void VideoControls_ControlChange(object sender, Crestron.SimplSharpPro.DeviceSupport.GenericEventArgs args)
        {
            var id = args.EventId;
            Debug.Console(2, this, "EventId {0}", args.EventId);

            if (id == VideoControlsEventIds.BrightnessFeedbackEventId)
            {
                VgaBrightnessFeedback.FireUpdate();
            }
            else if (id == VideoControlsEventIds.ContrastFeedbackEventId)
            {
                VgaContrastFeedback.FireUpdate();
            }
        }

        void Tx_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            ActiveVideoInputFeedback.FireUpdate();
            VideoSourceNumericFeedback.FireUpdate();
            AudioSourceNumericFeedback.FireUpdate();
            
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

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApi bridge)
        {
            LinkDmTxToApi(this, trilist, joinStart, joinMapKey, bridge);
        }

        /// <summary>
        /// Enables or disables free run
        /// </summary>
        /// <param name="enable"></param>
        public void SetFreeRunEnabled(bool enable)
        {
            if (enable)
            {
                Tx.VgaInput.FreeRun = eDmFreeRunSetting.Enabled;
            }
            else
            {
                Tx.VgaInput.FreeRun = eDmFreeRunSetting.Disabled;
            }
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
        /// Switches the audio/video source based on the integer value (0-Auto, 1-HDMI, 2-VGA, 3-Disable)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="type"></param>
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
            if((signalType | eRoutingSignalType.Video) == eRoutingSignalType.Video)
                Tx.VideoSource = (DmTx200Base.eSourceSelection)inputSelector;
            if ((signalType | eRoutingSignalType.Audio) == eRoutingSignalType.Audio)
                Tx.AudioSource = (DmTx200Base.eSourceSelection)inputSelector;
        }

		void Tx_BaseEvent(GenericBase device, BaseEventArgs args)
		{
			var id = args.EventId;
            Debug.Console(2, this, "EventId {0}", args.EventId);

            if (id == EndpointTransmitterBase.VideoSourceFeedbackEventId)
			{
                Debug.Console(2, this, "  Video Source: {0}", Tx.VideoSourceFeedback);
                VideoSourceNumericFeedback.FireUpdate();
                ActiveVideoInputFeedback.FireUpdate();
			}

			// ------------------------------ incomplete -----------------------------------------
            else if (id == EndpointTransmitterBase.AudioSourceFeedbackEventId)
			{
				Debug.Console(2, this, "  Audio Source: {0}", Tx.AudioSourceFeedback);
                AudioSourceNumericFeedback.FireUpdate();
			}
            
		}

        void InputStreamChangeEvent(EndpointInputStream inputStream, EndpointInputStreamEventArgs args)
        {
            Debug.Console(2, "{0} event {1} stream {2}", this.Tx.ToString(), inputStream.ToString(), args.EventId.ToString());

            if (args.EventId == EndpointInputStreamEventIds.HdcpSupportOffFeedbackEventId)
            {
                HdmiInHdcpCapabilityFeedback.FireUpdate();
            }
            else if (args.EventId == EndpointInputStreamEventIds.HdcpSupportOnFeedbackEventId)
            {
                HdmiInHdcpCapabilityFeedback.FireUpdate();
            }
            else if (args.EventId == EndpointInputStreamEventIds.FreeRunFeedbackEventId)
            {
                FreeRunEnabledFeedback.FireUpdate();
            }
        }

		/// <summary>
		/// Relays the input stream change to the appropriate RoutingInputPort.
		/// </summary>
		void FowardInputStreamChange(RoutingInputPortWithVideoStatuses inputPort, int eventId)
		{
			if (eventId == EndpointInputStreamEventIds.SyncDetectedFeedbackEventId)
			{
				inputPort.VideoStatus.VideoSyncFeedback.FireUpdate();
				AnyVideoInput.VideoStatus.VideoSyncFeedback.FireUpdate();
			}
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