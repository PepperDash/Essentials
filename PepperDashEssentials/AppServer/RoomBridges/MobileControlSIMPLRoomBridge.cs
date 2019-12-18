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
using PepperDash.Essentials.AppServer;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Room.Config;


namespace PepperDash.Essentials.Room.MobileControl
{
	public class MobileControlSIMPLRoomBridge : MobileControlBridgeBase, IDelayedConfiguration
	{
		public class BoolJoin
		{

            /// <summary>
            /// 1
            /// </summary>
            public const uint ConfigIsInEssentials = 100;

			/// <summary>
			/// 301
			/// </summary>
			public const uint RoomIsOn = 301;

			/// <summary>
			/// 12
			/// </summary>
			public const uint PrivacyMute = 12;

			/// <summary>
			/// 41
			/// </summary>
			public const uint PromptForCode = 41;
			/// <summary>
			/// 42
			/// </summary>
			public const uint ClientJoined = 42;
			/// <summary>
			/// 51
			/// </summary>
			public const uint ActivityShare = 51;
			/// <summary>
			/// 52
			/// </summary>
			public const uint ActivityPhoneCall = 52;
			/// <summary>
			/// 53
			/// </summary>
			public const uint ActivityVideoCall = 53;

			/// <summary>
			/// 1
			/// </summary>
			public const uint MasterVolumeIsMuted = 1;
			/// <summary>
			/// 1
			/// </summary>
			public const uint MasterVolumeMuteToggle = 1;
			/// <summary>
			/// 1
			/// </summary>
			public const uint VolumeMutesJoinStart = 1;
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
			public const uint SourceHasChanged = 71;

			/// <summary>
			/// 261 - The start of the range of speed dial visibles
			/// </summary>
			public const uint SpeedDialVisibleStartJoin = 261;
			/// <summary>
			/// 501
			/// </summary>
			public const uint ConfigIsReady = 501;
			/// <summary>
			/// 502
			/// </summary>
			public const uint HideVideoConfRecents = 502;
			/// <summary>
			/// 503
			/// </summary>
			public const uint ShowCameraWhenNotInCall = 503;
			/// <summary>
			/// 504
			/// </summary>
			public const uint UseSourceEnabled = 504;
			/// <summary>
			/// 601
			/// </summary>
			public const uint SourceShareDisableJoinStart = 601;
			/// <summary>
			/// 621
			/// </summary>
			public const uint SourceIsEnabledJoinStart = 621;
		}

		public class UshortJoin
		{
			/// <summary>
			/// 1
			/// </summary>
			public const uint MasterVolumeLevel = 1;
			/// <summary>
			/// 1
			/// </summary>
			public const uint VolumeSlidersJoinStart = 1;
			/// <summary>
			/// 61
			/// </summary>
			public const uint ShutdownPromptDuration = 61;
			/// <summary>
			/// 101
			/// </summary>
			public const uint NumberOfAuxFaders = 101;
		}

		public class StringJoin
		{
			/// <summary>
			/// 1
			/// </summary>
			public const uint VolumeSliderNamesJoinStart = 1;
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
			/// <summary>
			/// 601
			/// </summary>
			public const uint SourceNameJoinStart = 601;
			/// <summary>
			/// 621
			/// </summary>
			public const uint SourceIconJoinStart = 621;
			/// <summary>
			/// 641
			/// </summary>
			public const uint SourceKeyJoinStart = 641;
			/// <summary>
			/// 661
			/// </summary>
			public const uint SourceTypeJoinStart = 661;
			/// <summary>
			/// 761
			/// </summary>
			public const uint CameraNearNameStart = 761;
			/// <summary>
			/// 770 - presence of this name on the input will cause the camera to be added
			/// </summary>
			public const uint CameraFarName = 770;
		}

		/// <summary>
		/// Fires when config is ready to go
		/// </summary>
		public event EventHandler<EventArgs> ConfigurationIsReady;

		public ThreeSeriesTcpIpEthernetIntersystemCommunications EISC { get; private set; }

        public MobileControlSIMPLRoomJoinMap JoinMap { get; private set; }

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

		MobileControlDdvc01DeviceBridge SourceBridge;

		SIMPLAtcMessenger AtcMessenger;
		SIMPLVtcMessenger VtcMessenger;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="name"></param>
		/// <param name="ipId"></param>
		public MobileControlSIMPLRoomBridge(string key, string name, uint ipId)
			: base(key, name)
		{
			try
			{
				EISC = new ThreeSeriesTcpIpEthernetIntersystemCommunications(ipId, "127.0.0.2", Global.ControlSystem);
				var reg = EISC.Register();
				if (reg != Crestron.SimplSharpPro.eDeviceRegistrationUnRegistrationResponse.Success)
					Debug.Console(0, this, "Cannot connect EISC at IPID {0}: \r{1}", ipId, reg);

                JoinMap = new MobileControlSIMPLRoomJoinMap();

                // TODO: Possibly set up alternate constructor or take in joinMapKey and joinStart properties in constructor
                

                JoinMap.OffsetJoinNumbers(1);

				SourceBridge = new MobileControlDdvc01DeviceBridge(key + "-sourceBridge", "DDVC01 source bridge", EISC);
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

            var atcKey = string.Format("atc-{0}-{1}", this.Key, Parent.Key);
			AtcMessenger = new SIMPLAtcMessenger(atcKey, EISC, "/device/audioCodec");
			AtcMessenger.RegisterWithAppServer(Parent);

            var vtcKey = string.Format("atc-{0}-{1}", this.Key, Parent.Key);
			VtcMessenger = new SIMPLVtcMessenger(vtcKey, EISC, "/device/videoCodec");
			VtcMessenger.RegisterWithAppServer(Parent);

			EISC.SigChange += EISC_SigChange;
			EISC.OnlineStatusChange += (o, a) =>
			{
				Debug.Console(1, this, "DDVC EISC online={0}. Config is ready={1}. Use Essentials Config={2}",
                    a.DeviceOnLine, EISC.BooleanOutput[BoolJoin.ConfigIsReady].BoolValue, EISC.BooleanOutput[BoolJoin.ConfigIsInEssentials].BoolValue);

				if (a.DeviceOnLine && EISC.BooleanOutput[BoolJoin.ConfigIsReady].BoolValue)
					LoadConfigValues();

                if (a.DeviceOnLine && EISC.BooleanOutput[BoolJoin.ConfigIsInEssentials].BoolValue)
                    UseEssentialsConfig();
			};
			// load config if it's already there
			if (EISC.IsOnline && EISC.BooleanOutput[BoolJoin.ConfigIsReady].BoolValue) // || EISC.BooleanInput[BoolJoin.ConfigIsReady].BoolValue)
				LoadConfigValues();

            if (EISC.IsOnline && EISC.BooleanOutput[BoolJoin.ConfigIsInEssentials].BoolValue)
            {
                UseEssentialsConfig();
            }

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

        void UseEssentialsConfig()
        {
            ConfigIsLoaded = false;

            SetupDeviceMessengers();

            Debug.Console(0, this, "******* ESSENTIALS CONFIG: \r{0}", JsonConvert.SerializeObject(ConfigReader.ConfigObject, Formatting.Indented));

            var handler = ConfigurationIsReady;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }

            ConfigIsLoaded = true;
        }

		/// <summary>
		/// Setup the actions to take place on various incoming API calls
		/// </summary>
		void SetupFunctions()
		{
			Parent.AddAction(@"/room/room1/promptForCode", new Action(() => EISC.PulseBool(BoolJoin.PromptForCode)));
			Parent.AddAction(@"/room/room1/clientJoined", new Action(() => EISC.PulseBool(BoolJoin.ClientJoined)));

			Parent.AddAction(@"/room/room1/status", new Action(SendFullStatus));

			Parent.AddAction(@"/room/room1/source", new Action<SourceSelectMessageContent>(c =>
			{
				EISC.SetString(StringJoin.SelectedSourceKey, c.SourceListItem);
				EISC.PulseBool(BoolJoin.SourceHasChanged);
			}));

			Parent.AddAction(@"/room/room1/defaultsource", new Action(() => 
				EISC.PulseBool(BoolJoin.ActivityShare)));
			Parent.AddAction(@"/room/room1/activityPhone", new Action(() =>
				EISC.PulseBool(BoolJoin.ActivityPhoneCall)));
			Parent.AddAction(@"/room/room1/activityVideo", new Action(() =>
				EISC.PulseBool(BoolJoin.ActivityVideoCall)));

			Parent.AddAction(@"/room/room1/volumes/master/level", new Action<ushort>(u => 
				EISC.SetUshort(UshortJoin.MasterVolumeLevel, u)));
			Parent.AddAction(@"/room/room1/volumes/master/muteToggle", new Action(() => 
				EISC.PulseBool(BoolJoin.MasterVolumeIsMuted)));
			Parent.AddAction(@"/room/room1/volumes/master/privacyMuteToggle", new Action(() =>
				EISC.PulseBool(BoolJoin.PrivacyMute)));


			// /xyzxyz/volumes/master/muteToggle ---> BoolInput[1]

			for (uint i = 2; i <= 7; i++)
			{
				var index = i;
				Parent.AddAction(string.Format(@"/room/room1/volumes/level-{0}/level", index), new Action<ushort>(u =>
					EISC.SetUshort(index, u)));
				Parent.AddAction(string.Format(@"/room/room1/volumes/level-{0}/muteToggle", index), new Action(() =>
					EISC.PulseBool(index)));
			}

			Parent.AddAction(@"/room/room1/shutdownStart", new Action(() =>
				EISC.PulseBool(BoolJoin.ShutdownStart)));
			Parent.AddAction(@"/room/room1/shutdownEnd", new Action(() =>
				EISC.PulseBool(BoolJoin.ShutdownEnd)));
			Parent.AddAction(@"/room/room1/shutdownCancel", new Action(() =>
				EISC.PulseBool(BoolJoin.ShutdownCancel)));
		}




		/// <summary>
		/// 
		/// </summary>
		/// <param name="devKey"></param>
		void SetupSourceFunctions(string devKey)
		{
			SourceDeviceMapDictionary sourceJoinMap = new SourceDeviceMapDictionary();

			var prefix = string.Format("/device/{0}/", devKey);

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

			// map MasterVolumeIsMuted join -> status/volumes/master/muted
			// 

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
			EISC.SetBoolSigAction(BoolJoin.PrivacyMute, b => 
				PostStatusMessage(new
				{
					volumes = new 
					{
						master = new 
						{
							privacyMuted = b
						}
					}
				}));

			for (uint i = 2; i <= 7; i++)
			{
				var index = i; // local scope for lambdas
				EISC.SetUShortSigAction(index, u => // start at join 2
				{
					// need a dict in order to create the level-n property on auxFaders
					var dict = new Dictionary<string, object>();
					dict.Add("level-" + index, new { level = u });
					PostStatusMessage(new
					{
						volumes = new
						{
							auxFaders = dict,
						}
					});
				});
				EISC.SetBoolSigAction(index, b =>
				{
					// need a dict in order to create the level-n property on auxFaders
					var dict = new Dictionary<string, object>();
					dict.Add("level-" + index, new { muted = b });
					PostStatusMessage(new
					{
						volumes = new
						{
							auxFaders = dict,
						}
					});
				});
			}

			EISC.SetUShortSigAction(UshortJoin.NumberOfAuxFaders, u => 
				PostStatusMessage(new {
					volumes = new {
						numberOfAuxFaders = u,
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

			// Activity modes
			EISC.SetSigTrueAction(BoolJoin.ActivityShare, () => UpdateActivity(1));
			EISC.SetSigTrueAction(BoolJoin.ActivityPhoneCall, () => UpdateActivity(2));
			EISC.SetSigTrueAction(BoolJoin.ActivityVideoCall, () => UpdateActivity(3));
		}


		/// <summary>
		/// Updates activity states
		/// </summary>
		void UpdateActivity(int mode)
		{
			PostStatusMessage(new
			{
				activityMode = mode,
			});
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
			//if (co.Rooms == null)
			// always start fresh in case simpl changed
			co.Rooms = new List<DeviceConfig>();
			var rm = new DeviceConfig();
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
			rm.Name = EISC.StringOutput[StringJoin.ConfigRoomName].StringValue;
			rm.Key = "room1";
			rm.Type = "ddvc01";

			DDVC01RoomPropertiesConfig rmProps;
			if (rm.Properties == null)
				rmProps = new DDVC01RoomPropertiesConfig();
			else
				rmProps = JsonConvert.DeserializeObject<DDVC01RoomPropertiesConfig>(rm.Properties.ToString());

			rmProps.Help = new EssentialsHelpPropertiesConfig();
			rmProps.Help.CallButtonText = EISC.StringOutput[StringJoin.ConfigHelpNumber].StringValue;
			rmProps.Help.Message = EISC.StringOutput[StringJoin.ConfigHelpMessage].StringValue;

			rmProps.Environment = new EssentialsEnvironmentPropertiesConfig(); // enabled defaults to false

			rmProps.RoomPhoneNumber = EISC.StringOutput[StringJoin.ConfigRoomPhoneNumber].StringValue;
			rmProps.RoomURI = EISC.StringOutput[StringJoin.ConfigRoomURI].StringValue;
			rmProps.SpeedDials = new List<DDVC01SpeedDial>();

			// This MAY need a check 
			rmProps.AudioCodecKey = "audioCodec";
			rmProps.VideoCodecKey = "videoCodec";

			// volume control names
			var volCount = EISC.UShortOutput[UshortJoin.NumberOfAuxFaders].UShortValue;

            //// use Volumes object or?
            //rmProps.VolumeSliderNames = new List<string>();
            //for(uint i = 701; i <= 700 + volCount; i++)
            //{
            //    rmProps.VolumeSliderNames.Add(EISC.StringInput[i].StringValue);
            //}

			// There should be Mobile Control devices in here, I think...
			if(co.Devices == null)
				co.Devices = new List<DeviceConfig>();

			// clear out previous DDVC devices
			co.Devices.RemoveAll(d => 
				d.Key.StartsWith("source-", StringComparison.OrdinalIgnoreCase)
				|| d.Key.Equals("audioCodec", StringComparison.OrdinalIgnoreCase) 
				|| d.Key.Equals("videoCodec", StringComparison.OrdinalIgnoreCase));
			
			rmProps.SourceListKey = "default";
			rm.Properties = JToken.FromObject(rmProps);

			// Source list! This might be brutal!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

			var groupMap = GetSourceGroupDictionary();

			co.SourceLists = new Dictionary<string,Dictionary<string,SourceListItem>>();
			var newSl = new Dictionary<string, SourceListItem>();
			// add "none" source if VTC present

			if (!string.IsNullOrEmpty(rmProps.VideoCodecKey))
			{
				var codecOsd = new SourceListItem()
				{
					Name = "None",
					IncludeInSourceList = true,
					Order = 1,
					Type = eSourceListItemType.Route,
					SourceKey = ""
				};
				newSl.Add("Source-None", codecOsd);
			}
			// add sources...
			for (uint i = 0; i <= 19; i++)
			{
				var name = EISC.StringOutput[StringJoin.SourceNameJoinStart + i].StringValue;
				if (EISC.BooleanOutput[BoolJoin.UseSourceEnabled].BoolValue
					&& !EISC.BooleanOutput[BoolJoin.SourceIsEnabledJoinStart + i].BoolValue)
				{
					continue;
				}		
				else if(!EISC.BooleanOutput[BoolJoin.UseSourceEnabled].BoolValue && string.IsNullOrEmpty(name))
					break;
				var icon = EISC.StringOutput[StringJoin.SourceIconJoinStart + i].StringValue;
				var key = EISC.StringOutput[StringJoin.SourceKeyJoinStart + i].StringValue;
				var type = EISC.StringOutput[StringJoin.SourceTypeJoinStart + i].StringValue;
				var disableShare = EISC.BooleanOutput[BoolJoin.SourceShareDisableJoinStart + i].BoolValue;

				Debug.Console(0, this, "Adding source {0} '{1}'", key, name);
				var newSLI = new SourceListItem{
					Icon = icon,
					Name = name,
					Order = (int)i + 10,
					SourceKey = key,
					Type = eSourceListItemType.Route,
					DisableCodecSharing = disableShare,
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

				if (group.ToLower().StartsWith("settopbox")) // Add others here as needed
				{
					SetupSourceFunctions(key);
				}
			}

			co.SourceLists.Add("default", newSl);

			// Build "audioCodec" config if we need
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

			// Build Video codec config
			if (!string.IsNullOrEmpty(rmProps.VideoCodecKey))
			{
				// No favorites, for now?
				var favs = new List<PepperDash.Essentials.Devices.Common.Codec.CodecActiveCallItem>();

				// cameras
				var camsProps = new List<object>();
				for (uint i = 0; i < 9; i++)
				{
					var name = EISC.GetString(i + StringJoin.CameraNearNameStart);
					if (!string.IsNullOrEmpty(name))
					{
						camsProps.Add(new
						{
							name = name,
							selector = "camera" + (i + 1),
						});
					}
				}
				var farName = EISC.GetString(StringJoin.CameraFarName);
				if (!string.IsNullOrEmpty(farName))
				{
					camsProps.Add(new
					{
						name = farName,
						selector = "cameraFar",
					});
				}

				var props = new
				{
					favorites = favs,
					cameras = camsProps,
				};
				var str = "videoCodec";
				var conf = new DeviceConfig()
				{
					Group = str,
					Key = str,
					Name = str,
					Type = str,
					Properties = JToken.FromObject(props)
				};
				co.Devices.Add(conf);
			}

            SetupDeviceMessengers();

			Debug.Console(0, this, "******* CONFIG FROM DDVC: \r{0}", JsonConvert.SerializeObject(ConfigReader.ConfigObject, Formatting.Indented));

			var handler = ConfigurationIsReady;
			if (handler != null)
			{
				handler(this, new EventArgs());
			}

			ConfigIsLoaded = true;
		}

        /// <summary>
        /// Iterates device config and adds messengers as neede for each device type
        /// </summary>
        void SetupDeviceMessengers()
        {
            try
            {
                foreach (var device in ConfigReader.ConfigObject.Devices)
                {
                    if (device.Group.Equals("simplmessenger"))
                    {
                        var props = JsonConvert.DeserializeObject<SimplMessengerPropertiesConfig>(device.Properties.ToString());

                        var messengerKey = string.Format("device-{0}-{1}", this.Key, Parent.Key);

                        if (DeviceManager.GetDeviceForKey(messengerKey) != null)
                        {
                            Debug.Console(2, this, "Messenger with key: {0} already exists. Skipping...", messengerKey);
                            continue;
                        }

                        var dev = ConfigReader.ConfigObject.GetDeviceForKey(props.DeviceKey);

                        if (dev == null)
                        {
                            Debug.Console(1, this, "Unable to find device config for key: '{0}'", props.DeviceKey);
                            continue;
                        }

                        var type = device.Type.ToLower();
                        MessengerBase messenger = null;

                        if (type.Equals("simplcameramessenger"))
                        {
                            Debug.Console(2, this, "Adding SIMPLCameraMessenger for: '{0}'", props.DeviceKey);
                            messenger = new SIMPLCameraMessenger(messengerKey, EISC, "/device/" + props.DeviceKey, props.JoinStart);
                        }
                        else if (type.Equals("simplroutemessenger"))
                        {
                            Debug.Console(2, this, "Adding SIMPLRouteMessenger for: '{0}'", props.DeviceKey);
                            messenger = new SIMPLRouteMessenger(messengerKey, EISC, "/device/" + props.DeviceKey, props.JoinStart);
                        }

                        if (messenger != null)
                        {
                            DeviceManager.AddDevice(messenger);
                            messenger.RegisterWithAppServer(Parent);
                        }
                        else
                        {
                            Debug.Console(2, this, "Unable to add messenger for device: '{0}' of type: '{1}'", props.DeviceKey, type);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Console(2, this, "Error Setting up Device Managers: {0}", e);
            }
        }

		/// <summary>
		/// 
		/// </summary>
		void SendFullStatus()
		{
			if (ConfigIsLoaded)
			{
                var count = EISC.UShortOutput[UshortJoin.NumberOfAuxFaders].UShortValue;

                Debug.Console(1, this, "The Fader Count is : {0}", count);

                // build volumes object, serialize and put in content of method below

                // Create auxFaders
				var auxFaderDict = new Dictionary<string, Volume>();
				for (uint i = 2; i <= count; i++)
				{
					auxFaderDict.Add("level-" + i,
						new Volume("level-" + i,
							EISC.UShortOutput[i].UShortValue,
							EISC.BooleanOutput[i].BoolValue,
							EISC.StringOutput[i].StringValue,
							true,
							"someting.png"));
				}

                var volumes = new Volumes();

                volumes.Master = new Volume("master", 
                                EISC.UShortOutput[UshortJoin.MasterVolumeLevel].UShortValue,
                                EISC.BooleanOutput[BoolJoin.MasterVolumeIsMuted].BoolValue,
                  				EISC.StringOutput[1].StringValue,
				                true,
				                "something.png");
				volumes.Master.HasPrivacyMute = true;
				volumes.Master.PrivacyMuted = EISC.BooleanOutput[BoolJoin.PrivacyMute].BoolValue;

                volumes.AuxFaders = auxFaderDict;
				volumes.NumberOfAuxFaders = EISC.UShortInput[UshortJoin.NumberOfAuxFaders].UShortValue;

                PostStatusMessage(new
                    {
						activityMode = GetActivityMode(),
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
		/// Returns the activity mode int
		/// </summary>
		/// <returns></returns>
		int GetActivityMode()
		{
			if (EISC.BooleanOutput[BoolJoin.ActivityPhoneCall].BoolValue) return 2;
			else if (EISC.BooleanOutput[BoolJoin.ActivityShare].BoolValue) return 1;
			else if (EISC.BooleanOutput[BoolJoin.ActivityVideoCall].BoolValue) return 3;
			return 0;
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
            if (uo != null)
            {
                if (uo is Action<bool>)
                    (uo as Action<bool>)(args.Sig.BoolValue);
                else if (uo is Action<ushort>)
                    (uo as Action<ushort>)(args.Sig.UShortValue);
                else if (uo is Action<string>)
                    (uo as Action<string>)(args.Sig.StringValue);
            }
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
				{ "pc", "pc" },
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