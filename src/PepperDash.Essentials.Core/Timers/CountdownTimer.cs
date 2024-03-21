using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Core
{
    public class SecondsCountdownTimer: IKeyed
    {
        public event EventHandler<EventArgs> HasStarted;
        public event EventHandler<EventArgs> HasFinished;
        public event EventHandler<EventArgs> WasCancelled;

        public string Key { get; private set; }

        public BoolFeedback IsRunningFeedback { get; private set; }
        bool _isRunning;

        public IntFeedback PercentFeedback { get; private set; }
        public StringFeedback TimeRemainingFeedback { get; private set; }

        public bool CountsDown { get; set; }

        /// <summary>
        /// The number of seconds to countdown
        /// </summary>
        public int SecondsToCount { get; set; }
        
        public DateTime StartTime { get; private set; }
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

                    Debug.LogMessage(LogEventLevel.Verbose, this,
                        "timeSpan.Minutes == {0}, timeSpan.Seconds == {1}, timeSpan.TotalSeconds == {2}",
                        timeSpan.Minutes, timeSpan.Seconds, timeSpan.TotalSeconds);

                    if (Math.Floor(timeSpan.TotalSeconds) < 60 && Math.Floor(timeSpan.TotalSeconds) >= 0) //ignore milliseconds
                    {
                        return String.Format("{0:00}", timeSpan.Seconds);
                    }

                    return Math.Floor(timeSpan.TotalSeconds) < 0
                        ? "00"
                        : String.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);
                });

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
            _secondTimer = new CTimer(SecondElapsedTimerCallback, null, 0, 1000);
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
                _secondTimer.Stop();
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
        }
    }
}