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
        public string Key { get; private set; }

        public BoolFeedback IsRunningFeedback { get; private set; }
        bool _IsRunning;

        public IntFeedback PercentFeedback { get; private set; }
        public StringFeedback TimeRemainingFeedback { get; private set; }

        public bool CountsDown { get; set; }
        public int SecondsToCount { get; set; }
        
        public Action CompletionAction { get; set; }
        public Action CancelAction { get; set; }

        CTimer SecondTimer;
        public DateTime StartTime { get; private set; }
        public DateTime FinishTime { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public SecondsCountdownTimer(string key)
        {
            Key = key;
            IsRunningFeedback = new BoolFeedback(() => _IsRunning);

            TimeRemainingFeedback = new StringFeedback(() =>
                {
                    // Need to handle up and down here.

                    if (StartTime == null || FinishTime == null)
                        return "";
                    var timeSpan = FinishTime - DateTime.Now;
                    return Math.Round(timeSpan.TotalSeconds).ToString();
                });

            PercentFeedback = new IntFeedback(() =>
            {
                if (StartTime == null || FinishTime == null)
                    return 0;
                double percent = (FinishTime - DateTime.Now).TotalSeconds
                    / (FinishTime - StartTime).TotalSeconds 
                    * 100;
                return (int)percent;
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            if (_IsRunning)
                return;
            StartTime = DateTime.Now;
            FinishTime = StartTime + TimeSpan.FromSeconds(SecondsToCount);

            if (SecondTimer != null)
                SecondTimer.Stop();
            SecondTimer = new CTimer(SecondElapsedTimerCallback, null, 0, 1000);
            _IsRunning = true;
            IsRunningFeedback.FireUpdate();

        }

        public void Cancel()
        {
            CleanUp();
            var a = CancelAction;
            if (a != null)
                a();
        }

        public void Reset()
        {
            _IsRunning = false;
            Start();
        }

        public void Finish()
        {
            CleanUp();
            var a = CompletionAction;
            if (a != null)
                a();
        }

        void CleanUp()
        {
            if (SecondTimer != null)
                SecondTimer.Stop();
            _IsRunning = false;
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