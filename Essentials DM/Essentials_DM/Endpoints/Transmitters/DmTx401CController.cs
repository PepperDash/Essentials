using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
//using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.DM.Config;

namespace PepperDash.Essentials.DM
{
	using eVst = DmTx401C.eSourceSelection;

	public class DmTx401CController : DmTxControllerBase, IRoutingInputsOutputs, IHasFeedback, IIROutputPorts, IComPorts
	{
		public DmTx401C Tx { get; private set; }

		public RoutingInputPortWithVideoStatuses AnyVideoInput { get; private set; }
		public RoutingInputPortWithVideoStatuses HdmiIn { get; private set; }
		public RoutingInputPortWithVideoStatuses DisplayPortIn { get; private set; }
		public RoutingInputPortWithVideoStatuses VgaIn { get; private set; }
		public RoutingInputPortWithVideoStatuses CompositeIn { get; private set; }
		public RoutingOutputPort DmOut { get; private set; }

		public StringFeedback ActiveVideoInputFeedback { get; private set; }

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
				eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, 1, this,
				VideoStatusHelper.GetHdmiInputStatusFuncs(tx.HdmiInput));
			DisplayPortIn = new RoutingInputPortWithVideoStatuses(DmPortName.DisplayPortIn,
				eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, 2, this,
				VideoStatusHelper.GetDisplayPortInputStatusFuncs(tx.DisplayPortInput));
			VgaIn = new RoutingInputPortWithVideoStatuses(DmPortName.VgaIn,
				eRoutingSignalType.Video, eRoutingPortConnectionType.Vga, 3, this, 
				VideoStatusHelper.GetVgaInputStatusFuncs(tx.VgaInput));
			CompositeIn = new RoutingInputPortWithVideoStatuses(DmPortName.CompositeIn,
				eRoutingSignalType.Video, eRoutingPortConnectionType.Composite, 3, this,
				VideoStatusHelper.GetVgaInputStatusFuncs(tx.VgaInput));

			ActiveVideoInputFeedback = new StringFeedback(new Cue("ActiveVideoInput", 0, eCueType.String),
				() => ActualVideoInput.ToString());


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
				eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.None, 0, this, combinedFuncs);

			DmOut = new RoutingOutputPort(DmPortName.DmOut, eRoutingSignalType.AudioVideo, 
				eRoutingPortConnectionType.DmCat, null, this);
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

		void Tx_BaseEvent(GenericBase device, BaseEventArgs args)
		{
			var id = args.EventId;
			if (id == EndpointTransmitterBase.VideoSourceFeedbackEventId)
			{
				Debug.Console(2, this, "  Video Source: {0}", Tx.VideoSourceFeedback);
			}

			// ------------------------------ incomplete -----------------------------------------
			else if (id == EndpointTransmitterBase.AudioSourceFeedbackEventId)
			{
				Debug.Console(2, this, "  Audio Source: {0}", Tx.AudioSourceFeedback);
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