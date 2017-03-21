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
	using eVst = Crestron.SimplSharpPro.DeviceSupport.eX02VideoSourceType;

	public class DmTx4k302CController : DmTxControllerBase, IRoutingInputsOutputs, IHasFeedback, 
		IIROutputPorts, IComPorts
	{
		public DmTx4k302C Tx { get; private set; }

		public RoutingInputPortWithVideoStatuses AnyVideoInput { get; private set; }
		public RoutingInputPortWithVideoStatuses HdmiIn1 { get; private set; }
		public RoutingInputPortWithVideoStatuses HdmiIn2 { get; private set; }
		public RoutingInputPortWithVideoStatuses VgaIn { get; private set; }
		public RoutingOutputPort DmOut { get; private set; }
		public RoutingOutputPort HdmiLoopOut { get; private set; }

		public StringFeedback ActiveVideoInputFeedback { get; private set; }

		/// <summary>
		/// Helps get the "real" inputs, including when in Auto
		/// </summary>
		public Crestron.SimplSharpPro.DeviceSupport.eX02VideoSourceType ActualActiveVideoInput
		{
			get
			{
				if (Tx.VideoSourceFeedback != eVst.Auto)
					return Tx.VideoSourceFeedback;
				else // auto
				{
					if (Tx.HdmiInputs[1].SyncDetectedFeedback.BoolValue)
						return eVst.Hdmi1;
					else if (Tx.HdmiInputs[2].SyncDetectedFeedback.BoolValue)
						return eVst.Hdmi2;
					else if (Tx.VgaInput.SyncDetectedFeedback.BoolValue)
						return eVst.Vga;
					else
						return eVst.AllDisabled;
				}
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
					VgaIn, 
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="name"></param>
		/// <param name="tx"></param>
		public DmTx4k302CController(string key, string name, DmTx4k302C tx)
			: base(key, name, tx)
		{
			Tx = tx;

			HdmiIn1 = new RoutingInputPortWithVideoStatuses(DmPortName.HdmiIn1,
				eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, 1, this,
				VideoStatusHelper.GetHdmiInputStatusFuncs(tx.HdmiInputs[1]));
			HdmiIn2 = new RoutingInputPortWithVideoStatuses(DmPortName.HdmiIn2,
				eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, 2, this,
				VideoStatusHelper.GetHdmiInputStatusFuncs(tx.HdmiInputs[2]));
			VgaIn = new RoutingInputPortWithVideoStatuses(DmPortName.VgaIn,
				eRoutingSignalType.Video, eRoutingPortConnectionType.Vga, 3, this, 
				VideoStatusHelper.GetVgaInputStatusFuncs(tx.VgaInput));
			ActiveVideoInputFeedback = new StringFeedback(new Cue("ActiveVideoInput", 0, eCueType.String),
				() => ActualActiveVideoInput.ToString());

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
						if (ActualActiveVideoInput == eVst.Hdmi2)
							return tx.HdmiInputs[2].VideoAttributes.HdcpStateFeedback.ToString();
						return "";
					},

				VideoResolutionFeedbackFunc = () =>
					{
						if (ActualActiveVideoInput == eVst.Hdmi1)
							return tx.HdmiInputs[1].VideoAttributes.GetVideoResolutionString();
						if (ActualActiveVideoInput == eVst.Hdmi2)
							return tx.HdmiInputs[2].VideoAttributes.GetVideoResolutionString();
						if (ActualActiveVideoInput == eVst.Vga)
							return tx.VgaInput.VideoAttributes.GetVideoResolutionString();
						return "";
					},
				VideoSyncFeedbackFunc = () =>
					(ActualActiveVideoInput == eVst.Hdmi1
					&& tx.HdmiInputs[1].SyncDetectedFeedback.BoolValue)
					|| (ActualActiveVideoInput == eVst.Hdmi2
					&& tx.HdmiInputs[2].SyncDetectedFeedback.BoolValue)
					|| (ActualActiveVideoInput == eVst.Vga
					&& tx.VgaInput.SyncDetectedFeedback.BoolValue)
			};

			AnyVideoInput = new RoutingInputPortWithVideoStatuses(DmPortName.AnyVideoIn,
				eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.None, 0, this, combinedFuncs);

			DmOut = new RoutingOutputPort(DmPortName.DmOut, eRoutingSignalType.AudioVideo, 
				eRoutingPortConnectionType.DmCat, null, this);
			HdmiLoopOut = new RoutingOutputPort(DmPortName.HdmiLoopOut, eRoutingSignalType.AudioVideo,
				eRoutingPortConnectionType.Hdmi, null, this);
		}

		public override bool CustomActivate()
		{
			// Link up all of these damned events to the various RoutingPorts via a helper handler
			Tx.HdmiInputs[1].InputStreamChange += (o, a) => FowardInputStreamChange(HdmiIn1, a.EventId);
			Tx.HdmiInputs[1].VideoAttributes.AttributeChange += (o, a) => ForwardVideoAttributeChange(HdmiIn1, a.EventId);

			Tx.HdmiInputs[2].InputStreamChange += (o, a) => FowardInputStreamChange(HdmiIn2, a.EventId);
			Tx.HdmiInputs[2].VideoAttributes.AttributeChange += (o, a) => ForwardVideoAttributeChange(HdmiIn2, a.EventId);

			Tx.VgaInput.InputStreamChange += (o, a) => FowardInputStreamChange(VgaIn, a.EventId);
			Tx.VgaInput.VideoAttributes.AttributeChange += (o, a) => ForwardVideoAttributeChange(VgaIn, a.EventId);

			// Base does register and sets up comm monitoring.
			return base.CustomActivate();
		}

		void Tx_BaseEvent(GenericBase device, BaseEventArgs args)
		{
			var id = args.EventId;
			if (id == DmTx4k302C.VideoSourceFeedbackEventId)
			{
				Debug.Console(2, this, "  Video Source: {0}", Tx.VideoSourceFeedback);
			}

			// ------------------------------ incomplete -----------------------------------------
			else if (id == DmTx4k302C.AudioSourceFeedbackEventId)
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