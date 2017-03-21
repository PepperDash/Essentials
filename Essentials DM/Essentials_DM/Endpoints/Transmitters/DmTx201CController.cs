using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.DM.Config;

namespace PepperDash.Essentials.DM
{
	public class DmTx201SBasicController : DmTxControllerBase, IRoutingInputsOutputs, IHasFeedback
	{
		public DmTx201S Tx { get; private set; }

		public RoutingInputPortWithVideoStatuses AnyVideoInput { get; private set; }
		public RoutingInputPortWithVideoStatuses HdmiInput { get; private set; }
		public RoutingInputPortWithVideoStatuses VgaInput { get; private set; }
		public RoutingOutputPort DmOutput { get; private set; }
		public StringFeedback ActiveVideoInputFeedback { get; private set; }

		/// <summary>
		/// Helps get the "real" inputs, including when in Auto
		/// </summary>
		public DmTx200Base.eSourceSelection ActualVideoInput
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
				return new RoutingPortCollection<RoutingOutputPort> { DmOutput };
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="name"></param>
		/// <param name="tx"></param>
		public DmTx201SBasicController(string key, string name, DmTx201S tx)
			: base(key, name, tx)
		{
			Tx = tx;

			//Can this be combined into helper somehow??
			var combinedFuncs = new VideoStatusFuncsWrapper
			{
				HdcpActiveFeedbackFunc = () =>
					ActualVideoInput == DmTx200Base.eSourceSelection.Digital
					&& tx.HdmiInput.VideoAttributes.HdcpActiveFeedback.BoolValue,

				HdcpStateFeedbackFunc = () =>
					ActualVideoInput == DmTx200Base.eSourceSelection.Digital
					? tx.HdmiInput.VideoAttributes.HdcpStateFeedback.ToString()
					: "",

				VideoResolutionFeedbackFunc = () =>
					ActualVideoInput == DmTx200Base.eSourceSelection.Digital
					? Tx.HdmiInput.VideoAttributes.GetVideoResolutionString()
					: Tx.VgaInput.VideoAttributes.GetVideoResolutionString(),

				VideoSyncFeedbackFunc = () =>
					ActualVideoInput == DmTx200Base.eSourceSelection.Digital
					? HdmiInput.VideoStatus.VideoSyncFeedback.BoolValue
					: VgaInput.VideoStatus.VideoSyncFeedback.BoolValue
			};
			HdmiInput = new RoutingInputPortWithVideoStatuses(DmPortName.HdmiIn,
				eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, 1, this,
				VideoStatusHelper.GetHdmiInputStatusFuncs(tx.HdmiInput));
			VgaInput = new RoutingInputPortWithVideoStatuses(DmPortName.VgaIn,
				eRoutingSignalType.Video, eRoutingPortConnectionType.Vga, 2, this, 
				VideoStatusHelper.GetVgaInputStatusFuncs(tx.VgaInput));
			AnyVideoInput = new RoutingInputPortWithVideoStatuses(DmPortName.AnyVideoIn,
				eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.None, 0, this, combinedFuncs);

			DmOutput = new RoutingOutputPort(DmPortName.DmOut, eRoutingSignalType.AudioVideo,
				eRoutingPortConnectionType.DmCat, null, this);
			
			ActiveVideoInputFeedback = new StringFeedback(new Cue("ActiveVideoInput", 0, eCueType.String),
				() => ActualVideoInput.ToString());
		}

		public override bool CustomActivate()
		{
			Tx.HdmiInput.InputStreamChange += (o, a) => FireInputStreamChange(HdmiInput, a.EventId);
			Tx.HdmiInput.VideoAttributes.AttributeChange += (o, a) => FireVideoAttributeChange(HdmiInput, a.EventId);

			Tx.VgaInput.InputStreamChange += (o, a) => FireInputStreamChange(VgaInput, a.EventId);
			Tx.VgaInput.VideoAttributes.AttributeChange += (o, a) => FireVideoAttributeChange(VgaInput, a.EventId);

			// Base does register and sets up comm monitoring.
			return base.CustomActivate();
		}

		void Tx_BaseEvent(GenericBase device, BaseEventArgs args)
		{
			var id = args.EventId;
			if (id == DmTx201S.VideoSourceFeedbackEventID)
			{
				Debug.Console(2, this, "  Video Source: {0}", Tx.VideoSourceFeedback);
			}

			// ------------------------------ incomplete -----------------------------------------
			else if (id == DmTx201S.AudioSourceFeedbackEventID)
			{
				Debug.Console(2, this, "  Audio Source: {0}", Tx.AudioSourceFeedback);
			}
		}

		/// <summary>
		/// Relays the input stream change to the appropriate RoutingInputPort.
		/// </summary>
		void FireInputStreamChange(RoutingInputPortWithVideoStatuses inputPort, int eventId)
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
			}
		}

		public override List<Feedback> Feedbacks
		{
			get
			{
				List<Feedback> list = AnyVideoInput.VideoStatus.ToList();
				list.AddRange(base.Feedbacks);
				list.Add(ActiveVideoInputFeedback);
				return list;
			}
		}
	}
}