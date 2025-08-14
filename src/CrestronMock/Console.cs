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
  }

  public enum CDS_ERROR
  {
    CDS_SUCCESS = 0,
    CDS_ERROR = -1
  }
}
