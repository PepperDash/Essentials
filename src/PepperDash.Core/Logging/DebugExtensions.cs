using Serilog;
using Serilog.Events;
using System;
using Log = PepperDash.Core.Debug;

namespace PepperDash.Core.Logging
{
    public static class DebugExtensions
    {
        public static void LogException(this IKeyed device, Exception ex, string message, params object[] args)
        {
            Log.LogMessage(ex, message, device, args);
        }
        public static void LogVerbose(this IKeyed device, string message, params object[] args)
        {
            Log.LogMessage(LogEventLevel.Verbose, device, message, args);
        }

        public static void LogDebug(this IKeyed device, string message, params object[] args)
        {
            Log.LogMessage(LogEventLevel.Debug, device, message, args);
        }

        public static void LogInformation(this IKeyed device, string message, params object[] args)
        {
            Log.LogMessage(LogEventLevel.Information, device, message, args);
        }

        public static void LogWarning(this IKeyed device, string message, params object[] args)
        {
            Log.LogMessage(LogEventLevel.Warning, device, message, args);
        }

        public static void LogError(this IKeyed device, string message, params object[] args)
        {
            Log.LogMessage(LogEventLevel.Error, device, message, args);
        }

        public static void LogFatal(this IKeyed device, string message, params object[] args)
        {
            Log.LogMessage(LogEventLevel.Fatal, device, message, args);
        }
    }
}
