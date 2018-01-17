using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.EthernetCommunication;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Room.Cotija
{
	public class CotijaDdvc01RoomBridge : Device
	{
		public class BoolJoin
		{
			/// <summary>
			/// 2
			/// </summary>
			public const uint RoomIsOn = 301;

			/// <summary>
			/// 51
			/// </summary>
			public const uint ActivitySharePress = 51;
			/// <summary>
			/// 52
			/// </summary>
			public const uint ActivityPhoneCallPress = 52;
			/// <summary>
			/// 53
			/// </summary>
			public const uint ActivityVideoCallPress = 53;

			/// <summary>
			/// 4
			/// </summary>
			public const uint MasterVolumeIsMuted = 1;
			/// <summary>
			/// 4
			/// </summary>
			public const uint MasterVolumeMuteToggle = 1;

			/// <summary>
			/// 61
			/// </summary>
			public const uint ShutdownCancel = 61;
			/// <summary>
			/// 62
			/// </summary>
			public const uint ShutdownEnd = 62;			
			/// <summary>
			/// 63
			/// </summary>
			public const uint ShutdownStart = 63;



			/// <summary>
			/// 71
			/// </summary>
			public const uint SourceHasChanged = 72;
			/// <summary>
			/// 501
			/// </summary>
			public const uint ConfigIsReady = 501;
		}

		public class UshortJoin
		{
			/// <summary>
			/// 
			/// </summary>
			public const uint MasterVolumeLevel = 1;

			public const uint ShutdownPromptDuration = 61;
		}

		public class StringJoin
		{
			/// <summary>
			/// 
			/// </summary>
			public const uint SelectedSourceKey = 3;
			
			/// <summary>
			/// 501
			/// </summary>
			public const uint ConfigRoomName = 501;
			/// <summary>
			/// 502
			/// </summary>
			public const uint ConfigHelpMessage = 502;
			/// <summary>
			/// 503
			/// </summary>
			public const uint ConfigHelpNumber = 503;
			/// <summary>
			/// 504
			/// </summary>
			public const uint ConfigRoomPhoneNumber = 504;
			/// <summary>
			/// 505
			/// </summary>
			public const uint ConfigRoomURI = 505;
		}


		public ThreeSeriesTcpIpEthernetIntersystemCommunications EISC { get; private set; }

		CotijaSystemController Parent;

		public bool ConfigIsLoaded { get; private set; }

		public CotijaDdvc01RoomBridge(string key, string name, uint ipId)
			: base(key, name)
		{
			Key = key;
			try
			{
				EISC = new ThreeSeriesTcpIpEthernetIntersystemCommunications(ipId, "127.0.0.2", Global.ControlSystem);
				var reg = EISC.Register();
				if (reg != Crestron.SimplSharpPro.eDeviceRegistrationUnRegistrationResponse.Success)
					Debug.Console(0, this, "Cannot connect EISC at IPID {0}: \r{1}", ipId, reg);
			}
			catch (Exception)
			{
				throw;
			}
		}

		/// <summary>
		/// Finish wiring up everything after all devices are created
		/// </summary>
		/// <returns></returns>
		public override bool CustomActivate()
		{
			
			Parent = DeviceManager.AllDevices.FirstOrDefault(d => d is CotijaSystemController) as CotijaSystemController;
			if (Parent == null)
			{
				Debug.Console(0, this, "ERROR: Cannot build CotijaDdvc01RoomBridge. System controller not present");
				return false;
			}

			SetupFunctions();
			SetupFeedbacks();
			EISC.SigChange += EISC_SigChange;
			return base.CustomActivate();
		}


		/// <summary>
		/// Setup the actions to take place on various incoming API calls
		/// </summary>
		void SetupFunctions()
		{
			Parent.AddAction(@"/room/room1/status", new Action(SendFullStatus));

			Parent.AddAction(@"/room/room1/source", new Action<SourceSelectMessageContent>(c =>
			{
				EISC.SetString(StringJoin.SelectedSourceKey, c.SourceListItem);
				EISC.PulseBool(BoolJoin.SourceHasChanged);
			}));

			Parent.AddAction(@"/room/room1/activityshare", new Action(() => 
				EISC.PulseBool(BoolJoin.ActivitySharePress)));

			Parent.AddAction(@"/room/room1/masterVolumeLevel", new Action<ushort>(u => 
				EISC.SetUshort(UshortJoin.MasterVolumeLevel, u)));
			Parent.AddAction(@"/room/room1/masterVolumeMuteToggle", new Action(() => 
				EISC.PulseBool(BoolJoin.MasterVolumeIsMuted)));

			Parent.AddAction(@"/room/room1/shutdownStart", new Action(() =>
				EISC.PulseBool(BoolJoin.ShutdownStart)));
			Parent.AddAction(@"/room/room1/shutdownEnd", new Action(() =>
				EISC.PulseBool(BoolJoin.ShutdownEnd)));
			Parent.AddAction(@"/room/room1/shutdownCancel", new Action(() =>
				EISC.PulseBool(BoolJoin.ShutdownCancel)));
		}

		/// <summary>
		/// Links feedbacks to whatever is gonna happen!
		/// </summary>
		void SetupFeedbacks()
		{
			// Power 
			EISC.SetBoolSigAction(BoolJoin.RoomIsOn, b =>
				PostStatusMessage(new
					{
						isOn = b
					}));

			// Source change things
			EISC.SetSigTrueAction(BoolJoin.SourceHasChanged, () =>
				PostStatusMessage(new
					{
						selectedSourceKey = EISC.StringOutput[StringJoin.SelectedSourceKey].StringValue
					}));

			// Volume things
			EISC.SetUShortSigAction(UshortJoin.MasterVolumeLevel, u =>
				PostStatusMessage(new
					{
						masterVolumeLevel = u
					}));

			EISC.SetBoolSigAction(BoolJoin.MasterVolumeIsMuted, b =>
				PostStatusMessage(new
					{
						masterVolumeMuteState = b
					}));

			// shutdown things
			EISC.SetSigTrueAction(BoolJoin.ShutdownCancel, new Action(() =>
				PostStatusMessage(new
				{
					state = "wasCancelled"
				})));
			EISC.SetSigTrueAction(BoolJoin.ShutdownEnd, new Action(() =>
				PostStatusMessage(new
				{
					state = "hasFinished"
				})));
			EISC.SetSigTrueAction(BoolJoin.ShutdownStart, new Action(() =>
				PostStatusMessage(new
				{
					state = "hasStarted",
					duration = EISC.UShortOutput[UshortJoin.ShutdownPromptDuration].UShortValue
				})));

			// Config things
			EISC.SetSigTrueAction(BoolJoin.ConfigIsReady, LoadConfigValues);


		}

		/// <summary>
		/// Reads in config values when the Simpl program is ready
		/// </summary>
		void LoadConfigValues()
		{
			ConfigIsLoaded = false;
			ConfigIsLoaded = true;

			// send config changed status???
		}

		void SendFullStatus()
		{
			if (ConfigIsLoaded)
			{
				PostStatusMessage(new
					{
						isOn = EISC.BooleanOutput[BoolJoin.RoomIsOn].BoolValue,
						selectedSourceKey = EISC.StringOutput[StringJoin.SelectedSourceKey].StringValue,
						masterVolumeLevel = EISC.UShortOutput[UshortJoin.MasterVolumeLevel].UShortValue,
						masterVolumeMuteState = EISC.BooleanOutput[BoolJoin.MasterVolumeIsMuted].BoolValue
					});
			}
			else
			{
				PostStatusMessage(new
				{
					error = "systemNotReady"
				});
			}
		}



		/// <summary>
		/// Helper for posting status message
		/// </summary>
		/// <param name="contentObject">The contents of the content object</param>
		void PostStatusMessage(object contentObject)
		{
			Parent.PostToServer(JObject.FromObject(new
				{
					type = "/room/status/",
					content = contentObject
				}));
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="currentDevice"></param>
		/// <param name="args"></param>
		void EISC_SigChange(object currentDevice, Crestron.SimplSharpPro.SigEventArgs args)
		{
			if (Debug.Level >= 1)
				Debug.Console(1, this, "DDVC EISC change: {0} {1}={2}", args.Sig.Type, args.Sig.Number, args.Sig.StringValue);
			var uo = args.Sig.UserObject;
			if (uo is Action<bool>)
				(uo as Action<bool>)(args.Sig.BoolValue);
			else if (uo is Action<ushort>)
				(uo as Action<ushort>)(args.Sig.UShortValue);
			else if (uo is Action<string>)
				(uo as Action<string>)(args.Sig.StringValue);
		}
	}
}