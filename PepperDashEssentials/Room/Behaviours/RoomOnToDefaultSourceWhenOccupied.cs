using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Crestron.SimplSharp;
using Crestron.SimplSharp.Scheduler;
using Newtonsoft.Json;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Occupancy;

namespace PepperDash.Essentials.Room.Behaviours
{
    /// <summary>
    /// A device that when linked to a room can power the room on when enabled during scheduled hours.
    /// </summary>
    public class RoomOnToDefaultSourceWhenOccupied : Device
    {
        RoomOnToDefaultSourceWhenOccupiedConfig Config;

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

        public RoomOnToDefaultSourceWhenOccupied(string key, RoomOnToDefaultSourceWhenOccupiedConfig config) 
            : base(key)
        {

            CrestronConsole.AddNewConsoleCommand((o) => FeatureEventGroup.ClearAllEvents(), "ClearAllEvents", "Clears all scheduled events for this group", ConsoleAccessLevelEnum.AccessAdministrator);

            Config = config;

            FeatureEventGroup = new ScheduledEventGroup(this.Key);

            FeatureEventGroup.RetrieveAllEvents();

            AddPostActivationAction(() =>
            {
                if (Room.RoomOccupancy != null)
                    Room.RoomOccupancy.RoomIsOccupiedFeedback.OutputChange += new EventHandler<FeedbackEventArgs>(RoomIsOccupiedFeedback_OutputChange);
                else
                    Debug.Console(1, this, "Room has no RoomOccupancy object set");

                var fusionRoomKey = Config.RoomKey + "-fusion";

                FusionRoom = DeviceManager.GetDeviceForKey(fusionRoomKey) as Fusion.EssentialsHuddleSpaceFusionSystemControllerBase;

                if (FusionRoom == null)
                    Debug.Console(1, this, "Unable to get Fusion Room from Device Manager with key: {0}", fusionRoomKey);
            });
        }

        public override bool CustomActivate()
        {
            Room = DeviceManager.GetDeviceForKey(Config.RoomKey) as EssentialsRoomBase;

            if (Room != null)
            {
 

                // Creates the event group and adds it to the Global Scheduler list
                //Global.Scheduler.AddEventGroup(FeatureEventGroup);

                try
                {
                    FeatureEnabledTime = DateTime.Parse(Config.OccupancyStartTime);

                    if (FeatureEnabledTime != null)
                    {
                        Debug.Console(1, this, "Enabled Time: {0}", FeatureEnabledTime.ToString());
                    }
                    else
                        Debug.Console(1, this, "Unable to parse {0} to DateTime", Config.OccupancyStartTime);
                }
                catch (Exception e)
                {
                    Debug.Console(1, this, "Unable to parse OccupancyStartTime property: {0} \n Error: {1}", Config.OccupancyStartTime, e);
                }

                try
                {
                    FeatureDisabledTime = DateTime.Parse(Config.OccupancyEndTime);

                    if (FeatureDisabledTime != null)
                    {
                        Debug.Console(1, this, "Disabled Time: {0}", FeatureDisabledTime.ToString());
                    }
                    else
                        Debug.Console(1, this, "Unable to parse {0} to DateTime", Config.OccupancyEndTime);
                }
                catch (Exception e)
                {
                    Debug.Console(1, this, "Unable to parse a DateTime config value \n Error: {1}", e);
                }

                AddEnableEventToGroup();

                AddDisableEventToGroup();

                FeatureEnabled = CheckIfFeatureShouldBeEnabled();

                FeatureEventGroup.UserGroupCallBack += new ScheduledEventGroup.UserEventGroupCallBack(FeatureEventGroup_UserGroupCallBack);

                FeatureEventGroup.EnableAllEvents();
            }
            else
                Debug.Console(1, this, "Unable to get room from Device Manager with key: {0}", Config.RoomKey);

            return base.CustomActivate();
        }

        void FeatureEventGroup_UserGroupCallBack(ScheduledEvent SchEvent, ScheduledEventCommon.eCallbackReason type)
        {
            if (type == ScheduledEventCommon.eCallbackReason.NormalExpiration)
            {
                if (SchEvent.Name == FeatureEnableEventName)
                {
                    if (Config.EnableRoomOnWhenOccupied)
                        FeatureEnabled = true;

                    Debug.Console(1, this, "*****Feature Enabled.*****");
                }
                else if (SchEvent.Name == FeatureDisableEventName)
                {
                    FeatureEnabled = false;

                    Debug.Console(1, this, "*****Feature Disabled.*****");
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

            if(Config.EnableRoomOnWhenOccupied)
            {
                if (DateTime.Now.CompareTo(FeatureEnabledTime) > 0 && FeatureDisabledTime.CompareTo(DateTime.Now) >= 0)
                {
                    enabled = true;
                }
            }
            return enabled;
        }

        /// <summary>
        /// Respond to Occupancy status event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RoomIsOccupiedFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            if(e.BoolValue)
            {
                // Occupancy detected

                if (FeatureEnabled)
                {
                    // Check room power state first
                    if(!Room.OnFeedback.BoolValue)
                        Room.PowerOnToDefaultOrLastSource();
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

            schEvent.Acknowledgeable = true;

            schEvent.Persistent = true;

            schEvent.DateAndTime.SetFirstDayOfWeek(ScheduledEventCommon.eFirstDayOfWeek.Sunday);

            // Set config driven properties

            if (schEvent.Name == FeatureEnableEventName)
            {
                schEvent.Description = "Enables the RoomOnToDefaultSourceWhenOccupiedFeature";

                // Check to make sure the date for this event is in the future
                if (DateTime.Now.CompareTo(FeatureEnabledTime) > 0)
                    schEvent.DateAndTime.SetAbsoluteEventTime(FeatureEnabledTime.AddDays(1));
                else
                    schEvent.DateAndTime.SetAbsoluteEventTime(FeatureEnabledTime);

                Debug.Console(1, this, "Event '{0}' Absolute time set to {1}", schEvent.Name, schEvent.DateAndTime.ToString());

                CalculateAndSetAcknowledgeExpirationTimeout(schEvent, FeatureEnabledTime, FeatureDisabledTime);

                schEvent.Recurrence.Weekly((ScheduledEventCommon.eWeekDays)CalculateDaysEnum());
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
                FeatureEnableEvent = FeatureEventGroup.ScheduledEvents[FeatureEnableEventName];
                Debug.Console(1, this, "Enable event already found in group");

                // TODO: Check config times and days against DateAndTime of existing event.  If different, may need to remove event and recreate?

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

                // TODO: Check config times and days against DateAndTime of existing event.  If different, may need to remove event and recreate?
            }

        }


        /// <summary>
        /// Calculates the correct enum value for the event recurrence based on the config values
        /// </summary>
        /// <returns></returns>
        ScheduledEventCommon.eWeekDays CalculateDaysEnum()
        {
            ScheduledEventCommon.eWeekDays value = new ScheduledEventCommon.eWeekDays();

            if (Config.EnableSunday)
                value = value + (int)ScheduledEventCommon.eWeekDays.Sunday;
            if (Config.EnableMonday)
                value = value + (int)ScheduledEventCommon.eWeekDays.Monday;
            if (Config.EnableTuesday)
                value = value + (int)ScheduledEventCommon.eWeekDays.Tuesday;
            if (Config.EnableWednesday)
                value = value + (int)ScheduledEventCommon.eWeekDays.Wednesday;
            if (Config.EnableThursday)
                value = value + (int)ScheduledEventCommon.eWeekDays.Thursday;
            if (Config.EnableFriday)
                value = value + (int)ScheduledEventCommon.eWeekDays.Friday;
            if (Config.EnableSaturday)
                value = value + (int)ScheduledEventCommon.eWeekDays.Saturday;

            return value;
        }

        /// <summary>
        /// Callback for event that enables feature
        /// </summary>
        /// <param name="SchEvent"></param>
        /// <param name="type"></param>
        void FeatureEnableEvent_UserCallBack(ScheduledEvent SchEvent, ScheduledEventCommon.eCallbackReason type)
        {
            if (type == ScheduledEventCommon.eCallbackReason.NormalExpiration)
            {
                if(Config.EnableRoomOnWhenOccupied)
                    FeatureEnabled = true;

                Debug.Console(1, this, "RoomOnToDefaultSourceWhenOccupied Feature Enabled.");
            }
        }

        /// <summary>
        /// Callback for event that enables feature
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
}