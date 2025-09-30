using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Newtonsoft.Json.Linq;
using PepperDash.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Handler for press/hold/release messages
    /// </summary>
    public static class PressAndHoldHandler
    {
        private const long ButtonHeartbeatInterval = 1000;

        private static readonly Dictionary<string, CTimer> _pushedActions = new Dictionary<string, CTimer>();

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

        private static void AddTimer(string deviceKey, Action<bool> action)
        {
            Debug.LogDebug("Attempting to add timer for {deviceKey}", deviceKey);

            if (_pushedActions.TryGetValue(deviceKey, out CTimer cancelTimer))
            {
                Debug.LogDebug("Timer for {deviceKey} already exists", deviceKey);
                return;
            }

            Debug.LogDebug("Adding timer for {deviceKey} with due time {dueTime}", deviceKey, ButtonHeartbeatInterval);

            action(true);

            cancelTimer = new CTimer(o =>
            {
                Debug.LogDebug("Timer expired for {deviceKey}", deviceKey);

                action(false);

                _pushedActions.Remove(deviceKey);
            }, ButtonHeartbeatInterval);

            _pushedActions.Add(deviceKey, cancelTimer);
        }

        private static void ResetTimer(string deviceKey, Action<bool> action)
        {
            Debug.LogDebug("Attempting to reset timer for {deviceKey}", deviceKey);

            if (!_pushedActions.TryGetValue(deviceKey, out CTimer cancelTimer))
            {
                Debug.LogDebug("Timer for {deviceKey} not found", deviceKey);
                return;
            }

            Debug.LogDebug("Resetting timer for {deviceKey} with due time {dueTime}", deviceKey, ButtonHeartbeatInterval);

            cancelTimer.Reset(ButtonHeartbeatInterval);
        }

        private static void StopTimer(string deviceKey, Action<bool> action)
        {
            Debug.LogDebug("Attempting to stop timer for {deviceKey}", deviceKey);

            if (!_pushedActions.TryGetValue(deviceKey, out CTimer cancelTimer))
            {
                Debug.LogDebug("Timer for {deviceKey} not found", deviceKey);
                return;
            }

            Debug.LogDebug("Stopping timer for {deviceKey} with due time {dueTime}", deviceKey, ButtonHeartbeatInterval);

            action(false);
            cancelTimer.Stop();
            _pushedActions.Remove(deviceKey);
        }

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
        /// HandlePressAndHold method
        /// </summary>
        public static void HandlePressAndHold(string deviceKey, JToken content, Action<bool> action)
        {
            var msg = content.ToObject<MobileControlSimpleContent<string>>();

            Debug.LogDebug("Handling press and hold message of {type} for {deviceKey}", msg.Value, deviceKey);

            var timerHandler = GetPressAndHoldHandler(msg.Value);

            if (timerHandler == null)
            {
                return;
            }

            timerHandler(deviceKey, action);
        }
    }
}
