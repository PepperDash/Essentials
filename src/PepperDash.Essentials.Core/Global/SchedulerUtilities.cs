using System;
using Crestron.SimplSharp.Scheduler;
using PepperDash.Core;
using PepperDash.Essentials.Room.Config;

namespace PepperDash.Essentials.Core
{
    public static class SchedulerUtilities
    {
        /// <summary>
        /// Checks the day of week in eventTime to see if it matches the weekdays defined in the recurrence enum.
        /// </summary>
        /// <param name="eventTime"></param>
        /// <param name="recurrence"></param>
        /// <returns></returns>
        public static bool CheckIfDayOfWeekMatchesRecurrenceDays(DateTime eventTime, ScheduledEventCommon.eWeekDays recurrence)
        {
            bool isMatch = false;

            var dayOfWeek = eventTime.DayOfWeek;

            Debug.Console(1, "[Scheduler]: eventTime day of week is: {0}", dayOfWeek);

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

            Debug.Console(1, "[Scheduler]: eventTime day of week matches recurrence days: {0}", isMatch);

            return isMatch;
        }

        public static bool CheckEventTimeForMatch(ScheduledEvent evnt, DateTime time)
        {
            return evnt.DateAndTime.Hour == time.Hour && evnt.DateAndTime.Minute == time.Minute;
        }

        public static bool CheckEventRecurrenceForMatch(ScheduledEvent evnt, ScheduledEventCommon.eWeekDays days)
        {
            return evnt.Recurrence.RecurrenceDays == days;
        }

        public static void CreateEventFromConfig(ScheduledEventConfig config, ScheduledEventGroup group, ScheduledEvent.UserEventCallBack handler)
        {
            if (group == null)
            {
                Debug.Console(0, "Unable to create event. Group is null");
                return;
            }
            var scheduledEvent = new ScheduledEvent(config.Key, group)
            {
                Acknowledgeable = config.Acknowledgeable,
                Persistent      = config.Persistent
            };

            scheduledEvent.UserCallBack += handler;

            scheduledEvent.DateAndTime.SetFirstDayOfWeek(ScheduledEventCommon.eFirstDayOfWeek.Sunday);

            var eventTime = DateTime.Parse(config.Time);

            if (DateTime.Now > eventTime)
            {
                eventTime = eventTime.AddDays(1);
            }

            Debug.Console(2, "[Scheduler] Current Date day of week: {0} recurrence days: {1}", eventTime.DayOfWeek,
                config.Days);

            var dayOfWeekConverted = ConvertDayOfWeek(eventTime);

            Debug.Console(1, "[Scheduler] eventTime Day: {0}", dayOfWeekConverted);

            while (!dayOfWeekConverted.IsFlagSet(config.Days))
            {
                eventTime = eventTime.AddDays(1);

                dayOfWeekConverted = ConvertDayOfWeek(eventTime);
            }

            scheduledEvent.DateAndTime.SetAbsoluteEventTime(eventTime);

            scheduledEvent.Recurrence.Weekly(config.Days);

            if (config.Enable)
            {
                scheduledEvent.Enable();
            }
            else
            {
                scheduledEvent.Disable();
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
            var lFlag  = Convert.ToInt64(flag);

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