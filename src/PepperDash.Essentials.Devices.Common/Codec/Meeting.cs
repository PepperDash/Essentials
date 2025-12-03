

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.Codec
{
  /// <summary>
  /// Represents a Meeting
  /// </summary>
  public class Meeting
  {
    /// <summary>
    /// Minutes before the meeting to show warning
    /// </summary>
    [JsonProperty("minutesBeforeMeeting")]
    public int MinutesBeforeMeeting;

    /// <summary>
    /// Gets or sets the meeting ID
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }
    /// <summary>
    /// Gets or sets the meeting organizer
    /// </summary>
    [JsonProperty("organizer")]
    public string Organizer { get; set; }
    /// <summary>
    /// Gets or sets the Title
    /// </summary>
    [JsonProperty("title")]
    public string Title { get; set; }
    /// <summary>
    /// Gets or sets the Agenda
    /// </summary>
    [JsonProperty("agenda")]
    public string Agenda { get; set; }

    /// <summary>
    /// Gets the meeting warning time span in minutes before the meeting starts
    /// </summary>
    [JsonProperty("meetingWarningMinutes")]
    public TimeSpan MeetingWarningMinutes
    {
      get { return TimeSpan.FromMinutes(MinutesBeforeMeeting); }
    }
    /// <summary>
    /// Gets the time remaining until the meeting starts
    /// </summary>
    [JsonProperty("timeToMeetingStart")]
    public TimeSpan TimeToMeetingStart
    {
      get
      {
        return StartTime - DateTime.Now;
      }
    }
    /// <summary>
    /// Gets the time remaining until the meeting ends
    /// </summary>
    [JsonProperty("timeToMeetingEnd")]
    public TimeSpan TimeToMeetingEnd
    {
      get
      {
        return EndTime - DateTime.Now;
      }
    }
    /// <summary>
    /// Gets or sets the StartTime
    /// </summary>
    [JsonProperty("startTime")]
    public DateTime StartTime { get; set; }
    /// <summary>
    /// Gets or sets the EndTime
    /// </summary>
    [JsonProperty("endTime")]
    public DateTime EndTime { get; set; }
    /// <summary>
    /// Gets the duration of the meeting
    /// </summary>
    [JsonProperty("duration")]
    public TimeSpan Duration
    {
      get
      {
        return EndTime - StartTime;
      }
    }
    /// <summary>
    /// Gets or sets the Privacy
    /// </summary>
    [JsonProperty("privacy")]
    public eMeetingPrivacy Privacy { get; set; }
    /// <summary>
    /// Gets a value indicating whether the meeting can be joined
    /// </summary>
    [JsonProperty("joinable")]
    public bool Joinable
    {
      get
      {
        var joinable = StartTime.AddMinutes(-MinutesBeforeMeeting) <= DateTime.Now
            && DateTime.Now <= EndTime.AddSeconds(-_joinableCooldownSeconds);
        //Debug.LogMessage(LogEventLevel.Verbose, "Meeting Id: {0} joinable: {1}", Id, joinable);
        return joinable;
      }
    }

    /// <summary>
    /// Gets or sets the Dialable
    /// </summary>
    [JsonProperty("dialable")]
    public bool Dialable { get; set; }

    //public string ConferenceNumberToDial { get; set; }

    /// <summary>
    /// Gets or sets the ConferencePassword
    /// </summary>
    [JsonProperty("conferencePassword")]
    public string ConferencePassword { get; set; }

    /// <summary>
    /// Gets or sets the IsOneButtonToPushMeeting
    /// </summary>
    [JsonProperty("isOneButtonToPushMeeting")]
    public bool IsOneButtonToPushMeeting { get; set; }

    /// <summary>
    /// Gets or sets the Calls
    /// </summary>
    [JsonProperty("calls")]
    public List<Call> Calls { get; private set; }

    /// <summary>
    /// Tracks the change types that have already been notified for
    /// Gets or sets the NotifiedChangeTypes
    /// </summary>
    [JsonIgnore]
    public eMeetingEventChangeType NotifiedChangeTypes { get; set; }

    [JsonIgnore] private readonly int _joinableCooldownSeconds;

    /// <summary>
    /// Constructor for Meeting <see cref="Meeting"/>
    /// </summary>
    public Meeting()
    {
      Calls = new List<Call>();
      _joinableCooldownSeconds = 300;
    }

    /// <summary>
    /// Constructor for Meeting <see cref="Meeting"/>
    /// </summary>
    /// <param name="joinableCooldownSeconds">Number of seconds after meeting start when it is no longer joinable</param>
    public Meeting(int joinableCooldownSeconds)
    {
      Calls = new List<Call>();
      _joinableCooldownSeconds = joinableCooldownSeconds;
    }



    #region Overrides of Object

    /// <summary>
    /// ToString method
    /// </summary>
    /// <inheritdoc />
    public override string ToString()
    {
      return string.Format("{0}:{1}: {2}-{3}", Title, Agenda, StartTime, EndTime);
    }

    #endregion
  }

}
