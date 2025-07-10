using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Newtonsoft.Json.Linq;
using PepperDash.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Static handler for managing press and hold button actions with automatic timeout functionality
    /// </summary>
    public static class PressAndHoldHandler
    {
        /// <summary>
        /// The interval in milliseconds for button heartbeat timeout
        /// </summary>
        private const long ButtonHeartbeatInterval = 1000;

        /// <summary>
        /// Dictionary of active timers for pressed actions, keyed by device key
        /// </summary>
        private static readonly Dictionary<string, CTimer> _pushedActions = new Dictionary<string, CTimer>();

        /// <summary>
        /// Dictionary of action handlers for different button states
        /// </summary>
        private static readonly Dictionary<string, Action<string, Action<bool>>> _pushedActionHandlers;

        /// <summary>
        /// Static constructor that initializes the action handlers for different button states
        /// </summary>
        static PressAndHoldHandler()
        {
            _pushedActionHandlers = new Dictionary<string, Action<string, Action<bool>>>
            {
                {"pressed", AddTimer },
                {"held", ResetTimer },
                {"released", StopTimer }
            };
        }

        /// <summary>
        /// Adds a timer for a device key and executes the action with true state
        /// </summary>
        /// <param name="deviceKey">The unique key for the device</param>
        /// <param name="action">The action to execute with boolean state</param>
        private static void AddTimer(string deviceKey, Action<bool> action)
        {
            Debug.LogMessage(Serilog.Events.LogEventLevel.Debug, "Attempting to add timer for {deviceKey}", deviceKey);

            if (_pushedActions.TryGetValue(deviceKey, out CTimer cancelTimer))
            {
                Debug.LogMessage(Serilog.Events.LogEventLevel.Debug, "Timer for {deviceKey} already exists", deviceKey);
                return;
            }

            Debug.LogMessage(Serilog.Events.LogEventLevel.Debug, "Adding timer for {deviceKey} with due time {dueTime}", deviceKey, ButtonHeartbeatInterval);

            action(true);

            cancelTimer = new CTimer(o =>
            {
                Debug.LogMessage(Serilog.Events.LogEventLevel.Debug, "Timer expired for {deviceKey}", deviceKey);

                action(false);

                _pushedActions.Remove(deviceKey);
            }, ButtonHeartbeatInterval);

            _pushedActions.Add(deviceKey, cancelTimer);
        }

        /// <summary>
        /// Resets an existing timer for the specified device key
        /// </summary>
        /// <param name="deviceKey">The unique key for the device</param>
        /// <param name="action">The action associated with the timer</param>
        private static void ResetTimer(string deviceKey, Action<bool> action)
        {
            Debug.LogMessage(Serilog.Events.LogEventLevel.Debug, "Attempting to reset timer for {deviceKey}", deviceKey);

            if (!_pushedActions.TryGetValue(deviceKey, out CTimer cancelTimer))
            {
                Debug.LogMessage(Serilog.Events.LogEventLevel.Debug, "Timer for {deviceKey} not found", deviceKey);
                return;
            }

            Debug.LogMessage(Serilog.Events.LogEventLevel.Debug, "Resetting timer for {deviceKey} with due time {dueTime}", deviceKey, ButtonHeartbeatInterval);

            cancelTimer.Reset(ButtonHeartbeatInterval);
        }

        /// <summary>
        /// Stops and removes the timer for the specified device key
        /// </summary>
        /// <param name="deviceKey">The unique key for the device</param>
        /// <param name="action">The action to execute with false state before stopping</param>
        private static void StopTimer(string deviceKey, Action<bool> action)
        {
            Debug.LogMessage(Serilog.Events.LogEventLevel.Debug, "Attempting to stop timer for {deviceKey}", deviceKey);

            if (!_pushedActions.TryGetValue(deviceKey, out CTimer cancelTimer))
            {
                Debug.LogMessage(Serilog.Events.LogEventLevel.Debug, "Timer for {deviceKey} not found", deviceKey);
                return;
            }

            Debug.LogMessage(Serilog.Events.LogEventLevel.Debug, "Stopping timer for {deviceKey} with due time {dueTime}", deviceKey, ButtonHeartbeatInterval);

            action(false);
            cancelTimer.Stop();
            _pushedActions.Remove(deviceKey);
        }

        /// <summary>
        /// Gets the appropriate press and hold handler for the specified value
        /// </summary>
        /// <param name="value">The button state value (pressed, held, released)</param>
        /// <returns>The handler action for the specified state, or null if not found</returns>
        public static Action<string, Action<bool>> GetPressAndHoldHandler(string value)
        {
            Debug.LogMessage(Serilog.Events.LogEventLevel.Debug, "Getting press and hold handler for {value}", value);

            if (!_pushedActionHandlers.TryGetValue(value, out Action<string, Action<bool>> handler))
            {
                Debug.LogMessage(Serilog.Events.LogEventLevel.Debug, "Press and hold handler for {value} not found", value);
                return null;
            }

            Debug.LogMessage(Serilog.Events.LogEventLevel.Debug, "Got handler for {value}", value);

            return handler;
        }

        /// <summary>
        /// Handles press and hold messages by parsing the content and executing the appropriate handler
        /// </summary>
        /// <param name="deviceKey">The unique key for the device</param>
        /// <param name="content">The JSON content containing the button state</param>
        /// <param name="action">The action to execute with boolean state</param>
        public static void HandlePressAndHold(string deviceKey, JToken content, Action<bool> action)
        {
            var msg = content.ToObject<MobileControlSimpleContent<string>>();

            Debug.LogMessage(Serilog.Events.LogEventLevel.Debug, "Handling press and hold message of {type} for {deviceKey}", msg.Value, deviceKey);

            var timerHandler = GetPressAndHoldHandler(msg.Value);

            if (timerHandler == null)
            {
                return;
            }

            timerHandler(deviceKey, action);
        }
    }
}
