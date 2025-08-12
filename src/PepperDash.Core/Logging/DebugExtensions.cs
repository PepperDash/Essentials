using System;
using Serilog.Events;
using Log = PepperDash.Core.Debug;

namespace PepperDash.Core.Logging
{
    public static class DebugExtensions
    {
        /// <summary>
        /// LogException method
        /// </summary>
        public static void LogException(this IKeyed device, Exception ex, string message, params object[] args)
        {
            Log.LogMessage(ex, message, device: device, args);
        }

        /// <summary>
        /// LogVerbose method
        /// </summary>
        public static void LogVerbose(this IKeyed device, Exception ex, string message, params object[] args)
        {
            Log.LogVerbose(ex, device, message, args);
        }

        /// <summary>
        /// LogVerbose method
        /// </summary>
        public static void LogVerbose(this IKeyed device, string message, params object[] args)
        {
            Log.LogVerbose(device, message, args);
        }

        /// <summary>
        /// LogDebug method
        /// </summary>
        public static void LogDebug(this IKeyed device, Exception ex, string message, params object[] args)
        {
            Log.LogDebug(ex, device, message, args);
        }

        /// <summary>
        /// LogDebug method
        /// </summary>
        public static void LogDebug(this IKeyed device, string message, params object[] args)
        {
            Log.LogDebug(device, message, args);
        }

        /// <summary>
        /// LogInformation method
        /// </summary>
        public static void LogInformation(this IKeyed device, Exception ex, string message, params object[] args)
        {
            Log.LogInformation(ex, device, message, args);
        }

        /// <summary>
        /// LogInformation method
        /// </summary>
        public static void LogInformation(this IKeyed device, string message, params object[] args)
        {
            Log.LogInformation(device, message, args);
        }

        /// <summary>
        /// LogWarning method
        /// </summary>
        public static void LogWarning(this IKeyed device, Exception ex, string message, params object[] args)
        {
            Log.LogWarning(ex, device, message, args);
        }

        /// <summary>
        /// LogWarning method
        /// </summary>
        public static void LogWarning(this IKeyed device, string message, params object[] args)
        {
            Log.LogWarning(device, message, args);
        }

        /// <summary>
        /// LogError method
        /// </summary>
        public static void LogError(this IKeyed device, Exception ex, string message, params object[] args)
        {
            Log.LogError(ex, device, message, args);
        }

        /// <summary>
        /// LogError method
        /// </summary>
        public static void LogError(this IKeyed device, string message, params object[] args)
        {
            Log.LogError(device, message, args);
        }

        /// <summary>
        /// LogFatal method
        /// </summary>
        public static void LogFatal(this IKeyed device, Exception ex, string message, params object[] args)
        {
            Log.LogFatal(ex, device, message, args);
        }

        /// <summary>
        /// LogFatal method
        /// </summary>
        public static void LogFatal(this IKeyed device, string message, params object[] args)
        {
            Log.LogFatal(device, message, args);
        }
    }
}
