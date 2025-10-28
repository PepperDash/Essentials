using Newtonsoft.Json;

/// <summary>
/// Config properties for an IEssentialsRoomFusionController device
/// </summary>
public class IEssentialsRoomFusionControllerPropertiesConfig
{
    /// <summary>
    /// Gets or sets the IP ID of the Fusion Room Controller
    /// </summary>
    [JsonProperty("ipId")]
    public uint IpId { get; set; }

    /// <summary>
    /// Gets or sets the join map key
    /// </summary>
    [JsonProperty("joinMapKey")]
    public string JoinMapKey { get; set; }

    /// <summary>
    /// Gets or sets the room key associated with this Fusion Room Controller
    /// </summary>
    [JsonProperty("roomKey")]
    public string RoomKey { get; set; }
}