using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharpPro.EthernetCommunication;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
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
			/// 301
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
			/// 1
			/// </summary>
			public const uint MasterVolumeIsMuted = 1;
			/// <summary>
			/// 1
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
			/// 72
			/// </summary>
			public const uint SourceHasChanged = 72;
			/// <summary>
			/// 261 - The start of the range of speed dial visibles
			/// </summary>
			public const uint SpeedDialVisibleStartJoin = 261;
			/// <summary>
			/// 501
			/// </summary>
			public const uint ConfigIsReady = 501;
		}

		public class UshortJoin
		{
			/// <summary>
			/// 1
			/// </summary>
			public const uint MasterVolumeLevel = 1;

			/// <summary>
			/// 61
			/// </summary>
			public const uint ShutdownPromptDuration = 61;
		}

		public class StringJoin
		{
			/// <summary>
			/// 71
			/// </summary>
			public const uint SelectedSourceKey = 71;

			/// <summary>
			/// 241
			/// </summary>
			public const uint SpeedDialNameStartJoin = 241;

			/// <summary>
			/// 251
			/// </summary>
			public const uint SpeedDialNumberStartJoin = 251;
			
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
			/// <summary>
			/// 401
			/// </summary>
			public const uint UserCodeToSystem = 401;
			/// <summary>
			/// 402
			/// </summary>
			public const uint ServerUrl = 402;
		}

		/// <summary>
		/// Fires when config is ready to go
		/// </summary>
		public event EventHandler<EventArgs> ConfigurationIsReady;

		public ThreeSeriesTcpIpEthernetIntersystemCommunications EISC { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public bool ConfigIsLoaded { get; private set; }

		public override string RoomName
		{
			get { 
				var name = EISC.StringOutput[StringJoin.ConfigRoomName].StringValue;
				return string.IsNullOrEmpty(name) ? "Not Loaded" : name;
			}
		}

		CotijaDdvc01DeviceBridge SourceBridge;

		Ddvc01AtcMessenger AtcMessenger;


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

				SourceBridge = new CotijaDdvc01DeviceBridge(key + "-sourceBridge", "DDVC01 source bridge", EISC);
				DeviceManager.AddDevice(SourceBridge);
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
			Debug.Console(0, this, "Final activation. Setting up actions and feedbacks");
			SetupFunctions();
			SetupFeedbacks();

			AtcMessenger = new Ddvc01AtcMessenger(EISC, "/device/audioCodec");
			AtcMessenger.RegisterWithAppServer(Parent);

			EISC.SigChange += EISC_SigChange;
			EISC.OnlineStatusChange += (o, a) =>
			{
				Debug.Console(1, this, "DDVC EISC online={0}. Config is ready={1}", a.DeviceOnLine, EISC.BooleanOutput[BoolJoin.ConfigIsReady].BoolValue);
				if (a.DeviceOnLine && EISC.BooleanOutput[BoolJoin.ConfigIsReady].BoolValue)
					LoadConfigValues();
			};
			// load config if it's already there
			if (EISC.IsOnline && EISC.BooleanOutput[BoolJoin.ConfigIsReady].BoolValue) // || EISC.BooleanInput[BoolJoin.ConfigIsReady].BoolValue)
				LoadConfigValues();


			CrestronConsole.AddNewConsoleCommand(s =>
			{
				for (uint i = 1; i < 1000; i++)
				{
					if (s.ToLower().Equals("b"))
					{
						CrestronConsole.ConsoleCommandResponse("D{0,6} {1} - ", i, EISC.BooleanOutput[i].BoolValue);
					}
					else if (s.ToLower().Equals("u"))
					{
						CrestronConsole.ConsoleCommandResponse("U{0,6} {1,8} - ", i, EISC.UShortOutput[i].UShortValue);
					}
					else if (s.ToLower().Equals("s"))
					{
						var val = EISC.StringOutput[i].StringValue;
						if(!string.IsNullOrEmpty(val))
							CrestronConsole.ConsoleCommandResponse("S{0,6} {1}\r", i, EISC.StringOutput[i].StringValue);
					}

				}
			}, "mobilebridgedump", "Dumps DDVC01 bridge EISC data b,u,s", ConsoleAccessLevelEnum.AccessOperator);

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

			Parent.AddAction(@"/room/room1/defaultsource", new Action(() => 
				EISC.PulseBool(BoolJoin.ActivitySharePress)));
			Parent.AddAction(@"/room/room1/activityVideo", new Action(() =>
				EISC.PulseBool(BoolJoin.ActivityVideoCallPress)));
			Parent.AddAction(@"/room/room1/activityPhone", new Action(() =>
				EISC.PulseBool(BoolJoin.ActivityPhoneCallPress)));


			Parent.AddAction(@"/room/room1/volumes/master/level", new Action<ushort>(u => 
				EISC.SetUshort(UshortJoin.MasterVolumeLevel, u)));
			Parent.AddAction(@"/room/room1/volumes/master/muteToggle", new Action(() => 
				EISC.PulseBool(BoolJoin.MasterVolumeIsMuted)));

			Parent.AddAction(@"/room/room1/shutdownStart", new Action(() =>
				EISC.PulseBool(BoolJoin.ShutdownStart)));
			Parent.AddAction(@"/room/room1/shutdownEnd", new Action(() =>
				EISC.PulseBool(BoolJoin.ShutdownEnd)));
			Parent.AddAction(@"/room/room1/shutdownCancel", new Action(() =>
				EISC.PulseBool(BoolJoin.ShutdownCancel)));


            // Source Device (Current Source)'

            SourceDeviceMapDictionary sourceJoinMap = new SourceDeviceMapDictionary();

            var prefix = @"/device/currentSource/";

            foreach (var item in sourceJoinMap)
            {
				var join = item.Value;
                Parent.AddAction(string.Format("{0}{1}", prefix, item.Key), new PressAndHoldAction(b => EISC.SetBool(join, b)));
            }
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
                    volumes = new
                    {
                        master = new
                        {
                            level = u
                        }
                    }
                }));

			EISC.SetBoolSigAction(BoolJoin.MasterVolumeIsMuted, b =>
                PostStatusMessage(new
                {
                    volumes = new
                    {
                        master = new
                        {
                            muted = b
                        }
                    }
                }));


			// shutdown things
			EISC.SetSigTrueAction(BoolJoin.ShutdownCancel, new Action(() =>
				PostMessage("/room/shutdown/", new
				{
					state = "wasCancelled"
				})));
			EISC.SetSigTrueAction(BoolJoin.ShutdownEnd, new Action(() =>
				PostMessage("/room/shutdown/", new
				{
					state = "hasFinished"
				})));
			EISC.SetSigTrueAction(BoolJoin.ShutdownStart, new Action(() =>
				PostMessage("/room/shutdown/", new
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

            co.Info.RuntimeInfo.AppName = Assembly.GetExecutingAssembly().GetName().Name;
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            co.Info.RuntimeInfo.AssemblyVersion = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);


			//Room
			if (co.Rooms == null)
				co.Rooms = new List<EssentialsRoomConfig>();
			var rm = new EssentialsRoomConfig();
            if (co.Rooms.Count == 0)
            {
                Debug.Console(0, this, "Adding room to config");
                co.Rooms.Add(rm);
            }
            else
            {
                Debug.Console(0, this, "Replacing Room[0] in config");
                co.Rooms[0] = rm;
            }
			rm.Name = EISC.StringOutput[501].StringValue;
			rm.Key = "room1";
			rm.Type = "ddvc01";

			DDVC01RoomPropertiesConfig rmProps;
			if (rm.Properties == null)
				rmProps = new DDVC01RoomPropertiesConfig();
			else
				rmProps = JsonConvert.DeserializeObject<DDVC01RoomPropertiesConfig>(rm.Properties.ToString());

			rmProps.Help = new EssentialsHelpPropertiesConfig();
			rmProps.Help.CallButtonText = EISC.StringOutput[503].StringValue;
			rmProps.Help.Message = EISC.StringOutput[502].StringValue;

			rmProps.Environment = new EssentialsEnvironmentPropertiesConfig(); // enabled defaults to false

			rmProps.RoomPhoneNumber = EISC.StringOutput[504].StringValue;
			rmProps.RoomURI = EISC.StringOutput[505].StringValue;
			rmProps.SpeedDials = new List<DDVC01SpeedDial>();
			// add speed dials as long as there are more - up to 4
			for (uint i = 512; i <= 519; i = i + 2)
			{
				var num = EISC.StringOutput[i].StringValue;
				if (string.IsNullOrEmpty(num))
					break;
				var name = EISC.StringOutput[i + 1].StringValue;
				rmProps.SpeedDials.Add(new DDVC01SpeedDial { Number = num, Name = name});
			}

			// This MAY need a check 
			rmProps.AudioCodecKey = "audioCodec";
			rmProps.VideoCodecKey = null; // "videoCodec";

			// volume control names
			var volCount = EISC.UShortOutput[701].UShortValue;

            //// use Volumes object or?
            //rmProps.VolumeSliderNames = new List<string>();
            //for(uint i = 701; i <= 700 + volCount; i++)
            //{
            //    rmProps.VolumeSliderNames.Add(EISC.StringInput[i].StringValue);
            //}

			// There should be cotija devices in here, I think...
			if(co.Devices == null)
				co.Devices = new List<DeviceConfig>();

			// clear out previous DDVC devices
			co.Devices.RemoveAll(d => d.Key.StartsWith("source-", StringComparison.OrdinalIgnoreCase));
			
			rmProps.SourceListKey = "default";
			rm.Properties = JToken.FromObject(rmProps);

			// Source list! This might be brutal!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

			var groupMap = GetSourceGroupDictionary();

			co.SourceLists = new Dictionary<string,Dictionary<string,SourceListItem>>();
			var newSl = new Dictionary<string, SourceListItem>();
			// add sources...
			for (uint i = 0; i<= 19; i++)
			{
				var name = EISC.StringOutput[601 + i].StringValue;
				if(string.IsNullOrEmpty(name))
					break;
				var icon = EISC.StringOutput[651 + i].StringValue;
				var key = EISC.StringOutput[671 + i].StringValue;

				var type = EISC.StringOutput[701 + i].StringValue;

				Debug.Console(0, this, "Adding source {0} '{1}'", key, name);
				var newSLI = new SourceListItem{
					Icon = icon,
					Name = name,
					Order = (int)i + 1,
					SourceKey = key,
					Type = eSourceListItemType.Route
				};
				newSl.Add(key, newSLI);
                
				string group = "genericsource";
				if (groupMap.ContainsKey(type))
				{
					group = groupMap[type];
				}
				
				// add dev to devices list
				var devConf = new DeviceConfig {
					Group = group,
					Key = key,
					Name = name,
					Type = type
				};
				co.Devices.Add(devConf);
			}

			co.SourceLists.Add("default", newSl);

			// build "audioCodec" config if we need
			if (!string.IsNullOrEmpty(rmProps.AudioCodecKey))
			{
				var acFavs = new List<PepperDash.Essentials.Devices.Common.Codec.CodecActiveCallItem>();
				for (uint i = 0; i < 4; i++)
				{
					if (!EISC.GetBool(BoolJoin.SpeedDialVisibleStartJoin + i))
					{
						break;
					}
					acFavs.Add(new PepperDash.Essentials.Devices.Common.Codec.CodecActiveCallItem()
					{
						Name = EISC.GetString(StringJoin.SpeedDialNameStartJoin + i),
						Number = EISC.GetString(StringJoin.SpeedDialNumberStartJoin + i),
						Type = PepperDash.Essentials.Devices.Common.Codec.eCodecCallType.Audio
					});
				}

				var acProps = new
				{
					favorites = acFavs
				};

				var acStr = "audioCodec";
				var acConf = new DeviceConfig()
				{
					Group = acStr,
					Key = acStr,
					Name = acStr,
					Type = acStr,
					Properties = JToken.FromObject(acProps)
				};
				co.Devices.Add(acConf);
			}	

			Debug.Console(0, this, "******* CONFIG FROM DDVC: \r{0}", JsonConvert.SerializeObject(ConfigReader.ConfigObject, Formatting.Indented));

			var handler = ConfigurationIsReady;
			if (handler != null)
			{
				handler(this, new EventArgs());
			}

			ConfigIsLoaded = true;
		}

		/// <summary>
		/// 
		/// </summary>
		void SendFullStatus()
		{
			if (ConfigIsLoaded)
			{
                var count = EISC.UShortOutput[801].UShortValue;

                Debug.Console(1, this, "The Fader Count is : {0}", count);

                // build volumes object, serialize and put in content of method below

                var auxFaders = new List<Volume>();

                // Create auxFaders
                for (uint i = 2; i <= count; i++)
                {
                    auxFaders.Add(
                        new Volume(string.Format("level-{0}", i), 
                            EISC.UShortOutput[i].UShortValue,
                            EISC.BooleanOutput[i].BoolValue,
                            EISC.StringOutput[800 + i].StringValue,
                            true,
                            "someting.png"));
                }

                var volumes = new Volumes();

                volumes.Master = new Volume("master", 
                                EISC.UShortOutput[UshortJoin.MasterVolumeLevel].UShortValue,
                                EISC.BooleanOutput[BoolJoin.MasterVolumeIsMuted].BoolValue,
                  				EISC.StringOutput[801].StringValue,
				                true,
				                "something.png");

                volumes.AuxFaders = auxFaders;

                PostStatusMessage(new
                    {
                        isOn = EISC.BooleanOutput[BoolJoin.RoomIsOn].BoolValue,
                        selectedSourceKey = EISC.StringOutput[StringJoin.SelectedSourceKey].StringValue,
                        volumes = volumes
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
			Parent.SendMessageToServer(JObject.FromObject(new
				{
					type = "/room/status/",
					content = contentObject
				}));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="messageType"></param>
		/// <param name="contentObject"></param>
		void PostMessage(string messageType, object contentObject)
		{
			Parent.SendMessageToServer(JObject.FromObject(new
			{
				type = messageType,
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

		/// <summary>
		/// Returns the mapping of types to groups, for setting up devices.
		/// </summary>
		/// <returns></returns>
		Dictionary<string, string> GetSourceGroupDictionary()
		{
			//type, group
			var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				{ "laptop", "pc" },
				{ "wireless", "genericsource" },
				{ "iptv", "settopbox" }

			};
			return d;
		}

		/// <summary>
		/// updates the usercode from server
		/// </summary>
		protected override void UserCodeChange()
		{
			Debug.Console(1, this, "Server user code changed: {0}", UserCode);
			EISC.StringInput[StringJoin.UserCodeToSystem].StringValue = UserCode;
			EISC.StringInput[StringJoin.ServerUrl].StringValue = Parent.Config.ClientAppUrl;
		}
	}
}