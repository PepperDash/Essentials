using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crestron.SimplSharp
{
  public static class ErrorLog
  {
    public static void Error(string message, params object[] args)
    {
      Console.WriteLine($"[ERROR] {string.Format(message, args)}");
    }

    public static void Notice(string message, params object[] args)
    {
      Console.WriteLine($"[NOTICE] {string.Format(message, args)}");
    }

    public static void Warn(string message, params object[] args)
    {
      Console.WriteLine($"[WARN] {string.Format(message, args)}");
    }

    public static void Info(string message, params object[] args)
    {
      Console.WriteLine($"[INFO] {string.Format(message, args)}");
    }
  }
}

namespace Crestron.SimplSharp.CrestronDataStore
{
  public static class CrestronDataStoreStatic
  {
    public static CDS_ERROR SetLocalStringValue(string key, string value)
    {
      return CDS_ERROR.CDS_SUCCESS;
    }

    public static CDS_ERROR GetLocalStringValue(string key, out string value)
    {
      value = "";
      return CDS_ERROR.CDS_SUCCESS;
    }

    /// <summary>Initialize the Crestron data store</summary>
    /// <returns>0 on success, negative on error</returns>
    public static int InitCrestronDataStore()
    {
      // Mock implementation
      return 0;
    }

    /// <summary>Get a boolean value from local storage</summary>
    /// <param name="key">The key to retrieve</param>
    /// <param name="value">The retrieved value</param>
    /// <returns>0 on success, negative on error</returns>
    public static int GetLocalBoolValue(string key, out bool value)
    {
      // Mock implementation - always return false for now
      value = false;
      return 0;
    }

    /// <summary>Set a boolean value in local storage</summary>
    /// <param name="key">The key to set</param>
    /// <param name="value">The value to set</param>
    /// <returns>0 on success, negative on error</returns>
    public static int SetLocalBoolValue(string key, bool value)
    {
      // Mock implementation
      return 0;
    }

    /// <summary>Get an integer value from local storage</summary>
    /// <param name="key">The key to retrieve</param>
    /// <param name="value">The retrieved value</param>
    /// <returns>0 on success, negative on error</returns>
    public static int GetLocalIntValue(string key, out int value)
    {
      // Mock implementation - always return 0 for now
      value = 0;
      return 0;
    }

    /// <summary>Set an integer value in local storage</summary>
    /// <param name="key">The key to set</param>
    /// <param name="value">The value to set</param>
    /// <returns>0 on success, negative on error</returns>
    public static int SetLocalIntValue(string key, int value)
    {
      // Mock implementation
      return 0;
    }

    /// <summary>Set an unsigned integer value in local storage</summary>
    /// <param name="key">The key to set</param>
    /// <param name="value">The value to set</param>
    /// <returns>0 on success, negative on error</returns>
    public static int SetLocalUintValue(string key, uint value)
    {
      // Mock implementation
      return 0;
    }
  }

  public enum CDS_ERROR
  {
    CDS_SUCCESS = 0,
    CDS_ERROR = -1
  }

  /// <summary>Mock CrestronDataStore for local data storage</summary>
  public static class CrestronDataStore
  {
    /// <summary>Error constant for CDS operations</summary>
    public static readonly int CDS_ERROR = -1;
  }
}
