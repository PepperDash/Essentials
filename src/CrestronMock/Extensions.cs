using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crestron.SimplSharp
{
  public static class CrestronInvoke
  {
    public static void BeginInvoke(Func<object> func, object? state = null)
    {
      Task.Run(func);
    }

    public static void BeginInvoke(Action action)
    {
      Task.Run(action);
    }
  }

  public static class CrestronEthernetHelper
  {
    public static List<string> GetEthernetAdaptersInfo()
    {
      return new List<string> { "MockAdapter" };
    }

    public static string GetEthernetParameter(string adapter, string parameter)
    {
      return "MockValue";
    }
  }
}

namespace Crestron.SimplSharp.Net.Https
{
  public class UrlParser
  {
    public string Url { get; set; }

    public UrlParser(string url)
    {
      Url = url;
    }
  }

  public class HttpsHeader
  {
    public string Name { get; set; }
    public string Value { get; set; }

    public HttpsHeader(string name, string value)
    {
      Name = name;
      Value = value;
    }
  }

  public class HttpException : Exception
  {
    public HttpException(string message) : base(message) { }
    public HttpException(string message, Exception innerException) : base(message, innerException) { }
  }
}

namespace System.Collections.Generic
{
  public static class DictionaryExtensions
  {
    public static void AddHeader(this Dictionary<string, string> dictionary, Crestron.SimplSharp.Net.Https.HttpsHeader header)
    {
      dictionary[header.Name] = header.Value;
    }
  }
}
