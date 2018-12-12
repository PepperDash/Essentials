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
	public class VideoCodecBaseMessenger : MessengerBase
	{
		/// <summary>
		/// 
		/// </summary>
		public VideoCodecBase Codec { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="codec"></param>
		public VideoCodecBaseMessenger(VideoCodecBase codec, string messagePath) : base(messagePath)
		{
			if (codec == null)
				throw new ArgumentNullException("codec");

			Codec = codec;
			codec.CallStatusChange += new EventHandler<CodecCallStatusItemChangeEventArgs>(codec_CallStatusChange);
			codec.IsReadyChange += new EventHandler<EventArgs>(codec_IsReadyChange);

			var dirCodec = codec as IHasDirectory;
			if (dirCodec != null)
			{
				dirCodec.DirectoryResultReturned += new EventHandler<DirectoryEventArgs>(dirCodec_DirectoryResultReturned);
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void dirCodec_DirectoryResultReturned(object sender, DirectoryEventArgs e)
		{
			var dir = e.Directory;
			PostStatusMessage(new
			{
				currentDirectory = e.Directory
			});
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void codec_IsReadyChange(object sender, EventArgs e)
		{
			PostStatusMessage(new
			{
				isReady = true
			});
		}

		/// <summary>
		/// Called from base's RegisterWithAppServer method
		/// </summary>
		/// <param name="appServerController"></param>
		protected override void CustomRegisterWithAppServer(CotijaSystemController appServerController)
		{
			appServerController.AddAction("/device/videoCodec/isReady", new Action(SendIsReady));
			appServerController.AddAction("/device/videoCodec/fullStatus", new Action(SendVtcFullMessageObject));
			appServerController.AddAction("/device/videoCodec/dial", new Action<string>(s => Codec.Dial(s)));
			appServerController.AddAction("/device/videoCodec/endCallById", new Action<string>(s =>
			{
				var call = GetCallWithId(s);
				if (call != null)
					Codec.EndCall(call);
			}));
			appServerController.AddAction(MessagePath + "/endAllCalls", new Action(Codec.EndAllCalls));
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
			appServerController.AddAction(MessagePath + "/directoryRoot", new Action(GetDirectoryRoot));			
			appServerController.AddAction(MessagePath + "/directoryById", new Action<string>(s => GetDirectory(s)));
			appServerController.AddAction(MessagePath + "/directorySearch", new Action<string>(s => DirectorySearch(s)));
			appServerController.AddAction(MessagePath + "/privacyModeOn", new Action(Codec.PrivacyModeOn));
			appServerController.AddAction(MessagePath + "/privacyModeOff", new Action(Codec.PrivacyModeOff));
			appServerController.AddAction(MessagePath + "/privacyModeToggle", new Action(Codec.PrivacyModeToggle));
			appServerController.AddAction(MessagePath + "/sharingStart", new Action(Codec.StartSharing));
			appServerController.AddAction(MessagePath + "/sharingStop", new Action(Codec.StopSharing));
			appServerController.AddAction(MessagePath + "/standbyOn", new Action(Codec.StandbyActivate));
			appServerController.AddAction(MessagePath + "/standbyOff", new Action(Codec.StandbyDeactivate));
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
		/// 
		/// </summary>
		/// <param name="s"></param>
		void DirectorySearch(string s)
		{
			var dirCodec = Codec as IHasDirectory;
			if (dirCodec != null)
			{
				dirCodec.SearchDirectory(s);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		void GetDirectory(string id)
		{
			var dirCodec = Codec as IHasDirectory;
			if(dirCodec == null)
			{
				return;
			}
			dirCodec.GetDirectoryFolderContents(id);
		}

		/// <summary>
		/// 
		/// </summary>
		void GetDirectoryRoot()
		{
			var dirCodec = Codec as IHasDirectory;
			if (dirCodec == null)
			{
				// do something else?
				return;
			}
			if (!dirCodec.PhonebookSyncState.InitialSyncComplete)
			{
				PostStatusMessage(new
				{
					initialSyncComplete = false
				});
				return;
			}

			PostStatusMessage(new
			{
				currentDirectory = dirCodec.DirectoryRoot
			});
		}

		/// <summary>
		/// Handler for codec changes
		/// </summary>
		void codec_CallStatusChange(object sender, CodecCallStatusItemChangeEventArgs e)
		{
			SendVtcFullMessageObject();
		}

		/// <summary>
		/// 
		/// </summary>
		void SendIsReady()
		{
			PostStatusMessage(new
			{
				isReady = Codec.IsReady
			});
		}

		/// <summary>
		/// Helper method to build call status for vtc
		/// </summary>
		/// <returns></returns>
		void SendVtcFullMessageObject()
		{
			if (!Codec.IsReady)
			{
				return;
			}

			var info = Codec.CodecInfo;
			PostStatusMessage(new
			{
				isInCall = Codec.IsInCall,
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
					sipPhoneNumber = info.SipPhoneNumber,
					sipURI = info.SipUri
				},
				showSelfViewByDefault = Codec.ShowSelfViewByDefault,
				hasDirectory = Codec is IHasDirectory
			});
		}
	}
}