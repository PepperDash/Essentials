using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

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
 
        CTimer SecondTimer;

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

                    if (timeSpan.TotalSeconds < 60)
                    {
                        return Math.Round(timeSpan.TotalSeconds).ToString();
                    }
                    else if (timeSpan.TotalSeconds < 0)
                    {
                        return "0";
                    }
                    else
                    {
                        Debug.Console(2, this, "timeSpan.Minutes == {0}, timeSpan.Seconds == {1}", timeSpan.Minutes, timeSpan.Seconds);
                        return String.Format("{0:c}", timeSpan);
                    }
                });

            PercentFeedback = new IntFeedback(() =>
            {
                var percent = (FinishTime - DateTime.Now).TotalSeconds
                    / (FinishTime - StartTime).TotalSeconds 
                    * 100;
                return (int)percent;
            });
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

            if (SecondTimer != null)
                SecondTimer.Stop();
            SecondTimer = new CTimer(SecondElapsedTimerCallback, null, 0, 1000);
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
            if (SecondTimer != null)
                SecondTimer.Stop();
            _isRunning = false;
            IsRunningFeedback.FireUpdate(); 
        }

        void SecondElapsedTimerCallback(object o)
        {
            PercentFeedback.FireUpdate();
            TimeRemainingFeedback.FireUpdate();

            if (DateTime.Now >= FinishTime)
                Finish();
        }
    }
}