

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

using Newtonsoft.Json;


namespace PepperDash.Essentials.Core.Timers
{
    /// <summary>
    /// A device that runs a retriggerable timer and can execute actions specified in config 
    /// </summary>
    [Description("A retriggerable timer device")]
    public class RetriggerableTimer : EssentialsDevice
    {
        private RetriggerableTimerPropertiesConfig _propertiesConfig;

        private CTimer _timer;
        private long _timerIntervalMs;

        public RetriggerableTimer(string key, DeviceConfig config)
            : base(key, config.Name)
        {
            var props = config.Properties.ToObject<RetriggerableTimerPropertiesConfig>();
            _propertiesConfig = props;

            if (_propertiesConfig != null)
            {
                _timerIntervalMs = _propertiesConfig.TimerIntervalMs;
            }
        }

        public override bool CustomActivate()
        {
            if (_propertiesConfig.StartTimerOnActivation)
            {
                StartTimer();
            }

            return base.CustomActivate();
        }

        private void CleanUpTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
            }

            _timer = null;
        }

        public void StartTimer()
        {
             CleanUpTimer();
             Debug.Console(0, this, "Starting Timer");

             _timer = new CTimer(TimerElapsedCallback, GetActionFromConfig(eRetriggerableTimerEvents.Elapsed), _timerIntervalMs, _timerIntervalMs);
        }

        public void StopTimer()
        {
            Debug.Console(0, this, "Stopping Timer");
            _timer.Stop();

            ExecuteAction(GetActionFromConfig(eRetriggerableTimerEvents.Stopped));
        }

        private DeviceActionWrapper GetActionFromConfig(eRetriggerableTimerEvents eventType)
        {
            var action = _propertiesConfig.Events[eRetriggerableTimerEvents.Elapsed];

            if (action != null)
                return action;
            else return null;
        }

        /// <summary>
        /// Executes the Elapsed action from confing when the timer elapses
        /// </summary>
        /// <param name="o"></param>
        private void TimerElapsedCallback(object action)
        {
            Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "Timer Elapsed. Executing Action");

            if (action == null)
            {
                Debug.Console(1, this, "Timer elapsed but unable to execute action. Action is null.");
                return;
            }

            var devAction = action as DeviceActionWrapper;
            if (devAction != null)
                ExecuteAction(devAction);
            else
            {
                Debug.Console(2, this, "Unable to cast action as DeviceActionWrapper. Cannot Execute");
            }

        }

        private void ExecuteAction(DeviceActionWrapper action)
        {
            if (action == null)
                return;

            try
            {
                DeviceJsonApi.DoDeviceAction(action);
            }
            catch (Exception e)
            {
                Debug.Console(2, this, "Error Executing Action: {0}", e);
            }
            //finally  // Not sure this is needed
            //{
            //    _Timer.Reset(0, _TimerIntervalMs);
            //}
        }
    }

    /// <summary>
    /// Configuration Properties for RetriggerableTimer
    /// </summary>
    public class RetriggerableTimerPropertiesConfig
    {
        [JsonProperty("startTimerOnActivation")]
        public bool StartTimerOnActivation { get; set; }

        [JsonProperty("timerIntervalMs")]
        public long TimerIntervalMs { get; set; }

        [JsonProperty("events")]
        public Dictionary<eRetriggerableTimerEvents, DeviceActionWrapper> Events { get; set; }

        public RetriggerableTimerPropertiesConfig()
        {
            Events = new Dictionary<eRetriggerableTimerEvents, DeviceActionWrapper>();
        }
    }

    /// <summary>
    /// The set of values describing events on the timer
    /// </summary>
    public enum eRetriggerableTimerEvents
    {
        Elapsed,
        Stopped,
    }

    /// <summary>
    /// Factory class
    /// </summary>
    public class RetriggerableTimerFactory : EssentialsDeviceFactory<RetriggerableTimer>
    {
        public RetriggerableTimerFactory()
        {
            TypeNames = new List<string>() { "retriggerabletimer" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new RetriggerableTimer Device");

            return new RetriggerableTimer(dc.Key, dc);
        }
    }


}