using System;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace PepperDash.Essentials.WebSocketServer
{
  /// <summary>
  /// Represents a JoinResponse
  /// </summary>
  public class JoinResponse
  {

    /// <summary>
    /// Gets or sets the ClientId
    /// </summary>
    [JsonProperty("clientId")]
    public string ClientId { get; set; }

    [JsonProperty("roomKey")]
    public string RoomKey { get; set; }

    [JsonProperty("systemUUid")]
    public string SystemUuid { get; set; }


    /// <summary>
    /// Gets or sets the RoomUuid
    /// </summary>
    [JsonProperty("roomUUid")]
    public string RoomUuid { get; set; }


    /// <summary>
    /// Gets or sets the Config
    /// </summary>
    [JsonProperty("config")]
    public object Config { get; set; }


    /// <summary>
    /// Gets or sets the CodeExpires
    /// </summary>
    [JsonProperty("codeExpires")]
    public DateTime CodeExpires { get; set; }


    /// <summary>
    /// Gets or sets the UserCode
    /// </summary>
    [JsonProperty("userCode")]
    public string UserCode { get; set; }


    /// <summary>
    /// Gets or sets the UserAppUrl
    /// </summary>
    [JsonProperty("userAppUrl")]
    public string UserAppUrl { get; set; }


    /// <summary>
    /// Gets or sets the EnableDebug
    /// </summary>
    [JsonProperty("enableDebug")]
    public bool EnableDebug { get; set; }

    ///  
    public Dictionary<string, DeviceInterfaceInfo> DeviceInterfaceSupport { get; set; }
  }
}
