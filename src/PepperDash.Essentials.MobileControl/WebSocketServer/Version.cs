using Newtonsoft.Json;


namespace PepperDash.Essentials.WebSocketServer
{
  /// <summary>
  /// Represents a Version
  /// </summary>
  public class Version
  {
    /// <summary>
    /// Server version this Websocket is connected to 
    /// </summary>
    [JsonProperty("serverVersion")]
    public string ServerVersion { get; set; }

    /// <summary>
    /// True if the server is on a processor
    /// </summary>

    [JsonProperty("serverIsRunningOnProcessorHardware")]
    public bool ServerIsRunningOnProcessorHardware { get; private set; }

    /// <summary>
    /// Initialize an instance of the <see cref="Version"/> class
    /// </summary>
    /// <remarks>
    /// The <see cref="ServerIsRunningOnProcessorHardware"/> property is set to true by default.
    /// </remarks>
    public Version()
    {
      ServerIsRunningOnProcessorHardware = true;
    }
  }
}
