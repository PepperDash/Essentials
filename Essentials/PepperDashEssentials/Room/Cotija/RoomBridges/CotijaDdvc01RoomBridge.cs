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
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Room.Config;


namespace PepperDash.Essentials.Room.Cotija
{
	public class CotijaDdvc01RoomBridge : CotijaBridgeBase, IDelayedConfiguration
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

		/// <summary>
		/// Fires when the config is ready, to be used by the controller class to forward config to server
		/// </summary>
		public event EventHandler<EventArgs> ConfigurationIsReady;

		public ThreeSeriesTcpIpEthernetIntersystemCommunications EISC { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public bool ConfigIsLoaded { get; private set; }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="name"></param>
		/// <param name="ipId"></param>
		public CotijaDdvc01RoomBridge(string key, string name, uint ipId)
			: base(key, name)
		{
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
		/// Finish wiring up everything after all devices are created. The base class will hunt down the related
		/// parent controller and link them up.
		/// </summary>
		/// <returns></returns>
		public override bool CustomActivate()
		{
			SetupFunctions();
			SetupFeedbacks();
			EISC.SigChange += EISC_SigChange;
			EISC.OnlineStatusChange += (o, a) =>
			{
				if (a.DeviceOnLine)
					LoadConfigValues();
			};
			// load config if it's already there
			if (EISC.IsOnline) // || EISC.BooleanInput[BoolJoin.ConfigIsReady].BoolValue)
				LoadConfigValues();
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

			Debug.Console(1, this, "Loading configuration from DDVC01 EISC bridge");
			ConfigIsLoaded = false;

			var co = ConfigReader.ConfigObject;

			//Room
			if (co.Rooms == null)
				co.Rooms = new List<EssentialsRoomConfig>();
			if (co.Rooms.Count == 0)
				co.Rooms.Add(new EssentialsRoomConfig());
			var rm = co.Rooms[0];
			rm.Name = EISC.StringInput[501].StringValue;
			rm.Key = "room1";
			rm.Type = "ddvc01";

			DDVC01RoomPropertiesConfig rmProps;
			if (rm.Properties == null)
				rmProps = new DDVC01RoomPropertiesConfig();
			else
				rmProps = JsonConvert.DeserializeObject<DDVC01RoomPropertiesConfig>(rm.Properties.ToString());
			
			rmProps.Help = new EssentialsHelpPropertiesConfig();
			rmProps.Help.Message = EISC.StringInput[502].StringValue;
			rmProps.Help.CallButtonText = EISC.StringInput[503].StringValue;
			rmProps.RoomPhoneNumber = EISC.StringInput[504].StringValue;
			rmProps.RoomURI = EISC.StringInput[505].StringValue;
			rmProps.SpeedDials = new List<DDVC01SpeedDial>();
			// add speed dials as long as there are more - up to 4
			for (uint i = 512; i <= 519; i = i + 2)
			{
				var num = EISC.StringInput[i].StringValue;
				if (string.IsNullOrEmpty(num))
					break;
				var name = EISC.StringInput[i + 1].StringValue;
				rmProps.SpeedDials.Add(new DDVC01SpeedDial { Number = num, Name = name});
			}
			// volume control names
			var volCount = EISC.UShortInput[701].UShortValue;
			rmProps.VolumeSliderNames = new List<string>();
			for(uint i = 701; i <= 700 + volCount; i++)
			{
				rmProps.VolumeSliderNames.Add(EISC.StringInput[i].StringValue);
			}

			// There should be cotija devices in here, I think...
			if(co.Devices == null)
				co.Devices = new List<DeviceConfig>();
			
			// Source list! This might be brutal!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			rmProps.SourceListKey = "default";
			co.SourceLists = new Dictionary<string,Dictionary<string,SourceListItem>>();
			var newSl = new Dictionary<string, SourceListItem>();
			// add sources...
			for (uint i = 0; i<= 19; i++)
			{
				var name = EISC.StringInput[601 + i].StringValue;
				if(string.IsNullOrEmpty(name))
					break;
				var icon = EISC.StringInput[651 + i].StringValue;
				var key = EISC.StringInput[671 + i].StringValue;
				var type = EISC.StringInput[701 + i].StringValue;
				var newSLI = new SourceListItem{
					Icon = icon,
					Name = name,
					Order = (int)i + 1,
					SourceKey = key,
				};
				
				// add dev to devices list
				var devConf = new DeviceConfig {
					Group = "ddvc01",
					Key = key,
					Name = name,
					Type = type
				};
				co.Devices.Add(devConf);
			}

			co.SourceLists.Add("default", newSl);

			Debug.Console(0, this, "******* CONFIG FROM DDVC: \r{0}", JsonConvert.SerializeObject(ConfigReader.ConfigObject, Formatting.Indented));

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