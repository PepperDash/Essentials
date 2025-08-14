using System;
using System.Collections.Generic;

namespace Crestron.SimplSharp.CrestronLogger
{
  /// <summary>Mock CrestronLogger for .NET 8 compatibility</summary>
  public static class CrestronLogger
  {
    /// <summary>Write to log</summary>
    /// <param name="logName">Log name</param>
    /// <param name="message">Message</param>
    /// <param name="mode">Logger mode</param>
    public static void WriteToLog(string logName, string message, LoggerModeEnum mode)
    {
      Console.WriteLine($"[{logName}] {message}");
    }

    /// <summary>Write to log with level</summary>
    /// <param name="message">Message</param>
    /// <param name="level">Log level</param>
    public static void WriteToLog(string message, uint level)
    {
      Console.WriteLine($"[Level {level}] {message}");
    }

    /// <summary>Initialize logger</summary>
    /// <param name="bufferSize">Buffer size</param>
    /// <param name="mode">Logger mode</param>
    public static void Initialize(int bufferSize, LoggerModeEnum mode)
    {
      // Mock implementation
    }

    /// <summary>Print the log</summary>
    /// <param name="includeAll">Include all log entries</param>
    /// <returns>Log entries as string list</returns>
    public static List<string> PrintTheLog(bool includeAll = false)
    {
      return new List<string> { "Mock log entry" };
    }

    /// <summary>Clear the log</summary>
    /// <param name="clearAll">Clear all entries</param>
    /// <returns>Success message</returns>
    public static string Clear(bool clearAll)
    {
      return "Log cleared (mock)";
    }
  }

  /// <summary>Logger mode enumeration</summary>
  public enum LoggerModeEnum
  {
    /// <summary>Append mode</summary>
    LoggingModeAppend = 0,
    /// <summary>Overwrite mode</summary>
    LoggingModeOverwrite = 1,
    /// <summary>RM mode</summary>
    RM = 2
  }
}
