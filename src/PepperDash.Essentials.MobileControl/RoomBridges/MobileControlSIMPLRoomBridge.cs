using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.EthernetCommunication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.AppServer;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Devices.Common.Cameras;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Room.Config;
using System;
using System.Collections.Generic;


namespace PepperDash.Essentials.Room.MobileControl
{
    // ReSharper disable once InconsistentNaming
    public class MobileControlSIMPLRoomBridge : MobileControlBridgeBase, IDelayedConfiguration
    {
        private const int SupportedDisplayCount = 10;

        /// <summary>
        /// Fires when config is ready to go
        /// </summary>
        public event EventHandler<EventArgs> ConfigurationIsReady;

        public ThreeSeriesTcpIpEthernetIntersystemCommunications Eisc { get; private set; }

        public MobileControlSIMPLRoomJoinMap JoinMap { get; private set; }

        public Dictionary<string, MessengerBase> DeviceMessengers { get; private set; }


        /// <summary>
        /// 
        /// </summary>
        public bool ConfigIsLoaded { get; private set; }

        public override string RoomName
        {
            get
            {
                var name = Eisc.StringOutput[JoinMap.ConfigRoomName.JoinNumber].StringValue;
                return string.IsNullOrEmpty(name) ? "Not Loaded" : name;
            }
        }

        public override string RoomKey
        {
            get { return "room1"; }
        }

        private readonly MobileControlSimplDeviceBridge _sourceBridge;

        private SIMPLAtcMessenger _atcMessenger;
        private SIMPLVtcMessenger _vtcMessenger;
        private SimplDirectRouteMessenger _directRouteMessenger;

        private const string _syntheticDeviceKey = "syntheticDevice";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="ipId"></param>
        public MobileControlSIMPLRoomBridge(string key, string name, uint ipId)
            : base(key, "")
        {
            Eisc = new ThreeSeriesTcpIpEthernetIntersystemCommunications(ipId, "127.0.0.2", Global.ControlSystem);
            var reg = Eisc.Register();
            if (reg != eDeviceRegistrationUnRegistrationResponse.Success)
                Debug.Console(0, this, "Cannot connect EISC at IPID {0}: \r{1}", ipId, reg);

            JoinMap = new MobileControlSIMPLRoomJoinMap(1);

            _sourceBridge = new MobileControlSimplDeviceBridge(key + "-sourceBridge", "SIMPL source bridge", Eisc);
            DeviceManager.AddDevice(_sourceBridge);

            CrestronConsole.AddNewConsoleCommand((s) => JoinMap.PrintJoinMapInfo(), "printmobilejoinmap", "Prints the MobileControlSIMPLRoomBridge JoinMap", ConsoleAccessLevelEnum.AccessOperator);

            AddPostActivationAction(() =>
                {
                    // Inform the SIMPL program that config can be sent
                    Eisc.BooleanInput[JoinMap.ReadyForConfig.JoinNumber].BoolValue = true;

                    Eisc.SigChange += EISC_SigChange;
                    Eisc.OnlineStatusChange += (o, a) =>
                    {
                        if (!a.DeviceOnLine)
                        {
                            return;
                        }

                        Debug.Console(1, this, "SIMPL EISC online={0}. Config is ready={1}. Use Essentials Config={2}",
                            a.DeviceOnLine, Eisc.BooleanOutput[JoinMap.ConfigIsReady.JoinNumber].BoolValue,
                            Eisc.BooleanOutput[JoinMap.ConfigIsLocal.JoinNumber].BoolValue);

                        if (Eisc.BooleanOutput[JoinMap.ConfigIsReady.JoinNumber].BoolValue)
                            LoadConfigValues();

                        if (Eisc.BooleanOutput[JoinMap.ConfigIsLocal.JoinNumber].BoolValue)
                            UseEssentialsConfig();
                    };
                    // load config if it's already there
                    if (Eisc.BooleanOutput[JoinMap.ConfigIsReady.JoinNumber].BoolValue)
                    {
                        LoadConfigValues();
                    }

                    if (Eisc.BooleanOutput[JoinMap.ConfigIsLocal.JoinNumber].BoolValue)
                    {
                        UseEssentialsConfig();
                    }
                });
        }


        /// <summary>
        /// Finish wiring up everything after all devices are created. The base class will hunt down the related
        /// parent controller and link them up.
        /// </summary>
        /// <returns></returns>
        public override bool CustomActivate()
        {
            Debug.Console(0, this, "Final activation. Setting up actions and feedbacks");
            //SetupFunctions();
            //SetupFeedbacks();

            var atcKey = string.Format("atc-{0}-{1}", Key, Key);
            _atcMessenger = new SIMPLAtcMessenger(atcKey, Eisc, "/device/audioCodec");
            _atcMessenger.RegisterWithAppServer(Parent);

            var vtcKey = string.Format("atc-{0}-{1}", Key, Key);
            _vtcMessenger = new SIMPLVtcMessenger(vtcKey, Eisc, "/device/videoCodec");
            _vtcMessenger.RegisterWithAppServer(Parent);

            var drKey = string.Format("directRoute-{0}-{1}", Key, Key);
            _directRouteMessenger = new SimplDirectRouteMessenger(drKey, Eisc, "/routing");
            _directRouteMessenger.RegisterWithAppServer(Parent);

            CrestronConsole.AddNewConsoleCommand(s =>
            {
                JoinMap.PrintJoinMapInfo();

                _atcMessenger.JoinMap.PrintJoinMapInfo();

                _vtcMessenger.JoinMap.PrintJoinMapInfo();

                _directRouteMessenger.JoinMap.PrintJoinMapInfo();

                // TODO: Update Source Bridge to use new JoinMap scheme
                //_sourceBridge.JoinMap.PrintJoinMapInfo();
            }, "printmobilebridge", "Prints MC-SIMPL bridge EISC data", ConsoleAccessLevelEnum.AccessOperator);

            return base.CustomActivate();
        }

        private void UseEssentialsConfig()
        {
            ConfigIsLoaded = false;

            SetupDeviceMessengers();

            Debug.Console(0, this, "******* ESSENTIALS CONFIG: \r{0}",
                JsonConvert.SerializeObject(ConfigReader.ConfigObject, Formatting.Indented));

            ConfigurationIsReady?.Invoke(this, new EventArgs());

            ConfigIsLoaded = true;
        }

#if SERIES4
        protected override void RegisterActions()
#else
        protected override void CustomRegisterWithAppServer(MobileControlSystemController appServerController)
#endif
        {
            SetupFunctions();
            SetupFeedbacks();
        }

        /// <summary>
        /// Setup the actions to take place on various incoming API calls
        /// </summary>
        private void SetupFunctions()
        {
            AddAction(@"/promptForCode",
                (id, content) => Eisc.PulseBool(JoinMap.PromptForCode.JoinNumber));
            AddAction(@"/clientJoined", (id, content) => Eisc.PulseBool(JoinMap.ClientJoined.JoinNumber));

            AddAction(@"/status", (id, content) => SendFullStatus());

            AddAction(@"/source", (id, content) =>
            {
                var msg = content.ToObject<SourceSelectMessageContent>();

                Eisc.SetString(JoinMap.CurrentSourceKey.JoinNumber, msg.SourceListItemKey);
                Eisc.PulseBool(JoinMap.SourceHasChanged.JoinNumber);
            });

            AddAction(@"/defaultsource", (id, content) =>
                Eisc.PulseBool(JoinMap.ActivityShare.JoinNumber));
            AddAction(@"/activityPhone", (id, content) =>
                Eisc.PulseBool(JoinMap.ActivityPhoneCall.JoinNumber));
            AddAction(@"/activityVideo", (id, content) =>
                Eisc.PulseBool(JoinMap.ActivityVideoCall.JoinNumber));

            AddAction(@"/volumes/master/level", (id, content) =>
            {
                var value = content["value"].Value<ushort>();

                Eisc.SetUshort(JoinMap.MasterVolume.JoinNumber, value);
            });

            AddAction(@"/volumes/master/muteToggle", (id, content) =>
                Eisc.PulseBool(JoinMap.MasterVolume.JoinNumber));
            AddAction(@"/volumes/master/privacyMuteToggle", (id, content) =>
                Eisc.PulseBool(JoinMap.PrivacyMute.JoinNumber));


            // /xyzxyz/volumes/master/muteToggle ---> BoolInput[1]

            var volumeStart = JoinMap.VolumeJoinStart.JoinNumber;
            var volumeEnd = JoinMap.VolumeJoinStart.JoinNumber + JoinMap.VolumeJoinStart.JoinSpan;

            for (uint i = volumeStart; i <= volumeEnd; i++)
            {
                var index = i;
                AddAction(string.Format(@"/volumes/level-{0}/level", index), (id, content) =>
                {
                    var value = content["value"].Value<ushort>();
                    Eisc.SetUshort(index, value);
                });

                AddAction(string.Format(@"/volumes/level-{0}/muteToggle", index), (id, content) =>
                    Eisc.PulseBool(index));
            }

            AddAction(@"/shutdownStart", (id, content) =>
                Eisc.PulseBool(JoinMap.ShutdownStart.JoinNumber));
            AddAction(@"/shutdownEnd", (id, content) =>
                Eisc.PulseBool(JoinMap.ShutdownEnd.JoinNumber));
            AddAction(@"/shutdownCancel", (id, content) =>
                Eisc.PulseBool(JoinMap.ShutdownCancel.JoinNumber));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="devKey"></param>
        private void SetupSourceFunctions(string devKey)
        {
            var sourceJoinMap = new SourceDeviceMapDictionary();

            var prefix = string.Format("/device/{0}/", devKey);

            foreach (var item in sourceJoinMap)
            {
                var join = item.Value;
                AddAction(string.Format("{0}{1}", prefix, item.Key), (id, content) =>
                {
                    HandlePressAndHoldEisc(content, b => Eisc.SetBool(join, b));
                });
            }
        }

        private void HandlePressAndHoldEisc(JToken content, Action<bool> action)
        {
            var state = content.ToObject<MobileControlSimpleContent<string>>();

            var timerHandler = PressAndHoldHandler.GetPressAndHoldHandler(state.Value);
            if (timerHandler == null)
            {
                return;
            }

            timerHandler(state.Value, action);

            action(state.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase));
        }


        /// <summary>
        /// Links feedbacks to whatever is gonna happen!
        /// </summary>
        private void SetupFeedbacks()
        {
            // Power 
            Eisc.SetBoolSigAction(JoinMap.RoomIsOn.JoinNumber, b =>
                PostStatus(new
                {
                    isOn = b
                }));

            // Source change things
            Eisc.SetSigTrueAction(JoinMap.SourceHasChanged.JoinNumber, () =>
                PostStatus(new
                {
                    selectedSourceKey = Eisc.StringOutput[JoinMap.CurrentSourceKey.JoinNumber].StringValue
                }));

            // Volume things
            Eisc.SetUShortSigAction(JoinMap.MasterVolume.JoinNumber, u =>
                PostStatus(new
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

            Eisc.SetBoolSigAction(JoinMap.MasterVolume.JoinNumber, b =>
                PostStatus(new
                {
                    volumes = new
                    {
                        master = new
                        {
                            muted = b
                        }
                    }
                }));
            Eisc.SetBoolSigAction(JoinMap.PrivacyMute.JoinNumber, b =>
                PostStatus(new
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
                Eisc.SetUShortSigAction(index, u => // start at join 2
                {
                    // need a dict in order to create the level-n property on auxFaders
                    var dict = new Dictionary<string, object> { { "level-" + index, new { level = u } } };
                    PostStatus(new
                    {
                        volumes = new
                        {
                            auxFaders = dict,
                        }
                    });
                });
                Eisc.SetBoolSigAction(index, b =>
                {
                    // need a dict in order to create the level-n property on auxFaders
                    var dict = new Dictionary<string, object> { { "level-" + index, new { muted = b } } };
                    PostStatus(new
                    {
                        volumes = new
                        {
                            auxFaders = dict,
                        }
                    });
                });
            }

            Eisc.SetUShortSigAction(JoinMap.NumberOfAuxFaders.JoinNumber, u =>
                PostStatus(new
                {
                    volumes = new
                    {
                        numberOfAuxFaders = u,
                    }
                }));

            // shutdown things
            Eisc.SetSigTrueAction(JoinMap.ShutdownCancel.JoinNumber, () =>
                PostMessage("/shutdown/", new
                {
                    state = "wasCancelled"
                }));
            Eisc.SetSigTrueAction(JoinMap.ShutdownEnd.JoinNumber, () =>
                PostMessage("/shutdown/", new
                {
                    state = "hasFinished"
                }));
            Eisc.SetSigTrueAction(JoinMap.ShutdownStart.JoinNumber, () =>
                PostMessage("/shutdown/", new
                {
                    state = "hasStarted",
                    duration = Eisc.UShortOutput[JoinMap.ShutdownPromptDuration.JoinNumber].UShortValue
                }));

            // Config things
            Eisc.SetSigTrueAction(JoinMap.ConfigIsReady.JoinNumber, LoadConfigValues);

            // Activity modes
            Eisc.SetSigTrueAction(JoinMap.ActivityShare.JoinNumber, () => UpdateActivity(1));
            Eisc.SetSigTrueAction(JoinMap.ActivityPhoneCall.JoinNumber, () => UpdateActivity(2));
            Eisc.SetSigTrueAction(JoinMap.ActivityVideoCall.JoinNumber, () => UpdateActivity(3));

            AppServerController.ApiOnlineAndAuthorized.LinkInputSig(Eisc.BooleanInput[JoinMap.ApiOnlineAndAuthorized.JoinNumber]);
        }


        /// <summary>
        /// Updates activity states
        /// </summary>
        private void UpdateActivity(int mode)
        {
            PostStatus(new
            {
                activityMode = mode,
            });
        }

        /// <summary>
        /// Synthesizes a source device config from the SIMPL config join data
        /// </summary>
        /// <param name="sli"></param>
        /// <param name="type"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private DeviceConfig GetSyntheticSourceDevice(SourceListItem sli, string type, uint i)
        {
            var groupMap = GetSourceGroupDictionary();
            var key = sli.SourceKey;
            var name = sli.Name;

            // If not, synthesize the device config
            var group = "genericsource";
            if (groupMap.ContainsKey(type))
            {
                group = groupMap[type];
            }

            // add dev to devices list
            var devConf = new DeviceConfig
            {
                Group = group,
                Key = key,
                Name = name,
                Type = type,
                Properties = new JObject(new JProperty(_syntheticDeviceKey, true)),
            };

            if (group.ToLower().StartsWith("settopbox")) // Add others here as needed
            {
                SetupSourceFunctions(key);
            }

            if (group.ToLower().Equals("simplmessenger"))
            {
                if (type.ToLower().Equals("simplcameramessenger"))
                {
                    var props = new SimplMessengerPropertiesConfig
                    {
                        DeviceKey = key,
                        JoinMapKey = ""
                    };
                    var joinStart = 1000 + (i * 100) + 1; // 1001, 1101, 1201, 1301... etc.
                    props.JoinStart = joinStart;
                    devConf.Properties = JToken.FromObject(props);
                }
            }

            return devConf;
        }

        /// <summary>
        /// Reads in config values when the Simpl program is ready
        /// </summary>
        private void LoadConfigValues()
        {
            Debug.Console(1, this, "Loading configuration from SIMPL EISC bridge");
            ConfigIsLoaded = false;

            var co = ConfigReader.ConfigObject;

            if (!string.IsNullOrEmpty(Eisc.StringOutput[JoinMap.PortalSystemUrl.JoinNumber].StringValue))
            {
                ConfigReader.ConfigObject.SystemUrl = Eisc.StringOutput[JoinMap.PortalSystemUrl.JoinNumber].StringValue;
            }

            co.Info.RuntimeInfo.AppName = Assembly.GetExecutingAssembly().GetName().Name;
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            co.Info.RuntimeInfo.AssemblyVersion = string.Format("{0}.{1}.{2}", version.Major, version.Minor,
                version.Build);

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
            rm.Name = Eisc.StringOutput[JoinMap.ConfigRoomName.JoinNumber].StringValue;
            rm.Key = "room1";
            rm.Type = "SIMPL01";

            var rmProps = rm.Properties == null
                ? new SimplRoomPropertiesConfig()
                : JsonConvert.DeserializeObject<SimplRoomPropertiesConfig>(rm.Properties.ToString());

            rmProps.Help = new EssentialsHelpPropertiesConfig
            {
                CallButtonText = Eisc.StringOutput[JoinMap.ConfigHelpNumber.JoinNumber].StringValue,
                Message = Eisc.StringOutput[JoinMap.ConfigHelpMessage.JoinNumber].StringValue
            };

            rmProps.Environment = new EssentialsEnvironmentPropertiesConfig(); // enabled defaults to false

            rmProps.RoomPhoneNumber = Eisc.StringOutput[JoinMap.ConfigRoomPhoneNumber.JoinNumber].StringValue;
            rmProps.RoomURI = Eisc.StringOutput[JoinMap.ConfigRoomUri.JoinNumber].StringValue;
            rmProps.SpeedDials = new List<SimplSpeedDial>();

            // This MAY need a check 
            if (Eisc.BooleanOutput[JoinMap.ActivityPhoneCallEnable.JoinNumber].BoolValue)
            {
                rmProps.AudioCodecKey = "audioCodec";
            }

            if (Eisc.BooleanOutput[JoinMap.ActivityVideoCallEnable.JoinNumber].BoolValue)
            {
                rmProps.VideoCodecKey = "videoCodec";
            }

            // volume control names

            //// use Volumes object or?
            //rmProps.VolumeSliderNames = new List<string>();
            //for(uint i = 701; i <= 700 + volCount; i++)
            //{
            //    rmProps.VolumeSliderNames.Add(EISC.StringInput[i].StringValue);
            //}

            // There should be Mobile Control devices in here, I think...
            if (co.Devices == null)
                co.Devices = new List<DeviceConfig>();

            // clear out previous SIMPL devices
            co.Devices.RemoveAll(d =>
                d.Key.StartsWith("source-", StringComparison.OrdinalIgnoreCase)
                || d.Key.Equals("audioCodec", StringComparison.OrdinalIgnoreCase)
                || d.Key.Equals("videoCodec", StringComparison.OrdinalIgnoreCase)
            || d.Key.StartsWith("destination-", StringComparison.OrdinalIgnoreCase));

            rmProps.SourceListKey = "default";
            rm.Properties = JToken.FromObject(rmProps);

            // Source list! This might be brutal!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            co.SourceLists = new Dictionary<string, Dictionary<string, SourceListItem>>();
            var newSl = new Dictionary<string, SourceListItem>();
            // add "none" source if VTC present

            if (!string.IsNullOrEmpty(rmProps.VideoCodecKey))
            {
                var codecOsd = new SourceListItem
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
            var useSourceEnabled = Eisc.BooleanOutput[JoinMap.UseSourceEnabled.JoinNumber].BoolValue;
            for (uint i = 0; i <= 19; i++)
            {
                var name = Eisc.StringOutput[JoinMap.SourceNameJoinStart.JoinNumber + i].StringValue;

                if (!Eisc.BooleanOutput[JoinMap.UseSourceEnabled.JoinNumber].BoolValue && string.IsNullOrEmpty(name))
                {
                    Debug.Console(1, "Source at join {0} does not have a name", JoinMap.SourceNameJoinStart.JoinNumber + i);
                    break;
                }


                var icon = Eisc.StringOutput[JoinMap.SourceIconJoinStart.JoinNumber + i].StringValue;
                var key = Eisc.StringOutput[JoinMap.SourceKeyJoinStart.JoinNumber + i].StringValue;
                var type = Eisc.StringOutput[JoinMap.SourceTypeJoinStart.JoinNumber + i].StringValue;
                var disableShare = Eisc.BooleanOutput[JoinMap.SourceShareDisableJoinStart.JoinNumber + i].BoolValue;
                var sourceEnabled = Eisc.BooleanOutput[JoinMap.SourceIsEnabledJoinStart.JoinNumber + i].BoolValue;
                var controllable = Eisc.BooleanOutput[JoinMap.SourceIsControllableJoinStart.JoinNumber + i].BoolValue;
                var audioSource = Eisc.BooleanOutput[JoinMap.SourceIsAudioSourceJoinStart.JoinNumber + i].BoolValue;

                Debug.Console(0, this, "Adding source {0} '{1}'", key, name);

                var sourceKey = Eisc.StringOutput[JoinMap.SourceControlDeviceKeyJoinStart.JoinNumber + i].StringValue;

                var newSli = new SourceListItem
                {
                    Icon = icon,
                    Name = name,
                    Order = (int)i + 10,
                    SourceKey = string.IsNullOrEmpty(sourceKey) ? key : sourceKey, // Use the value from the join if defined
                    Type = eSourceListItemType.Route,
                    DisableCodecSharing = disableShare,
                    IncludeInSourceList = !useSourceEnabled || sourceEnabled,
                    IsControllable = controllable,
                    IsAudioSource = audioSource
                };
                newSl.Add(key, newSli);

                var existingSourceDevice = co.GetDeviceForKey(newSli.SourceKey);

                var syntheticDevice = GetSyntheticSourceDevice(newSli, type, i);

                // Look to see if this is a device that already exists in Essentials and get it
                if (existingSourceDevice != null)
                {
                    Debug.Console(0, this, "Found device with key: {0} in Essentials.", key);

                    if (existingSourceDevice.Properties.Value<bool>(_syntheticDeviceKey))
                    {
                        Debug.Console(0, this, "Updating previous device config with new values");
                        existingSourceDevice = syntheticDevice;
                    }
                    else
                    {
                        Debug.Console(0, this, "Using existing Essentials device (non synthetic)");
                    }
                }
                else
                {
                    co.Devices.Add(syntheticDevice);
                }
            }

            co.SourceLists.Add("default", newSl);

            if (Eisc.BooleanOutput[JoinMap.SupportsAdvancedSharing.JoinNumber].BoolValue)
            {
                if (co.DestinationLists == null)
                {
                    co.DestinationLists = new Dictionary<string, Dictionary<string, DestinationListItem>>();
                }

                CreateDestinationList(co);
            }

            // Build "audioCodec" config if we need
            if (!string.IsNullOrEmpty(rmProps.AudioCodecKey))
            {
                var acFavs = new List<CodecActiveCallItem>();
                for (uint i = 0; i < 4; i++)
                {
                    if (!Eisc.GetBool(JoinMap.SpeedDialVisibleStartJoin.JoinNumber + i))
                    {
                        break;
                    }
                    acFavs.Add(new CodecActiveCallItem
                    {
                        Name = Eisc.GetString(JoinMap.SpeedDialNameStartJoin.JoinNumber + i),
                        Number = Eisc.GetString(JoinMap.SpeedDialNumberStartJoin.JoinNumber + i),
                        Type = eCodecCallType.Audio
                    });
                }

                var acProps = new
                {
                    favorites = acFavs
                };

                const string acStr = "audioCodec";
                var acConf = new DeviceConfig
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
                var favs = new List<CodecActiveCallItem>();

                // cameras
                var camsProps = new List<object>();
                for (uint i = 0; i < 9; i++)
                {
                    var name = Eisc.GetString(i + JoinMap.CameraNearNameStart.JoinNumber);
                    if (!string.IsNullOrEmpty(name))
                    {
                        camsProps.Add(new
                        {
                            name,
                            selector = "camera" + (i + 1),
                        });
                    }
                }
                var farName = Eisc.GetString(JoinMap.CameraFarName.JoinNumber);
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
                const string str = "videoCodec";
                var conf = new DeviceConfig
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

            Debug.Console(0, this, "******* CONFIG FROM SIMPL: \r{0}",
                JsonConvert.SerializeObject(ConfigReader.ConfigObject, Formatting.Indented));

            ConfigurationIsReady?.Invoke(this, new EventArgs());

            ConfigIsLoaded = true;
        }

        private DeviceConfig GetSyntheticDestinationDevice(string key, string name)
        {
            // If not, synthesize the device config
            var devConf = new DeviceConfig
            {
                Group = "genericdestination",
                Key = key,
                Name = name,
                Type = "genericdestination",
                Properties = new JObject(new JProperty(_syntheticDeviceKey, true)),
            };

            return devConf;
        }

        private void CreateDestinationList(BasicConfig co)
        {
            var useDestEnable = Eisc.BooleanOutput[JoinMap.UseDestinationEnable.JoinNumber].BoolValue;

            var newDl = new Dictionary<string, DestinationListItem>();

            for (uint i = 0; i < SupportedDisplayCount; i++)
            {
                var name = Eisc.StringOutput[JoinMap.DestinationNameJoinStart.JoinNumber + i].StringValue;
                var routeType = Eisc.StringOutput[JoinMap.DestinationTypeJoinStart.JoinNumber + i].StringValue;
                var key = Eisc.StringOutput[JoinMap.DestinationDeviceKeyJoinStart.JoinNumber + i].StringValue;
                //var order = Eisc.UShortOutput[JoinMap.DestinationOrderJoinStart.JoinNumber + i].UShortValue;
                var enabled = Eisc.BooleanOutput[JoinMap.DestinationIsEnabledJoinStart.JoinNumber + i].BoolValue;

                if (useDestEnable && !enabled)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                Debug.Console(0, this, "Adding destination {0} - {1}", key, name);

                eRoutingSignalType parsedType;
                try
                {
                    parsedType = (eRoutingSignalType)Enum.Parse(typeof(eRoutingSignalType), routeType, true);
                }
                catch
                {
                    Debug.Console(0, this, "Error parsing destination type: {0}", routeType);
                    parsedType = eRoutingSignalType.AudioVideo;
                }

                var newDli = new DestinationListItem
                {
                    Name = name,
                    Order = (int)i,
                    SinkKey = key,
                    SinkType = parsedType,
                };

                if (!newDl.ContainsKey(key))
                {
                    newDl.Add(key, newDli);
                }
                else
                {
                    newDl[key] = newDli;
                }

                if (!_directRouteMessenger.DestinationList.ContainsKey(newDli.SinkKey))
                {
                    //add same DestinationListItem to dictionary for messenger in order to allow for correlation by index
                    _directRouteMessenger.DestinationList.Add(key, newDli);
                }
                else
                {
                    _directRouteMessenger.DestinationList[key] = newDli;
                }

                var existingDev = co.GetDeviceForKey(key);

                var syntheticDisplay = GetSyntheticDestinationDevice(key, name);

                if (existingDev != null)
                {
                    Debug.Console(0, this, "Found device with key: {0} in Essentials.", key);

                    if (existingDev.Properties.Value<bool>(_syntheticDeviceKey))
                    {
                        Debug.Console(0, this, "Updating previous device config with new values");
                    }
                    else
                    {
                        Debug.Console(0, this, "Using existing Essentials device (non synthetic)");
                    }
                }
                else
                {
                    co.Devices.Add(syntheticDisplay);
                }
            }

            if (!co.DestinationLists.ContainsKey("default"))
            {
                co.DestinationLists.Add("default", newDl);
            }
            else
            {
                co.DestinationLists["default"] = newDl;
            }

            _directRouteMessenger.RegisterForDestinationPaths();
        }

        /// <summary>
        /// Iterates device config and adds messengers as neede for each device type
        /// </summary>
        private void SetupDeviceMessengers()
        {
            DeviceMessengers = new Dictionary<string, MessengerBase>();

            try
            {
                foreach (var device in ConfigReader.ConfigObject.Devices)
                {
                    if (device.Group.Equals("simplmessenger"))
                    {
                        var props =
                            JsonConvert.DeserializeObject<SimplMessengerPropertiesConfig>(device.Properties.ToString());

                        var messengerKey = string.Format("device-{0}-{1}", Key, Key);

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
                            messenger = new SIMPLCameraMessenger(messengerKey, Eisc, "/device/" + props.DeviceKey,
                                props.JoinStart);
                        }
                        else if (type.Equals("simplroutemessenger"))
                        {
                            Debug.Console(2, this, "Adding SIMPLRouteMessenger for: '{0}'", props.DeviceKey);
                            messenger = new SIMPLRouteMessenger(messengerKey, Eisc, "/device/" + props.DeviceKey,
                                props.JoinStart);
                        }

                        if (messenger != null)
                        {
                            DeviceManager.AddDevice(messenger);
                            DeviceMessengers.Add(device.Key, messenger);
                            messenger.RegisterWithAppServer(Parent);
                        }
                        else
                        {
                            Debug.Console(2, this, "Unable to add messenger for device: '{0}' of type: '{1}'",
                                props.DeviceKey, type);
                        }
                    }
                    else
                    {
                        var dev = DeviceManager.GetDeviceForKey(device.Key);

                        if (dev != null)
                        {
                            if (dev is CameraBase)
                            {
                                var camDevice = dev as CameraBase;
                                Debug.Console(1, this, "Adding CameraBaseMessenger for device: {0}", dev.Key);
                                var cameraMessenger = new CameraBaseMessenger(device.Key + "-" + Key, camDevice,
                                    "/device/" + device.Key);
                                DeviceMessengers.Add(device.Key, cameraMessenger);
                                DeviceManager.AddDevice(cameraMessenger);
                                cameraMessenger.RegisterWithAppServer(Parent);
                                continue;
                            }
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
        private void SendFullStatus()
        {
            if (ConfigIsLoaded)
            {
                var count = Eisc.UShortOutput[JoinMap.NumberOfAuxFaders.JoinNumber].UShortValue;

                Debug.Console(1, this, "The Fader Count is : {0}", count);

                // build volumes object, serialize and put in content of method below

                // Create auxFaders
                var auxFaderDict = new Dictionary<string, Volume>();

                var volumeStart = JoinMap.VolumeJoinStart.JoinNumber;

                for (var i = volumeStart; i <= count; i++)
                {
                    auxFaderDict.Add("level-" + i,
                        new Volume("level-" + i,
                            Eisc.UShortOutput[i].UShortValue,
                            Eisc.BooleanOutput[i].BoolValue,
                            Eisc.StringOutput[i].StringValue,
                            true,
                            "someting.png"));
                }

                var volumes = new Volumes
                {
                    Master = new Volume("master",
                        Eisc.UShortOutput[JoinMap.MasterVolume.JoinNumber].UShortValue,
                        Eisc.BooleanOutput[JoinMap.MasterVolume.JoinNumber].BoolValue,
                        Eisc.StringOutput[JoinMap.MasterVolume.JoinNumber].StringValue,
                        true,
                        "something.png")
                    {
                        HasPrivacyMute = true,
                        PrivacyMuted = Eisc.BooleanOutput[JoinMap.PrivacyMute.JoinNumber].BoolValue
                    },
                    AuxFaders = auxFaderDict,
                    NumberOfAuxFaders = Eisc.UShortInput[JoinMap.NumberOfAuxFaders.JoinNumber].UShortValue
                };

                // TODO: Add property to status message to indicate if advanced sharing is supported and if users can change share mode

                PostStatus(new
                {
                    activityMode = GetActivityMode(),
                    isOn = Eisc.BooleanOutput[JoinMap.RoomIsOn.JoinNumber].BoolValue,
                    selectedSourceKey = Eisc.StringOutput[JoinMap.CurrentSourceKey.JoinNumber].StringValue,
                    volumes,
                    supportsAdvancedSharing = Eisc.BooleanOutput[JoinMap.SupportsAdvancedSharing.JoinNumber].BoolValue,
                    userCanChangeShareMode = Eisc.BooleanOutput[JoinMap.UserCanChangeShareMode.JoinNumber].BoolValue,
                });
            }
            else
            {
                PostStatus(new
                {
                    error = "systemNotReady"
                });
            }
        }

        /// <summary>
        /// Returns the activity mode int
        /// </summary>
        /// <returns></returns>
        private int GetActivityMode()
        {
            if (Eisc.BooleanOutput[JoinMap.ActivityPhoneCall.JoinNumber].BoolValue) return 2;
            if (Eisc.BooleanOutput[JoinMap.ActivityShare.JoinNumber].BoolValue) return 1;

            return Eisc.BooleanOutput[JoinMap.ActivityVideoCall.JoinNumber].BoolValue ? 3 : 0;
        }

        /// <summary>
        /// Helper for posting status message
        /// </summary>
        /// <param name="contentObject">The contents of the content object</param>
        private void PostStatus(object contentObject)
        {
            AppServerController.SendMessageObject(new MobileControlMessage
            {
                Type = "/status/",
                Content = JToken.FromObject(contentObject)
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="contentObject"></param>
        private void PostMessage(string messageType, object contentObject)
        {
            AppServerController.SendMessageObject(new MobileControlMessage
            {
                Type = messageType,
                Content = JToken.FromObject(contentObject)
            });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentDevice"></param>
        /// <param name="args"></param>
        private void EISC_SigChange(object currentDevice, SigEventArgs args)
        {
            if (Debug.Level >= 1)
                Debug.Console(1, this, "SIMPL EISC change: {0} {1}={2}", args.Sig.Type, args.Sig.Number,
                    args.Sig.StringValue);
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
        private Dictionary<string, string> GetSourceGroupDictionary()
        {
            //type, group
            var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"laptop", "pc"},
                {"pc", "pc"},
                {"wireless", "genericsource"},
                {"iptv", "settopbox"},
                {"simplcameramessenger", "simplmessenger"},
                {"camera", "camera"},

            };
            return d;
        }

        /// <summary>
        /// updates the usercode from server
        /// </summary>
        protected override void UserCodeChange()
        {

            Debug.Console(1, this, "Server user code changed: {0}", UserCode);

            var qrUrl = string.Format("{0}/api/rooms/{1}/{3}/qr?x={2}", AppServerController.Host, AppServerController.SystemUuid, new Random().Next(), "room1");
            QrCodeUrl = qrUrl;

            Debug.Console(1, this, "Server user code changed: {0} - {1}", UserCode, qrUrl);

            OnUserCodeChanged();

            Eisc.StringInput[JoinMap.UserCodeToSystem.JoinNumber].StringValue = UserCode;
            Eisc.StringInput[JoinMap.ServerUrl.JoinNumber].StringValue = McServerUrl;
            Eisc.StringInput[JoinMap.QrCodeUrl.JoinNumber].StringValue = QrCodeUrl;

        }
    }
}