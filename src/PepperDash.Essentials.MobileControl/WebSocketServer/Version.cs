using Newtonsoft.Json;


namespace PepperDash.Essentials.WebSocketServer
{
  /// <summary>
  /// Class to describe the server version info
  /// </summary>
  public class Version
  {
    [JsonProperty("serverVersion")]
    public string ServerVersion { get; set; }

    [JsonProperty("serverIsRunningOnProcessorHardware")]
    public bool ServerIsRunningOnProcessorHardware { get; private set; }

    public Version()
    {
      ServerIsRunningOnProcessorHardware = true;
    }
  }
}
