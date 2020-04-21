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
				var name = EISC.StringOutput[JoinMap.ConfigRoomName.JoinNumber].StringValue;
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

                JoinMap = new MobileControlSIMPLRoomJoinMap(1);

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
                    a.DeviceOnLine, EISC.BooleanOutput[JoinMap.ConfigIsReady.JoinNumber].BoolValue, EISC.BooleanOutput[JoinMap.ConfigIsLocal.JoinNumber].BoolValue);

				if (a.DeviceOnLine && EISC.BooleanOutput[JoinMap.ConfigIsReady.JoinNumber].BoolValue)
					LoadConfigValues();

                if (a.DeviceOnLine && EISC.BooleanOutput[JoinMap.ConfigIsLocal.JoinNumber].BoolValue)
                    UseEssentialsConfig();
			};
			// load config if it's already there
			if (EISC.IsOnline && EISC.BooleanOutput[JoinMap.ConfigIsReady.JoinNumber].BoolValue) // || EISC.BooleanInput[JoinMap.ConfigIsReady].BoolValue)
				LoadConfigValues();

            if (EISC.IsOnline && EISC.BooleanOutput[JoinMap.ConfigIsLocal.JoinNumber].BoolValue)
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
			Parent.AddAction(@"/room/room1/promptForCode", new Action(() => EISC.PulseBool(JoinMap.PromptForCode.JoinNumber)));
			Parent.AddAction(@"/room/room1/clientJoined", new Action(() => EISC.PulseBool(JoinMap.ClientJoined.JoinNumber)));

			Parent.AddAction(@"/room/room1/status", new Action(SendFullStatus));

			Parent.AddAction(@"/room/room1/source", new Action<SourceSelectMessageContent>(c =>
			{
				EISC.SetString(JoinMap.CurrentSourceKey.JoinNumber, c.SourceListItem);
				EISC.PulseBool(JoinMap.SourceHasChanged.JoinNumber);
			}));

			Parent.AddAction(@"/room/room1/defaultsource", new Action(() => 
				EISC.PulseBool(JoinMap.ActivityShare.JoinNumber)));
			Parent.AddAction(@"/room/room1/activityPhone", new Action(() =>
				EISC.PulseBool(JoinMap.ActivityPhoneCall.JoinNumber)));
			Parent.AddAction(@"/room/room1/activityVideo", new Action(() =>
				EISC.PulseBool(JoinMap.ActivityVideoCall.JoinNumber)));

			Parent.AddAction(@"/room/room1/volumes/master/level", new Action<ushort>(u => 
				EISC.SetUshort(JoinMap.MasterVolume.JoinNumber, u)));
			Parent.AddAction(@"/room/room1/volumes/master/muteToggle", new Action(() => 
				EISC.PulseBool(JoinMap.MasterVolume.JoinNumber)));
			Parent.AddAction(@"/room/room1/volumes/master/privacyMuteToggle", new Action(() =>
				EISC.PulseBool(JoinMap.PrivacyMute.JoinNumber)));


			// /xyzxyz/volumes/master/muteToggle ---> BoolInput[1]

            var volumeStart = JoinMap.VolumeJoinStart.JoinNumber;
            var volumeEnd = JoinMap.VolumeJoinStart.JoinNumber + JoinMap.VolumeJoinStart.JoinSpan;

            for (uint i = volumeStart; i <= volumeEnd; i++)
			{
				var index = i;
				Parent.AddAction(string.Format(@"/room/room1/volumes/level-{0}/level", index), new Action<ushort>(u =>
					EISC.SetUshort(index, u)));
				Parent.AddAction(string.Format(@"/room/room1/volumes/level-{0}/muteToggle", index), new Action(() =>
					EISC.PulseBool(index)));
			}

			Parent.AddAction(@"/room/room1/shutdownStart", new Action(() =>
				EISC.PulseBool(JoinMap.ShutdownStart.JoinNumber)));
			Parent.AddAction(@"/room/room1/shutdownEnd", new Action(() =>
				EISC.PulseBool(JoinMap.ShutdownEnd.JoinNumber)));
			Parent.AddAction(@"/room/room1/shutdownCancel", new Action(() =>
				EISC.PulseBool(JoinMap.ShutdownCancel.JoinNumber)));
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
			EISC.SetBoolSigAction(JoinMap.RoomIsOn.JoinNumber, b =>
				PostStatusMessage(new
					{
						isOn = b
					}));

			// Source change things
			EISC.SetSigTrueAction(JoinMap.SourceHasChanged.JoinNumber, () =>
				PostStatusMessage(new
					{
						selectedSourceKey = EISC.StringOutput[JoinMap.CurrentSourceKey.JoinNumber].StringValue
					}));

			// Volume things
			EISC.SetUShortSigAction(JoinMap.MasterVolume.JoinNumber, u =>
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

			EISC.SetBoolSigAction(JoinMap.MasterVolume.JoinNumber, b =>
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
			EISC.SetBoolSigAction(JoinMap.PrivacyMute.JoinNumber, b => 
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

            var volumeStart = JoinMap.VolumeJoinStart.JoinNumber;
            var volumeEnd = JoinMap.VolumeJoinStart.JoinNumber + JoinMap.VolumeJoinStart.JoinSpan;

            for (uint i = volumeStart; i <= volumeEnd; i++)
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

			EISC.SetUShortSigAction(JoinMap.NumberOfAuxFaders.JoinNumber, u => 
				PostStatusMessage(new {
					volumes = new {
						numberOfAuxFaders = u,
					}
				}));

			// shutdown things
			EISC.SetSigTrueAction(JoinMap.ShutdownCancel.JoinNumber, new Action(() =>
				PostMessage("/room/shutdown/", new
				{
					state = "wasCancelled"
				})));
			EISC.SetSigTrueAction(JoinMap.ShutdownEnd.JoinNumber, new Action(() =>
				PostMessage("/room/shutdown/", new
				{
					state = "hasFinished"
				})));
			EISC.SetSigTrueAction(JoinMap.ShutdownStart.JoinNumber, new Action(() =>
				PostMessage("/room/shutdown/", new
				{
					state = "hasStarted",
					duration = EISC.UShortOutput[JoinMap.ShutdownPromptDuration.JoinNumber].UShortValue
				})));

			// Config things
			EISC.SetSigTrueAction(JoinMap.ConfigIsReady.JoinNumber, LoadConfigValues);

			// Activity modes
			EISC.SetSigTrueAction(JoinMap.ActivityShare.JoinNumber, () => UpdateActivity(1));
			EISC.SetSigTrueAction(JoinMap.ActivityPhoneCall.JoinNumber, () => UpdateActivity(2));
			EISC.SetSigTrueAction(JoinMap.ActivityVideoCall.JoinNumber, () => UpdateActivity(3));
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
			rm.Name = EISC.StringOutput[JoinMap.ConfigRoomName.JoinNumber].StringValue;
			rm.Key = "room1";
			rm.Type = "ddvc01";

			DDVC01RoomPropertiesConfig rmProps;
			if (rm.Properties == null)
				rmProps = new DDVC01RoomPropertiesConfig();
			else
				rmProps = JsonConvert.DeserializeObject<DDVC01RoomPropertiesConfig>(rm.Properties.ToString());

			rmProps.Help = new EssentialsHelpPropertiesConfig();
			rmProps.Help.CallButtonText = EISC.StringOutput[JoinMap.ConfigHelpNumber.JoinNumber].StringValue;
			rmProps.Help.Message = EISC.StringOutput[JoinMap.ConfigHelpMessage.JoinNumber].StringValue;

			rmProps.Environment = new EssentialsEnvironmentPropertiesConfig(); // enabled defaults to false

			rmProps.RoomPhoneNumber = EISC.StringOutput[JoinMap.ConfigRoomPhoneNumber.JoinNumber].StringValue;
			rmProps.RoomURI = EISC.StringOutput[JoinMap.ConfigRoomURI.JoinNumber].StringValue;
			rmProps.SpeedDials = new List<DDVC01SpeedDial>();

			// This MAY need a check 
			rmProps.AudioCodecKey = "audioCodec";
			rmProps.VideoCodecKey = "videoCodec";

			// volume control names
			var volCount = EISC.UShortOutput[JoinMap.NumberOfAuxFaders.JoinNumber].UShortValue;

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
				var name = EISC.StringOutput[JoinMap.SourceNameJoinStart.JoinNumber + i].StringValue;
				if (EISC.BooleanOutput[JoinMap.UseSourceEnabled.JoinNumber].BoolValue
					&& !EISC.BooleanOutput[JoinMap.SourceIsEnabledJoinStart.JoinNumber + i].BoolValue)
				{
					continue;
				}		
				else if(!EISC.BooleanOutput[JoinMap.UseSourceEnabled.JoinNumber].BoolValue && string.IsNullOrEmpty(name))
					break;
				var icon = EISC.StringOutput[JoinMap.SourceIconJoinStart.JoinNumber + i].StringValue;
				var key = EISC.StringOutput[JoinMap.SourceKeyJoinStart.JoinNumber + i].StringValue;
				var type = EISC.StringOutput[JoinMap.SourceTypeJoinStart.JoinNumber + i].StringValue;
				var disableShare = EISC.BooleanOutput[JoinMap.SourceShareDisableJoinStart.JoinNumber + i].BoolValue;

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
					if (!EISC.GetBool(JoinMap.SpeedDialVisibleStartJoin.JoinNumber + i))
					{
						break;
					}
					acFavs.Add(new PepperDash.Essentials.Devices.Common.Codec.CodecActiveCallItem()
					{
						Name = EISC.GetString(JoinMap.SpeedDialNameStartJoin.JoinNumber + i),
						Number = EISC.GetString(JoinMap.SpeedDialNumberStartJoin.JoinNumber + i),
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
					var name = EISC.GetString(i + JoinMap.CameraNearNameStart.JoinNumber);
					if (!string.IsNullOrEmpty(name))
					{
						camsProps.Add(new
						{
							name = name,
							selector = "camera" + (i + 1),
						});
					}
				}
				var farName = EISC.GetString(JoinMap.CameraFarName.JoinNumber);
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
                var count = EISC.UShortOutput[JoinMap.NumberOfAuxFaders.JoinNumber].UShortValue;

                Debug.Console(1, this, "The Fader Count is : {0}", count);

                // build volumes object, serialize and put in content of method below

                // Create auxFaders
				var auxFaderDict = new Dictionary<string, Volume>();

                var volumeStart = JoinMap.VolumeJoinStart.JoinNumber;
                var volumeEnd = JoinMap.VolumeJoinStart.JoinNumber + JoinMap.VolumeJoinStart.JoinSpan;

                for (uint i = volumeStart; i <= count; i++)
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
                                EISC.UShortOutput[JoinMap.MasterVolume.JoinNumber].UShortValue,
                                EISC.BooleanOutput[JoinMap.MasterVolume.JoinNumber].BoolValue,
                  				EISC.StringOutput[JoinMap.MasterVolume.JoinNumber].StringValue,
				                true,
				                "something.png");
				volumes.Master.HasPrivacyMute = true;
				volumes.Master.PrivacyMuted = EISC.BooleanOutput[JoinMap.PrivacyMute.JoinNumber].BoolValue;

                volumes.AuxFaders = auxFaderDict;
				volumes.NumberOfAuxFaders = EISC.UShortInput[JoinMap.NumberOfAuxFaders.JoinNumber].UShortValue;

                PostStatusMessage(new
                    {
						activityMode = GetActivityMode(),
                        isOn = EISC.BooleanOutput[JoinMap.RoomIsOn.JoinNumber].BoolValue,
                        selectedSourceKey = EISC.StringOutput[JoinMap.CurrentSourceKey.JoinNumber].StringValue,
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
			if (EISC.BooleanOutput[JoinMap.ActivityPhoneCall.JoinNumber].BoolValue) return 2;
			else if (EISC.BooleanOutput[JoinMap.ActivityShare.JoinNumber].BoolValue) return 1;
			else if (EISC.BooleanOutput[JoinMap.ActivityVideoCall.JoinNumber].BoolValue) return 3;
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
			EISC.StringInput[JoinMap.UserCodeToSystem.JoinNumber].StringValue = UserCode;
			EISC.StringInput[JoinMap.ServerUrl.JoinNumber].StringValue = Parent.Config.ClientAppUrl;
		}
	}

    public class MobileControlSIMPLRoomBridgeFactory : EssentialsDeviceFactory<MobileControlSIMPLRoomBridge>
    {
        public MobileControlSIMPLRoomBridgeFactory()
        {
            TypeNames = new List<string>() { "mobilecontrolbridge-ddvc01", "mobilecontrolbridge-simpl" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new MobileControlSIMPLRoomBridge Device");

            var comm = CommFactory.GetControlPropertiesConfig(dc);

            var bridge = new PepperDash.Essentials.Room.MobileControl.MobileControlSIMPLRoomBridge(dc.Key, dc.Name, comm.IpIdInt);
            bridge.AddPreActivationAction(() =>
            {
                var parent = DeviceManager.AllDevices.FirstOrDefault(d => d.Key == "appServer") as MobileControlSystemController;
                if (parent == null)
                {
                    Debug.Console(0, bridge, "ERROR: Cannot connect bridge. System controller not present");
                }
                Debug.Console(0, bridge, "Linking to parent controller");
                bridge.AddParent(parent);
                parent.AddBridge(bridge);
            });

            return bridge;
        }
    }

}