using System;
using Serilog.Events;
using Log = PepperDash.Core.Debug;

namespace PepperDash.Core.Logging;

public static class DebugExtensions
{
    /// <summary>
    /// Logs an exception as two entries: an Error with <paramref name="message"/> followed by the
    /// exception's message, then a Verbose entry with the stack trace.
    /// </summary>
    /// <remarks>
    /// The exception object isn't attached to either entry. Sinks that print an attached exception
    /// would otherwise repeat the stack trace in the Error entry, and most sinks print only the rendered
    /// message and never show an attached exception at all. The stack trace is therefore in the text of
    /// the Verbose entry, which is only visible when verbose logging is on.
    /// </remarks>
    /// <param name="device">The device the entry is logged for.</param>
    /// <param name="ex">The exception. When null only the Error entry is logged.</param>
    /// <param name="message">Message template for the Error entry, filled from <paramref name="args"/>.</param>
    /// <param name="args">Arguments for <paramref name="message"/>.</param>
    public static void LogException(this IKeyed device, Exception ex, string message, params object[] args)
    {
        message = message ?? string.Empty;

        if (ex == null)
        {
            Log.LogMessage(LogEventLevel.Error, device, message, args);
            return;
        }

        // The exception message is added as literal text, with braces escaped, so it can't be read as
        // a template placeholder and doesn't change which argument fills which placeholder in message.
        var exceptionMessage = (ex.Message ?? string.Empty).Replace("{", "{{").Replace("}", "}}");
        Log.LogMessage(LogEventLevel.Error, device, message + ": " + exceptionMessage, args);

        Log.LogMessage(LogEventLevel.Verbose, device, "Stack Trace:\r\n{stackTrace:l}", ex.StackTrace ?? "(not available)");
    }

    public static void LogVerbose(this IKeyed device, Exception ex, string message, params object[] args)
    {
        Log.LogMessage(LogEventLevel.Verbose, ex, message, device, args);
    }

    public static void LogVerbose(this IKeyed device, string message, params object[] args)
    {
        Log.LogMessage(LogEventLevel.Verbose, device, message, args);
    }

    public static void LogDebug(this IKeyed device, Exception ex, string message, params object[] args)
    {
        Log.LogMessage(LogEventLevel.Debug, ex, message, device, args);
    }

    public static void LogDebug(this IKeyed device, string message, params object[] args)
    {
        Log.LogMessage(LogEventLevel.Debug, device, message, args);
    }

    public static void LogInformation(this IKeyed device, Exception ex, string message, params object[] args)
    {
        Log.LogMessage(LogEventLevel.Information, ex, message, device, args);
    }

    public static void LogInformation(this IKeyed device, string message, params object[] args)
    {
        Log.LogMessage(LogEventLevel.Information, device, message, args);
    }

    public static void LogWarning(this IKeyed device, Exception ex, string message, params object[] args)
    {
        Log.LogMessage(LogEventLevel.Warning, ex, message, device, args);
    }

    public static void LogWarning(this IKeyed device, string message, params object[] args)
    {
        Log.LogMessage(LogEventLevel.Warning, device, message, args);
    }

    public static void LogError(this IKeyed device, Exception ex, string message, params object[] args)
    {
        Log.LogMessage(LogEventLevel.Error, ex, message, device, args);
    }

    public static void LogError(this IKeyed device, string message, params object[] args)
    {
        Log.LogMessage(LogEventLevel.Error, device, message, args);
    }

    public static void LogFatal(this IKeyed device, Exception ex, string message, params object[] args)
    {
        Log.LogMessage(LogEventLevel.Fatal, ex, message, device, args);
    }

    public static void LogFatal(this IKeyed device, string message, params object[] args)
    {
        Log.LogMessage(LogEventLevel.Fatal, device, message, args);
    }
}
