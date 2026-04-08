using System;
using System.Collections.Generic;
using System.Timers;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;
using Newtonsoft.Json;
using Serilog.Events;


namespace PepperDash.Essentials.Core.Timers;

/// <summary>
/// A device that runs a retriggerable timer and can execute actions specified in config 
/// </summary>
[Description("A retriggerable timer device")]
public class RetriggerableTimer : EssentialsDevice
{
    private RetriggerableTimerPropertiesConfig _propertiesConfig;

    private Timer _timer;
    private long _timerIntervalMs;

    /// <summary>
    /// Constructor for RetriggerableTimer
    /// </summary>
    /// <param name="key"></param>
    /// <param name="config"></param>
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

    /// <inheritdoc />
    protected override bool CustomActivate()
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

    /// <summary>
    /// Starts the timer with the interval specified in config. When the timer elapses, it executes the action specified in config for the Elapsed event. If the timer is already running, it will reset and start again.
    /// When the timer is stopped, it executes the action specified in config for the Stopped event.
    /// </summary>
    public void StartTimer()
    {
        CleanUpTimer();
        Debug.LogMessage(LogEventLevel.Information, this, "Starting Timer");

        var action = GetActionFromConfig(eRetriggerableTimerEvents.Elapsed);
        _timer = new Timer(_timerIntervalMs) { AutoReset = true };
        _timer.Elapsed += (s, e) => TimerElapsedCallback(action);
        _timer.Start();
    }

    /// <summary>
    /// Stops the timer. If the timer is stopped before it elapses, it will execute the action specified in config for the Stopped event. If the timer is not running, this method does nothing.
    /// If the timer is running, it will stop the timer and execute the Stopped action from config. If the timer is not running, it will do nothing.
    /// If the timer is running and the Stopped action is not specified in config, it will stop the timer and do nothing else. If the timer is running and the Stopped action is specified in config, it will stop the timer and execute the action. If the timer is not running, it will do nothing regardless
    /// </summary>
    public void StopTimer()
    {
        Debug.LogMessage(LogEventLevel.Information, this, "Stopping Timer");
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
    /// <param name="action">The action to execute when the timer elapses</param>
    private void TimerElapsedCallback(object action)
    {
        Debug.LogMessage(LogEventLevel.Debug, this, "Timer Elapsed. Executing Action");

        if (action == null)
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Timer elapsed but unable to execute action. Action is null.");
            return;
        }

        var devAction = action as DeviceActionWrapper;
        if (devAction != null)
            ExecuteAction(devAction);
        else
        {
            Debug.LogMessage(LogEventLevel.Verbose, this, "Unable to cast action as DeviceActionWrapper. Cannot Execute");
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
            Debug.LogMessage(LogEventLevel.Verbose, this, "Error Executing Action: {0}", e);
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
    /// <summary>
    /// When true, the timer will start immediately upon activation. When false, the timer will not start until StartTimer is called.
    /// </summary>
    [JsonProperty("startTimerOnActivation")]
    public bool StartTimerOnActivation { get; set; }

    /// <summary>
    /// The interval at which the timer elapses, in milliseconds. This is required and must be greater than 0. If this value is not set or is less than or equal to 0, the timer will not start and an error will be logged.
    /// </summary>
    [JsonProperty("timerIntervalMs")]
    public long TimerIntervalMs { get; set; }

    /// <summary>
    /// The actions to execute when timer events occur. The key is the type of event, and the value is the action to execute when that event occurs.
    /// This is required and must contain at least an action for the Elapsed event. 
    /// If an action for the Stopped event is not included, then when the timer is stopped, it will simply stop without executing any action.
    /// </summary>
    [JsonProperty("events")]
    public Dictionary<eRetriggerableTimerEvents, DeviceActionWrapper> Events { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
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
    /// <summary>
    /// Elapsed event state
    /// </summary>
    Elapsed,

    /// <summary>
    /// Stopped event state
    /// </summary>
    Stopped,
}

/// <summary>
/// Factory class
/// </summary>
public class RetriggerableTimerFactory : EssentialsDeviceFactory<RetriggerableTimer>
{
    /// <summary>
    /// Constructor for factory
    ///
    /// </summary>
    public RetriggerableTimerFactory()
    {
        TypeNames = new List<string>() { "retriggerabletimer" };
    }

    /// <inheritdoc />
    public override EssentialsDevice BuildDevice(DeviceConfig dc)
    {
        Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new RetriggerableTimer Device");

        return new RetriggerableTimer(dc.Key, dc);
    }
}