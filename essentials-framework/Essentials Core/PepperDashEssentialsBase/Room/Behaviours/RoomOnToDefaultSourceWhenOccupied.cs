using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Crestron.SimplSharp;
using Crestron.SimplSharp.Scheduler;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Devices;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// A device that when linked to a room can power the room on when enabled during scheduled hours.
    /// </summary>
    public class RoomOnToDefaultSourceWhenOccupied : ReconfigurableDevice
    {
        RoomOnToDefaultSourceWhenOccupiedConfig PropertiesConfig;

        public bool FeatureEnabled { get; private set; }

        public DateTime FeatureEnabledTime { get; private set; }

        ScheduledEvent FeatureEnableEvent;

        const string FeatureEnableEventName = "EnableRoomOnToDefaultSourceWhenOccupied";

        public DateTime FeatureDisabledTime { get; private set; }

        ScheduledEvent FeatureDisableEvent;

        const string FeatureDisableEventName = "DisableRoomOnToDefaultSourceWhenOccupied";

        ScheduledEventGroup FeatureEventGroup;

        public EssentialsRoomBase Room { get; private set; }

        private Fusion.EssentialsHuddleSpaceFusionSystemControllerBase FusionRoom;

        public RoomOnToDefaultSourceWhenOccupied(DeviceConfig config) :
            base (config)
        {
            PropertiesConfig = JsonConvert.DeserializeObject<RoomOnToDefaultSourceWhenOccupiedConfig>(config.Properties.ToString());

            FeatureEventGroup = new ScheduledEventGroup(this.Key);

            FeatureEventGroup.RetrieveAllEvents();

            // Add to the global class for tracking
            Scheduler.AddEventGroup(FeatureEventGroup);

            AddPostActivationAction(() =>
            {
                // Subscribe to room event to know when RoomOccupancy is set and ready to be subscribed to
                if (Room != null)
                    Room.RoomOccupancyIsSet += new EventHandler<EventArgs>(RoomOccupancyIsSet);

                else
                    Debug.Console(1, this, "Room has no RoomOccupancy object set");

                var fusionRoomKey = PropertiesConfig.RoomKey + "-fusion";

                FusionRoom = DeviceManager.GetDeviceForKey(fusionRoomKey) as Core.Fusion.EssentialsHuddleSpaceFusionSystemControllerBase;

                if (FusionRoom == null)
                    Debug.Console(1, this, "Unable to get Fusion Room from Device Manager with key: {0}", fusionRoomKey);
            });
        }

        public override bool CustomActivate()
        {
            SetUpDevice();

            return base.CustomActivate();
        }

        /// <summary>
        /// Sets up device based on config values
        /// </summary>
        void SetUpDevice()
        {
            Room = DeviceManager.GetDeviceForKey(PropertiesConfig.RoomKey) as EssentialsRoomBase;

            if (Room != null)
            {
                try
                {
                    FeatureEnabledTime = DateTime.Parse(PropertiesConfig.OccupancyStartTime);

                    if (FeatureEnabledTime != null)
                    {
                        Debug.Console(1, this, "Enabled Time: {0}", FeatureEnabledTime.ToString());
                    }
                    else
                        Debug.Console(1, this, "Unable to parse {0} to DateTime", PropertiesConfig.OccupancyStartTime);
                }
                catch (Exception e)
                {
                    Debug.Console(1, this, "Unable to parse OccupancyStartTime property: {0} \n Error: {1}", PropertiesConfig.OccupancyStartTime, e);
                }

                try
                {
                    FeatureDisabledTime = DateTime.Parse(PropertiesConfig.OccupancyEndTime);

                    if (FeatureDisabledTime != null)
                    {
                        Debug.Console(1, this, "Disabled Time: {0}", FeatureDisabledTime.ToString());
                    }
                    else
                        Debug.Console(1, this, "Unable to parse {0} to DateTime", PropertiesConfig.OccupancyEndTime);
                }
                catch (Exception e)
                {
                    Debug.Console(1, this, "Unable to parse a DateTime config value \n Error: {1}", e);
                }

                if (!PropertiesConfig.EnableRoomOnWhenOccupied)
                    FeatureEventGroup.ClearAllEvents();
                else
                {
                    AddEnableEventToGroup();

                    AddDisableEventToGroup();

                    FeatureEventGroup.UserGroupCallBack += new ScheduledEventGroup.UserEventGroupCallBack(FeatureEventGroup_UserGroupCallBack);

                    FeatureEventGroup.EnableAllEvents();
                }

                FeatureEnabled = CheckIfFeatureShouldBeEnabled();
            }
            else
                Debug.Console(1, this, "Unable to get room from Device Manager with key: {0}", PropertiesConfig.RoomKey);
        }


        protected override void CustomSetConfig(DeviceConfig config)
        {
            var newPropertiesConfig = JsonConvert.DeserializeObject<RoomOnToDefaultSourceWhenOccupiedConfig>(config.Properties.ToString());

            if(newPropertiesConfig != null)
                PropertiesConfig = newPropertiesConfig;

            ConfigWriter.UpdateDeviceConfig(config);

            SetUpDevice();
        }

        /// <summary>
        /// Subscribe to feedback from RoomIsOccupiedFeedback on Room
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RoomOccupancyIsSet(object sender, EventArgs e)
        {
            if (Room.RoomOccupancy != null)
            {
                Room.RoomOccupancy.RoomIsOccupiedFeedback.OutputChange -= RoomIsOccupiedFeedback_OutputChange;
                Room.RoomOccupancy.RoomIsOccupiedFeedback.OutputChange += new EventHandler<FeedbackEventArgs>(RoomIsOccupiedFeedback_OutputChange);
                Debug.Console(1, this, "Subscribed to RoomOccupancy status from: '{0}'", Room.Key);
            }
        }

        void FeatureEventGroup_UserGroupCallBack(ScheduledEvent SchEvent, ScheduledEventCommon.eCallbackReason type)
        {
            if (type == ScheduledEventCommon.eCallbackReason.NormalExpiration)
            {
                if (SchEvent.Name == FeatureEnableEventName)
                {
                    if (PropertiesConfig.EnableRoomOnWhenOccupied)
                        FeatureEnabled = true;

                    Debug.Console(1, this, "*****Feature Enabled by event.*****");
                }
                else if (SchEvent.Name == FeatureDisableEventName)
                {
                    FeatureEnabled = false;

                    Debug.Console(1, this, "*****Feature Disabled by event.*****");
                }
            }
        }

        /// <summary>
        /// Checks if the feature should be currently enabled.  Used on startup if processor starts after start time but before end time
        /// </summary>
        /// <returns></returns>
        bool CheckIfFeatureShouldBeEnabled()
        {
            bool enabled = false;

            if(PropertiesConfig.EnableRoomOnWhenOccupied)
            {
                Debug.Console(1, this, "Current Time: {0} \n FeatureEnabledTime: {1} \n FeatureDisabledTime: {2}", DateTime.Now, FeatureEnabledTime, FeatureDisabledTime);

                if (DateTime.Now.TimeOfDay.CompareTo(FeatureEnabledTime.TimeOfDay) >= 0 && FeatureDisabledTime.TimeOfDay.CompareTo(DateTime.Now.TimeOfDay) > 0)
                {
                    if (SchedulerUtilities.CheckIfDayOfWeekMatchesRecurrenceDays(DateTime.Now, CalculateDaysOfWeekRecurrence()))
                    {
                        enabled = true;
                    }
                }
            }

            if(enabled)
                Debug.Console(1, this, "*****Feature Enabled*****");
            else
                Debug.Console(1, this, "*****Feature Disabled*****");

            return enabled;
        }

        /// <summary>
        /// Respond to Occupancy status event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RoomIsOccupiedFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            Debug.Console(1, this, "RoomIsOccupiedFeeback.OutputChange event fired. e.BoolValue: {0}", e.BoolValue);
            if(e.BoolValue)
            {
                // Occupancy detected

                if (FeatureEnabled)
                {
                    // Check room power state first
                    if (!Room.OnFeedback.BoolValue)
                    {
                        Debug.Console(1, this, "Powering Room on to default source");
                        Room.RunDefaultPresentRoute();
                    }
                }
            }
        }

        void CreateEvent(ScheduledEvent schEvent, string name)
        {
            Debug.Console(1, this, "Adding Event: '{0}'", name);
            // Create the event
            if (schEvent == null)
                schEvent = new ScheduledEvent(name, FeatureEventGroup);

            // Set up its initial properties

            if(!schEvent.Acknowledgeable)
                schEvent.Acknowledgeable = true;

            if(!schEvent.Persistent)
                schEvent.Persistent = true;

            schEvent.DateAndTime.SetFirstDayOfWeek(ScheduledEventCommon.eFirstDayOfWeek.Sunday);

            // Set config driven properties

            if (schEvent.Name == FeatureEnableEventName)
            {
                schEvent.Description = "Enables the RoomOnToDefaultSourceWhenOccupiedFeature";

                var eventRecurrennce = CalculateDaysOfWeekRecurrence();

                var eventTime = new DateTime();

                // Check to make sure the date for this event is in the future
                if (DateTime.Now.CompareTo(FeatureEnabledTime) > 0)
                    eventTime = FeatureEnabledTime.AddDays(1);
                else
                    eventTime = FeatureEnabledTime;

                Debug.Console(1, this, "eventTime (before recurrence check): {0}", eventTime);

                // Check day of week against recurrence days and move date ahead as necessary to avoid throwing an exception by trying to set the event
                // start date on a day of the week that doesn't match teh recurrence values
                while(!SchedulerUtilities.CheckIfDayOfWeekMatchesRecurrenceDays(eventTime, eventRecurrennce))
                {
                    eventTime = eventTime.AddDays(1);
                    Debug.Console(1, this, "eventTime does not fall on a recurrence weekday.  eventTime: {0}", eventTime);
                }

                schEvent.DateAndTime.SetAbsoluteEventTime(eventTime);

                Debug.Console(1, this, "Event '{0}' Absolute time set to {1}", schEvent.Name, schEvent.DateAndTime.ToString());

                CalculateAndSetAcknowledgeExpirationTimeout(schEvent, FeatureEnabledTime, FeatureDisabledTime);

                schEvent.Recurrence.Weekly(eventRecurrennce);

            }
            else if (schEvent.Name == FeatureDisableEventName)
            {
                schEvent.Description = "Disables the RoomOnToDefaultSourceWhenOccupiedFeature";

                // Check to make sure the date for this event is in the future
                if (DateTime.Now.CompareTo(FeatureDisabledTime) > 0)
                    schEvent.DateAndTime.SetAbsoluteEventTime(FeatureDisabledTime.AddDays(1));
                else
                    schEvent.DateAndTime.SetAbsoluteEventTime(FeatureDisabledTime);

                Debug.Console(1, this, "Event '{0}' Absolute time set to {1}", schEvent.Name, schEvent.DateAndTime.ToString());

                CalculateAndSetAcknowledgeExpirationTimeout(schEvent, FeatureDisabledTime, FeatureEnabledTime);

                schEvent.Recurrence.Daily();
            }
        }

        void CalculateAndSetAcknowledgeExpirationTimeout(ScheduledEvent schEvent, DateTime time1, DateTime time2)
        {
            Debug.Console(1, this, "time1.Hour = {0}", time1.Hour);
            Debug.Console(1, this, "time2.Hour = {0}", time2.Hour);
            Debug.Console(1, this, "time1.Minute = {0}", time1.Minute);
            Debug.Console(1, this, "time2.Minute = {0}", time2.Minute);

            // Calculate the Acknowledge Expiration timer to be the time between the enable and dispable events, less one minute
            var ackHours = time2.Hour - time1.Hour;
            if(ackHours < 0)
                ackHours = ackHours + 24;
            var ackMinutes = time2.Minute - time1.Minute;

            Debug.Console(1, this, "ackHours = {0}, ackMinutes = {1}", ackHours, ackMinutes);

            var ackTotalMinutes = ((ackHours * 60) + ackMinutes) - 1;

            var ackExpHour = ackTotalMinutes / 60;
            var ackExpMinutes = ackTotalMinutes % 60;

            Debug.Console(1, this, "Acknowledge Expiration Timeout: {0} hours, {1} minutes", ackExpHour, ackExpMinutes);

            schEvent.AcknowledgeExpirationTimeout.Hour = (ushort)(ackHours);
            schEvent.AcknowledgeExpirationTimeout.Minute = (ushort)(ackExpMinutes);
        }

        /// <summary>
        /// Checks existing event to see if it matches the execution time
        /// </summary>
        /// <param name="existingEvent"></param>
        /// <returns></returns>
        bool CheckExistingEventTimeForMatch(ScheduledEvent existingEvent, DateTime newTime)
        {
            bool isMatch = true;

            // Check to see if hour and minute match
            if (existingEvent.DateAndTime.Hour != newTime.Hour || existingEvent.DateAndTime.Minute != newTime.Minute)
                return false;


            return isMatch;
        }

        /// <summary>
        /// Checks existing event to see if it matches the recurrence days
        /// </summary>
        /// <param name="existingEvent"></param>
        /// <param name="eWeekdays"></param>
        /// <returns></returns>
        bool CheckExistingEventRecurrenceForMatch(ScheduledEvent existingEvent, ScheduledEventCommon.eWeekDays eWeekdays)
        {
            bool isMatch = true;

            // Check to see if recurrence matches
            if (eWeekdays != existingEvent.Recurrence.RecurrenceDays)
                return false;

            return isMatch;
        }

        /// <summary>
        /// Adds the Enable event to the local event group and sets its properties based on config
        /// </summary>
        void AddEnableEventToGroup()
        {
            if (!FeatureEventGroup.ScheduledEvents.ContainsKey(FeatureEnableEventName))
            {
                CreateEvent(FeatureEnableEvent, FeatureEnableEventName);
            }
            else
            {
                // Check if existing event has same time and recurrence as config values

                FeatureEnableEvent = FeatureEventGroup.ScheduledEvents[FeatureEnableEventName];
                Debug.Console(1, this, "Enable event already found in group");

                // Check config times and days against DateAndTime of existing event.  If different, delete existing event and create new event
                if(!CheckExistingEventTimeForMatch(FeatureEnableEvent, FeatureEnabledTime) || !CheckExistingEventRecurrenceForMatch(FeatureEnableEvent, CalculateDaysOfWeekRecurrence()))
                {
                    Debug.Console(1, this, "Existing event does not match new config properties. Deleting exisiting event: '{0}'", FeatureEnableEvent.Name);
                    FeatureEventGroup.DeleteEvent(FeatureEnableEvent);

                    FeatureEnableEvent = null;

                    CreateEvent(FeatureEnableEvent, FeatureEnableEventName);
                }
            }

        }

        /// <summary>
        /// Adds the Enable event to the local event group and sets its properties based on config
        /// </summary>
        void AddDisableEventToGroup()
        {
            if (!FeatureEventGroup.ScheduledEvents.ContainsKey(FeatureDisableEventName))
            {
                CreateEvent(FeatureDisableEvent, FeatureDisableEventName);
            }
            else
            {
                FeatureDisableEvent = FeatureEventGroup.ScheduledEvents[FeatureDisableEventName];
                Debug.Console(1, this, "Disable event already found in group");

                // Check config times against DateAndTime of existing event.  If different, delete existing event and create new event
                if(!CheckExistingEventTimeForMatch(FeatureDisableEvent, FeatureDisabledTime))
                {
                    Debug.Console(1, this, "Existing event does not match new config properties. Deleting exisiting event: '{0}'", FeatureDisableEvent.Name);

                    FeatureEventGroup.DeleteEvent(FeatureDisableEvent);

                    FeatureDisableEvent = null;

                    CreateEvent(FeatureDisableEvent, FeatureDisableEventName);
                }
            }
        }


        /// <summary>
        /// Calculates the correct bitfield enum value for the event recurrence based on the config values
        /// </summary>
        /// <returns></returns>
        ScheduledEventCommon.eWeekDays CalculateDaysOfWeekRecurrence()
        {
            ScheduledEventCommon.eWeekDays value = new ScheduledEventCommon.eWeekDays();

            if (PropertiesConfig.EnableSunday)
                value = value | ScheduledEventCommon.eWeekDays.Sunday;
            if (PropertiesConfig.EnableMonday)
                value = value | ScheduledEventCommon.eWeekDays.Monday;
            if (PropertiesConfig.EnableTuesday)
                value = value | ScheduledEventCommon.eWeekDays.Tuesday;
            if (PropertiesConfig.EnableWednesday)
                value = value | ScheduledEventCommon.eWeekDays.Wednesday;
            if (PropertiesConfig.EnableThursday)
                value = value | ScheduledEventCommon.eWeekDays.Thursday;
            if (PropertiesConfig.EnableFriday)
                value = value | ScheduledEventCommon.eWeekDays.Friday;
            if (PropertiesConfig.EnableSaturday)
                value = value | ScheduledEventCommon.eWeekDays.Saturday;

            return value;
        }

        /// <summary>
        /// Callback for event that enables feature.  Enables feature if config property is true
        /// </summary>
        /// <param name="SchEvent"></param>
        /// <param name="type"></param>
        void FeatureEnableEvent_UserCallBack(ScheduledEvent SchEvent, ScheduledEventCommon.eCallbackReason type)
        {
            if (type == ScheduledEventCommon.eCallbackReason.NormalExpiration)
            {
                if(PropertiesConfig.EnableRoomOnWhenOccupied)
                    FeatureEnabled = true;

                Debug.Console(1, this, "RoomOnToDefaultSourceWhenOccupied Feature Enabled.");
            }
        }

        /// <summary>
        /// Callback for event that enables feature.  Disables feature
        /// </summary>
        /// <param name="SchEvent"></param>
        /// <param name="type"></param>
        void FeatureDisableEvent_UserCallBack(ScheduledEvent SchEvent, ScheduledEventCommon.eCallbackReason type)
        {
            if (type == ScheduledEventCommon.eCallbackReason.NormalExpiration)
            {
                FeatureEnabled = false;

                Debug.Console(1, this, "RoomOnToDefaultSourceWhenOccupied Feature Disabled.");
            }
        }
    }

    public class RoomOnToDefaultSourceWhenOccupiedConfig
    {
        [JsonProperty("roomKey")]
        public string RoomKey { get; set; }

        [JsonProperty("enableRoomOnWhenOccupied")]
        public bool EnableRoomOnWhenOccupied { get; set; }

        [JsonProperty("occupancyStartTime")]
        public string OccupancyStartTime { get; set; }

        [JsonProperty("occupancyEndTime")]
        public string OccupancyEndTime { get; set; }

        [JsonProperty("enableSunday")]
        public bool EnableSunday { get; set; }

        [JsonProperty("enableMonday")]
        public bool EnableMonday { get; set; }

        [JsonProperty("enableTuesday")]
        public bool EnableTuesday { get; set; }

        [JsonProperty("enableWednesday")]
        public bool EnableWednesday { get; set; }

        [JsonProperty("enableThursday")]
        public bool EnableThursday { get; set; }

        [JsonProperty("enableFriday")]
        public bool EnableFriday { get; set; }

        [JsonProperty("enableSaturday")]
        public bool EnableSaturday { get; set; }
    }

    public class RoomOnToDefaultSourceWhenOccupiedFactory : EssentialsDeviceFactory<RoomOnToDefaultSourceWhenOccupied>
    {
        public RoomOnToDefaultSourceWhenOccupiedFactory()
        {
            TypeNames = new List<string>() { "roomonwhenoccupancydetectedfeature" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new RoomOnToDefaultSourceWhenOccupied Device");
            return new RoomOnToDefaultSourceWhenOccupied(dc);
        }
    }

}