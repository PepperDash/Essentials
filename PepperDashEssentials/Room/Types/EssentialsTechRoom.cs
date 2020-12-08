using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Scheduler;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Presets;
using PepperDash.Essentials.Devices.Common;
using PepperDash.Essentials.Room.Config;

namespace PepperDash.Essentials
{
    public class EssentialsTechRoom : EssentialsRoomBase
    {
        private readonly EssentialsTechRoomConfig _config;
        private readonly Dictionary<string, TwoWayDisplayBase> _displays;

        private readonly DevicePresetsModel _tunerPresets;
        private readonly Dictionary<string, IRSetTopBoxBase> _tuners;

        private ScheduledEventGroup _roomScheduledEventGroup;

        public EssentialsTechRoom(DeviceConfig config) : base(config)
        {
            _config = config.Properties.ToObject<EssentialsTechRoomConfig>();

            _tunerPresets = new DevicePresetsModel(String.Format("{0}-presets", config.Key), _config.PresetsFileName);

            _tunerPresets.LoadChannels();

            _tuners = GetDevices<IRSetTopBoxBase>(_config.Tuners);
            _displays = GetDevices<TwoWayDisplayBase>(_config.Displays);

            RoomPowerIsOnFeedback = new BoolFeedback(() => RoomPowerIsOn);

            SubscribeToDisplayFeedbacks();

            CreateOrUpdateScheduledEvents();
        }

        public DevicePresetsModel TunerPresets
        {
            get { return _tunerPresets; }
        }

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

        private void SubscribeToDisplayFeedbacks()
        {
            foreach (var display in _displays)
            {
                display.Value.PowerIsOnFeedback.OutputChange +=
                    (sender, args) => RoomPowerIsOnFeedback.InvokeFireUpdate();
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
            var existingEvent = _config.ScheduledEvents.FirstOrDefault(e => e.Key == scheduledEvent.Key);

            if (existingEvent == null)
            {
                _config.ScheduledEvents.Add(scheduledEvent);
            }

            //create or update event based on config
            CreateOrUpdateSingleEvent(scheduledEvent);
            //save config
            Config.Properties = JToken.FromObject(_config);

            CustomSetConfig(Config);
            //Fire Event
            OnScheduledEventUpdate();
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
                foreach (var a in eventConfig.Actions)
                {
                    DeviceJsonApi.DoDeviceAction(a);
                }
            });
        }


        public void RoomPowerOn()
        {
            foreach (var display in _displays)
            {
                display.Value.PowerOn();
            }
        }

        public void RoomPowerOff()
        {
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

        protected override Func<bool> IsWarmingFeedbackFunc
        {
            get { return () => false; }
        }

        protected override Func<bool> IsCoolingFeedbackFunc
        {
            get { return () => false; }
        }

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
    }

    public class ScheduledEventEventArgs : EventArgs
    {
        public List<ScheduledEventConfig> ScheduledEvents;
    }
}