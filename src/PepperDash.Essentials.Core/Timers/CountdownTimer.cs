using System;
using System.Timers;

using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Core;

/// <summary>
/// A class that represents a countdown timer with feedbacks for time remaining, percent, and seconds
/// </summary>
public class SecondsCountdownTimer: IKeyed
{
    /// <summary>
    /// Event triggered when the timer starts.
    /// </summary>
    public event EventHandler<EventArgs> HasStarted;
    /// <summary>
    /// Event triggered when the timer finishes.
    /// </summary>
    public event EventHandler<EventArgs> HasFinished;
    /// <summary>
    /// Event triggered when the timer is cancelled.
    /// </summary>
    public event EventHandler<EventArgs> WasCancelled;

    /// <inheritdoc />
    public string Key { get; private set; }

    /// <summary>
    /// Indicates whether the timer is currently running
    /// </summary> 
    public BoolFeedback IsRunningFeedback { get; private set; }
    bool _isRunning;

    /// <summary>
    /// Feedback for the percentage of time remaining
    /// </summary>
    public IntFeedback PercentFeedback { get; private set; }

    /// <summary>
    /// Feedback for the time remaining in a string format
     // </summary>
    public StringFeedback TimeRemainingFeedback { get; private set; }

    /// <summary>
    /// Feedback for the time remaining in seconds
    /// </summary>
    public IntFeedback SecondsRemainingFeedback { get; private set; }

    /// <summary>
    /// When true, the timer will count down immediately upon calling Start. When false, the timer will count up, and when Finish is called, it will stop counting and fire the HasFinished event.
    /// </summary>
    public bool CountsDown { get; set; }

    /// <summary>
    /// The number of seconds to countdown
    /// </summary>
    public int SecondsToCount { get; set; }
    
    /// <summary>
    /// The time at which the timer was started. Used to calculate percent and time remaining. Will be DateTime.MinValue if the timer is not currently running.
    /// </summary>
    public DateTime StartTime { get; private set; }

    /// <summary>
    /// The time at which the timer will finish counting down. Used to calculate percent and time remaining. Will be DateTime.MinValue if the timer is not currently running.
    /// </summary>
    public DateTime FinishTime { get; private set; }

    private Timer _secondTimer;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="key"></param>
    public SecondsCountdownTimer(string key)
    {
        Key = key;
        IsRunningFeedback = new BoolFeedback(() => _isRunning);

        TimeRemainingFeedback = new StringFeedback(() =>
            {
                // Need to handle up and down here.

                var timeSpan = FinishTime - DateTime.Now;

                Debug.LogMessage(LogEventLevel.Verbose,
                    "timeSpan.Minutes == {0}, timeSpan.Seconds == {1}, timeSpan.TotalSeconds == {2}", this,
                    timeSpan.Minutes, timeSpan.Seconds, timeSpan.TotalSeconds);

                if (Math.Floor(timeSpan.TotalSeconds) < 60 && Math.Floor(timeSpan.TotalSeconds) >= 0) //ignore milliseconds
                {
                    return String.Format("{0:00}", timeSpan.Seconds);
                }

                return Math.Floor(timeSpan.TotalSeconds) < 0
                    ? "00"
                    : String.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);
            });

        SecondsRemainingFeedback = new IntFeedback(() => (int)(FinishTime - DateTime.Now).TotalSeconds);

        PercentFeedback =
            new IntFeedback(
                () =>
                    (int)
                        (Math.Floor((FinishTime - DateTime.Now).TotalSeconds)/
                         Math.Floor((FinishTime - StartTime).TotalSeconds)*100));
    }

    /// <summary>
    /// Starts the Timer
    /// </summary>
    public void Start()
    {
        if (_isRunning)
            return;
        StartTime = DateTime.Now;
        FinishTime = StartTime + TimeSpan.FromSeconds(SecondsToCount);

        if (_secondTimer != null)
            _secondTimer.Stop();
        _secondTimer = new Timer(1000) { AutoReset = true };
        _secondTimer.Elapsed += (s, e) => SecondElapsedTimerCallback(null);
        _secondTimer.Start();
        SecondElapsedTimerCallback(null);
        _isRunning = true;
        IsRunningFeedback.FireUpdate();

        var handler = HasStarted;
        if (handler != null)
            handler(this, new EventArgs());
    }

    /// <summary>
    /// Restarts the timer
    /// </summary>
    public void Reset()
    {
        _isRunning = false;
        IsRunningFeedback.FireUpdate();
        Start();
    }

    /// <summary>
    /// Cancels the timer (without triggering it to finish)
    /// </summary>
    public void Cancel()
    {
        StopHelper();
        
        var handler = WasCancelled;
        if (handler != null)
            handler(this, new EventArgs());
    }

    /// <summary>
    /// Called upon expiration, or calling this will force timer to finish.
    /// </summary>
    public void Finish()
    {
        StopHelper();

        var handler = HasFinished;
        if (handler != null)
            handler(this, new EventArgs());
    }

    void StopHelper()
    {
        if (_secondTimer != null)
        {
            _secondTimer.Stop();
            _secondTimer = null;
        }

        _isRunning = false;
        IsRunningFeedback.FireUpdate(); 
    }

    void SecondElapsedTimerCallback(object o)
    {
        if (DateTime.Now >= FinishTime)
        {
            Finish();
            return;
        }

        PercentFeedback.FireUpdate();
        TimeRemainingFeedback.FireUpdate();
        SecondsRemainingFeedback.FireUpdate();
    }
}