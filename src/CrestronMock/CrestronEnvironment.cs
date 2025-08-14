using System;

namespace Crestron.SimplSharp
{
  // Console and logging types needed by CrestronConsole and CrestronLogger
  public delegate string ConsoleCommandFunction(string parameters);

  public enum ConsoleAccessLevelEnum
  {
    AccessOperator = 0,
    AccessProgrammer = 1,
    AccessAdministrator = 2
  }

  public class ConsoleCommandParameterSpecClass
  {
    // Mock implementation
  }

  /// <summary>Mock CrestronEnvironment for system event handling</summary>
  public static class CrestronEnvironment
  {
    /// <summary>Event fired when program status changes</summary>
    public static event Action<eProgramStatusEventType>? ProgramStatusEventHandler;

    /// <summary>Event fired when ethernet status changes</summary>
    public static event Action<EthernetEventArgs>? EthernetEventHandler;

    /// <summary>Gets the device platform</summary>
    public static string DevicePlatform => "Mock";

    /// <summary>Gets the runtime environment</summary>
    public static string RuntimeEnvironment => "Test";

    /// <summary>Triggers a program status event (for testing)</summary>
    /// <param name="eventType">Event type</param>
    public static void TriggerProgramStatusEvent(eProgramStatusEventType eventType)
    {
      ProgramStatusEventHandler?.Invoke(eventType);
    }

    /// <summary>Triggers an ethernet event (for testing)</summary>
    /// <param name="args">Event arguments</param>
    public static void TriggerEthernetEvent(EthernetEventArgs args)
    {
      EthernetEventHandler?.Invoke(args);
    }
  }

  /// <summary>Mock ethernet event type enumeration</summary>
  public enum eEthernetEventType
  {
    /// <summary>Link down</summary>
    LinkDown = 0,
    /// <summary>Link up</summary>
    LinkUp = 1
  }

  /// <summary>Mock CrestronConsole for console output</summary>
  public static class CrestronConsole
  {
    /// <summary>Prints a line to the console</summary>
    /// <param name="message">Message to print</param>
    public static void PrintLine(string message)
    {
      // Mock implementation - could write to System.Console in test environment
      Console.WriteLine($"[CrestronConsole] {message}");
    }

    /// <summary>Prints formatted text to the console</summary>
    /// <param name="format">Format string</param>
    /// <param name="args">Arguments</param>
    public static void PrintLine(string format, params object[] args)
    {
      Console.WriteLine($"[CrestronConsole] {string.Format(format, args)}");
    }

    /// <summary>Prints text to the console without a newline</summary>
    /// <param name="message">Message to print</param>
    public static void Print(string message)
    {
      Console.Write($"[CrestronConsole] {message}");
    }

    /// <summary>Console command response</summary>
    /// <param name="command">Command to execute</param>
    /// <returns>Response string</returns>
    public static string ConsoleCommandResponse(string command)
    {
      return $"Mock response for command: {command}";
    }

    /// <summary>Add new console command</summary>
    /// <param name="function">Command function</param>
    /// <param name="command">Command name</param>
    /// <param name="help">Help text</param>
    /// <param name="accessLevel">Access level</param>
    /// <returns>0 for success</returns>
    public static int AddNewConsoleCommand(ConsoleCommandFunction function, string command, string help, ConsoleAccessLevelEnum accessLevel)
    {
      return 0; // Mock success
    }

    /// <summary>Add new console command with parameter spec</summary>
    /// <param name="function">Command function</param>
    /// <param name="command">Command name</param>
    /// <param name="help">Help text</param>
    /// <param name="accessLevel">Access level</param>
    /// <param name="spec">Parameter specification</param>
    /// <returns>0 for success</returns>
    public static int AddNewConsoleCommand(ConsoleCommandFunction function, string command, string help, ConsoleAccessLevelEnum accessLevel, ConsoleCommandParameterSpecClass spec)
    {
      return 0; // Mock success
    }

    /// <summary>Send control system command</summary>
    /// <param name="command">Command to send</param>
    /// <param name="programNumber">Program number</param>
    public static void SendControlSystemCommand(string command, uint programNumber)
    {
      // Mock implementation
    }
  }
}

namespace Crestron.SimplSharp.CrestronIO
{
  /// <summary>Mock File class for basic file operations</summary>
  public static class File
  {
    /// <summary>Checks if a file exists</summary>
    /// <param name="path">File path</param>
    /// <returns>True if file exists</returns>
    public static bool Exists(string path)
    {
      // Mock implementation - use System.IO.File for actual file operations
      return System.IO.File.Exists(path);
    }

    /// <summary>Reads all text from a file</summary>
    /// <param name="path">File path</param>
    /// <returns>File contents</returns>
    public static string ReadToEnd(string path)
    {
      return System.IO.File.ReadAllText(path);
    }

    /// <summary>Reads all text from a file with specified encoding</summary>
    /// <param name="path">File path</param>
    /// <param name="encoding">Text encoding</param>
    /// <returns>File contents</returns>
    public static string ReadToEnd(string path, System.Text.Encoding encoding)
    {
      return System.IO.File.ReadAllText(path, encoding);
    }

    /// <summary>Writes text to a file</summary>
    /// <param name="path">File path</param>
    /// <param name="contents">Contents to write</param>
    public static void WriteAllText(string path, string contents)
    {
      System.IO.File.WriteAllText(path, contents);
    }

    /// <summary>Deletes a file</summary>
    /// <param name="path">File path</param>
    public static void Delete(string path)
    {
      if (System.IO.File.Exists(path))
        System.IO.File.Delete(path);
    }
  }

  /// <summary>Mock Directory class for basic directory operations</summary>
  public static class Directory
  {
    /// <summary>Gets the application directory path</summary>
    /// <returns>Application directory path</returns>
    public static string GetApplicationDirectory()
    {
      // Mock implementation - return current directory or a typical Crestron path
      return System.IO.Directory.GetCurrentDirectory();
    }

    /// <summary>Gets the application root directory path</summary>
    /// <returns>Application root directory path</returns>
    public static string GetApplicationRootDirectory()
    {
      // Mock implementation - return current directory or a typical Crestron path
      return System.IO.Directory.GetCurrentDirectory();
    }

    /// <summary>Checks if a directory exists</summary>
    /// <param name="path">Directory path</param>
    /// <returns>True if directory exists</returns>
    public static bool Exists(string path)
    {
      return System.IO.Directory.Exists(path);
    }

    /// <summary>Creates a directory</summary>
    /// <param name="path">Directory path</param>
    public static void CreateDirectory(string path)
    {
      System.IO.Directory.CreateDirectory(path);
    }
  }

  /// <summary>Mock Path class for path operations</summary>
  public static class Path
  {
    /// <summary>Directory separator character</summary>
    public static readonly char DirectorySeparatorChar = System.IO.Path.DirectorySeparatorChar;

    /// <summary>Combines path strings</summary>
    /// <param name="path1">First path</param>
    /// <param name="path2">Second path</param>
    /// <returns>Combined path</returns>
    public static string Combine(string path1, string path2)
    {
      return System.IO.Path.Combine(path1, path2);
    }

    /// <summary>Gets the file name from a path</summary>
    /// <param name="path">Full path</param>
    /// <returns>File name</returns>
    public static string GetFileName(string path)
    {
      return System.IO.Path.GetFileName(path);
    }

    /// <summary>Gets the directory name from a path</summary>
    /// <param name="path">Full path</param>
    /// <returns>Directory name</returns>
    public static string GetDirectoryName(string path)
    {
      return System.IO.Path.GetDirectoryName(path) ?? string.Empty;
    }

    /// <summary>Gets the file extension from a path</summary>
    /// <param name="path">Full path</param>
    /// <returns>File extension</returns>
    public static string GetExtension(string path)
    {
      return System.IO.Path.GetExtension(path);
    }
  }
}
