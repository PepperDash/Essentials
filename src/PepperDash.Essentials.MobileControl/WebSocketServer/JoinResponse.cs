using System;
using Newtonsoft.Json;


namespace PepperDash.Essentials.WebSocketServer
{
  /// <summary>
  /// Represents the structure of the join response
  /// </summary>
  public class JoinResponse
  {
    [JsonProperty("clientId")]
    public string ClientId { get; set; }

    [JsonProperty("roomKey")]
    public string RoomKey { get; set; }

    [JsonProperty("systemUUid")]
    public string SystemUuid { get; set; }

    [JsonProperty("roomUUid")]
    public string RoomUuid { get; set; }

    [JsonProperty("config")]
    public object Config { get; set; }

    [JsonProperty("codeExpires")]
    public DateTime CodeExpires { get; set; }

    [JsonProperty("userCode")]
    public string UserCode { get; set; }

    [JsonProperty("userAppUrl")]
    public string UserAppUrl { get; set; }

    [JsonProperty("enableDebug")]
    public bool EnableDebug { get; set; }
  }
}
