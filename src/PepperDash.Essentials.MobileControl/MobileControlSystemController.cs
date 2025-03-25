using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Net.Http;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Prng;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.AppServer;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.CrestronIO;
using PepperDash.Essentials.Core.DeviceInfo;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Lighting;
using PepperDash.Essentials.Core.Monitoring;
using PepperDash.Essentials.Core.Queues;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Core.Shades;
using PepperDash.Essentials.Core.Web;
using PepperDash.Essentials.Devices.Common.AudioCodec;
using PepperDash.Essentials.Devices.Common.Cameras;
using PepperDash.Essentials.Devices.Common.SoftCodec;
using PepperDash.Essentials.Devices.Common.VideoCodec;
using PepperDash.Essentials.Room.MobileControl;
using PepperDash.Essentials.Services;
using PepperDash.Essentials.WebApiHandlers;
using Serilog.Events;
using WebSocketSharp;
using DisplayBase = PepperDash.Essentials.Devices.Common.Displays.DisplayBase;
using TwoWayDisplayBase = PepperDash.Essentials.Devices.Common.Displays.TwoWayDisplayBase;
#if SERIES4
#endif

namespace PepperDash.Essentials
{
    public class MobileControlSystemController : EssentialsDevice, IMobileControl
    {
        private bool _initialized = false;
        private const long ServerReconnectInterval = 5000;
        private const long PingInterval = 25000;

        private readonly Dictionary<string, List<IMobileControlAction>> _actionDictionary =
            new Dictionary<string, List<IMobileControlAction>>(
                StringComparer.InvariantCultureIgnoreCase
            );

        public Dictionary<string, List<IMobileControlAction>> ActionDictionary => _actionDictionary;

        private readonly GenericQueue _receiveQueue;
        private readonly List<MobileControlBridgeBase> _roomBridges =
            new List<MobileControlBridgeBase>();

#if SERIES4
        private readonly Dictionary<string, IMobileControlMessenger> _messengers =
            new Dictionary<string, IMobileControlMessenger>();

        private readonly Dictionary<string, IMobileControlMessenger> _defaultMessengers =
            new Dictionary<string, IMobileControlMessenger>();
#else
        private readonly Dictionary<string, MessengerBase> _deviceMessengers =
            new Dictionary<string, MessengerBase>();
#endif

        private readonly GenericQueue _transmitToServerQueue;

        private readonly GenericQueue _transmitToClientsQueue;

        private bool _disableReconnect;
        private WebSocket _wsClient2;

        public MobileControlApiService ApiService { get; private set; }

        public List<MobileControlBridgeBase> RoomBridges => _roomBridges;

#if SERIES4
        private readonly MobileControlWebsocketServer _directServer;

        public MobileControlWebsocketServer DirectServer => _directServer;
#endif
        private readonly CCriticalSection _wsCriticalSection = new CCriticalSection();

        public string SystemUrl; //set only from SIMPL Bridge!

        public bool Connected => _wsClient2 != null && _wsClient2.IsAlive;

        private IEssentialsRoomCombiner _roomCombiner;

        public string SystemUuid
        {
            get
            {
                // Check to see if the SystemUuid value is populated. If not populated from configuration, check for value from SIMPL bridge.
                if (
                    !string.IsNullOrEmpty(ConfigReader.ConfigObject.SystemUuid)
                    && ConfigReader.ConfigObject.SystemUuid != "missing url"
                )
                {
                    return ConfigReader.ConfigObject.SystemUuid;
                }

                Debug.Console(
                    0,
                    this,
                    Debug.ErrorLogLevel.Notice,
                    "No system_url value defined in config.  Checking for value from SIMPL Bridge."
                );

                if (!string.IsNullOrEmpty(SystemUrl))
                {
                    Debug.Console(
                        0,
                        this,
                        Debug.ErrorLogLevel.Error,
                        "No system_url value defined in config or SIMPL Bridge.  Unable to connect to Mobile Control."
                    );
                    return string.Empty;
                }

                var result = Regex.Match(SystemUrl, @"https?:\/\/.*\/systems\/(.*)\/#.*");
                string uuid = result.Groups[1].Value;
                return uuid;
            }
        }

        public BoolFeedback ApiOnlineAndAuthorized { get; private set; }

        /// <summary>
        /// Used for tracking HTTP debugging
        /// </summary>
        private bool _httpDebugEnabled;

        private bool _isAuthorized;

        /// <summary>
        /// Tracks if the system is authorized to the API server
        /// </summary>
        public bool IsAuthorized
        {
            get { return _isAuthorized; }
            private set
            {
                if (value == _isAuthorized)
                    return;

                _isAuthorized = value;
                ApiOnlineAndAuthorized.FireUpdate();
            }
        }

        private DateTime _lastAckMessage;

        public DateTime LastAckMessage => _lastAckMessage;

        private CTimer _pingTimer;

        private CTimer _serverReconnectTimer;
        private LogLevel _wsLogLevel = LogLevel.Error;

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public MobileControlSystemController(string key, string name, MobileControlConfig config)
            : base(key, name)
        {
            Config = config;
            
            // The queue that will collect the incoming messages in the order they are received
            //_receiveQueue = new ReceiveQueue(key, ParseStreamRx);
            _receiveQueue = new GenericQueue(
                key + "-rxqueue",
                Crestron.SimplSharpPro.CrestronThread.Thread.eThreadPriority.HighPriority,
                25
            );

            // The queue that will collect the outgoing messages in the order they are received
            _transmitToServerQueue = new GenericQueue(
                key + "-txqueue",
                Crestron.SimplSharpPro.CrestronThread.Thread.eThreadPriority.HighPriority,
                25
            );

#if SERIES4
            if (Config.DirectServer != null && Config.DirectServer.EnableDirectServer)
            {
                _directServer = new MobileControlWebsocketServer(
                    Key + "-directServer",
                    Config.DirectServer.Port,
                    this
                );
                DeviceManager.AddDevice(_directServer);

                _transmitToClientsQueue = new GenericQueue(
                    key + "-clienttxqueue",
                    Crestron.SimplSharpPro.CrestronThread.Thread.eThreadPriority.HighPriority,
                    25
                );
            }
#endif

            Host = config.ServerUrl;
            if (!Host.StartsWith("http"))
            {
                Host = "https://" + Host;
            }

            ApiService = new MobileControlApiService(Host);

            Debug.Console(
                0,
                this,
                "Mobile UI controller initializing for server:{0}",
                config.ServerUrl
            );

            if (Global.Platform == eDevicePlatform.Appliance)
            {
                AddConsoleCommands();
            }

            AddPreActivationAction(() => LinkSystemMonitorToAppServer());

            AddPreActivationAction(() => SetupDefaultDeviceMessengers());

            AddPreActivationAction(() => SetupDefaultRoomMessengers());

            AddPreActivationAction(() => AddWebApiPaths());

            AddPreActivationAction(() =>
            {
                _roomCombiner = DeviceManager.AllDevices.OfType<IEssentialsRoomCombiner>().FirstOrDefault();

                if(_roomCombiner == null)
                    return;

                _roomCombiner.RoomCombinationScenarioChanged += OnRoomCombinationScenarioChanged;
            });

            CrestronEnvironment.ProgramStatusEventHandler +=
                CrestronEnvironment_ProgramStatusEventHandler;

            ApiOnlineAndAuthorized = new BoolFeedback(() =>
            {
                if (_wsClient2 == null)
                    return false;

                return _wsClient2.IsAlive && IsAuthorized;
            });
        }        

        private void SetupDefaultRoomMessengers()
        {
            Debug.LogMessage(LogEventLevel.Verbose, "Setting up room messengers", this);
            foreach (var room in DeviceManager.AllDevices.OfType<IEssentialsRoom>())
            {
                Debug.LogMessage(
                    LogEventLevel.Verbose,
                    "Setting up room messengers for room: {key}",
                    this,
                    room.Key
                );
                var messenger = new MobileControlEssentialsRoomBridge(room);

                messenger.AddParent(this);

                _roomBridges.Add(messenger);

                AddDefaultDeviceMessenger(messenger);

                Debug.LogMessage(
                    LogEventLevel.Verbose,
                    "Attempting to set up default room messengers for room: {0}",
                    this,
                    room.Key
                );

                if (room is IRoomEventSchedule)
                {
                    Debug.LogMessage(LogEventLevel.Information, "Setting up event schedule messenger for room: {key}", this, room.Key);

                    var scheduleMessenger = new RoomEventScheduleMessenger(
                        $"{room.Key}-schedule-{Key}",
                        string.Format("/room/{0}", room.Key),
                        room as IRoomEventSchedule
                    );

                    AddDefaultDeviceMessenger(scheduleMessenger);
                }

                if (room is ITechPassword)
                {
                    Debug.LogMessage(LogEventLevel.Information, "Setting up tech password messenger for room: {key}", this, room.Key);

                    var techPasswordMessenger = new ITechPasswordMessenger(
                        $"{room.Key}-techPassword-{Key}",
                        string.Format("/room/{0}", room.Key),
                        room as ITechPassword
                    );

                    AddDefaultDeviceMessenger(techPasswordMessenger);
                }

                if (room is IShutdownPromptTimer)
                {
                    Debug.LogMessage(LogEventLevel.Information, "Setting up shutdown prompt timer messenger for room: {key}", this, room.Key);

                    var shutdownPromptTimerMessenger = new IShutdownPromptTimerMessenger(
                        $"{room.Key}-shutdownPromptTimer-{Key}",
                        string.Format("/room/{0}", room.Key),
                        room as IShutdownPromptTimer
                    );

                    AddDefaultDeviceMessenger(shutdownPromptTimerMessenger);
                }

                if (room is ILevelControls levelControls)
                {
                    Debug.LogMessage(LogEventLevel.Information, "Setting up level controls messenger for room: {key}", this, room.Key);

                    var levelControlsMessenger = new ILevelControlsMessenger(
                        $"{room.Key}-levelControls-{Key}",
                        $"/room/{room.Key}",
                        levelControls
                    );

                    AddDefaultDeviceMessenger(levelControlsMessenger);
                }
            }
        }

        /// <summary>
        /// Set up the messengers for each device type
        /// </summary>
        private void SetupDefaultDeviceMessengers()
        {
            bool messengerAdded = false;
            var allDevices = DeviceManager.AllDevices.Where((d) => !(d is IEssentialsRoom));
            Debug.LogMessage(
                LogEventLevel.Verbose,
                "All Devices that aren't rooms count: {0}",
                this,
                allDevices?.Count()
            );
            var count = allDevices.Count();
            foreach (var device in allDevices)
            {
                try
                {
                    Debug.LogMessage(
                        LogEventLevel.Verbose,
                        "Attempting to set up device messengers for device: {0}",
                        this,
                        device.Key
                    );
                    // StatusMonitorBase which is prop of ICommunicationMonitor is not a PepperDash.Core.Device, but is in the device array
                    if (device is ICommunicationMonitor)
                    {
                        Debug.LogMessage(
                            LogEventLevel.Verbose,
                            "Trying to cast to ICommunicationMonitor for device: {0}",
                            this,
                            device.Key
                        );
                        var commMonitor = device as ICommunicationMonitor;
                        if (commMonitor == null)
                        {
                            Debug.LogMessage(
                                LogEventLevel.Debug,
                                "[Error] CommunicationMonitor cast is null for device: {0}. Skipping CommunicationMonitorMessenger",
                                this,
                                device.Key
                            );
                            Debug.LogMessage(
                                LogEventLevel.Debug,
                                "AllDevices Completed a device. Devices Left: {0}",
                                this,
                                --count
                            );
                            continue;
                        }
                        Debug.LogMessage(
                            LogEventLevel.Debug,
                            "Adding CommunicationMonitorMessenger for device: {0}",
                            this,
                            device.Key
                        );
                        var commMessenger = new ICommunicationMonitorMessenger(
                            $"{device.Key}-commMonitor-{Key}",
                            string.Format("/device/{0}", device.Key),
                            commMonitor
                        );
                        AddDefaultDeviceMessenger(commMessenger);
                        messengerAdded = true;
                    }

                    if (device is CameraBase)
                    {
                        Debug.Console(
                            2,
                            this,
                            "Adding CameraBaseMessenger for device: {0}",
                            device.Key
                        );

                        var cameraMessenger = new CameraBaseMessenger(
                            $"{device.Key}-cameraBase-{Key}",
                            device as CameraBase,
                            $"/device/{device.Key}"
                        );

                        AddDefaultDeviceMessenger(cameraMessenger);

                        messengerAdded = true;
                    }

                    if (device is BlueJeansPc)
                    {
                        Debug.Console(
                            2,
                            this,
                            "Adding IRunRouteActionMessnger for device: {0}",
                            device.Key
                        );

                        var routeMessenger = new RunRouteActionMessenger(
                            $"{device.Key}-runRouteAction-{Key}",
                            device as BlueJeansPc,
                            $"/device/{device.Key}"
                        );

                        AddDefaultDeviceMessenger(routeMessenger);

                        messengerAdded = true;
                    }

                    if (device is ITvPresetsProvider)
                    {
                        Debug.LogMessage(
                            LogEventLevel.Verbose,
                            "Trying to cast to ITvPresetsProvider for device: {0}",
                            this,
                            device.Key
                        );

                        var presetsDevice = device as ITvPresetsProvider;

                        if (presetsDevice.TvPresets == null)
                        {
                            Debug.Console(
                                2,
                                this,
                                "TvPresets is null for device: '{0}'. Skipping DevicePresetsModelMessenger",
                                device.Key
                            );
                        }
                        else
                        {
                            Debug.Console(
                                2,
                                this,
                                "Adding ITvPresetsProvider for device: {0}",
                                device.Key
                            );

                            var presetsMessenger = new DevicePresetsModelMessenger(
                                $"{device.Key}-presets-{Key}",
                                $"/device/{device.Key}",
                                presetsDevice
                            );

                            AddDefaultDeviceMessenger(presetsMessenger);

                            messengerAdded = true;
                        }
                    }

                    if (device is DisplayBase)
                    {
                        Debug.Console(2, this, "Adding actions for device: {0}", device.Key);

                        var dbMessenger = new DisplayBaseMessenger(
                            $"{device.Key}-displayBase-{Key}",
                            $"/device/{device.Key}",
                            device as DisplayBase
                        );

                        AddDefaultDeviceMessenger(dbMessenger);

                        messengerAdded = true;
                    }

                    if (device is Core.DisplayBase)
                    {
                        Debug.Console(2, this, "Adding actions for device: {0}", device.Key);

                        var dbMessenger = new CoreDisplayBaseMessenger(
                            $"{device.Key}-displayBase-{Key}",
                            $"/device/{device.Key}",
                            device as Core.DisplayBase
                        );
                        AddDefaultDeviceMessenger(dbMessenger);

                        messengerAdded = true;
                    }

                    if (device is TwoWayDisplayBase)
                    {
                        var display = device as TwoWayDisplayBase;
                        Debug.Console(
                            2,
                            this,
                            "Adding TwoWayDisplayBase for device: {0}",
                            device.Key
                        );
                        var twoWayDisplayMessenger = new TwoWayDisplayBaseMessenger(
                            $"{device.Key}-twoWayDisplay-{Key}",
                            string.Format("/device/{0}", device.Key),
                            display
                        );
                        AddDefaultDeviceMessenger(twoWayDisplayMessenger);

                        messengerAdded = true;
                    }

                    if (device is Core.TwoWayDisplayBase)
                    {
                        var display = device as Core.TwoWayDisplayBase;
                        Debug.Console(
                            2,
                            this,
                            "Adding TwoWayDisplayBase for device: {0}",
                            device.Key
                        );
                        var twoWayDisplayMessenger = new CoreTwoWayDisplayBaseMessenger(
                            $"{device.Key}-twoWayDisplay-{Key}",
                            string.Format("/device/{0}", device.Key),
                            display
                        );
                        AddDefaultDeviceMessenger(twoWayDisplayMessenger);

                        messengerAdded = true;
                    }

                    if (device is IBasicVolumeWithFeedback)
                    {
                        var deviceKey = device.Key;
                        Debug.Console(
                            2,
                            this,
                            "Adding IBasicVolumeControlWithFeedback for device: {0}",
                            deviceKey
                        );
                        var volControlDevice = device as IBasicVolumeWithFeedback;
                        var messenger = new DeviceVolumeMessenger(
                            $"{device.Key}-volume-{Key}",
                            string.Format("/device/{0}", deviceKey),
                            volControlDevice
                        );
                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;
                    }

                    if (device is ILightingScenes)
                    {
                        var deviceKey = device.Key;
                        Debug.Console(
                            2,
                            this,
                            "Adding LightingBaseMessenger for device: {0}",
                            deviceKey
                        );
                        var lightingDevice = device as ILightingScenes;
                        var messenger = new ILightingScenesMessenger(
                            $"{device.Key}-lighting-{Key}",
                            lightingDevice,
                            string.Format("/device/{0}", deviceKey)
                        );
                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;
                    }

                    if (device is IShadesOpenCloseStop)
                    {
                        var deviceKey = device.Key;
                        var shadeDevice = device as IShadesOpenCloseStop;
                        Debug.Console(
                            2,
                            this,
                            "Adding ShadeBaseMessenger for device: {0}",
                            deviceKey
                        );
                        var messenger = new IShadesOpenCloseStopMessenger(
                            $"{device.Key}-shades-{Key}",
                            shadeDevice,
                            string.Format("/device/{0}", deviceKey)
                        );
                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;
                    }

                    if (device is VideoCodecBase codec)
                    {
                        Debug.Console(
                            2,
                            this,
                            $"Adding VideoCodecBaseMessenger for device: {codec.Key}"
                        );

                        var messenger = new VideoCodecBaseMessenger(
                            $"{codec.Key}-videoCodec-{Key}",
                            codec,
                            $"/device/{codec.Key}"
                        );

                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;
                    }

                    if (device is AudioCodecBase audioCodec)
                    {
                        Debug.Console(
                            2,
                            this,
                            $"Adding AudioCodecBaseMessenger for device: {audioCodec.Key}"
                        );

                        var messenger = new AudioCodecBaseMessenger(
                            $"{audioCodec.Key}-audioCodec-{Key}",
                            audioCodec,
                            $"/device/{audioCodec.Key}"
                        );

                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;
                    }

                    if (device is ISetTopBoxControls)
                    {
                        Debug.Console(
                            2,
                            this,
                            $"Adding ISetTopBoxControlMessenger for device: {device.Key}"
                        );

                        var dev = device as PepperDash.Core.Device;

                        var messenger = new ISetTopBoxControlsMessenger(
                            $"{device.Key}-stb-{Key}",
                            $"/device/{device.Key}",
                            dev
                        );

                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;
                    }

                    if (device is IChannel)
                    {
                        Debug.Console(
                            2,
                            this,
                            $"Adding IChannelMessenger for device: {device.Key}"
                        );

                        var dev = device as PepperDash.Core.Device;

                        var messenger = new IChannelMessenger(
                            $"{device.Key}-channel-{Key}",
                            $"/device/{device.Key}",
                            dev
                        );

                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;
                    }

                    if (device is IColor)
                    {
                        Debug.Console(2, this, $"Adding IColorMessenger for device: {device.Key}");

                        var dev = device as PepperDash.Core.Device;

                        var messenger = new IColorMessenger(
                            $"{device.Key}-color-{Key}",
                            $"/device/{device.Key}",
                            dev
                        );

                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;
                    }

                    if (device is IDPad)
                    {
                        Debug.Console(2, this, $"Adding IDPadMessenger for device: {device.Key}");

                        var dev = device as PepperDash.Core.Device;

                        var messenger = new IDPadMessenger(
                            $"{device.Key}-dPad-{Key}",
                            $"/device/{device.Key}",
                            dev
                        );

                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;
                    }

                    if (device is INumericKeypad)
                    {
                        Debug.Console(
                            2,
                            this,
                            $"Adding INumericKeyapdMessenger for device: {device.Key}"
                        );

                        var dev = device as PepperDash.Core.Device;

                        var messenger = new INumericKeypadMessenger(
                            $"{device.Key}-numericKeypad-{Key}",
                            $"/device/{device.Key}",
                            dev
                        );

                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;
                    }

                    if (device is IHasPowerControl)
                    {
                        Debug.Console(
                            2,
                            this,
                            $"Adding IHasPowerControlMessenger for device: {device.Key}"
                        );

                        var dev = device as PepperDash.Core.Device;

                        var messenger = new IHasPowerMessenger(
                            $"{device.Key}-powerControl-{Key}",
                            $"/device/{device.Key}",
                            dev
                        );

                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;
                    }

                    if (device is IHasPowerControlWithFeedback powerControl)
                    {
                        var deviceKey = device.Key;
                        Debug.Console(
                            2,
                            this,
                            "Adding IHasPowerControlWithFeedbackMessenger for device: {0}",
                            deviceKey
                        );

                        var messenger = new IHasPowerControlWithFeedbackMessenger(
                            $"{device.Key}-powerFeedback-{Key}",
                            string.Format("/device/{0}", deviceKey),
                            powerControl
                        );
                        AddDefaultDeviceMessenger(messenger);
                        messengerAdded = true;
                    }

                    if (device is ITransport)
                    {
                        Debug.Console(
                            2,
                            this,
                            $"Adding ITransportMessenger for device: {device.Key}"
                        );

                        var dev = device as PepperDash.Core.Device;

                        var messenger = new IChannelMessenger(
                            $"{device.Key}-transport-{Key}",
                            $"/device/{device.Key}",
                            dev
                        );

                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;
                    }

                    if (device is IHasCurrentSourceInfoChange)
                    {
                        Debug.Console(
                            2,
                            this,
                            $"Adding IHasCurrentSourceInfoMessenger for device: {device.Key}"
                        );

                        var messenger = new IHasCurrentSourceInfoMessenger(
                            $"{device.Key}-currentSource-{Key}",
                            $"/device/{device.Key}",
                            device as IHasCurrentSourceInfoChange
                        );

                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;
                    }

                    if (device is ISwitchedOutput)
                    {
                        Debug.Console(
                            2,
                            this,
                            $"Adding ISwitchedOutputMessenger for device: {device.Key}"
                        );

                        var messenger = new ISwitchedOutputMessenger(
                            $"{device.Key}-switchedOutput-{Key}",
                            device as ISwitchedOutput,
                            $"/device/{device.Key}"
                        );

                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;
                    }

                    if (device is IDeviceInfoProvider provider)
                    {
                        Debug.Console(
                            2,
                            this,
                            $"Adding IHasDeviceInfoMessenger for device: {device.Key}"
                        );

                        var messenger = new DeviceInfoMessenger(
                            $"{device.Key}-deviceInfo-{Key}",
                            $"/device/{device.Key}",
                            provider
                        );

                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;
                    }

                    if (device is ILevelControls levelControls)
                    {
                        Debug.Console(
                            2,
                            this,
                            $"Adding LevelControlsMessenger for device: {device.Key}"
                        );

                        var messenger = new ILevelControlsMessenger(
                            $"{device.Key}-levelControls-{Key}",
                            $"/device/{device.Key}",
                            levelControls
                        );

                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;
                    }

                    // This will work if TKey and TSelector are both string types.
                    // Otherwise plugin device needs to instantiate ISelectableItemsMessenger and add it to the controller.
                    if (device is IHasInputs<string, string> inputs)
                    {
                        Debug.Console(2, this, $"Adding InputsMessenger for device: {device.Key}");

                        var messenger = new ISelectableItemsMessenger<string>(
                            $"{device.Key}-inputs-{Key}",
                            $"/device/{device.Key}",
                            inputs.Inputs,
                            "inputs"
                        );

                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;
                    }

                    if (device is IHasInputs<byte, int> byteIntInputs)
                    {
                           Debug.Console(2, this, $"Adding InputsMessenger for device: {device.Key}");

                        var messenger = new ISelectableItemsMessenger<byte>(
                            $"{device.Key}-inputs-{Key}",
                            $"/device/{device.Key}",
                            byteIntInputs.Inputs,
                            "inputs"
                        );

                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;  
                    }

                    if (device is IHasInputs<string> stringInputs)
                    {
                        Debug.Console(2, this, $"Adding InputsMessenger for device: {device.Key}");

                        var messenger = new ISelectableItemsMessenger<string>(
                            $"{device.Key}-inputs-{Key}",
                            $"/device/{device.Key}",
                            stringInputs.Inputs,
                            "inputs"
                        );

                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;
                    }

                    if (device is IHasInputs<byte> byteInputs)
                    {
                        Debug.Console(2, this, $"Adding InputsMessenger for device: {device.Key}");

                        var messenger = new ISelectableItemsMessenger<byte>(
                            $"{device.Key}-inputs-{Key}",
                            $"/device/{device.Key}",
                            byteInputs.Inputs,
                            "inputs"
                        );

                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;
                    }

                    if (device is IHasInputs<int> intInputs)
                    {
                        Debug.Console(2, this, $"Adding InputsMessenger for device: {device.Key}");

                        var messenger = new ISelectableItemsMessenger<int>(
                            $"{device.Key}-inputs-{Key}",
                            $"/device/{device.Key}",
                            intInputs.Inputs,
                            "inputs"
                        );

                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;
                    }


                    if (device is IMatrixRouting matrix)
                    {
                        Debug.LogMessage(
                            Serilog.Events.LogEventLevel.Verbose,
                            "Adding IMatrixRoutingMessenger for device: {key}",
                            this,
                            device.Key
                        );

                        var messenger = new IMatrixRoutingMessenger(
                            $"{device.Key}-matrixRouting",
                            $"/device/{device.Key}",
                            matrix
                        );

                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;
                    }

                    if (device is ITemperatureSensor tempSensor)
                    {
                        Debug.LogMessage(
                            Serilog.Events.LogEventLevel.Verbose,
                            "Adding ITemperatureSensor for device: {key}",
                            this,
                            device.Key
                        );

                        var messenger = new ITemperatureSensorMessenger(
                            $"{device.Key}-tempSensor",
                            tempSensor,
                            $"/device/{device.Key}"
                        );

                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;
                    }

                    if (device is IHumiditySensor humSensor)
                    {
                        Debug.LogMessage(
                            Serilog.Events.LogEventLevel.Verbose,
                            "Adding IHumiditySensor for device: {key}",
                            this,
                            device.Key
                        );

                        var messenger = new IHumiditySensorMessenger(
                            $"{device.Key}-humiditySensor",
                            humSensor,
                            $"/device/{device.Key}"
                        );

                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;
                    }

                    if (device is IEssentialsRoomCombiner roomCombiner)
                    {
                        Debug.Console(
                            2,
                            this,
                            $"Adding IEssentialsRoomCombinerMessenger for device: {device.Key}"
                        );

                        var messenger = new IEssentialsRoomCombinerMessenger(
                            $"{device.Key}-roomCombiner-{Key}",
                            $"/device/{device.Key}",
                            roomCombiner
                        );

                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;
                    }
                    if (device is IProjectorScreenLiftControl screenLiftControl)
                    {
                        Debug.Console(
                            2,
                            this,
                            $"Adding IProjectorScreenLiftControlMessenger for device: {device.Key}"
                        );

                        var messenger = new IProjectorScreenLiftControlMessenger(
                            $"{device.Key}-screenLiftControl-{Key}",
                            $"/device/{device.Key}",
                            screenLiftControl
                        );

                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;
                    }

                   if (device is IDspPresets dspPresets)
                    {
                        Debug.Console(
                            2,
                            this,
                            $"Adding IDspPresetsMessenger for device: {device.Key}"
                        );

                        var messenger = new IDspPresetsMessenger(
                            $"{device.Key}-dspPresets-{Key}",
                            $"/device/{device.Key}",
                            dspPresets
                        );

                        AddDefaultDeviceMessenger(messenger);

                        messengerAdded = true;
                    }

                    Debug.LogMessage(
                        LogEventLevel.Verbose,
                        "Trying to cast to generic device for device: {key}",
                        this,
                        device.Key
                    );
                    if (device is EssentialsDevice)
                    {
                        var genericDevice = device as EssentialsDevice;
                        if (genericDevice == null || messengerAdded)
                        {
                            Debug.LogMessage(
                                LogEventLevel.Verbose,
                                "Skipping GenericMessenger for device: {0}. Messenger Added: {1}. GenericDevice null: {2}",
                                this,
                                device.Key,
                                messengerAdded,
                                genericDevice == null
                            );
                            Debug.LogMessage(
                                LogEventLevel.Debug,
                                "AllDevices Completed a device. Devices Left: {0}",
                                this,
                                --count
                            );
                            continue;
                        }
                        Debug.LogMessage(
                            LogEventLevel.Debug,
                            "Adding GenericMessenger for device: {0}",
                            this,
                            genericDevice?.Key
                        );
                        AddDefaultDeviceMessenger(
                            new GenericMessenger(
                                genericDevice.Key + "-" + Key + "-generic",
                                genericDevice,
                                string.Format("/device/{0}", genericDevice.Key)
                            )
                        );
                    }
                    else
                    {
                        Debug.LogMessage(
                            LogEventLevel.Verbose,
                            "Not Essentials Device. Skipping GenericMessenger for device: {0}",
                            this,
                            device.Key
                        );
                    }
                    Debug.LogMessage(
                        LogEventLevel.Debug,
                        "AllDevices Completed a device. Devices Left: {0}",
                        this,
                        --count
                    );
                }
                catch (Exception ex)
                {
                    Debug.LogMessage(
                        LogEventLevel.Verbose,
                        "[ERROR] setting up default device messengers: {0}",
                        this,
                        ex.Message
                    );
                    Debug.LogMessage(ex, "[ERROR] setting up default device messengers", this);
                }
            }
        }

        private void AddWebApiPaths()
        {
            var apiServer = DeviceManager
                .AllDevices.OfType<EssentialsWebApi>()
                .FirstOrDefault(d => d.Key == "essentialsWebApi");

            if (apiServer == null)
            {
                Debug.Console(0, this, "No API Server available");
                return;
            }

            var routes = new List<HttpCwsRoute>
            {
                new HttpCwsRoute($"device/{Key}/authorize")
                {
                    Name = "MobileControlAuthorize",
                    RouteHandler = new MobileAuthRequestHandler(this)
                },
                new HttpCwsRoute($"device/{Key}/info")
                {
                    Name = "MobileControlInformation",
                    RouteHandler = new MobileInfoHandler(this)
                },
                new HttpCwsRoute($"device/{Key}/actionPaths")
                {
                    Name = "MobileControlActionPaths",
                    RouteHandler = new ActionPathsHandler(this)
                }
            };

            apiServer.AddRoute(routes);
        }

        private void AddConsoleCommands()
        {
            CrestronConsole.AddNewConsoleCommand(
                AuthorizeSystem,
                "mobileauth",
                "Authorizes system to talk to Mobile Control server",
                ConsoleAccessLevelEnum.AccessOperator
            );
            CrestronConsole.AddNewConsoleCommand(
                s => ShowInfo(),
                "mobileinfo",
                "Shows information for current mobile control session",
                ConsoleAccessLevelEnum.AccessOperator
            );
            CrestronConsole.AddNewConsoleCommand(
                s =>
                {
                    s = s.Trim();
                    if (!string.IsNullOrEmpty(s))
                    {
                        _httpDebugEnabled = (s.Trim() != "0");
                    }
                    CrestronConsole.ConsoleCommandResponse(
                        "HTTP Debug {0}",
                        _httpDebugEnabled ? "Enabled" : "Disabled"
                    );
                },
                "mobilehttpdebug",
                "1 enables more verbose HTTP response debugging",
                ConsoleAccessLevelEnum.AccessOperator
            );
            CrestronConsole.AddNewConsoleCommand(
                TestHttpRequest,
                "mobilehttprequest",
                "Tests an HTTP get to URL given",
                ConsoleAccessLevelEnum.AccessOperator
            );

            CrestronConsole.AddNewConsoleCommand(
                PrintActionDictionaryPaths,
                "mobileshowactionpaths",
                "Prints the paths in the Action Dictionary",
                ConsoleAccessLevelEnum.AccessOperator
            );
            CrestronConsole.AddNewConsoleCommand(
                s =>
                {
                    _disableReconnect = false;
                    Debug.Console(
                        1,
                        this,
                        Debug.ErrorLogLevel.Notice,
                        "User command: {0}",
                        "mobileConnect"
                    );
                    ConnectWebsocketClient();
                },
                "mobileconnect",
                "Forces connect of websocket",
                ConsoleAccessLevelEnum.AccessOperator
            );
            CrestronConsole.AddNewConsoleCommand(
                s =>
                {
                    _disableReconnect = true;
                    Debug.Console(
                        1,
                        this,
                        Debug.ErrorLogLevel.Notice,
                        "User command: {0}",
                        "mobileDisco"
                    );
                    CleanUpWebsocketClient();
                },
                "mobiledisco",
                "Disconnects websocket",
                ConsoleAccessLevelEnum.AccessOperator
            );

            CrestronConsole.AddNewConsoleCommand(
                ParseStreamRx,
                "mobilesimulateaction",
                "Simulates a message from the server",
                ConsoleAccessLevelEnum.AccessOperator
            );

            CrestronConsole.AddNewConsoleCommand(
                SetWebsocketDebugLevel,
                "mobilewsdebug",
                "Set Websocket debug level",
                ConsoleAccessLevelEnum.AccessProgrammer
            );
        }

        public MobileControlConfig Config { get; private set; }

        public string Host { get; private set; }

        public string ClientAppUrl => Config.ClientAppUrl;

        private void OnRoomCombinationScenarioChanged(
            object sender,
            EventArgs eventArgs
        )
        {
            SendMessageObject(new MobileControlMessage { Type = "/system/roomCombinationChanged" });
        }

        public bool CheckForDeviceMessenger(string key)
        {
            return _messengers.ContainsKey(key);
        }

#if SERIES4
        public void AddDeviceMessenger(IMobileControlMessenger messenger)
#else
        public void AddDeviceMessenger(MessengerBase messenger)
#endif
        {
            if (_messengers.ContainsKey(messenger.Key))
            {
                Debug.Console(1, this, "Messenger with key {0} already added", messenger.Key);
                return;
            }

            if (messenger is IDelayedConfiguration simplMessenger)
            {
                simplMessenger.ConfigurationIsReady += Bridge_ConfigurationIsReady;
            }

            if (messenger is MobileControlBridgeBase roomBridge)
            {
                _roomBridges.Add(roomBridge);
            }

            Debug.Console(
                2,
                this,
                "Adding messenger with key {0} for path {1}",
                messenger.Key,
                messenger.MessagePath
            );

            _messengers.Add(messenger.Key, messenger);

            messenger.RegisterWithAppServer(this);
        }

        private void AddDefaultDeviceMessenger(IMobileControlMessenger messenger)
        {
            if (_defaultMessengers.ContainsKey(messenger.Key))
            {
                Debug.Console(
                    1,
                    this,
                    "Default messenger with key {0} already added",
                    messenger.Key
                );
                return;
            }

            if (messenger is IDelayedConfiguration simplMessenger)
            {
                simplMessenger.ConfigurationIsReady += Bridge_ConfigurationIsReady;
            }
            Debug.Console(
                2,
                this,
                "Adding default messenger with key {0} for path {1}",
                messenger.Key,
                messenger.MessagePath
            );

            _defaultMessengers.Add(messenger.Key, messenger);

            if (_initialized)
            {
                RegisterMessengerWithServer(messenger);
            }
        }

        private void RegisterMessengerWithServer(IMobileControlMessenger messenger)
        {
            Debug.Console(
                2,
                this,
                "Registering messenger with key {0} for path {1}",
                messenger.Key,
                messenger.MessagePath
            );

            messenger.RegisterWithAppServer(this);
        }

        public override void Initialize()
        {
            foreach (var messenger in _messengers)
            {
                try
                {
                    RegisterMessengerWithServer(messenger.Value);
                }
                catch (Exception ex)
                {
                    Debug.Console(
                        0,
                        this,
                        $"Exception registering paths for {messenger.Key}: {ex.Message}"
                    );
                    Debug.Console(
                        2,
                        this,
                        $"Exception registering paths for {messenger.Key}: {ex.StackTrace}"
                    );
                    continue;
                }
            }

            foreach (var messenger in _defaultMessengers)
            {
                try
                {
                    RegisterMessengerWithServer(messenger.Value);
                }
                catch (Exception ex)
                {
                    Debug.Console(
                        0,
                        this,
                        $"Exception registering paths for {messenger.Key}: {ex.Message}"
                    );
                    Debug.Console(
                        2,
                        this,
                        $"Exception registering paths for {messenger.Key}: {ex.StackTrace}"
                    );
                    continue;
                }
            }

            var simplMessengers = _messengers.OfType<IDelayedConfiguration>().ToList();

            if (simplMessengers.Count > 0)
            {
                return;
            }

            _initialized = true;

            RegisterSystemToServer();
        }

        #region IMobileControl Members

        public static IMobileControl GetAppServer()
        {
            try
            {
                var appServer =
                    DeviceManager.GetDevices().SingleOrDefault(s => s is IMobileControl)
                    as MobileControlSystemController;
                return appServer;
            }
            catch (Exception e)
            {
                Debug.Console(0, "Unable to find MobileControlSystemController in Devices: {0}", e);
                return null;
            }
        }

        /// <summary>
        /// Generates the url and creates the websocket client
        /// </summary>
        private bool CreateWebsocket()
        {
            if (_wsClient2 != null)
            {
                _wsClient2.Close();
                _wsClient2 = null;
            }

            if (string.IsNullOrEmpty(SystemUuid))
            {
                Debug.Console(
                    0,
                    this,
                    Debug.ErrorLogLevel.Error,
                    "System UUID not defined. Unable to connect to Mobile Control"
                );
                return false;
            }

            var wsHost = Host.Replace("http", "ws");
            var url = string.Format("{0}/system/join/{1}", wsHost, SystemUuid);

            _wsClient2 = new WebSocket(url)
            {
                Log =
                {
                    Output = (data, s) =>
                        Debug.Console(
                            1,
                            Debug.ErrorLogLevel.Notice,
                            "Message from websocket: {0}",
                            data
                        )
                }
            };

            _wsClient2.SslConfiguration.EnabledSslProtocols =
                System.Security.Authentication.SslProtocols.Tls11
                | System.Security.Authentication.SslProtocols.Tls12;

            _wsClient2.OnMessage += HandleMessage;
            _wsClient2.OnOpen += HandleOpen;
            _wsClient2.OnError += HandleError;
            _wsClient2.OnClose += HandleClose;

            return true;
        }

        public void LinkSystemMonitorToAppServer()
        {
            if (CrestronEnvironment.DevicePlatform != eDevicePlatform.Appliance)
            {
                Debug.Console(
                    0,
                    this,
                    Debug.ErrorLogLevel.Notice,
                    "System Monitor does not exist for this platform. Skipping..."
                );
                return;
            }

            if (!(DeviceManager.GetDeviceForKey("systemMonitor") is SystemMonitorController sysMon))
            {
                return;
            }

            var key = sysMon.Key + "-" + Key;
            var messenger = new SystemMonitorMessenger(key, sysMon, "/device/systemMonitor");

            AddDeviceMessenger(messenger);
        }

        /*        public void CreateMobileControlRoomBridge(IEssentialsRoom room, IMobileControl parent)
                {
                    var bridge = new MobileControlEssentialsRoomBridge(room);
                    AddBridgePostActivationAction(bridge);
                    DeviceManager.AddDevice(bridge);
                }     */

        #endregion

        private void SetWebsocketDebugLevel(string cmdparameters)
        {
            if (CrestronEnvironment.ProgramCompatibility == eCrestronSeries.Series4)
            {
                Debug.Console(
                    0,
                    this,
                    "Setting websocket log level not currently allowed on 4 series."
                );
                return; // Web socket log level not currently allowed in series4
            }

            if (string.IsNullOrEmpty(cmdparameters))
            {
                Debug.Console(0, this, "Current Websocket debug level: {0}", _wsLogLevel);
                return;
            }

            if (cmdparameters.ToLower().Contains("help") || cmdparameters.ToLower().Contains("?"))
            {
                Debug.Console(
                    0,
                    this,
                    "valid options are:\r\n{0}\r\n{1}\r\n{2}\r\n{3}\r\n{4}\r\n{5}\r\n",
                    LogLevel.Trace,
                    LogLevel.Debug,
                    LogLevel.Info,
                    LogLevel.Warn,
                    LogLevel.Error,
                    LogLevel.Fatal
                );
            }

            try
            {
                var debugLevel = (LogLevel)Enum.Parse(typeof(LogLevel), cmdparameters, true);

                _wsLogLevel = debugLevel;

                if (_wsClient2 != null)
                {
                    _wsClient2.Log.Level = _wsLogLevel;
                }

                Debug.Console(0, this, "Websocket log level set to {0}", debugLevel);
            }
            catch
            {
                Debug.Console(
                    0,
                    this,
                    "{0} is not a valid debug level. Valid options are: {1}, {2}, {3}, {4}, {5}, {6}",
                    cmdparameters,
                    LogLevel.Trace,
                    LogLevel.Debug,
                    LogLevel.Info,
                    LogLevel.Warn,
                    LogLevel.Error,
                    LogLevel.Fatal
                );
            }
        }

        /*        private void AddBridgePostActivationAction(MobileControlBridgeBase bridge)
                {
                    bridge.AddPostActivationAction(() =>
                    {
                        Debug.Console(0, bridge, "Linking to parent controller");
                        bridge.AddParent(this);
                        AddBridge(bridge);
                    });
                }*/

        /// <summary>
        /// Sends message to server to indicate the system is shutting down
        /// </summary>
        /// <param name="programEventType"></param>
        private void CrestronEnvironment_ProgramStatusEventHandler(
            eProgramStatusEventType programEventType
        )
        {
            if (
                programEventType != eProgramStatusEventType.Stopping
                || _wsClient2 == null
                || !_wsClient2.IsAlive
            )
            {
                return;
            }

            _disableReconnect = true;

            StopServerReconnectTimer();
            CleanUpWebsocketClient();
        }

        public void PrintActionDictionaryPaths(object o)
        {
            CrestronConsole.ConsoleCommandResponse("ActionDictionary Contents:\r\n");

            foreach (var (messengerKey, actionPath) in GetActionDictionaryPaths())
            {
                CrestronConsole.ConsoleCommandResponse($"<{messengerKey}> {actionPath}\r\n");
            }
        }

        public List<(string, string)> GetActionDictionaryPaths()
        {
            var paths = new List<(string, string)>();

            foreach (var item in _actionDictionary)
            {
                var messengers = item.Value.Select(a => a.Messenger).Cast<MessengerBase>();
                foreach (var messenger in messengers)
                {
                    foreach (var actionPath in messenger.GetActionPaths())
                    {
                        paths.Add((messenger.Key, $"{item.Key}{actionPath}"));
                    }
                }
            }

            return paths;
        }

        /// <summary>
        /// Adds an action to the dictionary
        /// </summary>
        /// <param name="key">The path of the API command</param>
        /// <param name="action">The action to be triggered by the commmand</param>
        public void AddAction<T>(T messenger, Action<string, string, JToken> action)
            where T : IMobileControlMessenger
        {
            if (
                _actionDictionary.TryGetValue(
                    messenger.MessagePath,
                    out List<IMobileControlAction> actionList
                )
            )
            {
                if (
                    actionList.Any(a =>
                        a.Messenger.GetType() == messenger.GetType()
                        && a.Messenger.DeviceKey == messenger.DeviceKey
                    )
                )
                {
                    Debug.Console(
                        0,
                        this,
                        $"Messenger of type {messenger.GetType().Name} already exists. Skipping actions for {messenger.Key}"
                    );
                    return;
                }

                actionList.Add(new MobileControlAction(messenger, action));
                return;
            }

            actionList = new List<IMobileControlAction>
            {
                new MobileControlAction(messenger, action)
            };

            _actionDictionary.Add(messenger.MessagePath, actionList);
        }

        /// <summary>
        /// Removes an action from the dictionary
        /// </summary>
        /// <param name="key"></param>
        public void RemoveAction(string key)
        {
            if (_actionDictionary.ContainsKey(key))
            {
                _actionDictionary.Remove(key);
            }
        }

        public MobileControlBridgeBase GetRoomBridge(string key)
        {
            return _roomBridges.FirstOrDefault((r) => r.RoomKey.Equals(key));
        }

        public IMobileControlRoomMessenger GetRoomMessenger(string key)
        {
            return _roomBridges.FirstOrDefault((r) => r.RoomKey.Equals(key));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Bridge_ConfigurationIsReady(object sender, EventArgs e)
        {
            Debug.Console(1, this, "Bridge ready.  Registering");

            // send the configuration object to the server

            if (_wsClient2 == null)
            {
                RegisterSystemToServer();
            }
            else if (!_wsClient2.IsAlive)
            {
                ConnectWebsocketClient();
            }
            else
            {
                SendInitialMessage();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="o"></param>
        private void ReconnectToServerTimerCallback(object o)
        {
            Debug.Console(1, this, "Attempting to reconnect to server...");

            ConnectWebsocketClient();
        }

        /// <summary>
        /// Verifies system connection with servers
        /// </summary>
        private void AuthorizeSystem(string code)
        {
            if (
                string.IsNullOrEmpty(SystemUuid)
                || SystemUuid.Equals("missing url", StringComparison.OrdinalIgnoreCase)
            )
            {
                CrestronConsole.ConsoleCommandResponse(
                    "System does not have a UUID. Please ensure proper configuration is loaded and restart."
                );
                return;
            }
            if (string.IsNullOrEmpty(code))
            {
                CrestronConsole.ConsoleCommandResponse(
                    "Please enter a grant code to authorize a system"
                );
                return;
            }
            if (string.IsNullOrEmpty(Config.ServerUrl))
            {
                CrestronConsole.ConsoleCommandResponse(
                    "Mobile control API address is not set. Check portal configuration"
                );
                return;
            }

            var authTask = ApiService.SendAuthorizationRequest(Host, code, SystemUuid);

            authTask.ContinueWith(t =>
            {
                var response = t.Result;

                if (response.Authorized)
                {
                    Debug.Console(0, this, "System authorized, sending config.");
                    RegisterSystemToServer();
                    return;
                }

                Debug.Console(0, this, response.Reason);
            });
        }

        /// <summary>
        /// Dumps info in response to console command.
        /// </summary>
        private void ShowInfo()
        {
            var url = Config != null ? Host : "No config";
            string name;
            string code;
            if (_roomBridges != null && _roomBridges.Count > 0)
            {
                name = _roomBridges[0].RoomName;
                code = _roomBridges[0].UserCode;
            }
            else
            {
                name = "No config";
                code = "Not available";
            }
            var conn = _wsClient2 == null ? "No client" : (_wsClient2.IsAlive ? "Yes" : "No");

            var secSinceLastAck = DateTime.Now - _lastAckMessage;
#if SERIES4
            if (Config.EnableApiServer)
            {
#endif
            CrestronConsole.ConsoleCommandResponse(
                @"Mobile Control Edge Server API Information:

	Server address: {0}
	System Name: {1}
    System URL: {2}
	System UUID: {3}
	System User code: {4}
	Connected?: {5}
    Seconds Since Last Ack: {6}",
                url,
                name,
                ConfigReader.ConfigObject.SystemUrl,
                SystemUuid,
                code,
                conn,
                secSinceLastAck.Seconds
            );
#if SERIES4
            }
            else
            {
                CrestronConsole.ConsoleCommandResponse(
                    @"
Mobile Control Edge Server API Information:
    Not Enabled in Config.
"
                );
            }

            if (
                Config.DirectServer != null
                && Config.DirectServer.EnableDirectServer
                && _directServer != null
            )
            {
                CrestronConsole.ConsoleCommandResponse(
                    @"
Mobile Control Direct Server Information:
    User App URL: {0}
    Server port: {1}
",
                    string.Format("{0}[insert_client_token]", _directServer.UserAppUrlPrefix),
                    _directServer.Port
                );

                CrestronConsole.ConsoleCommandResponse(
                    @"
    UI Client Info:
    Tokens Defined: {0}
    Clients Connected: {1}
",
                    _directServer.UiClients.Count,
                    _directServer.ConnectedUiClientsCount
                );

                var clientNo = 1;
                foreach (var clientContext in _directServer.UiClients)
                {
                    var isAlive = false;
                    var duration = "Not Connected";

                    if (clientContext.Value.Client != null)
                    {
                        isAlive = clientContext.Value.Client.Context.WebSocket.IsAlive;
                        duration = clientContext.Value.Client.ConnectedDuration.ToString();
                    }

                    CrestronConsole.ConsoleCommandResponse(
                        @"
Client {0}:
Room Key: {1}
Touchpanel Key: {6}
Token: {2}
Client URL: {3}
Connected: {4}
Duration: {5}
",
                        clientNo,
                        clientContext.Value.Token.RoomKey,
                        clientContext.Key,
                        string.Format("{0}{1}", _directServer.UserAppUrlPrefix, clientContext.Key),
                        isAlive,
                        duration,
                        clientContext.Value.Token.TouchpanelKey
                    );
                    clientNo++;
                }
            }
            else
            {
                CrestronConsole.ConsoleCommandResponse(
                    @"
Mobile Control Direct Server Infromation:
    Not Enabled in Config."
                );
            }
#endif
        }

        /// <summary>
        /// Registers the room with the server
        /// </summary>
        public void RegisterSystemToServer()
        {
#if SERIES4
            if (!Config.EnableApiServer)
            {
                Debug.Console(
                    0,
                    this,
                    "ApiServer disabled via config.  Cancelling attempt to register to server."
                );
                return;
            }
#endif
            var result = CreateWebsocket();

            if (!result)
            {
                Debug.Console(0, this, Debug.ErrorLogLevel.Error, "Unable to create websocket.");
                return;
            }

            ConnectWebsocketClient();
        }

        /// <summary>
        /// Connects the Websocket Client
        /// </summary>
        private void ConnectWebsocketClient()
        {
            try
            {
                _wsCriticalSection.Enter();

                // set to 99999 to let things work on 4-Series
                if (
                    (CrestronEnvironment.ProgramCompatibility & eCrestronSeries.Series4)
                    == eCrestronSeries.Series4
                )
                {
                    _wsClient2.Log.Level = (LogLevel)99999;
                }
                else if (
                    (CrestronEnvironment.ProgramCompatibility & eCrestronSeries.Series3)
                    == eCrestronSeries.Series3
                )
                {
                    _wsClient2.Log.Level = _wsLogLevel;
                }

                //This version of the websocket client is TLS1.2 ONLY

                //Fires OnMessage event when PING is received.
                _wsClient2.EmitOnPing = true;

                Debug.Console(
                    1,
                    this,
                    Debug.ErrorLogLevel.Notice,
                    "Connecting mobile control client to {0}",
                    _wsClient2.Url
                );

                TryConnect();
            }
            finally
            {
                _wsCriticalSection.Leave();
            }
        }

        /// <summary>
        /// Attempts to connect the websocket
        /// </summary>
        private void TryConnect()
        {
            try
            {
                IsAuthorized = false;
                _wsClient2.Connect();
            }
            catch (InvalidOperationException)
            {
                Debug.Console(
                    0,
                    Debug.ErrorLogLevel.Error,
                    "Maximum retries exceeded. Restarting websocket"
                );
                HandleConnectFailure();
            }
            catch (IOException ex)
            {
                Debug.Console(0, this, Debug.ErrorLogLevel.Error, "IO Exception\r\n{0}", ex);
                HandleConnectFailure();
            }
            catch (Exception ex)
            {
                Debug.Console(
                    0,
                    Debug.ErrorLogLevel.Error,
                    "Error on Websocket Connect: {0}\r\nStack Trace: {1}",
                    ex.Message,
                    ex.StackTrace
                );
                HandleConnectFailure();
            }
        }

        /// <summary>
        /// Gracefully handles conect failures by reconstructing the ws client and starting the reconnect timer
        /// </summary>
        private void HandleConnectFailure()
        {
            _wsClient2 = null;

            var wsHost = Host.Replace("http", "ws");
            var url = string.Format("{0}/system/join/{1}", wsHost, SystemUuid);
            _wsClient2 = new WebSocket(url)
            {
                Log =
                {
                    Output = (data, s) =>
                        Debug.Console(
                            1,
                            Debug.ErrorLogLevel.Notice,
                            "Message from websocket: {0}",
                            data
                        )
                }
            };

            _wsClient2.OnMessage -= HandleMessage;
            _wsClient2.OnOpen -= HandleOpen;
            _wsClient2.OnError -= HandleError;
            _wsClient2.OnClose -= HandleClose;

            _wsClient2.OnMessage += HandleMessage;
            _wsClient2.OnOpen += HandleOpen;
            _wsClient2.OnError += HandleError;
            _wsClient2.OnClose += HandleClose;

            StartServerReconnectTimer();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleOpen(object sender, EventArgs e)
        {
            StopServerReconnectTimer();
            StartPingTimer();
            Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "Mobile Control API connected");
            SendMessageObject(new MobileControlMessage { Type = "hello" });
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMessage(object sender, MessageEventArgs e)
        {
            if (e.IsPing)
            {
                _lastAckMessage = DateTime.Now;
                IsAuthorized = true;
                ResetPingTimer();
                return;
            }

            if (e.IsText && e.Data.Length > 0)
            {
                _receiveQueue.Enqueue(new ProcessStringMessage(e.Data, ParseStreamRx));
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleError(object sender, ErrorEventArgs e)
        {
            Debug.Console(1, this, "Websocket error {0}", e.Message);
            IsAuthorized = false;
            StartServerReconnectTimer();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleClose(object sender, CloseEventArgs e)
        {
            Debug.Console(
                1,
                this,
                Debug.ErrorLogLevel.Notice,
                "Websocket close {0} {1}, clean={2}",
                e.Code,
                e.Reason,
                e.WasClean
            );
            IsAuthorized = false;
            StopPingTimer();

            // Start the reconnect timer only if disableReconnect is false and the code isn't 4200. 4200 indicates system is not authorized;
            if (_disableReconnect || e.Code == 4200)
            {
                return;
            }

            StartServerReconnectTimer();
        }

        /// <summary>
        /// After a "hello" from the server, sends config and stuff
        /// </summary>
        private void SendInitialMessage()
        {
            Debug.Console(1, this, "Sending initial join message");

            var touchPanels = DeviceManager
                .AllDevices.OfType<IMobileControlTouchpanelController>()
                .Where(tp => !tp.UseDirectServer)
                .Select(
                    (tp) =>
                    {
                        return new { touchPanelKey = tp.Key, roomKey = tp.DefaultRoomKey };
                    }
                );

            var msg = new MobileControlMessage
            {
                Type = "join",
                Content = JToken.FromObject(
                    new { config = GetConfigWithPluginVersion(), touchPanels }
                )
            };

            SendMessageObject(msg);
        }

        public MobileControlEssentialsConfig GetConfigWithPluginVersion()
        {
            // Populate the application name and version number
            var confObject = new MobileControlEssentialsConfig(ConfigReader.ConfigObject);

            confObject.Info.RuntimeInfo.AppName = Assembly.GetExecutingAssembly().GetName().Name;

            var essentialsVersion = Global.AssemblyVersion;
            confObject.Info.RuntimeInfo.AssemblyVersion = essentialsVersion;

//#if DEBUG
//            // Set for local testing
//            confObject.RuntimeInfo.PluginVersion = "4.0.0-localBuild";
//#else
            // Populate the plugin version
            var pluginVersion = Assembly
                .GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);

            var fullVersionAtt = pluginVersion[0] as AssemblyInformationalVersionAttribute;

            if (fullVersionAtt != null)
            {
                var pluginInformationalVersion = fullVersionAtt.InformationalVersion;

                confObject.RuntimeInfo.PluginVersion = pluginInformationalVersion;
                confObject.RuntimeInfo.EssentialsVersion = Global.AssemblyVersion;
                confObject.RuntimeInfo.PepperDashCoreVersion = PluginLoader.PepperDashCoreAssembly.Version;
                confObject.RuntimeInfo.EssentialsPlugins = PluginLoader.EssentialsPluginAssemblies;
            }
//#endif
            return confObject;
        }

        public void SetClientUrl(string path, string roomKey = null)
        {
            var message = new MobileControlMessage
            {
                Type = string.IsNullOrEmpty(roomKey) ? $"/event/system/setUrl" : $"/event/room/{roomKey}/setUrl",
                Content = JToken.FromObject(new MobileControlSimpleContent<string> { Value = path })
            };

            SendMessageObject(message);
        }

        /// <summary>
        /// Sends any object type to server
        /// </summary>
        /// <param name="o"></param>
        public void SendMessageObject(IMobileControlMessage o)
        {
#if SERIES4
            if (Config.EnableApiServer)
            {
#endif
            _transmitToServerQueue.Enqueue(new TransmitMessage(o, _wsClient2));
#if SERIES4
            }

            if (
                Config.DirectServer != null
                && Config.DirectServer.EnableDirectServer
                && _directServer != null
            )
            {
                _transmitToClientsQueue.Enqueue(new MessageToClients(o, _directServer));
            }
#endif
        }

#if SERIES4
        public void SendMessageObjectToDirectClient(object o)
        {
            if (
                Config.DirectServer != null
                && Config.DirectServer.EnableDirectServer
                && _directServer != null
            )
            {
                _transmitToClientsQueue.Enqueue(new MessageToClients(o, _directServer));
            }
        }
#endif

        /// <summary>
        /// Disconnects the Websocket Client and stops the heartbeat timer
        /// </summary>
        private void CleanUpWebsocketClient()
        {
            if (_wsClient2 == null)
            {
                return;
            }

            Debug.Console(1, this, "Disconnecting websocket");

            _wsClient2.Close();
        }

        private void ResetPingTimer()
        {
            // This tells us we're online with the API and getting pings
            _pingTimer.Reset(PingInterval);
        }

        private void StartPingTimer()
        {
            StopPingTimer();
            _pingTimer = new CTimer(PingTimerCallback, null, PingInterval);
        }

        private void StopPingTimer()
        {
            if (_pingTimer == null)
            {
                return;
            }

            _pingTimer.Stop();
            _pingTimer.Dispose();
            _pingTimer = null;
        }

        private void PingTimerCallback(object o)
        {
            Debug.Console(
                1,
                this,
                Debug.ErrorLogLevel.Notice,
                "Ping timer expired. Closing websocket"
            );

            try
            {
                _wsClient2.Close();
            }
            catch (Exception ex)
            {
                Debug.Console(
                    0,
                    Debug.ErrorLogLevel.Error,
                    "Exception closing websocket: {0}\r\nStack Trace: {1}",
                    ex.Message,
                    ex.StackTrace
                );

                HandleConnectFailure();
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void StartServerReconnectTimer()
        {
            StopServerReconnectTimer();
            _serverReconnectTimer = new CTimer(
                ReconnectToServerTimerCallback,
                ServerReconnectInterval
            );
            Debug.Console(1, this, "Reconnect Timer Started.");
        }

        /// <summary>
        /// Does what it says
        /// </summary>
        private void StopServerReconnectTimer()
        {
            if (_serverReconnectTimer == null)
            {
                return;
            }
            _serverReconnectTimer.Stop();
            _serverReconnectTimer = null;
        }

        /// <summary>
        /// Resets reconnect timer and updates usercode
        /// </summary>
        /// <param name="content"></param>
        private void HandleHeartBeat(JToken content)
        {
            SendMessageObject(new MobileControlMessage { Type = "/system/heartbeatAck" });

            var code = content["userCode"];
            if (code == null)
            {
                return;
            }

            foreach (var b in _roomBridges)
            {
                b.SetUserCode(code.Value<string>());
            }
        }

        //private void HandleClientJoined(JToken content)
        //{
        //    var clientId = content["clientId"].Value<string>();
        //    var roomKey = content["roomKey"].Value<string>();

        //    SendMessageObject(
        //        new MobileControlMessage
        //        {
        //            Type = "/system/roomKey",
        //            ClientId = clientId,
        //            Content = roomKey
        //        }
        //    );
        //}

        private void HandleClientJoined(JToken content)
        {
            var clientId = content["clientId"].Value<string>();
            var roomKey = content["roomKey"].Value<string>();
            var touchpanelKey = content.SelectToken("touchpanelKey"); //content["touchpanelKey"].Value<string>();

            if (_roomCombiner == null)
            {
                var message = new MobileControlMessage
                {
                    Type = "/system/roomKey",
                    ClientId = clientId,
                    Content = roomKey
                };

                SendMessageObject(message);
                return;
            }

            if (!_roomCombiner.CurrentScenario.UiMap.ContainsKey(roomKey))
            {
                Debug.Console(0, this,
                    "Unable to find correct roomKey for {0} in current scenario. Returning {0} as roomKey", roomKey);

                var message = new MobileControlMessage
                {
                    Type = "/system/roomKey",
                    ClientId = clientId,
                    Content = roomKey
                };

                SendMessageObject(message);
                return;
            }

            var newRoomKey = _roomCombiner.CurrentScenario.UiMap[roomKey];

            var newMessage = new MobileControlMessage
            {
                Type = "/system/roomKey",
                ClientId = clientId,
                Content = newRoomKey
            };

            SendMessageObject(newMessage);
        }

        private void HandleUserCode(JToken content, Action<string, string> action = null)
        {
            var code = content["userCode"];

            JToken qrChecksum;

            try
            {
                qrChecksum = content.SelectToken("qrChecksum", false);
            }
            catch
            {
                qrChecksum = new JValue(string.Empty);
            }

            Debug.Console(
                1,
                this,
                "QR checksum: {0}",
                qrChecksum == null ? string.Empty : qrChecksum.Value<string>()
            );

            if (code == null)
            {
                return;
            }

            if (action == null)
            {
                foreach (var bridge in _roomBridges)
                {
                    bridge.SetUserCode(code.Value<string>(), qrChecksum.Value<string>());
                }

                return;
            }

            action(code.Value<string>(), qrChecksum.Value<string>());
        }

        public void HandleClientMessage(string message)
        {
            _receiveQueue.Enqueue(new ProcessStringMessage(message, ParseStreamRx));
        }

        /// <summary>
        ///
        /// </summary>
        private void ParseStreamRx(string messageText)
        {
            if (string.IsNullOrEmpty(messageText))
            {
                return;
            }

            if (!messageText.Contains("/system/heartbeat"))
            {
                Debug.LogMessage(
                    LogEventLevel.Debug,
                    "Message RX: {messageText}",
                    this,
                    messageText
                );
            }

            try
            {
                var message = JsonConvert.DeserializeObject<MobileControlMessage>(messageText);

                switch (message.Type)
                {
                    case "hello":
                        SendInitialMessage();
                        break;
                    case "/system/heartbeat":
                        HandleHeartBeat(message.Content);
                        break;
                    case "/system/userCode":
                        HandleUserCode(message.Content);
                        break;
                    case "/system/clientJoined":
                        HandleClientJoined(message.Content);
                        break;
                    case "/system/reboot":
                        SystemMonitorController.ProcessorReboot();
                        break;
                    case "/system/programReset":
                        SystemMonitorController.ProgramReset(InitialParametersClass.ApplicationNumber);
                        break;
                    case "raw":
                        var wrapper = message.Content.ToObject<DeviceActionWrapper>();
                        DeviceJsonApi.DoDeviceAction(wrapper);
                        break;
                    case "close":
                        Debug.Console(1, this, "Received close message from server.");
                        break;
                    default:
                        // Incoming message example
                        // /room/roomA/status
                        // /room/roomAB/status

                        // ActionDictionary Keys example
                        // /room/roomA
                        // /room/roomAB

                        // Can't do direct comparison because it will match /room/roomA with /room/roomA/xxx instead of /room/roomAB/xxx
                        var handlersKv = _actionDictionary.FirstOrDefault(kv => message.Type.StartsWith(kv.Key + "/")); // adds trailing slash to ensure above case is handled
                        

                        if (handlersKv.Key == null)
                        {
                            this.LogInformation("-- Warning: Incoming message has no registered handler {type}", message.Type);
                            break;
                        }

                        var handlers = handlersKv.Value;

                        foreach (var handler in handlers)
                        {
                            Task.Run(
                                () =>
                                    handler.Action(message.Type, message.ClientId, message.Content)
                            );
                        }

                        break;
                }
            }
            catch (Exception err)
            {
                Debug.LogMessage(
                    err,
                    "Unable to parse {message}",
                    this,
                    messageText
                );
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="s"></param>
        private void TestHttpRequest(string s)
        {
            {
                s = s.Trim();
                if (string.IsNullOrEmpty(s))
                {
                    PrintTestHttpRequestUsage();
                    return;
                }
                var tokens = s.Split(' ');
                if (tokens.Length < 2)
                {
                    CrestronConsole.ConsoleCommandResponse("Too few paramaters\r");
                    PrintTestHttpRequestUsage();
                    return;
                }

                try
                {
                    var url = tokens[1];
                    switch (tokens[0].ToLower())
                    {
                        case "get":
                            {
                                var resp = new HttpClient().Get(url);
                                CrestronConsole.ConsoleCommandResponse("RESPONSE:\r{0}\r\r", resp);
                            }
                            break;
                        case "post":
                            {
                                var resp = new HttpClient().Post(url, new byte[] { });
                                CrestronConsole.ConsoleCommandResponse("RESPONSE:\r{0}\r\r", resp);
                            }
                            break;
                        default:
                            CrestronConsole.ConsoleCommandResponse("Only get or post supported\r");
                            PrintTestHttpRequestUsage();
                            break;
                    }
                }
                catch (HttpException e)
                {
                    CrestronConsole.ConsoleCommandResponse("Exception in request:\r");
                    CrestronConsole.ConsoleCommandResponse(
                        "Response URL: {0}\r",
                        e.Response.ResponseUrl
                    );
                    CrestronConsole.ConsoleCommandResponse(
                        "Response Error Code: {0}\r",
                        e.Response.Code
                    );
                    CrestronConsole.ConsoleCommandResponse(
                        "Response body: {0}\r",
                        e.Response.ContentString
                    );
                }
            }
        }

        private void PrintTestHttpRequestUsage()
        {
            CrestronConsole.ConsoleCommandResponse("Usage: mobilehttprequest:N get/post url\r");
        }
    }

    public class ClientSpecificUpdateRequest
    {
        public ClientSpecificUpdateRequest(Action<string> action)
        {
            ResponseMethod = action;
        }

        public Action<string> ResponseMethod { get; private set; }
    }

    public class UserCodeChanged
    {
        public Action<string, string> UpdateUserCode { get; private set; }

        public UserCodeChanged(Action<string, string> updateMethod)
        {
            UpdateUserCode = updateMethod;
        }
    }
}
