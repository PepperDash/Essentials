using Serilog.Events;
using System;
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
            Log.LogMessage(ex, message, device, args);
        }

        /// <summary>
        /// LogVerbose method
        /// </summary>
        public static void LogVerbose(this IKeyed device, Exception ex, string message, params object[] args)
        {
            Log.LogMessage(LogEventLevel.Verbose, ex, message, device, args);
        }

        /// <summary>
        /// LogVerbose method
        /// </summary>
        public static void LogVerbose(this IKeyed device, string message, params object[] args)
        {
            Log.LogMessage(LogEventLevel.Verbose, device, message, args);
        }

        /// <summary>
        /// LogDebug method
        /// </summary>
        public static void LogDebug(this IKeyed device, Exception ex, string message, params object[] args)
        {
            Log.LogMessage(LogEventLevel.Debug, ex, message, device, args);
        }

        /// <summary>
        /// LogDebug method
        /// </summary>
        public static void LogDebug(this IKeyed device, string message, params object[] args)
        {
            Log.LogMessage(LogEventLevel.Debug, device, message, args);
        }

        /// <summary>
        /// LogInformation method
        /// </summary>
        public static void LogInformation(this IKeyed device, Exception ex, string message, params object[] args)
        {
            Log.LogMessage(LogEventLevel.Information, ex, message, device, args);
        }

        /// <summary>
        /// LogInformation method
        /// </summary>
        public static void LogInformation(this IKeyed device, string message, params object[] args)
        {
            Log.LogMessage(LogEventLevel.Information, device, message, args);
        }

        /// <summary>
        /// LogWarning method
        /// </summary>
        public static void LogWarning(this IKeyed device, Exception ex, string message, params object[] args)
        {
            Log.LogMessage(LogEventLevel.Warning, ex, message, device, args);
        }

        /// <summary>
        /// LogWarning method
        /// </summary>
        public static void LogWarning(this IKeyed device, string message, params object[] args)
        {
            Log.LogMessage(LogEventLevel.Warning, device, message, args);
        }

        /// <summary>
        /// LogError method
        /// </summary>
        public static void LogError(this IKeyed device, Exception ex, string message, params object[] args)
        {
            Log.LogMessage(LogEventLevel.Error, ex, message, device, args);
        }

        /// <summary>
        /// LogError method
        /// </summary>
        public static void LogError(this IKeyed device, string message, params object[] args)
        {
            Log.LogMessage(LogEventLevel.Error, device, message, args);
        }

        /// <summary>
        /// LogFatal method
        /// </summary>
        public static void LogFatal(this IKeyed device, Exception ex, string message, params object[] args)
        {
            Log.LogMessage(LogEventLevel.Fatal, ex, message, device, args);
        }

        /// <summary>
        /// LogFatal method
        /// </summary>
        public static void LogFatal(this IKeyed device, string message, params object[] args)
        {
            Log.LogMessage(LogEventLevel.Fatal, device, message, args);
        }
    }
}
