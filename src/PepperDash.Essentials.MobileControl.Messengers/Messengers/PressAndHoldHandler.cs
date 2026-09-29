using System;
using System.Collections.Generic;
using System.Timers;
using Newtonsoft.Json.Linq;
using PepperDash.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Handler for press/hold/release messages
    /// </summary>
    /// <remarks>
    /// Each hold is tracked by a key, which should identify both the device and the action (for example
    /// <c>"camera1/cameraLeft"</c>), so that two holds on one device, such as pan and tilt together, run
    /// independently. A hold ends on "released", or when no "held" heartbeat arrives within
    /// <see cref="ButtonHeartbeatInterval"/>.
    /// </remarks>
    public static class PressAndHoldHandler
    {
        private const long ButtonHeartbeatInterval = 1000;

        private static readonly Dictionary<string, Timer> _pushedActions = new Dictionary<string, Timer>();

        // Messages are handled concurrently, and heartbeat timers expire on their own threads. Every
        // start, reset and stop runs under this lock, including the action itself, so a hold's action
        // is started and stopped exactly once and in order.
        private static readonly object _pushedActionsLock = new object();

        private static readonly Dictionary<string, Action<string, Action<bool>>> _pushedActionHandlers;

        static PressAndHoldHandler()
        {
            _pushedActionHandlers = new Dictionary<string, Action<string, Action<bool>>>
            {
                {"pressed", AddTimer },
                {"held", ResetTimer },
                {"released", StopTimer }
            };
        }

        private static void AddTimer(string key, Action<bool> action)
        {
            Debug.LogDebug("Attempting to add timer for {key}", key);

            lock (_pushedActionsLock)
            {
                if (_pushedActions.ContainsKey(key))
                {
                    Debug.LogDebug("Timer for {key} already exists", key);
                    return;
                }

                Debug.LogDebug("Adding timer for {key} with due time {dueTime}", key, ButtonHeartbeatInterval);

                var cancelTimer = new Timer(ButtonHeartbeatInterval) { AutoReset = false };
                cancelTimer.Elapsed += (s, e) => ExpireTimer(key, cancelTimer, action);

                _pushedActions.Add(key, cancelTimer);

                action(true);

                cancelTimer.Start();
            }
        }

        private static void ExpireTimer(string key, Timer cancelTimer, Action<bool> action)
        {
            lock (_pushedActionsLock)
            {
                // "released" may have ended this hold already, or a new hold may have replaced it.
                if (!_pushedActions.TryGetValue(key, out var current) || current != cancelTimer)
                {
                    return;
                }

                Debug.LogDebug("Timer expired for {key}", key);

                _pushedActions.Remove(key);
                cancelTimer.Dispose();
                action(false);
            }
        }

        private static void ResetTimer(string key, Action<bool> action)
        {
            Debug.LogDebug("Attempting to reset timer for {key}", key);

            lock (_pushedActionsLock)
            {
                if (!_pushedActions.TryGetValue(key, out Timer cancelTimer))
                {
                    Debug.LogDebug("Timer for {key} not found", key);
                    return;
                }

                Debug.LogDebug("Resetting timer for {key} with due time {dueTime}", key, ButtonHeartbeatInterval);

                cancelTimer.Stop();
                cancelTimer.Interval = ButtonHeartbeatInterval;
                cancelTimer.Start();
            }
        }

        private static void StopTimer(string key, Action<bool> action)
        {
            Debug.LogDebug("Attempting to stop timer for {key}", key);

            lock (_pushedActionsLock)
            {
                if (!_pushedActions.TryGetValue(key, out Timer cancelTimer))
                {
                    Debug.LogDebug("Timer for {key} not found", key);
                    return;
                }

                Debug.LogDebug("Stopping timer for {key}", key);

                _pushedActions.Remove(key);
                cancelTimer.Stop();
                cancelTimer.Dispose();
                action(false);
            }
        }

        /// <summary>
        /// Gets the handler for a given press and hold message type
        /// </summary>
        /// <param name="value">The press and hold message type.</param>
        /// <returns>The handler for the specified message type, called with the hold's key and action.</returns>
        public static Action<string, Action<bool>> GetPressAndHoldHandler(string value)
        {
            Debug.LogDebug("Getting press and hold handler for {value}", value);

            if (!_pushedActionHandlers.TryGetValue(value, out Action<string, Action<bool>> handler))
            {
                Debug.LogDebug("Press and hold handler for {value} not found", value);
                return null;
            }

            Debug.LogDebug("Got handler for {value}", value);

            return handler;
        }

        /// <summary>
        /// Handles a press/hold/release message for one action on a device
        /// </summary>
        /// <param name="deviceKey">The device the action belongs to.</param>
        /// <param name="actionPath">The action's message path, such as <c>"/cameraLeft"</c>, so each action on the device is held independently.</param>
        /// <param name="content">The message content, with a value of "pressed", "held" or "released".</param>
        /// <param name="action">The action to run: true to start, false to stop.</param>
        public static void HandlePressAndHold(string deviceKey, string actionPath, JToken content, Action<bool> action)
        {
            HandlePressAndHoldForKey($"{deviceKey}{actionPath}", content, action);
        }

        /// <summary>
        /// Handles a press/hold/release message, tracking one hold per device
        /// </summary>
        /// <remarks>
        /// Two actions held at once on the same device share one hold, so the second is ignored. Prefer
        /// the overload that takes the action path.
        /// </remarks>
        public static void HandlePressAndHold(string deviceKey, JToken content, Action<bool> action)
        {
            HandlePressAndHoldForKey(deviceKey, content, action);
        }

        private static void HandlePressAndHoldForKey(string key, JToken content, Action<bool> action)
        {
            var msg = content.ToObject<MobileControlSimpleContent<string>>();

            Debug.LogDebug("Handling press and hold message of {type} for {key}", msg.Value, key);

            var timerHandler = GetPressAndHoldHandler(msg.Value);

            if (timerHandler == null)
            {
                return;
            }

            timerHandler(key, action);
        }
    }
}
