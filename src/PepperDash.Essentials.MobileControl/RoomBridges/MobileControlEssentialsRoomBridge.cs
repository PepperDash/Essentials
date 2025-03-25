using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Room.MobileControl;
using PepperDash.Essentials.Room.Config;
using PepperDash.Essentials.Devices.Common.VideoCodec;
using PepperDash.Essentials.Devices.Common.AudioCodec;
using PepperDash.Essentials.Devices.Common.Cameras;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PepperDash.Essentials.Devices.Common.Room;
using IShades = PepperDash.Essentials.Core.Shades.IShades;
using ShadeBase = PepperDash.Essentials.Devices.Common.Shades.ShadeBase;
using PepperDash.Essentials.Devices.Common.TouchPanel;
using Crestron.SimplSharp;
using Volume = PepperDash.Essentials.Room.MobileControl.Volume;
using PepperDash.Essentials.Core.CrestronIO;
using PepperDash.Essentials.Core.Lighting;
using PepperDash.Essentials.Core.Shades;
using PepperDash.Core.Logging;



#if SERIES4
using PepperDash.Essentials.AppServer;
#endif

namespace PepperDash.Essentials
{
    public class MobileControlEssentialsRoomBridge : MobileControlBridgeBase
    {
        private List<JoinToken> _touchPanelTokens = new List<JoinToken>();
        public IEssentialsRoom Room { get; private set; }

        public string DefaultRoomKey { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public override string RoomName
        {
            get { return Room.Name; }
        }

        public override string RoomKey
        {
            get { return Room.Key; }
        }

        public MobileControlEssentialsRoomBridge(IEssentialsRoom room) :
            this($"mobileControlBridge-{room.Key}", room.Key, room)
        {
            Room = room;
        }

        public MobileControlEssentialsRoomBridge(string key, string roomKey, IEssentialsRoom room) : base(key, $"/room/{room.Key}", room as Device)
        {
            DefaultRoomKey = roomKey;

            AddPreActivationAction(GetRoom);
        }

#if SERIES4
        protected override void RegisterActions()
#else
        protected override void CustomRegisterWithAppServer(MobileControlSystemController appServerController)
#endif
        {
            // we add actions to the messaging system with a path, and a related action. Custom action
            // content objects can be handled in the controller's LineReceived method - and perhaps other
            // sub-controller parsing could be attached to these classes, so that the systemController
            // doesn't need to know about everything.

            this.LogInformation("Registering Actions with AppServer");

            AddAction("/promptForCode", (id, content) => OnUserPromptedForCode());
            AddAction("/clientJoined", (id, content) => OnClientJoined());

            AddAction("/touchPanels", (id, content) => OnTouchPanelsUpdated(content));

            AddAction($"/userApp", (id, content) => OnUserAppUpdated(content));

            AddAction("/userCode", (id, content) =>
            {
                var msg = content.ToObject<UserCodeChangedContent>();

                SetUserCode(msg.UserCode, msg.QrChecksum ?? string.Empty);
            });


            // Source Changes and room off
            AddAction("/status", (id, content) =>
            {
                SendFullStatusForClientId(id, Room);
            });

            if (Room is IRunRouteAction routeRoom)
                AddAction("/source", (id, content) =>
                {

                    var msg = content.ToObject<SourceSelectMessageContent>();

                    this.LogVerbose("Received request to route to source: {sourceListKey} on list: {sourceList}", msg.SourceListItemKey, msg.SourceListKey);

                    routeRoom.RunRouteAction(msg.SourceListItemKey, msg.SourceListKey);
                });

            if (Room is IRunDirectRouteAction directRouteRoom)
            {
                AddAction("/directRoute", (id, content) =>
                {
                    var msg = content.ToObject<DirectRoute>();


                    this.LogVerbose("Running direct route from {sourceKey} to {destinationKey} with signal type {signalType}", msg.SourceKey, msg.DestinationKey, msg.SignalType);

                    directRouteRoom.RunDirectRoute(msg.SourceKey, msg.DestinationKey, msg.SignalType);
                });
            }


            if (Room is IRunDefaultPresentRoute defaultRoom)
                AddAction("/defaultsource", (id, content) => defaultRoom.RunDefaultPresentRoute());

            if (Room is IHasCurrentVolumeControls volumeRoom)
            {
                volumeRoom.CurrentVolumeDeviceChange += Room_CurrentVolumeDeviceChange;

                if (volumeRoom.CurrentVolumeControls == null) return;

                AddAction("/volumes/master/level", (id, content) =>
                {
                    var msg = content.ToObject<MobileControlSimpleContent<ushort>>();


                    if (volumeRoom.CurrentVolumeControls is IBasicVolumeWithFeedback basicVolumeWithFeedback)
                        basicVolumeWithFeedback.SetVolume(msg.Value);
                });

                AddAction("/volumes/master/muteToggle", (id, content) => volumeRoom.CurrentVolumeControls.MuteToggle());

                AddAction("/volumes/master/muteOn", (id, content) =>
                {
                    if (volumeRoom.CurrentVolumeControls is IBasicVolumeWithFeedback basicVolumeWithFeedback)
                        basicVolumeWithFeedback.MuteOn();
                });

                AddAction("/volumes/master/muteOff", (id, content) =>
                {
                    if (volumeRoom.CurrentVolumeControls is IBasicVolumeWithFeedback basicVolumeWithFeedback)
                        basicVolumeWithFeedback.MuteOff();
                });

                AddAction("/volumes/master/volumeUp", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) =>
                    {
                        if (volumeRoom.CurrentVolumeControls is IBasicVolumeWithFeedback basicVolumeWithFeedback)
                        {
                            basicVolumeWithFeedback.VolumeUp(b);
                        }
                    }
                ));

                AddAction("/volumes/master/volumeDown", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) =>
                {
                    if (volumeRoom.CurrentVolumeControls is IBasicVolumeWithFeedback basicVolumeWithFeedback)
                    {
                        basicVolumeWithFeedback.VolumeDown(b);
                    }
                }
                ));


                // Registers for initial volume events, if possible
                if (volumeRoom.CurrentVolumeControls is IBasicVolumeWithFeedback currentVolumeDevice)
                {
                    this.LogVerbose("Registering for volume feedback events");

                    currentVolumeDevice.MuteFeedback.OutputChange += MuteFeedback_OutputChange;
                    currentVolumeDevice.VolumeLevelFeedback.OutputChange += VolumeLevelFeedback_OutputChange;
                }
            }

            if (Room is IHasCurrentSourceInfoChange sscRoom)
                sscRoom.CurrentSourceChange += Room_CurrentSingleSourceChange;

            if (Room is IEssentialsHuddleVtc1Room vtcRoom)
            {
                if (vtcRoom.ScheduleSource != null)
                {
                    var key = vtcRoom.Key + "-" + Key;

                    if (!AppServerController.CheckForDeviceMessenger(key))
                    {
                        var scheduleMessenger = new IHasScheduleAwarenessMessenger(key, vtcRoom.ScheduleSource,
                            $"/room/{vtcRoom.Key}");
                        AppServerController.AddDeviceMessenger(scheduleMessenger);
                    }
                }

                vtcRoom.InCallFeedback.OutputChange += InCallFeedback_OutputChange;
            }

            if (Room is IPrivacy privacyRoom)
            {
                AddAction("/volumes/master/privacyMuteToggle", (id, content) => privacyRoom.PrivacyModeToggle());

                privacyRoom.PrivacyModeIsOnFeedback.OutputChange += PrivacyModeIsOnFeedback_OutputChange;
            }


            if (Room is IRunDefaultCallRoute defCallRm)
            {
                AddAction("/activityVideo", (id, content) => defCallRm.RunDefaultCallRoute());
            }

            Room.OnFeedback.OutputChange += OnFeedback_OutputChange;
            Room.IsCoolingDownFeedback.OutputChange += IsCoolingDownFeedback_OutputChange;
            Room.IsWarmingUpFeedback.OutputChange += IsWarmingUpFeedback_OutputChange;

            AddTechRoomActions();
        }

        private void OnTouchPanelsUpdated(JToken content)
        {
            var message = content.ToObject<ApiTouchPanelToken>();

            _touchPanelTokens = message.TouchPanels;

            UpdateTouchPanelAppUrls(message.UserAppUrl);
        }

        private void UpdateTouchPanelAppUrls(string userAppUrl)
        {
            foreach (var tp in _touchPanelTokens)
            {
                var dev = DeviceManager.AllDevices.OfType<IMobileControlTouchpanelController>().FirstOrDefault((tpc) => tpc.Key.Equals(tp.TouchpanelKey, StringComparison.InvariantCultureIgnoreCase));

                if (dev == null)
                {
                    continue;
                }

                //UpdateAppUrl($"{userAppUrl}?token={tp.Token}");

                dev.SetAppUrl($"{userAppUrl}?token={tp.Token}");
            }
        }

        private void OnUserAppUpdated(JToken content)
        {
            var message = content.ToObject<ApiTouchPanelToken>();

            Debug.LogMessage(Serilog.Events.LogEventLevel.Information, "Updating User App URL to {userAppUrl}. Full Message: {@message}", this, message.UserAppUrl, content);

            UpdateTouchPanelAppUrls(message.UserAppUrl);
        }

        private void InCallFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            var state = new RoomStateMessage
            {
                IsInCall = e.BoolValue
            };
            PostStatusMessage(state);
        }

        private void GetRoom()
        {
            if (Room != null)
            {
                this.LogInformation("Room with key {key} already linked.", DefaultRoomKey);
                return;
            }


            if (!(DeviceManager.GetDeviceForKey(DefaultRoomKey) is IEssentialsRoom tempRoom))
            {
                this.LogInformation("Room with key {key} not found or is not an Essentials Room", DefaultRoomKey);
                return;
            }

            Room = tempRoom;
        }

        protected override void UserCodeChange()
        {
            Debug.LogMessage(Serilog.Events.LogEventLevel.Debug, "Server user code changed: {userCode}", this, UserCode);

            var qrUrl = string.Format("{0}/rooms/{1}/{3}/qr?x={2}", Parent?.Host, Parent?.SystemUuid, new Random().Next(), DefaultRoomKey);

            QrCodeUrl = qrUrl;

            this.LogDebug("Server user code changed: {userCode} - {qrUrl}", UserCode, qrUrl);

            OnUserCodeChanged();
        }

        /*        /// <summary>
                /// Override of base: calls base to add parent and then registers actions and events.
                /// </summary>
                /// <param name="parent"></param>
                public override void AddParent(MobileControlSystemController parent)
                {
                    base.AddParent(parent);

                }*/

        private void AddTechRoomActions()
        {
            if (!(Room is IEssentialsTechRoom techRoom))
            {
                return;
            }

            AddAction("/roomPowerOn", (id, content) => techRoom.RoomPowerOn());
            AddAction("/roomPowerOff", (id, content) => techRoom.RoomPowerOff());
        }

        private void PrivacyModeIsOnFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            var state = new RoomStateMessage();

            var volumes = new Dictionary<string, Volume>
            {
                { "master",  new Volume("master")
                    {
                        PrivacyMuted = e.BoolValue
                    }
                }
            };

            state.Volumes = volumes;

            PostStatusMessage(state);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsSharingFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            // sharing source 
            string shareText;
            bool isSharing;

            if (Room is IHasCurrentSourceInfoChange srcInfoRoom && (Room is IHasVideoCodec vcRoom && (vcRoom.VideoCodec.SharingContentIsOnFeedback.BoolValue && srcInfoRoom.CurrentSourceInfo != null)))
            {
                shareText = srcInfoRoom.CurrentSourceInfo.PreferredName;
                isSharing = true;
            }
            else
            {
                shareText = "None";
                isSharing = false;
            }

            var state = new RoomStateMessage
            {
                Share = new ShareState
                {
                    CurrentShareText = shareText,
                    IsSharing = isSharing
                }
            };

            PostStatusMessage(state);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsWarmingUpFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            var state = new
            {
                isWarmingUp = e.BoolValue
            };

            PostStatusMessage(JToken.FromObject(state));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsCoolingDownFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            var state = new
            {
                isCoolingDown = e.BoolValue
            };
            PostStatusMessage(JToken.FromObject(state));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            var state = new
            {
                isOn = e.BoolValue
            };
            PostStatusMessage(JToken.FromObject(state));
        }

        private void Room_CurrentVolumeDeviceChange(object sender, VolumeDeviceChangeEventArgs e)
        {
            if (e.OldDev is IBasicVolumeWithFeedback)
            {
                var oldDev = e.OldDev as IBasicVolumeWithFeedback;
                oldDev.MuteFeedback.OutputChange -= MuteFeedback_OutputChange;
                oldDev.VolumeLevelFeedback.OutputChange -= VolumeLevelFeedback_OutputChange;
            }

            if (e.NewDev is IBasicVolumeWithFeedback)
            {
                var newDev = e.NewDev as IBasicVolumeWithFeedback;
                newDev.MuteFeedback.OutputChange += MuteFeedback_OutputChange;
                newDev.VolumeLevelFeedback.OutputChange += VolumeLevelFeedback_OutputChange;
            }
        }

        /// <summary>
        /// Event handler for mute changes
        /// </summary>
        private void MuteFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            var state = new RoomStateMessage();

            var volumes = new Dictionary<string, Volume>
            {
                { "master", new Volume("master", e.BoolValue) }
            };

            state.Volumes = volumes;

            PostStatusMessage(state);
        }

        /// <summary>
        /// Handles Volume changes on room
        /// </summary>
        private void VolumeLevelFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {

            var state = new
            {
                volumes = new Dictionary<string, Volume>
                {
                    { "master", new Volume("master", e.IntValue) }
                }
            };
            PostStatusMessage(JToken.FromObject(state));
        }


        private void Room_CurrentSingleSourceChange(SourceListItem info, ChangeType type)
        {
            /* Example message
             * {
                  "type":"/room/status",
                  "content": {
                    "selectedSourceKey": "off",
                  }
                }
             */

        }

        /// <summary>
        /// Sends the full status of the room to the server
        /// </summary>
        /// <param name="room"></param>
        private void SendFullStatusForClientId(string id, IEssentialsRoom room)
        {
            //Parent.SendMessageObject(GetFullStatus(room));
            var message = GetFullStatusForClientId(room);

            if (message == null)
            {
                return;
            }
            PostStatusMessage(message, id);
        }


        /// <summary>
        /// Gets full room status
        /// </summary>
        /// <param name="room">The room to get status of</param>
        /// <returns>The status response message</returns>
        private RoomStateMessage GetFullStatusForClientId(IEssentialsRoom room)
        {
            try
            {
                this.LogVerbose("GetFullStatus");

                var sourceKey = room is IHasCurrentSourceInfoChange ? (room as IHasCurrentSourceInfoChange).CurrentSourceInfoKey : null;

                var volumes = new Dictionary<string, Volume>();
                if (room is IHasCurrentVolumeControls rmVc)
                {
                    if (rmVc.CurrentVolumeControls is IBasicVolumeWithFeedback vc)
                    {
                        var volume = new Volume("master", vc.VolumeLevelFeedback.UShortValue, vc.MuteFeedback.BoolValue, "Volume", true, "");
                        if (room is IPrivacy privacyRoom)
                        {
                            volume.HasPrivacyMute = true;
                            volume.PrivacyMuted = privacyRoom.PrivacyModeIsOnFeedback.BoolValue;
                        }

                        volumes.Add("master", volume);
                    }
                }

                var state = new RoomStateMessage
                {
                    Configuration = GetRoomConfiguration(room),
                    ActivityMode = 1,
                    IsOn = room.OnFeedback.BoolValue,
                    SelectedSourceKey = sourceKey,
                    Volumes = volumes,
                    IsWarmingUp = room.IsWarmingUpFeedback.BoolValue,
                    IsCoolingDown = room.IsCoolingDownFeedback.BoolValue
                };

                if (room is IEssentialsHuddleVtc1Room vtcRoom)
                {
                    state.IsInCall = vtcRoom.InCallFeedback.BoolValue;
                }

                return state;
            } catch (Exception ex)
            {
                Debug.LogMessage(ex, "Error getting full status", this);
                return null;
            }
        }

        /// <summary>
        /// Determines the configuration of the room and the details about the devices associated with the room
        /// <param name="room"></param>
        /// <returns></returns>
        private RoomConfiguration GetRoomConfiguration(IEssentialsRoom room)
        {
            try
            {
                var configuration = new RoomConfiguration
                {
                    //ShutdownPromptSeconds = room.ShutdownPromptSeconds,
                    TouchpanelKeys = DeviceManager.AllDevices.
                    OfType<IMobileControlTouchpanelController>()
                    .Where((tp) => tp.DefaultRoomKey.Equals(room.Key, StringComparison.InvariantCultureIgnoreCase))
                    .Select(tp => tp.Key).ToList()
                };

                try
                {
                    var zrcTp = DeviceManager.AllDevices.OfType<IMobileControlTouchpanelController>().SingleOrDefault((tp) => tp.ZoomRoomController);

                    configuration.ZoomRoomControllerKey = zrcTp != null ? zrcTp.Key : null;
                }
                catch
                {
                    configuration.ZoomRoomControllerKey = room.Key;
                }

                if (room is IHasCiscoNavigatorTouchpanel ciscoNavRoom)
                {
                    Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, $"Setting CiscoNavigatorKey to: {ciscoNavRoom.CiscoNavigatorTouchpanelKey}", this);
                    configuration.CiscoNavigatorKey = ciscoNavRoom.CiscoNavigatorTouchpanelKey;
                }



                // find the room combiner for this room by checking if the room is in the list of rooms for the room combiner
                var roomCombiner = DeviceManager.AllDevices.OfType<IEssentialsRoomCombiner>().FirstOrDefault();

                configuration.RoomCombinerKey = roomCombiner != null ? roomCombiner.Key : null;


                if (room is IEssentialsRoomPropertiesConfig propertiesConfig)
                {
                    configuration.HelpMessage = propertiesConfig.PropertiesConfig.HelpMessageForDisplay;
                }

                if (room is IEssentialsHuddleSpaceRoom huddleRoom && !string.IsNullOrEmpty(huddleRoom.PropertiesConfig.HelpMessageForDisplay))
                {
                    this.LogVerbose("Getting huddle room config");
                    configuration.HelpMessage = huddleRoom.PropertiesConfig.HelpMessageForDisplay;
                    configuration.UiBehavior = huddleRoom.PropertiesConfig.UiBehavior;
                    configuration.DefaultPresentationSourceKey = huddleRoom.PropertiesConfig.DefaultSourceItem;

                }

                if (room is IEssentialsHuddleVtc1Room vtc1Room && !string.IsNullOrEmpty(vtc1Room.PropertiesConfig.HelpMessageForDisplay))
                {
                    this.LogVerbose("Getting vtc room config");
                    configuration.HelpMessage = vtc1Room.PropertiesConfig.HelpMessageForDisplay;
                    configuration.UiBehavior = vtc1Room.PropertiesConfig.UiBehavior;
                    configuration.DefaultPresentationSourceKey = vtc1Room.PropertiesConfig.DefaultSourceItem;
                }

                if (room is IEssentialsTechRoom techRoom && !string.IsNullOrEmpty(techRoom.PropertiesConfig.HelpMessage))
                {
                    this.LogVerbose("Getting tech room config");
                    configuration.HelpMessage = techRoom.PropertiesConfig.HelpMessage;
                }

                if (room is IHasVideoCodec vcRoom)
                {
                    if (vcRoom.VideoCodec != null)
                    {
                        this.LogVerbose("Getting codec config");
                        var type = vcRoom.VideoCodec.GetType();

                        configuration.HasVideoConferencing = true;
                        configuration.VideoCodecKey = vcRoom.VideoCodec.Key;
                        configuration.VideoCodecIsZoomRoom = type.Name.Equals("ZoomRoom", StringComparison.InvariantCultureIgnoreCase);
                    }
                };

                if (room is IHasAudioCodec acRoom)
                {
                    if (acRoom.AudioCodec != null)
                    {
                        this.LogVerbose("Getting audio codec config");
                        configuration.HasAudioConferencing = true;
                        configuration.AudioCodecKey = acRoom.AudioCodec.Key;
                    }
                }


                if (room is IHasMatrixRouting matrixRoutingRoom)
                {
                    this.LogVerbose("Getting matrix routing config");
                    configuration.MatrixRoutingKey = matrixRoutingRoom.MatrixRoutingDeviceKey;
                    configuration.EndpointKeys = matrixRoutingRoom.EndpointKeys;
                }

                if (room is IEnvironmentalControls envRoom)
                {
                    this.LogVerbose("Getting environmental controls config. RoomHasEnvironmentalControls: {hasEnvironmentalControls}", envRoom.HasEnvironmentalControlDevices);
                    configuration.HasEnvironmentalControls = envRoom.HasEnvironmentalControlDevices;

                    if (envRoom.HasEnvironmentalControlDevices)
                    {
                        this.LogVerbose("Room Has {count} Environmental Control Devices.", envRoom.EnvironmentalControlDevices.Count);

                        foreach (var dev in envRoom.EnvironmentalControlDevices)
                        {
                            this.LogVerbose("Adding environmental device: {key}", dev.Key);

                            eEnvironmentalDeviceTypes type = eEnvironmentalDeviceTypes.None;

                            if (dev is ILightingScenes || dev is Devices.Common.Lighting.LightingBase)
                            {
                                type = eEnvironmentalDeviceTypes.Lighting;
                            }
                            else if (dev is ShadeBase || dev is IShadesOpenCloseStop || dev is IShadesOpenClosePreset)
                            {
                                type = eEnvironmentalDeviceTypes.Shade;
                            }
                            else if (dev is IShades)
                            {
                                type = eEnvironmentalDeviceTypes.ShadeController;
                            }
                            else if (dev is ISwitchedOutput)
                            {
                                type = eEnvironmentalDeviceTypes.Relay;
                            }

                            this.LogVerbose("Environmental Device Type: {type}", type);

                            var envDevice = new EnvironmentalDeviceConfiguration(dev.Key, type);

                            configuration.EnvironmentalDevices.Add(envDevice);
                        }
                    }
                    else
                    {
                        this.LogVerbose("Room Has No Environmental Control Devices");
                    }
                }

                if (room is IHasDefaultDisplay defDisplayRoom)
                {
                    this.LogVerbose("Getting default display config");
                    configuration.DefaultDisplayKey = defDisplayRoom.DefaultDisplay.Key;
                    configuration.Destinations.Add(eSourceListItemDestinationTypes.defaultDisplay, defDisplayRoom.DefaultDisplay.Key);
                }

                if (room is IHasMultipleDisplays multiDisplayRoom)
                {
                    this.LogVerbose("Getting multiple display config");

                    if (multiDisplayRoom.Displays == null)
                    {
                        this.LogVerbose("Displays collection is null");
                    }
                    else
                    {
                        this.LogVerbose("Displays collection exists");

                        configuration.Destinations = multiDisplayRoom.Displays.ToDictionary(kv => kv.Key, kv => kv.Value.Key);
                    }
                }

                if (room is IHasAccessoryDevices accRoom)
                {
                    Debug.LogMessage(Serilog.Events.LogEventLevel.Information, "Getting accessory devices config", this);

                    if (accRoom.AccessoryDeviceKeys == null)
                    {
                        Debug.LogMessage(Serilog.Events.LogEventLevel.Information, "Accessory devices collection is null", this);
                    }
                    else
                    {
                        Debug.LogMessage(Serilog.Events.LogEventLevel.Information, "Accessory devices collection exists", this);

                        configuration.AccessoryDeviceKeys = accRoom.AccessoryDeviceKeys;
                    }
                }

                var sourceList = ConfigReader.ConfigObject.GetSourceListForKey(room.SourceListKey);
                if (sourceList != null)
                {
                    this.LogVerbose("Getting source list config");
                    configuration.SourceList = sourceList;
                    configuration.HasRoutingControls = true;

                    foreach (var source in sourceList)
                    {
                        if (source.Value.SourceDevice is Devices.Common.IRSetTopBoxBase)
                        {
                            configuration.HasSetTopBoxControls = true;
                            continue;
                        }
                        else if (source.Value.SourceDevice is CameraBase)
                        {
                            configuration.HasCameraControls = true;
                            continue;
                        }
                    }
                }

                var destinationList = ConfigReader.ConfigObject.GetDestinationListForKey(room.DestinationListKey);

                if (destinationList != null)
                {
                    configuration.DestinationList = destinationList;
                }

                var audioControlPointList = ConfigReader.ConfigObject.GetAudioControlPointListForKey(room.AudioControlPointListKey);

                if (audioControlPointList != null)
                {
                    configuration.AudioControlPointList = audioControlPointList;
                }

                var cameraList = ConfigReader.ConfigObject.GetCameraListForKey(room.CameraListKey);

                if (cameraList != null)
                {
                    configuration.CameraList = cameraList;
                }

                return configuration;

            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Exception getting room configuration");
                return new RoomConfiguration();
            }
        }

    }

    public class RoomStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("configuration", NullValueHandling = NullValueHandling.Ignore)]
        public RoomConfiguration Configuration { get; set; }

        [JsonProperty("activityMode", NullValueHandling = NullValueHandling.Ignore)]
        public int? ActivityMode { get; set; }
        [JsonProperty("advancedSharingActive", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AdvancedSharingActive { get; set; }
        [JsonProperty("isOn", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsOn { get; set; }
        [JsonProperty("isWarmingUp", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsWarmingUp { get; set; }
        [JsonProperty("isCoolingDown", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsCoolingDown { get; set; }
        [JsonProperty("selectedSourceKey", NullValueHandling = NullValueHandling.Ignore)]
        public string SelectedSourceKey { get; set; }
        [JsonProperty("share", NullValueHandling = NullValueHandling.Ignore)]
        public ShareState Share { get; set; }

        [JsonProperty("volumes", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, Volume> Volumes { get; set; }

        [JsonProperty("isInCall", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsInCall { get; set; }
    }

    public class ShareState
    {
        [JsonProperty("currentShareText", NullValueHandling = NullValueHandling.Ignore)]
        public string CurrentShareText { get; set; }
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Enabled { get; set; }
        [JsonProperty("isSharing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsSharing { get; set; }
    }

    /// <summary>
    /// Represents the capabilities of the room and the associated device info
    /// </summary>
    public class RoomConfiguration
    {
        //[JsonProperty("shutdownPromptSeconds", NullValueHandling = NullValueHandling.Ignore)]
        //public int? ShutdownPromptSeconds { get; set; }

        [JsonProperty("hasVideoConferencing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasVideoConferencing { get; set; }
        [JsonProperty("videoCodecIsZoomRoom", NullValueHandling = NullValueHandling.Ignore)]
        public bool? VideoCodecIsZoomRoom { get; set; }
        [JsonProperty("hasAudioConferencing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasAudioConferencing { get; set; }
        [JsonProperty("hasEnvironmentalControls", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasEnvironmentalControls { get; set; }
        [JsonProperty("hasCameraControls", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasCameraControls { get; set; }
        [JsonProperty("hasSetTopBoxControls", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasSetTopBoxControls { get; set; }
        [JsonProperty("hasRoutingControls", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasRoutingControls { get; set; }

        [JsonProperty("touchpanelKeys", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> TouchpanelKeys { get; set; }

        [JsonProperty("zoomRoomControllerKey", NullValueHandling = NullValueHandling.Ignore)]
        public string ZoomRoomControllerKey { get; set; }

        [JsonProperty("ciscoNavigatorKey", NullValueHandling = NullValueHandling.Ignore)]
        public string CiscoNavigatorKey { get; set; }


        [JsonProperty("videoCodecKey", NullValueHandling = NullValueHandling.Ignore)]
        public string VideoCodecKey { get; set; }
        [JsonProperty("audioCodecKey", NullValueHandling = NullValueHandling.Ignore)]
        public string AudioCodecKey { get; set; }
        [JsonProperty("matrixRoutingKey", NullValueHandling = NullValueHandling.Ignore)]
        public string MatrixRoutingKey { get; set; }
        [JsonProperty("endpointKeys", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> EndpointKeys { get; set; }

        [JsonProperty("accessoryDeviceKeys", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> AccessoryDeviceKeys { get; set; }

        [JsonProperty("defaultDisplayKey", NullValueHandling = NullValueHandling.Ignore)]
        public string DefaultDisplayKey { get; set; }
        [JsonProperty("destinations", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<eSourceListItemDestinationTypes, string> Destinations { get; set; }
        [JsonProperty("environmentalDevices", NullValueHandling = NullValueHandling.Ignore)]
        public List<EnvironmentalDeviceConfiguration> EnvironmentalDevices { get; set; }
        [JsonProperty("sourceList", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, SourceListItem> SourceList { get; set; }

        [JsonProperty("destinationList", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string,  DestinationListItem> DestinationList { get; set;}

        [JsonProperty("audioControlPointList", NullValueHandling = NullValueHandling.Ignore)]
        public AudioControlPointListItem AudioControlPointList { get; set; }

        [JsonProperty("cameraList", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, CameraListItem> CameraList { get; set; }

        [JsonProperty("defaultPresentationSourceKey", NullValueHandling = NullValueHandling.Ignore)]
        public string DefaultPresentationSourceKey { get; set; }


        [JsonProperty("helpMessage", NullValueHandling = NullValueHandling.Ignore)]
        public string HelpMessage { get; set; }

        [JsonProperty("techPassword", NullValueHandling = NullValueHandling.Ignore)]
        public string TechPassword { get; set; }

        [JsonProperty("uiBehavior", NullValueHandling = NullValueHandling.Ignore)]
        public EssentialsRoomUiBehaviorConfig UiBehavior { get; set; }

        [JsonProperty("supportsAdvancedSharing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SupportsAdvancedSharing { get; set; }
        [JsonProperty("userCanChangeShareMode", NullValueHandling = NullValueHandling.Ignore)]
        public bool? UserCanChangeShareMode { get; set; }

        [JsonProperty("roomCombinerKey", NullValueHandling = NullValueHandling.Ignore)]
        public string RoomCombinerKey { get; set; }

        public RoomConfiguration()
        {
            Destinations = new Dictionary<eSourceListItemDestinationTypes, string>();
            EnvironmentalDevices = new List<EnvironmentalDeviceConfiguration>();
            SourceList = new Dictionary<string, SourceListItem>();
            TouchpanelKeys = new List<string>();
        }
    }

    public class EnvironmentalDeviceConfiguration
    {
        [JsonProperty("deviceKey", NullValueHandling = NullValueHandling.Ignore)]
        public string DeviceKey { get; private set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("deviceType", NullValueHandling = NullValueHandling.Ignore)]
        public eEnvironmentalDeviceTypes DeviceType { get; private set; }

        public EnvironmentalDeviceConfiguration(string key, eEnvironmentalDeviceTypes type)
        {
            DeviceKey = key;
            DeviceType = type;
        }
    }

    public enum eEnvironmentalDeviceTypes
    {
        None,
        Lighting,
        Shade,
        ShadeController,
        Relay,
    }

    public class ApiTouchPanelToken
    {
        [JsonProperty("touchPanels", NullValueHandling = NullValueHandling.Ignore)]
        public List<JoinToken> TouchPanels { get; set; } = new List<JoinToken>();

        [JsonProperty("userAppUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string UserAppUrl { get; set; } = "";
    }

#if SERIES3
    public class SourceSelectMessageContent
    {
        public string SourceListItem { get; set; }
        public string SourceListKey { get; set; }
    }

    public class DirectRoute
    {
        public string SourceKey { get; set; }
        public string DestinationKey { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="b"></param>
    public delegate void PressAndHoldAction(bool b);
#endif
}