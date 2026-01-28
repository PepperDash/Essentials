using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Represents a SecondsCountdownTimer
    /// </summary>
    public class SecondsCountdownTimer: IKeyed
    {
        /// <summary>
        /// Event fired when the timer starts
        /// </summary>
        public event EventHandler<EventArgs> HasStarted;

        /// <summary>
        /// Event fired when the timer finishes
        /// </summary>
        public event EventHandler<EventArgs> HasFinished;

        /// <summary>
        /// Event fired when the timer is cancelled
        /// </summary>
        public event EventHandler<EventArgs> WasCancelled;

        /// <summary>
        /// Gets or sets the Key
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Gets or sets the IsRunningFeedback
        /// </summary>
        public BoolFeedback IsRunningFeedback { get; private set; }
        bool _isRunning;

        /// <summary>
        /// Gets or sets the PercentFeedback
        /// </summary>
        public IntFeedback PercentFeedback { get; private set; }
        
        /// <summary>
        /// Gets or sets the TimeRemainingFeedback
        /// </summary>
        public StringFeedback TimeRemainingFeedback { get; private set; }

        /// <summary>
        /// Gets or sets the SecondsRemainingFeedback
        /// </summary>
        public IntFeedback SecondsRemainingFeedback { get; private set; }

        /// <summary>
        /// Gets or sets the CountsDown
        /// </summary>
        public bool CountsDown { get; set; }

        /// <summary>
        /// Gets or sets the SecondsToCount
        /// </summary>
        public int SecondsToCount { get; set; }
        
        /// <summary>
        /// Gets or sets the StartTime
        /// </summary>
        public DateTime StartTime { get; private set; }
        /// <summary>
        /// Gets or sets the FinishTime
        /// </summary>
        public DateTime FinishTime { get; private set; }
 
        private CTimer _secondTimer;

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
        /// Start method
        /// </summary>
        public void Start()
        {
            if (_isRunning)
                return;
            StartTime = DateTime.Now;
            FinishTime = StartTime + TimeSpan.FromSeconds(SecondsToCount);

            if (_secondTimer != null)
                _secondTimer.Stop();
            _secondTimer = new CTimer(SecondElapsedTimerCallback, null, 0, 1000);
            _isRunning = true;
            IsRunningFeedback.FireUpdate();

            var handler = HasStarted;
            if (handler != null)
                handler(this, new EventArgs());
        }

        /// <summary>
        /// Reset method
        /// </summary>
        public void Reset()
        {
            _isRunning = false;
            IsRunningFeedback.FireUpdate();
            Start();
        }

        /// <summary>
        /// Cancel method
        /// </summary>
        public void Cancel()
        {
            StopHelper();
            
            var handler = WasCancelled;
            if (handler != null)
                handler(this, new EventArgs());
        }

        /// <summary>
        /// Finish method
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
}