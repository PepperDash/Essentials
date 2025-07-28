using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using System;
using System.Collections.Generic;
using static Crestron.SimplSharpPro.Lighting.ZumWired.ZumNetBridgeRoom.ZumWiredRoomInterface;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Contains debug commands for use in various situations
    /// </summary>
    public static class Debug
    {
        private static readonly Dictionary<uint, LogEventLevel> _logLevels = new Dictionary<uint, LogEventLevel>()
        {
            {0, LogEventLevel.Information },
            {3, LogEventLevel.Warning },
            {4, LogEventLevel.Error },
            {5, LogEventLevel.Fatal },
            {1, LogEventLevel.Debug },
            {2, LogEventLevel.Verbose },
        };

        public static void LogMessage(LogEventLevel level, string message, params object[] args)
        {
            Log.Write(level, message, args);
        }

        /// <summary>
        /// LogMessage method
        /// </summary>
        public static void LogMessage(LogEventLevel level, Exception ex, string message, params object[] args)
        {
            Log.Write(level, ex, message, args);
        }

        /// <summary>
        /// LogMessage method
        /// </summary>
        public static void LogMessage(LogEventLevel level, IKeyed keyed, string message, params object[] args)
        {
            using (LogContext.PushProperty("Key", keyed.Key))
            {
                Log.Write(level, message, args);
            }
        }

        /// <summary>
        /// LogMessage method
        /// </summary>
        public static void LogMessage(LogEventLevel level, IKeyed keyed, Exception ex, string message, params object[] args)
        {
            using (LogContext.PushProperty("Key", keyed.Key))
            {
                Log.Write(level, ex, message, args);
            }
        }

        #region Explicit methods for logging levels
        /// <summary>
        /// LogVerbose method
        /// </summary>
        public static void LogVerbose(IKeyed keyed, string message, params object[] args)
        {
            using (LogContext.PushProperty("Key", keyed?.Key))
            {
                Log.Write(LogEventLevel.Verbose, message, args);
            }
        }

        /// <summary>
        /// LogVerbose method
        /// </summary>
        public static void LogVerbose(IKeyed keyed, Exception ex, string message, params object[] args)
        {
            using (LogContext.PushProperty("Key", keyed?.Key))
            {
                Log.Write(LogEventLevel.Verbose, ex, message, args);
            }
        }

        /// <summary>
        /// LogVerbose method
        /// </summary>
        public static void LogVerbose(string message, params object[] args)
        {
            Log.Write(LogEventLevel.Verbose, message, args);
        }

        /// <summary>
        /// LogVerbose method
        /// </summary>
        public static void LogVerbose(Exception ex, string message, params object[] args)
        {
            Log.Write(LogEventLevel.Verbose, ex, message, args);
        }

        /// <summary>
        /// LogDebug method
        /// </summary>
        public static void LogDebug(IKeyed keyed, string message, params object[] args)
        {
            LogMessage(LogEventLevel.Debug, keyed, message, args);
        }

        /// <summary>
        /// LogDebug method
        /// </summary>
        public static void LogDebug(IKeyed keyed, Exception ex, string message, params object[] args)
        {
            LogMessage(LogEventLevel.Debug, keyed, ex, message, args);
        }

        /// <summary>
        /// LogDebug method
        /// </summary>
        public static void LogDebug(string message, params object[] args)
        {
           Log.Debug(message, args);
        }

        /// <summary>
        /// LogDebug method
        /// </summary>
        public static void LogDebug(Exception ex, string message, params object[] args)
        {
            Log.Debug(ex, message, args);
        }

        /// <summary>
        /// LogInformation method
        /// </summary>
        public static void LogInformation(IKeyed keyed, string message, params object[] args)
        {
            LogMessage(LogEventLevel.Information, keyed, message, args);
        }

        /// <summary>
        /// LogInformation method
        /// </summary>
        public static void LogInformation(IKeyed keyed, Exception ex, string message, params object[] args)
        {
            LogMessage(LogEventLevel.Information, keyed, ex, message, args);
        }

        /// <summary>
        /// LogInformation method
        /// </summary>
        public static void LogInformation(string message, params object[] args)
        {
            Log.Information(message, args);
        }

        /// <summary>
        /// LogInformation method
        /// </summary>
        public static void LogInformation(Exception ex, string message, params object[] args)
        {
            Log.Information(ex, message, args);
        }

        /// <summary>
        /// LogWarning method
        /// </summary>
        public static void LogWarning(IKeyed keyed, string message, params object[] args)
        {
            LogMessage(LogEventLevel.Warning, keyed, message, args);
        }

        /// <summary>
        /// LogWarning method
        /// </summary>
        public static void LogWarning(Exception ex, IKeyed keyed, string message, params object[] args)
        {
            LogMessage(LogEventLevel.Warning, keyed, ex, message, args);
        }

        /// <summary>
        /// LogWarning method
        /// </summary>
        public static void LogWarning(string message, params object[] args)
        {
            LogMessage(LogEventLevel.Warning, message, args);
        }

        /// <summary>
        /// LogWarning method
        /// </summary>
        public static void LogWarning(Exception ex, string message, params object[] args)
        {
            LogMessage(LogEventLevel.Warning, ex, message, args);
        }

        /// <summary>
        /// LogError method
        /// </summary>
        public static void LogError(IKeyed keyed, string message, params object[] args)
        {
            LogMessage(LogEventLevel.Error, keyed, message, args);
        }

        /// <summary>
        /// LogError method
        /// </summary>
        public static void LogError(IKeyed keyed, Exception ex, string message, params object[] args)
        {
            LogMessage(LogEventLevel.Error, keyed, ex, message, args);
        }

        /// <summary>
        /// LogError method
        /// </summary>
        public static void LogError(string message, params object[] args)
        {
            LogMessage(LogEventLevel.Error, message, args);
        }

        /// <summary>
        /// LogError method
        /// </summary>
        public static void LogError(Exception ex, string message, params object[] args)
        {
            LogMessage(LogEventLevel.Error, ex, message, args);
        }

        /// <summary>
        /// LogFatal method
        /// </summary>
        public static void LogFatal(IKeyed keyed, string message, params object[] args)
        {
            LogMessage(LogEventLevel.Fatal, keyed, message, args);
        }

        /// <summary>
        /// LogFatal method
        /// </summary>
        public static void LogFatal(IKeyed keyed, Exception ex, string message, params object[] args)
        {
            LogMessage(LogEventLevel.Fatal, keyed, ex, message, args);
        }

        /// <summary>
        /// LogFatal method
        /// </summary>
        public static void LogFatal(string message, params object[] args)
        {
            LogMessage(LogEventLevel.Fatal, message, args);
        }

        /// <summary>
        /// LogFatal method
        /// </summary>
        public static void LogFatal(Exception ex, string message, params object[] args)
        {
            LogMessage(LogEventLevel.Fatal, ex, message, args);
        }

        #endregion

        private static void LogMessage(uint level, string format, params object[] items)
        {
            if (!_logLevels.ContainsKey(level)) return;

            var logLevel = _logLevels[level];

            LogMessage(logLevel, format, items);
        }

        private static void LogMessage(uint level, IKeyed keyed, string format, params object[] items)
        {
            if (!_logLevels.ContainsKey(level)) return;

            var logLevel = _logLevels[level];

            LogMessage(logLevel, keyed, format, items);
        }


        /// <summary>
        /// Enumeration of ErrorLogLevel values
        /// </summary>
        public enum ErrorLogLevel
        {
            /// <summary>
            /// Error
            /// </summary>
            Error,
            /// <summary>
            /// Warning
            /// </summary>
            Warning,
            /// <summary>
            /// Notice
            /// </summary>
            Notice,
            /// <summary>
            /// None
            /// </summary>
            None,
        }
    }
}