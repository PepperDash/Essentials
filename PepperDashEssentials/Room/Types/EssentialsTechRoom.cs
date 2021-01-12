using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Scheduler;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Presets;
using PepperDash.Essentials.Devices.Common;
using PepperDash.Essentials.Room.Config;

namespace PepperDash.Essentials
{
    public class EssentialsTechRoom : EssentialsRoomBase, ITvPresetsProvider, IBridgeAdvanced, IRunDirectRouteAction
    {
        private readonly EssentialsTechRoomConfig _config;
        private readonly Dictionary<string, TwoWayDisplayBase> _displays;

        private readonly DevicePresetsModel _tunerPresets;
        private readonly Dictionary<string, IRSetTopBoxBase> _tuners;

        private Dictionary<string, string> _currentPresets;
        private ScheduledEventGroup _roomScheduledEventGroup;

        /// <summary>
        /// 
        /// </summary>
        protected override Func<bool> IsWarmingFeedbackFunc
        {
            get
            {
                return () =>
                {
                    return _displays.All(kv => kv.Value.IsWarmingUpFeedback.BoolValue);
                };
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected override Func<bool> IsCoolingFeedbackFunc
        {
            get
            {
                return () =>
                {
                    return _displays.All(kv => kv.Value.IsCoolingDownFeedback.BoolValue);
                };
            }
        }

        public EssentialsTechRoom(DeviceConfig config) : base(config)
        {
            _config = config.Properties.ToObject<EssentialsTechRoomConfig>();

            _tunerPresets = new DevicePresetsModel(String.Format("{0}-presets", config.Key), _config.PresetsFileName);

            _tunerPresets.SetFileName(_config.PresetsFileName);

            _tunerPresets.PresetRecalled += TunerPresetsOnPresetRecalled;

            _tuners = GetDevices<IRSetTopBoxBase>(_config.Tuners);
            _displays = GetDevices<TwoWayDisplayBase>(_config.Displays);

            RoomPowerIsOnFeedback = new BoolFeedback(() => RoomPowerIsOn);

            SetUpTunerPresetsFeedback();

            SubscribeToDisplayFeedbacks();

            CreateOrUpdateScheduledEvents();
        }

        public Dictionary<string, StringFeedback> CurrentPresetsFeedbacks { get; private set; }

        public Dictionary<string, IRSetTopBoxBase> Tuners
        {
            get { return _tuners; }
        }

        public Dictionary<string, TwoWayDisplayBase> Displays
        {
            get { return _displays; }
        }

        public BoolFeedback RoomPowerIsOnFeedback { get; private set; }

        public bool RoomPowerIsOn
        {
            get { return _displays.All(kv => kv.Value.PowerIsOnFeedback.BoolValue); }
        }

        #region ITvPresetsProvider Members

        public DevicePresetsModel TvPresets
        {
            get { return _tunerPresets; }
        }

        #endregion

        private void TunerPresetsOnPresetRecalled(ISetTopBoxNumericKeypad device, string channel)
        {
            if (!_currentPresets.ContainsKey(device.Key))
            {
                return;
            }

            _currentPresets[device.Key] = channel;

            if (!CurrentPresetsFeedbacks.ContainsKey(device.Key))
            {
                CurrentPresetsFeedbacks[device.Key].FireUpdate();
            }
        }

        private void SetUpTunerPresetsFeedback()
        {
            _currentPresets = new Dictionary<string, string>();
            CurrentPresetsFeedbacks = new Dictionary<string, StringFeedback>();

            foreach (var setTopBox in _tuners)
            {
                var tuner = setTopBox.Value;
                _currentPresets.Add(tuner.Key, String.Empty);
                CurrentPresetsFeedbacks.Add(tuner.Key, new StringFeedback(() => _currentPresets[tuner.Key]));
            }
        }

        private void SubscribeToDisplayFeedbacks()
        {
            foreach (var display in _displays)
            {
                display.Value.PowerIsOnFeedback.OutputChange +=
                    (sender, args) =>
                    {
                        RoomPowerIsOnFeedback.InvokeFireUpdate();
                        IsWarmingUpFeedback.InvokeFireUpdate();
                        IsCoolingDownFeedback.InvokeFireUpdate();
                    };
            }
        }

        private void CreateOrUpdateScheduledEvents()
        {
            var eventsConfig = _config.ScheduledEvents;

            GetOrCreateScheduleGroup();

            foreach (var eventConfig in eventsConfig)
            {
                CreateOrUpdateSingleEvent(eventConfig);
            }

            _roomScheduledEventGroup.UserGroupCallBack += HandleScheduledEvent;
        }

        private void GetOrCreateScheduleGroup()
        {
            if (_roomScheduledEventGroup == null)
            {
                _roomScheduledEventGroup = Scheduler.GetEventGroup(Key) ?? new ScheduledEventGroup(Key);

                Scheduler.AddEventGroup(_roomScheduledEventGroup);
            }

            _roomScheduledEventGroup.RetrieveAllEvents();
        }

        private void CreateOrUpdateSingleEvent(ScheduledEventConfig scheduledEvent)
        {
            if (!_roomScheduledEventGroup.ScheduledEvents.ContainsKey(scheduledEvent.Key))
            {
                SchedulerUtilities.CreateEventFromConfig(scheduledEvent, _roomScheduledEventGroup, HandleScheduledEvent);
                return;
            }

            var roomEvent = _roomScheduledEventGroup.ScheduledEvents[scheduledEvent.Key];

            if (!SchedulerUtilities.CheckEventTimeForMatch(roomEvent, DateTime.Parse(scheduledEvent.Time)) &&
                !SchedulerUtilities.CheckEventRecurrenceForMatch(roomEvent, scheduledEvent.Days))
            {
                return;
            }

            Debug.Console(1, this,
                "Existing event does not match new config properties. Deleting existing event '{0}' and creating new event from configuration",
                roomEvent.Name);

            _roomScheduledEventGroup.DeleteEvent(roomEvent);

            SchedulerUtilities.CreateEventFromConfig(scheduledEvent, _roomScheduledEventGroup, HandleScheduledEvent);
        }

        public void AddOrUpdateScheduledEvent(ScheduledEventConfig scheduledEvent)
        {
            //update config based on key of scheduleEvent
            GetOrCreateScheduleGroup();
            var existingEventIndex = _config.ScheduledEvents.FindIndex((e) => e.Key == scheduledEvent.Key);

            if (existingEventIndex < 0)
            {
                _config.ScheduledEvents.Add(scheduledEvent);
            }
            else
            {
                _config.ScheduledEvents[existingEventIndex] = scheduledEvent;
            }

            //create or update event based on config
            CreateOrUpdateSingleEvent(scheduledEvent);
            //save config
            Config.Properties = JToken.FromObject(_config);

            CustomSetConfig(Config);
            //Fire Event
            OnScheduledEventUpdate();
        }

        public List<ScheduledEventConfig> GetScheduledEvents()
        {
            return _config.ScheduledEvents ?? new List<ScheduledEventConfig>();
        }

        private void OnScheduledEventUpdate()
        {
            var handler = ScheduledEventsChanged;

            if (handler == null)
            {
                return;
            }

            handler(this, new ScheduledEventEventArgs {ScheduledEvents = _config.ScheduledEvents});
        }

        public event EventHandler<ScheduledEventEventArgs> ScheduledEventsChanged;

        private void HandleScheduledEvent(ScheduledEvent schevent, ScheduledEventCommon.eCallbackReason type)
        {
            var eventConfig = _config.ScheduledEvents.FirstOrDefault(e => e.Key == schevent.Name);

            if (eventConfig == null)
            {
                Debug.Console(1, this, "Event with name {0} not found", schevent.Name);
                return;
            }

            Debug.Console(1, this, "Running actions for event {0}", schevent.Name);

            if (eventConfig.Acknowledgeable)
            {
                schevent.Acknowledge();
            }

            CrestronInvoke.BeginInvoke((o) =>
            {
                Debug.Console(2, this, "There are {0} actions to execute for this event.", eventConfig.Actions.Count);

                foreach (var a in eventConfig.Actions)
                {
                    Debug.Console(2, this, 
@"Attempting to run action:
DeviceKey: {0}
MethodName: {1}
Params: {2}"
                    , a.DeviceKey, a.MethodName, a.Params);
                    DeviceJsonApi.DoDeviceAction(a);
                }
            });
        }


        public void RoomPowerOn()
        {
            Debug.Console(2, this, "Room Powering On");

            var dummySource = DeviceManager.GetDeviceForKey(_config.DummySourceKey) as IRoutingOutputs;

            if (dummySource == null)
            {
                Debug.Console(1, this, "Unable to get source with key: {0}", _config.DummySourceKey);
                return;
            }

            foreach (var display in _displays)
            {
                RunDirectRoute(dummySource, display.Value);
            }
        }

        public void RoomPowerOff()
        {
            Debug.Console(2, this, "Room Powering Off");

            foreach (var display in _displays)
            {
                display.Value.PowerOff();
            }
        }

        private Dictionary<string, T> GetDevices<T>(ICollection<string> config) where T : IKeyed
        {
            try
            {
                var returnValue = DeviceManager.AllDevices.OfType<T>()
                    .Where(d => config.Contains(d.Key))
                    .ToDictionary(d => d.Key, d => d);

                return returnValue;
            }
            catch
            {
                Debug.Console(0, this, Debug.ErrorLogLevel.Error,
                    "Error getting devices. Check Essentials Configuration");
                return null;
            }
        }

        #region Overrides of EssentialsRoomBase

        protected override Func<bool> OnFeedbackFunc
        {
            get { return () => RoomPowerIsOn; }
        }

        protected override void EndShutdown()
        {
        }

        public override void SetDefaultLevels()
        {
        }

        public override void PowerOnToDefaultOrLastSource()
        {
        }

        public override bool RunDefaultPresentRoute()
        {
            return false;
        }

        public override void RoomVacatedForTimeoutPeriod(object o)
        {
        }

        #endregion

        #region Implementation of IBridgeAdvanced

        public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {

            var joinMap = new EssentialsTechRoomJoinMap(joinStart);
            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!String.IsNullOrEmpty(joinMapSerialized))
            {
                joinMap = JsonConvert.DeserializeObject<EssentialsTechRoomJoinMap>(joinMapSerialized);
            }

            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }
            uint i;
            if (_config.IsPrimary)
            {
                i = 0;
                foreach (var feedback in CurrentPresetsFeedbacks)
                {
                    feedback.Value.LinkInputSig(trilist.StringInput[(uint) (joinMap.CurrentPreset.JoinNumber + i)]);
                    i++;
                }

                trilist.OnlineStatusChange += (device, args) =>
                {
                    if (!args.DeviceOnLine)
                    {
                        return;
                    }

                    foreach (var feedback in CurrentPresetsFeedbacks)
                    {
                        feedback.Value.FireUpdate();
                    }
                };

                return;
            }

            i = 0;
            foreach (var setTopBox in _tuners)
            {
                var tuner = setTopBox;

                trilist.SetStringSigAction(joinMap.CurrentPreset.JoinNumber + i, s => _tunerPresets.Dial(s, tuner.Value));

                i++;
            }
        }

        #endregion

        private class EssentialsTechRoomJoinMap : JoinMapBaseAdvanced
        {
            [JoinName("currentPreset")]
            public JoinDataComplete CurrentPreset = new JoinDataComplete(new JoinData {JoinNumber = 1, JoinSpan = 16},
                new JoinMetadata {Description = "Current Tuner Preset", JoinType = eJoinType.Serial});

            public EssentialsTechRoomJoinMap(uint joinStart) : base(joinStart, typeof(EssentialsTechRoomJoinMap))
            {
            }
        }

        #region IRunDirectRouteAction Members

        private void RunDirectRoute(IRoutingOutputs source, IRoutingSink dest)
        {
             if (dest == null)
            {
                Debug.Console(1, this, "Cannot route, unknown destination '{0}'", dest.Key);
                return;
            }

            if (source == null)
            {
                dest.ReleaseRoute();
                if (dest is IHasPowerControl)
                    (dest as IHasPowerControl).PowerOff();
            }
            else
            {
                dest.ReleaseAndMakeRoute(source, eRoutingSignalType.Video);
            }
        }

        /// <summary>
        /// Attempts to route directly between a source and destination
        /// </summary>
        /// <param name="sourceKey"></param>
        /// <param name="destinationKey"></param>
        public void RunDirectRoute(string sourceKey, string destinationKey)
        {
            IRoutingSink dest = null;

            dest = DeviceManager.GetDeviceForKey(destinationKey) as IRoutingSink;

            var source = DeviceManager.GetDeviceForKey(sourceKey) as IRoutingOutputs;

            if (source == null || dest == null)
            {
                Debug.Console(1, this, "Cannot route unknown source or destination '{0}' to {1}", sourceKey, destinationKey);
                return;
            }
            RunDirectRoute(source, dest);
        }

        #endregion
    }

    public class ScheduledEventEventArgs : EventArgs
    {
        public List<ScheduledEventConfig> ScheduledEvents;
    }
}