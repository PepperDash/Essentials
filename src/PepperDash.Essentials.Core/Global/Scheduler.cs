using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharp.Scheduler;

using PepperDash.Core;
using PepperDash.Essentials.Core.Fusion;
using PepperDash.Essentials.Room.Config;
using Serilog.Events;
using Activator = System.Activator;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Global Scheduler for the system
    /// </summary>
    public static class Scheduler
    {
        private static readonly Dictionary<string, ScheduledEventGroup> EventGroups = new Dictionary<string,ScheduledEventGroup>();

        static Scheduler()
        {
            CrestronConsole.AddNewConsoleCommand(DeleteEventGroup, "DeleteEventGroup", "Deletes the event group by key", ConsoleAccessLevelEnum.AccessOperator);

            CrestronConsole.AddNewConsoleCommand(ClearEventsFromGroup, "ClearAllEvents", "Clears all scheduled events for this group", ConsoleAccessLevelEnum.AccessOperator);

            CrestronConsole.AddNewConsoleCommand(ListAllEventGroups, "ListAllEventGroups", "Lists all the event groups by key", ConsoleAccessLevelEnum.AccessOperator);

            CrestronConsole.AddNewConsoleCommand(ListAllEventsForGroup, "ListEventsForGroup",
                "Lists all events for the given group", ConsoleAccessLevelEnum.AccessOperator);
        }


        static void DeleteEventGroup(string groupName)
        {
            if (EventGroups.ContainsKey(groupName))
            {
                var group = EventGroups[groupName];
                 
                EventGroups.Remove(groupName);

                group.Dispose();

                group = null;
            }
        }   

        /// <summary>
        /// Clears (deletes) all events from a group
        /// </summary>
        /// <param name="groupName"></param>
        static void ClearEventsFromGroup(string groupName)
        {
            if (!EventGroups.ContainsKey(groupName))
            {
                Debug.LogMessage(LogEventLevel.Information,
                    "[Scheduler]: Unable to delete events from group '{0}'.  Group not found in EventGroups dictionary.", null,
                    groupName);
                return;
            }

            var group = EventGroups[groupName];

            if (group != null)
            {
                group.ClearAllEvents();

                Debug.LogMessage(LogEventLevel.Information, "[Scheduler]: All events deleted from group '{0}'", null, groupName);
            }
            else
                Debug.LogMessage(LogEventLevel.Information,
                    "[Scheduler]: Unable to delete events from group '{0}'.  Group not found in EventGroups dictionary.", null,
                    groupName);
        }

        static void ListAllEventGroups(string command)
        {
            CrestronConsole.ConsoleCommandResponse("Event Groups:");
            foreach (var group in EventGroups)
            {
                CrestronConsole.ConsoleCommandResponse($"{group.Key}");
            }
        }

        static void ListAllEventsForGroup(string args)
        {
            Debug.LogMessage(LogEventLevel.Information, "Getting events for group {0}...", null, args);

            ScheduledEventGroup group;

            if (!EventGroups.TryGetValue(args, out group))
            {
                Debug.LogMessage(LogEventLevel.Information, "Unabled to get event group for key {0}", null, args);
                return;
            }

            foreach (var evt in group.ScheduledEvents)
            {
                CrestronConsole.ConsoleCommandResponse(
$@"
****Event key {evt.Key}****
Event state: {evt.Value.EventState}
Event date/time: {evt.Value.DateAndTime}
Persistent: {evt.Value.Persistent}
Acknowlegable: {evt.Value.Acknowledgeable}
Recurrence: {evt.Value.Recurrence.Recurrence}
Recurrence Days: {evt.Value.Recurrence.RecurrenceDays}
********************");
            }
        }

        /// <summary>
        /// Adds the event group to the global list
        /// </summary>
        /// <returns></returns>
        /// <summary>
        /// AddEventGroup method
        /// </summary>
        public static void AddEventGroup(ScheduledEventGroup eventGroup)
        {
            // Add this group to the global collection
            if (!EventGroups.ContainsKey(eventGroup.Name))
                EventGroups.Add(eventGroup.Name, eventGroup);
        }

        /// <summary>
        /// Removes the event group from the global list
        /// </summary>
        /// <param name="eventGroup"></param>
        /// <summary>
        /// RemoveEventGroup method
        /// </summary>
        public static void RemoveEventGroup(ScheduledEventGroup eventGroup)
        {
            if(!EventGroups.ContainsKey(eventGroup.Name))
                EventGroups.Remove(eventGroup.Name);
        }

        /// <summary>
        /// Gets the event group by key
        /// </summary>
        /// <param name="key">key of the event group</param>
        /// <returns></returns>
        public static ScheduledEventGroup GetEventGroup(string key)
        {
            ScheduledEventGroup returnValue;

            return EventGroups.TryGetValue(key, out returnValue) ? returnValue : null;
        }
    }

    /// <summary>
    /// SchedulerUtilities class
    /// </summary>
    public static class SchedulerUtilities
    {
        /// <summary>
        /// Checks the day of week in eventTime to see if it matches the weekdays defined in the recurrence enum.
        /// </summary>
        /// <param name="eventTime"></param>
        /// <param name="recurrence"></param>
        /// <returns></returns>
        /// <summary>
        /// CheckIfDayOfWeekMatchesRecurrenceDays method
        /// </summary>
        public static bool CheckIfDayOfWeekMatchesRecurrenceDays(DateTime eventTime, ScheduledEventCommon.eWeekDays recurrence)
        {
            bool isMatch = false;

            var dayOfWeek = eventTime.DayOfWeek;

            Debug.LogMessage(LogEventLevel.Debug, "[Scheduler]: eventTime day of week is: {0}",null, dayOfWeek);
            switch (dayOfWeek)
            {   
                case DayOfWeek.Sunday:
                    {
                        if ((recurrence & ScheduledEventCommon.eWeekDays.Sunday) == ScheduledEventCommon.eWeekDays.Sunday)
                            isMatch = true;
                        break;
                    }
                case DayOfWeek.Monday:
                    {
                        if ((recurrence & ScheduledEventCommon.eWeekDays.Monday) == ScheduledEventCommon.eWeekDays.Monday)
                            isMatch = true;
                        break;
                    }
                case DayOfWeek.Tuesday:
                    {
                        if ((recurrence & ScheduledEventCommon.eWeekDays.Tuesday) == ScheduledEventCommon.eWeekDays.Tuesday)
                            isMatch = true;
                        break;
                    }
                case DayOfWeek.Wednesday:
                    {
                        if ((recurrence & ScheduledEventCommon.eWeekDays.Wednesday) == ScheduledEventCommon.eWeekDays.Wednesday)
                            isMatch = true;
                        break;
                    }
                case DayOfWeek.Thursday:
                    {
                        if ((recurrence & ScheduledEventCommon.eWeekDays.Thursday) == ScheduledEventCommon.eWeekDays.Thursday)
                            isMatch = true;
                        break;
                    }
                case DayOfWeek.Friday:
                    {
                        if ((recurrence & ScheduledEventCommon.eWeekDays.Friday) == ScheduledEventCommon.eWeekDays.Friday)
                            isMatch = true;
                        break;
                    }
                case DayOfWeek.Saturday:
                    {
                        if ((recurrence & ScheduledEventCommon.eWeekDays.Saturday) == ScheduledEventCommon.eWeekDays.Saturday)
                            isMatch = true;
                        break;
                    }
            }

            Debug.LogMessage(LogEventLevel.Debug, "[Scheduler]: eventTime day of week matches recurrence days: {0}", isMatch);

            return isMatch;
        }

        /// <summary>
        /// CheckEventTimeForMatch method
        /// </summary>
        public static bool CheckEventTimeForMatch(ScheduledEvent evnt, DateTime time)
        {
            return evnt.DateAndTime.Hour == time.Hour && evnt.DateAndTime.Minute == time.Minute;
        }

        /// <summary>
        /// CheckEventRecurrenceForMatch method
        /// </summary>
        public static bool CheckEventRecurrenceForMatch(ScheduledEvent evnt, ScheduledEventCommon.eWeekDays days)
        {
            return evnt.Recurrence.RecurrenceDays == days;
        }

        /// <summary>
        /// CreateEventFromConfig method
        /// </summary>
        public static void CreateEventFromConfig(ScheduledEventConfig config, ScheduledEventGroup group, ScheduledEvent.UserEventCallBack handler)
        {
            try
            {
                if (group == null)
                {
                    Debug.LogMessage(LogEventLevel.Information, "Unable to create event. Group is null", null, null);
                    return;
                }
                var scheduledEvent = new ScheduledEvent(config.Key, group)
                {
                    Acknowledgeable = config.Acknowledgeable,
                    Persistent = config.Persistent
                };

                scheduledEvent.UserCallBack += handler;

                scheduledEvent.DateAndTime.SetFirstDayOfWeek(ScheduledEventCommon.eFirstDayOfWeek.Sunday);

                var eventTime = DateTime.Parse(config.Time);

                if (DateTime.Now > eventTime)
                {
                    eventTime = eventTime.AddDays(1);
                }

                Debug.LogMessage(LogEventLevel.Verbose, "[Scheduler] Current Date day of week: {0} recurrence days: {1}", null, eventTime.DayOfWeek,
                    config.Days);

                var dayOfWeekConverted = ConvertDayOfWeek(eventTime);

                Debug.LogMessage(LogEventLevel.Debug, "[Scheduler] eventTime Day: {0}", null, dayOfWeekConverted);

                while (!dayOfWeekConverted.IsFlagSet(config.Days))
                {
                    eventTime = eventTime.AddDays(1);

                    dayOfWeekConverted = ConvertDayOfWeek(eventTime);
                }

                scheduledEvent.DateAndTime.SetAbsoluteEventTime(eventTime);

                scheduledEvent.Recurrence.Weekly(config.Days);

                Debug.LogMessage(LogEventLevel.Verbose, $"[Scheduler] Event State: {scheduledEvent.EventState}", null, null);

                if (config.Enable && scheduledEvent.EventState != ScheduledEventCommon.eEventState.Enabled)
                {
                    scheduledEvent.Enable();
                }
                else if (!config.Enable && scheduledEvent.EventState != ScheduledEventCommon.eEventState.Disabled)
                {
                    scheduledEvent.Disable();
                }
 
            }
            catch (Exception e)
            {

                Debug.LogMessage(LogEventLevel.Error, "Error creating scheduled event: {0}", null, e);
            }
        }

        private static ScheduledEventCommon.eWeekDays ConvertDayOfWeek(DateTime eventTime)
        {
            return (ScheduledEventCommon.eWeekDays) Enum.Parse(typeof(ScheduledEventCommon.eWeekDays), eventTime.DayOfWeek.ToString(), true);
        }

        private static bool IsFlagSet<T>(this T value, T flag) where T : struct
        {
            CheckIsEnum<T>(true);

            var lValue = Convert.ToInt64(value);
            var lFlag = Convert.ToInt64(flag);

            return (lValue & lFlag) != 0;
        }

        private static void CheckIsEnum<T>(bool withFlags)
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(string.Format("Type '{0}' is not an enum", typeof(T).FullName));
            if (withFlags && !Attribute.IsDefined(typeof(T), typeof(FlagsAttribute)))
                throw new ArgumentException(string.Format("Type '{0}' doesn't have the 'Flags' attribute", typeof(T).FullName));
        }
    }
}