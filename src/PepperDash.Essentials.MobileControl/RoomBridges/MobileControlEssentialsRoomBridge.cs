using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.AppServer;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.CrestronIO;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Lighting;
using PepperDash.Essentials.Core.Shades;
using PepperDash.Essentials.Devices.Common.AudioCodec;
using PepperDash.Essentials.Devices.Common.Cameras;
using PepperDash.Essentials.Devices.Common.Room;
using PepperDash.Essentials.Devices.Common.VideoCodec;
using PepperDash.Essentials.Room.Config;
using PepperDash.Essentials.WebSocketServer;
using IShades = PepperDash.Essentials.Core.Shades.IShades;
using ShadeBase = PepperDash.Essentials.Devices.Common.Shades.ShadeBase;

namespace PepperDash.Essentials.RoomBridges
{
    /// <summary>
    /// Represents a MobileControlEssentialsRoomBridge
    /// </summary>
    public class MobileControlEssentialsRoomBridge : MobileControlBridgeBase
    {
        private List<JoinToken> _touchPanelTokens = new List<JoinToken>();
        /// <summary>
        /// Gets or sets the Room
        /// </summary>
        public IEssentialsRoom Room { get; private set; }

        /// <summary>
        /// Gets or sets the DefaultRoomKey
        /// </summary>
        public string DefaultRoomKey { get; private set; }
        /// <summary>
        /// Gets the name of the room
        /// </summary>
        public override string RoomName
        {
            get { return Room.Name; }
        }

        /// <summary>
        /// Gets the key of the room
        /// </summary>
        public override string RoomKey
        {
            get { return Room.Key; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MobileControlEssentialsRoomBridge"/> class with the specified room
        /// </summary>
        /// <param name="room">The essentials room to bridge</param>
        public MobileControlEssentialsRoomBridge(IEssentialsRoom room) :
            this($"mobileControlBridge-{room.Key}", room.Key, room)
        {
            Room = room;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MobileControlEssentialsRoomBridge"/> class with the specified parameters
        /// </summary>
        /// <param name="key">The unique key for this bridge</param>
        /// <param name="roomKey">The key of the room to bridge</param>
        /// <param name="room">The essentials room to bridge</param>
        public MobileControlEssentialsRoomBridge(string key, string roomKey, IEssentialsRoom room) : base(key, $"/room/{room.Key}", room as Device)
        {
            DefaultRoomKey = roomKey;

            AddPreActivationAction(GetRoom);
        }

        /// <summary>
        /// Registers all message handling actions with the AppServer for this room bridge
        /// </summary>
        protected override void RegisterActions()
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

        /// <summary>
        /// Handles user code changes and generates QR code URL
        /// </summary>
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
                /// <summary>
                /// AddParent method
                /// </summary>
                /// <inheritdoc />
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

            if (Room is IHasCurrentSourceInfoChange srcInfoRoom && Room is IHasVideoCodec vcRoom && vcRoom.VideoCodec.SharingContentIsOnFeedback.BoolValue && srcInfoRoom.CurrentSourceInfo != null)
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
        /// <param name="id"></param>
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
            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Error getting full status", this);
                return null;
            }
        }

        /// <summary>
        /// Determines the configuration of the room and the details about the devices associated with the room
        /// </summary>
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

                    configuration.ZoomRoomControllerKey = zrcTp?.Key;
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

                configuration.RoomCombinerKey = roomCombiner?.Key;


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
                }
                ;

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

                            if (dev is ILightingScenes)
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

    /// <summary>
    /// Represents a RoomStateMessage
    /// </summary>
    public class RoomStateMessage : DeviceStateMessageBase
    {

        /// <summary>
        /// Gets or sets the Configuration
        /// </summary>
        [JsonProperty("configuration", NullValueHandling = NullValueHandling.Ignore)]
        public RoomConfiguration Configuration { get; set; }

        /// <summary>
        /// Gets or sets the activity mode of the room
        /// </summary>
        [JsonProperty("activityMode", NullValueHandling = NullValueHandling.Ignore)]
        public int? ActivityMode { get; set; }

        /// <summary>
        /// Gets or sets whether advanced sharing is active
        /// </summary>
        [JsonProperty("advancedSharingActive", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AdvancedSharingActive { get; set; }

        /// <summary>
        /// Gets or sets whether the room is powered on
        /// </summary>
        [JsonProperty("isOn", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsOn { get; set; }

        /// <summary>
        /// Gets or sets whether the room is warming up
        /// </summary>
        [JsonProperty("isWarmingUp", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsWarmingUp { get; set; }

        /// <summary>
        /// Gets or sets whether the room is cooling down
        /// </summary>
        [JsonProperty("isCoolingDown", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsCoolingDown { get; set; }

        /// <summary>
        /// Gets or sets the SelectedSourceKey
        /// </summary>
        [JsonProperty("selectedSourceKey", NullValueHandling = NullValueHandling.Ignore)]
        public string SelectedSourceKey { get; set; }

        /// <summary>
        /// Gets or sets the Share
        /// </summary>
        [JsonProperty("share", NullValueHandling = NullValueHandling.Ignore)]
        public ShareState Share { get; set; }

        /// <summary>
        /// Gets or sets the volume controls collection
        /// </summary>
        [JsonProperty("volumes", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, Volume> Volumes { get; set; }

        /// <summary>
        /// Gets or sets whether the room is in a call
        /// </summary>
        [JsonProperty("isInCall", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsInCall { get; set; }
    }

    /// <summary>
    /// Represents a ShareState
    /// </summary>
    public class ShareState
    {

        /// <summary>
        /// Gets or sets the CurrentShareText
        /// </summary>
        [JsonProperty("currentShareText", NullValueHandling = NullValueHandling.Ignore)]
        public string CurrentShareText { get; set; }

        /// <summary>
        /// Gets or sets whether sharing is enabled
        /// </summary>
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Enabled { get; set; }

        /// <summary>
        /// Gets or sets whether content is currently being shared
        /// </summary>
        [JsonProperty("isSharing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsSharing { get; set; }
    }

    /// <summary>
    /// Represents a RoomConfiguration
    /// </summary>
    public class RoomConfiguration
    {
        /// <summary>
        /// Gets or sets whether the room has video conferencing capabilities
        /// </summary>
        [JsonProperty("hasVideoConferencing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasVideoConferencing { get; set; }

        /// <summary>
        /// Gets or sets whether the video codec is a Zoom Room
        /// </summary>
        [JsonProperty("videoCodecIsZoomRoom", NullValueHandling = NullValueHandling.Ignore)]
        public bool? VideoCodecIsZoomRoom { get; set; }

        /// <summary>
        /// Gets or sets whether the room has audio conferencing capabilities
        /// </summary>
        [JsonProperty("hasAudioConferencing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasAudioConferencing { get; set; }

        /// <summary>
        /// Gets or sets whether the room has environmental controls (lighting, shades, etc.)
        /// </summary>
        [JsonProperty("hasEnvironmentalControls", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasEnvironmentalControls { get; set; }

        /// <summary>
        /// Gets or sets whether the room has camera controls
        /// </summary>
        [JsonProperty("hasCameraControls", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasCameraControls { get; set; }

        /// <summary>
        /// Gets or sets whether the room has set-top box controls
        /// </summary>
        [JsonProperty("hasSetTopBoxControls", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasSetTopBoxControls { get; set; }

        /// <summary>
        /// Gets or sets whether the room has routing controls
        /// </summary>
        [JsonProperty("hasRoutingControls", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasRoutingControls { get; set; }

        /// <summary>
        /// Gets or sets the TouchpanelKeys
        /// </summary>
        [JsonProperty("touchpanelKeys", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> TouchpanelKeys { get; set; }


        /// <summary>
        /// Gets or sets the ZoomRoomControllerKey
        /// </summary>
        [JsonProperty("zoomRoomControllerKey", NullValueHandling = NullValueHandling.Ignore)]
        public string ZoomRoomControllerKey { get; set; }


        /// <summary>
        /// Gets or sets the CiscoNavigatorKey
        /// </summary>
        [JsonProperty("ciscoNavigatorKey", NullValueHandling = NullValueHandling.Ignore)]
        public string CiscoNavigatorKey { get; set; }



        /// <summary>
        /// Gets or sets the VideoCodecKey
        /// </summary>
        [JsonProperty("videoCodecKey", NullValueHandling = NullValueHandling.Ignore)]
        public string VideoCodecKey { get; set; }


        /// <summary>
        /// Gets or sets the AudioCodecKey
        /// </summary>
        [JsonProperty("audioCodecKey", NullValueHandling = NullValueHandling.Ignore)]
        public string AudioCodecKey { get; set; }


        /// <summary>
        /// Gets or sets the MatrixRoutingKey
        /// </summary>
        [JsonProperty("matrixRoutingKey", NullValueHandling = NullValueHandling.Ignore)]
        public string MatrixRoutingKey { get; set; }


        /// <summary>
        /// Gets or sets the EndpointKeys
        /// </summary>
        [JsonProperty("endpointKeys", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> EndpointKeys { get; set; }


        /// <summary>
        /// Gets or sets the AccessoryDeviceKeys
        /// </summary>
        [JsonProperty("accessoryDeviceKeys", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> AccessoryDeviceKeys { get; set; }


        /// <summary>
        /// Gets or sets the DefaultDisplayKey
        /// </summary>
        [JsonProperty("defaultDisplayKey", NullValueHandling = NullValueHandling.Ignore)]
        public string DefaultDisplayKey { get; set; }

        /// <summary>
        /// Gets or sets the destinations dictionary keyed by destination type
        /// </summary>
        [JsonProperty("destinations", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<eSourceListItemDestinationTypes, string> Destinations { get; set; }


        /// <summary>
        /// Gets or sets the EnvironmentalDevices
        /// </summary>
        [JsonProperty("environmentalDevices", NullValueHandling = NullValueHandling.Ignore)]
        public List<EnvironmentalDeviceConfiguration> EnvironmentalDevices { get; set; }

        /// <summary>
        /// Gets or sets the source list for the room
        /// </summary>
        [JsonProperty("sourceList", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, SourceListItem> SourceList { get; set; }

        /// <summary>
        /// Gets or sets the destination list for the room
        /// </summary>
        [JsonProperty("destinationList", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, DestinationListItem> DestinationList { get; set; }


        /// <summary>
        /// Gets or sets the AudioControlPointList
        /// </summary>
        [JsonProperty("audioControlPointList", NullValueHandling = NullValueHandling.Ignore)]
        public AudioControlPointListItem AudioControlPointList { get; set; }

        /// <summary>
        /// Gets or sets the camera list for the room
        /// </summary>
        [JsonProperty("cameraList", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, CameraListItem> CameraList { get; set; }


        /// <summary>
        /// Gets or sets the DefaultPresentationSourceKey
        /// </summary>
        [JsonProperty("defaultPresentationSourceKey", NullValueHandling = NullValueHandling.Ignore)]
        public string DefaultPresentationSourceKey { get; set; }



        /// <summary>
        /// Gets or sets the HelpMessage
        /// </summary>
        [JsonProperty("helpMessage", NullValueHandling = NullValueHandling.Ignore)]
        public string HelpMessage { get; set; }


        /// <summary>
        /// Gets or sets the TechPassword
        /// </summary>
        [JsonProperty("techPassword", NullValueHandling = NullValueHandling.Ignore)]
        public string TechPassword { get; set; }


        /// <summary>
        /// Gets or sets the UiBehavior
        /// </summary>
        [JsonProperty("uiBehavior", NullValueHandling = NullValueHandling.Ignore)]
        public EssentialsRoomUiBehaviorConfig UiBehavior { get; set; }

        /// <summary>
        /// Gets or sets whether the room supports advanced sharing features
        /// </summary>
        [JsonProperty("supportsAdvancedSharing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SupportsAdvancedSharing { get; set; }

        /// <summary>
        /// Gets or sets whether the user can change the share mode
        /// </summary>
        [JsonProperty("userCanChangeShareMode", NullValueHandling = NullValueHandling.Ignore)]
        public bool? UserCanChangeShareMode { get; set; }


        /// <summary>
        /// Gets or sets the RoomCombinerKey
        /// </summary>
        [JsonProperty("roomCombinerKey", NullValueHandling = NullValueHandling.Ignore)]
        public string RoomCombinerKey { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomConfiguration"/> class
        /// </summary>
        public RoomConfiguration()
        {
            Destinations = new Dictionary<eSourceListItemDestinationTypes, string>();
            EnvironmentalDevices = new List<EnvironmentalDeviceConfiguration>();
            SourceList = new Dictionary<string, SourceListItem>();
            TouchpanelKeys = new List<string>();
        }
    }

    /// <summary>
    /// Represents a EnvironmentalDeviceConfiguration
    /// </summary>
    public class EnvironmentalDeviceConfiguration
    {

        /// <summary>
        /// Gets or sets the DeviceKey
        /// </summary>
        [JsonProperty("deviceKey", NullValueHandling = NullValueHandling.Ignore)]
        public string DeviceKey { get; private set; }


        /// <summary>
        /// Gets or sets the DeviceType
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("deviceType", NullValueHandling = NullValueHandling.Ignore)]
        public eEnvironmentalDeviceTypes DeviceType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentalDeviceConfiguration"/> class
        /// </summary>
        /// <param name="key">The device key</param>
        /// <param name="type">The environmental device type</param>
        public EnvironmentalDeviceConfiguration(string key, eEnvironmentalDeviceTypes type)
        {
            DeviceKey = key;
            DeviceType = type;
        }
    }

    /// <summary>
    /// Enumeration of environmental device types
    /// </summary>
    public enum eEnvironmentalDeviceTypes
    {
        /// <summary>
        /// No environmental device type specified
        /// </summary>
        None,
        /// <summary>
        /// Lighting device type
        /// </summary>
        Lighting,
        /// <summary>
        /// Shade device type
        /// </summary>
        Shade,
        /// <summary>
        /// Shade controller device type
        /// </summary>
        ShadeController,
        /// <summary>
        /// Relay device type
        /// </summary>
        Relay,
    }

    /// <summary>
    /// Represents a ApiTouchPanelToken
    /// </summary>
    public class ApiTouchPanelToken
    {

        /// <summary>
        /// Gets or sets the TouchPanels
        /// </summary>
        [JsonProperty("touchPanels", NullValueHandling = NullValueHandling.Ignore)]
        public List<JoinToken> TouchPanels { get; set; } = new List<JoinToken>();


        /// <summary>
        /// Gets or sets the UserAppUrl
        /// </summary>
        [JsonProperty("userAppUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string UserAppUrl { get; set; } = "";
    }
}