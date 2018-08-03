using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials.AppServer.Messengers
{
	/// <summary>
	/// Provides a messaging bridge for a VideoCodecBase
	/// </summary>
	public class VideoCodecBaseMessenger
	{
		/// <summary>
		/// 
		/// </summary>
		public VideoCodecBase Codec { get; private set; }

		public CotijaSystemController AppServerController { get; private set; }

		public string MessagePath { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="codec"></param>
		public VideoCodecBaseMessenger(VideoCodecBase codec, string messagePath)
		{
			if (codec == null)
				throw new ArgumentNullException("codec");
			if (string.IsNullOrEmpty(messagePath))
				throw new ArgumentException("messagePath must not be empty or null");

			MessagePath = messagePath;
			Codec = codec;
			codec.CallStatusChange += new EventHandler<CodecCallStatusItemChangeEventArgs>(codec_CallStatusChange);
		}

		/// <summary>
		/// Registers this codec's messaging with an app server controller
		/// </summary>
		/// <param name="appServerController"></param>
		public void RegisterWithAppServer(CotijaSystemController appServerController)
		{
			if (appServerController == null)
				throw new ArgumentNullException("appServerController");
			
			AppServerController = appServerController;

			appServerController.AddAction("/device/videoCodec/dial", new Action<string>(s => Codec.Dial(s)));
			appServerController.AddAction("/device/videoCodec/endCallById", new Action<string>(s =>
			{
				var call = GetCallWithId(s);
				if (call != null)
					Codec.EndCall(call);
			}));
			appServerController.AddAction(MessagePath + "/endAllCalls", new Action(() => Codec.EndAllCalls()));
			appServerController.AddAction(MessagePath + "/dtmf", new Action<string>(s => Codec.SendDtmf(s)));
			appServerController.AddAction(MessagePath + "/rejectById", new Action<string>(s =>
			{
				var call = GetCallWithId(s);
				if (call != null)
					Codec.RejectCall(call);
			}));
			appServerController.AddAction(MessagePath + "/acceptById", new Action<string>(s =>
			{
				var call = GetCallWithId(s);
				if (call != null)
					Codec.AcceptCall(call);
			}));
			appServerController.AddAction(MessagePath + "/privacyModeOn", new Action(() => Codec.PrivacyModeOn()));
			appServerController.AddAction(MessagePath + "/privacyModeOff", new Action(() => Codec.PrivacyModeOff()));
			appServerController.AddAction(MessagePath + "/privacyModeToggle", new Action(() => Codec.PrivacyModeToggle()));
			appServerController.AddAction(MessagePath + "/sharingStart", new Action(() => Codec.StartSharing()));
			appServerController.AddAction(MessagePath + "/sharingStop", new Action(() => Codec.StopSharing()));
			appServerController.AddAction(MessagePath + "/standbyOn", new Action(() => Codec.StandbyActivate()));
			appServerController.AddAction(MessagePath + "/standbyOff", new Action(() => Codec.StandbyDeactivate()));
		}

		public void GetFullStatusMessage()
		{

		}

		/// <summary>
		/// Helper to grab a call with string ID
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		CodecActiveCallItem GetCallWithId(string id)
		{
			return Codec.ActiveCalls.FirstOrDefault(c => c.Id == id);
		}

		/// <summary>
		/// Handler for codec changes
		/// </summary>
		void codec_CallStatusChange(object sender, CodecCallStatusItemChangeEventArgs e)
		{
			PostStatusMessage(new
			{
				vtc = GetVtcCallsMessageObject()
			});

		}

		/// <summary>
		/// Helper method to build call status for vtc
		/// </summary>
		/// <returns></returns>
		object GetVtcCallsMessageObject()
		{
			var info = Codec.CodecInfo;
			return new
			{
				isInCall = Codec.IsInCall,
				isReady = Codec.IsReady,
				privacyModeIsOn = Codec.PrivacyModeIsOnFeedback.BoolValue,
				sharingContentIsOn = Codec.SharingContentIsOnFeedback.BoolValue,
				sharingSource = Codec.SharingSourceFeedback.StringValue,
				standbyIsOn = Codec.StandbyIsOnFeedback.StringValue,
				calls = Codec.ActiveCalls,
				info = new
				{
					autoAnswerEnabled = info.AutoAnswerEnabled,
					e164Alias = info.E164Alias,
					h323Id = info.H323Id,
					ipAddress = info.IpAddress,
					multiSiteEnabled = info.MultiSiteOptionIsEnabled,
					sipPhoneNumber = info.SipPhoneNumber,
					sipURI = info.SipUri
				},
				showSelfViewByDefault = Codec.ShowSelfViewByDefault
			};
		}

		/// <summary>
		/// Helper for posting status message
		/// </summary>
		/// <param name="contentObject">The contents of the content object</param>
		void PostStatusMessage(object contentObject)
		{
			AppServerController.SendMessageToServer(JObject.FromObject(new
			{
				type = MessagePath,
				content = contentObject
			}));
		}
	}
}