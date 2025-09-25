using Newtonsoft.Json;


namespace PepperDash.Essentials.WebSocketServer
{
  /// <summary>
  /// Represents a Version
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
