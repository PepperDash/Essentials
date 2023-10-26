using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharp.Scheduler;

using PepperDash.Core;
using PepperDash.Essentials.Core.Fusion;
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
            CrestronConsole.AddNewConsoleCommand(ClearEventsFromGroup, "ClearAllEvents", "Clears all scheduled events for this group", ConsoleAccessLevelEnum.AccessOperator);

            CrestronConsole.AddNewConsoleCommand(ListAllEventGroups, "ListAllEventGroups", "Lists all the event groups by key", ConsoleAccessLevelEnum.AccessOperator);

            CrestronConsole.AddNewConsoleCommand(ListAllEventsForGroup, "ListEventsForGroup",
                "Lists all events for the given group", ConsoleAccessLevelEnum.AccessOperator);

        }

        /// <summary>
        /// Clears (deletes) all events from a group
        /// </summary>
        /// <param name="groupName"></param>
        static void ClearEventsFromGroup(string groupName)
        {
            if (!EventGroups.ContainsKey(groupName))
            {
                Debug.Console(0,
                    "[Scheduler]: Unable to delete events from group '{0}'.  Group not found in EventGroups dictionary.",
                    groupName);
                return;
            }

            var group = EventGroups[groupName];

            if (group != null)
            {
                group.ClearAllEvents();

                Debug.Console(0, "[Scheduler]: All events deleted from group '{0}'", groupName);
            }
            else
                Debug.Console(0,
                    "[Scheduler]: Unable to delete events from group '{0}'.  Group not found in EventGroups dictionary.",
                    groupName);
        }

        static void ListAllEventGroups(string command)
        {
            Debug.Console(0, "Event Groups:");
            foreach (var group in EventGroups)
            {
                Debug.Console(0, "{0}", group.Key);
            }
        }

        static void ListAllEventsForGroup(string args)
        {
            Debug.Console(0, "Getting events for group {0}...", args);

            ScheduledEventGroup group;

            if (!EventGroups.TryGetValue(args, out group))
            {
                Debug.Console(0, "Unabled to get event group for key {0}", args);
                return;
            }

            foreach (var evt in group.ScheduledEvents)
            {
                Debug.Console(0,
                    @"
****Event key {0}****
Event date/time: {1}
Persistent: {2}
Acknowlegable: {3}
Recurrence: {4}
Recurrence Days: {5}
********************", evt.Key, evt.Value.DateAndTime, evt.Value.Persistent, evt.Value.Acknowledgeable,
                    evt.Value.Recurrence.Recurrence, evt.Value.Recurrence.RecurrenceDays);
            }
        }

        /// <summary>
        /// Adds the event group to the global list
        /// </summary>
        /// <returns></returns>
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
        public static void RemoveEventGroup(ScheduledEventGroup eventGroup)
        {
            if(!EventGroups.ContainsKey(eventGroup.Name))
                EventGroups.Remove(eventGroup.Name);
        }

        public static ScheduledEventGroup GetEventGroup(string key)
        {
            ScheduledEventGroup returnValue;

            return EventGroups.TryGetValue(key, out returnValue) ? returnValue : null;
        }
    }
}