

using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.Codec
{
  /// <summary>
  /// Represents a CodecScheduleAwareness
  /// </summary>
  public class CodecScheduleAwareness
  {
    List<Meeting> _meetings;

    /// <summary>
    /// Event that is raised when a meeting event changes
    /// </summary>
    public event EventHandler<MeetingEventArgs> MeetingEventChange;

    /// <summary>
    /// Event that is raised when the meetings list has changed
    /// </summary>
    public event EventHandler<EventArgs> MeetingsListHasChanged;

    private int _meetingWarningMinutes = 5;

    //private Meeting _previousChangedMeeting;

    //private eMeetingEventChangeType _previousChangeType = eMeetingEventChangeType.Unknown;

    /// <summary>
    /// Gets or sets the number of minutes before a meeting to issue a warning
    /// </summary>
    public int MeetingWarningMinutes
    {
      get { return _meetingWarningMinutes; }
      set { _meetingWarningMinutes = value; }
    }

    /// <summary>
    /// Setter triggers MeetingsListHasChanged event
    /// </summary>
    public List<Meeting> Meetings
    {
      get
      {
        return _meetings;
      }
      set
      {
        _meetings = value;
        MeetingsListHasChanged?.Invoke(this, new EventArgs());
      }
    }

    private readonly CTimer _scheduleChecker;

    /// <summary>
    /// Initializes a new instance of the CodecScheduleAwareness class with default poll time
    /// </summary>
    public CodecScheduleAwareness()
    {
      Meetings = new List<Meeting>();

      _scheduleChecker = new CTimer(CheckSchedule, null, 1000, 1000);
    }

    /// <summary>
    /// Initializes a new instance of the CodecScheduleAwareness class with specified poll time
    /// </summary>
    /// <param name="pollTime">The poll time in milliseconds for checking schedule changes</param>
    public CodecScheduleAwareness(long pollTime)
    {
      Meetings = new List<Meeting>();

      _scheduleChecker = new CTimer(CheckSchedule, null, pollTime, pollTime);
    }

    /// <summary>
    /// Helper method to fire MeetingEventChange.  Should only fire once for each changeType on each meeting
    /// </summary>
    /// <param name="changeType"></param>
    /// <param name="meeting"></param>
    private void OnMeetingChange(eMeetingEventChangeType changeType, Meeting meeting)
    {
      Debug.LogMessage(LogEventLevel.Verbose, "*****************OnMeetingChange.  id: {0} changeType: {1}**********************", meeting.Id, changeType);
      if (changeType != (changeType & meeting.NotifiedChangeTypes))
      {
        // Add this change type to the NotifiedChangeTypes
        meeting.NotifiedChangeTypes |= changeType;
        MeetingEventChange?.Invoke(this, new MeetingEventArgs() { ChangeType = changeType, Meeting = meeting });
      }
      else
      {
        Debug.LogMessage(LogEventLevel.Verbose, "Meeting: {0} already notified of changeType: {1}", meeting.Id, changeType);
      }
    }


    /// <summary>
    /// Checks the schedule to see if any MeetingEventChange updates should be fired
    /// </summary>
    /// <param name="o"></param>
    private void CheckSchedule(object o)
    {
      //  Iterate the meeting list and check if any meeting need to do anything

      const double meetingTimeEpsilon = 0.05;
      foreach (var m in Meetings)
      {
        var changeType = eMeetingEventChangeType.Unknown;

        if (eMeetingEventChangeType.MeetingStartWarning != (m.NotifiedChangeTypes & eMeetingEventChangeType.MeetingStartWarning) && m.TimeToMeetingStart.TotalMinutes <= m.MeetingWarningMinutes.TotalMinutes && m.TimeToMeetingStart.Seconds > 0)       // Meeting is about to start
        {
          Debug.LogMessage(LogEventLevel.Verbose, "********************* MeetingStartWarning. TotalMinutes: {0}  Seconds: {1}", m.TimeToMeetingStart.TotalMinutes, m.TimeToMeetingStart.Seconds);
          changeType = eMeetingEventChangeType.MeetingStartWarning;
        }
        else if (eMeetingEventChangeType.MeetingStart != (m.NotifiedChangeTypes & eMeetingEventChangeType.MeetingStart) && Math.Abs(m.TimeToMeetingStart.TotalMinutes) < meetingTimeEpsilon)           // Meeting Start
        {
          Debug.LogMessage(LogEventLevel.Verbose, "********************* MeetingStart");
          changeType = eMeetingEventChangeType.MeetingStart;
        }
        else if (eMeetingEventChangeType.MeetingEndWarning != (m.NotifiedChangeTypes & eMeetingEventChangeType.MeetingEndWarning) && m.TimeToMeetingEnd.TotalMinutes <= m.MeetingWarningMinutes.TotalMinutes && m.TimeToMeetingEnd.Seconds > 0)    // Meeting is about to end
        {
          Debug.LogMessage(LogEventLevel.Verbose, "********************* MeetingEndWarning. TotalMinutes: {0}  Seconds: {1}", m.TimeToMeetingEnd.TotalMinutes, m.TimeToMeetingEnd.Seconds);
          changeType = eMeetingEventChangeType.MeetingEndWarning;
        }
        else if (eMeetingEventChangeType.MeetingEnd != (m.NotifiedChangeTypes & eMeetingEventChangeType.MeetingEnd) && Math.Abs(m.TimeToMeetingEnd.TotalMinutes) < meetingTimeEpsilon)             // Meeting has ended
        {
          Debug.LogMessage(LogEventLevel.Verbose, "********************* MeetingEnd");
          changeType = eMeetingEventChangeType.MeetingEnd;
        }

        if (changeType != eMeetingEventChangeType.Unknown)
        {
          OnMeetingChange(changeType, m);
        }
      }

    }
  }

}
