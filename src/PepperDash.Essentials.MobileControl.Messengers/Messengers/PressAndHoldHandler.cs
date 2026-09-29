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

        /// <summary>
        /// One active hold. Its <see cref="Gate"/> orders that hold's start, heartbeats and stop, and is
        /// the only lock held while device code runs, so a slow device never delays another hold.
        /// </summary>
        private sealed class Hold
        {
            public readonly object Gate = new object();

            /// <summary>
            /// Replaced on every heartbeat, so an expiry already queued for an earlier timer no longer
            /// matches and can't end a hold that was just extended.
            /// </summary>
            public Timer Timer;

            public bool Ended;
        }

        // Guards only membership of _holds, never device code. Lock order: _holdsLock may be taken while
        // holding a Gate, but a Gate is only taken under _holdsLock when it belongs to a new, unpublished hold.
        private static readonly object _holdsLock = new object();

        private static readonly Dictionary<string, Hold> _holds = new Dictionary<string, Hold>();

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

            var hold = new Hold();

            lock (_holdsLock)
            {
                if (_holds.ContainsKey(key))
                {
                    Debug.LogDebug("Timer for {key} already exists", key);
                    return;
                }

                // Take the new hold's gate before publishing it, so a release or heartbeat for this key
                // waits until the start has run.
                System.Threading.Monitor.Enter(hold.Gate);
                _holds.Add(key, hold);
            }

            try
            {
                Debug.LogDebug("Adding timer for {key} with due time {dueTime}", key, ButtonHeartbeatInterval);

                try
                {
                    action(true);
                }
                catch
                {
                    // Without this, the key would stay held and every later press would be ignored.
                    hold.Ended = true;
                    RemoveHold(key, hold);
                    throw;
                }

                StartNewTimer(key, hold, action);
            }
            finally
            {
                System.Threading.Monitor.Exit(hold.Gate);
            }
        }

        private static void ResetTimer(string key, Action<bool> action)
        {
            Debug.LogDebug("Attempting to reset timer for {key}", key);

            var hold = GetHold(key);

            if (hold == null)
            {
                Debug.LogDebug("Timer for {key} not found", key);
                return;
            }

            lock (hold.Gate)
            {
                if (hold.Ended)
                {
                    return;
                }

                Debug.LogDebug("Resetting timer for {key} with due time {dueTime}", key, ButtonHeartbeatInterval);

                StartNewTimer(key, hold, action);
            }
        }

        private static void StopTimer(string key, Action<bool> action)
        {
            Debug.LogDebug("Attempting to stop timer for {key}", key);

            var hold = GetHold(key);

            if (hold == null)
            {
                Debug.LogDebug("Timer for {key} not found", key);
                return;
            }

            lock (hold.Gate)
            {
                if (hold.Ended)
                {
                    return;
                }

                Debug.LogDebug("Stopping timer for {key}", key);

                // Removed only now, once this hold's start has finished, so a new press for the key
                // can't start before this one has stopped.
                RemoveHold(key, hold);
                EndHold(hold, action);
            }
        }

        private static void ExpireTimer(string key, Hold hold, Timer timer, Action<bool> action)
        {
            lock (hold.Gate)
            {
                // A release ended the hold, or a heartbeat replaced this timer: this expiry is stale.
                if (hold.Ended || hold.Timer != timer)
                {
                    return;
                }

                Debug.LogDebug("Timer expired for {key}", key);

                RemoveHold(key, hold);
                EndHold(hold, action);
            }
        }

        // Caller holds hold.Gate.
        private static void StartNewTimer(string key, Hold hold, Action<bool> action)
        {
            var previous = hold.Timer;

            var timer = new Timer(ButtonHeartbeatInterval) { AutoReset = false };
            timer.Elapsed += (s, e) => ExpireTimer(key, hold, timer, action);

            hold.Timer = timer;

            previous?.Stop();
            previous?.Dispose();

            timer.Start();
        }

        // Caller holds hold.Gate.
        private static void EndHold(Hold hold, Action<bool> action)
        {
            hold.Ended = true;

            hold.Timer?.Stop();
            hold.Timer?.Dispose();

            action(false);
        }

        private static Hold GetHold(string key)
        {
            lock (_holdsLock)
            {
                return _holds.TryGetValue(key, out var hold) ? hold : null;
            }
        }

        private static void RemoveHold(string key, Hold hold)
        {
            lock (_holdsLock)
            {
                if (_holds.TryGetValue(key, out var current) && current == hold)
                {
                    _holds.Remove(key);
                }
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
