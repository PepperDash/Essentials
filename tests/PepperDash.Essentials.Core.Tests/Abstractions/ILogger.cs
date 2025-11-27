using System;

namespace PepperDash.Essentials.Core.Tests.Abstractions
{
    /// <summary>
    /// Abstraction for logging operations to enable testing
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs a debug message
        /// </summary>
        /// <param name="source">Source of the log message</param>
        /// <param name="message">Message to log</param>
        /// <param name="args">Format arguments</param>
        void LogDebug(object source, string message, params object[] args);
        
        /// <summary>
        /// Logs a verbose message
        /// </summary>
        /// <param name="source">Source of the log message</param>
        /// <param name="message">Message to log</param>
        /// <param name="args">Format arguments</param>
        void LogVerbose(object source, string message, params object[] args);
    }
}