using Newtonsoft.Json;
using PepperDash.Core;

/// <summary>
/// Config properties for an IEssentialsRoomFusionController device
/// </summary>
public class IEssentialsRoomFusionControllerPropertiesConfig
{
    /// <summary>
    /// Gets or sets the IP ID of the Fusion Room Controller
    /// </summary>
    [JsonProperty("ipId")]
    public string IpId { get; set; }

    /// <summary>
    /// Gets the IP ID as a UInt16
    /// </summary>
    [JsonIgnore]
    public uint IpIdInt
    {
        get
        {
            // Try to parse the IpId string to UInt16 as hex
            if (ushort.TryParse(IpId, System.Globalization.NumberStyles.HexNumber, null, out ushort result))
            {
                return result;
            }
            else
            {
                Debug.LogWarning("Failed to parse IpId '{0}' as UInt16", IpId);
                return 0;
            }
        }
    }

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

    /// <summary>
    /// Gets or sets whether to use the Fusion room name for this room
    /// </summary>
    /// <remarks>Defaults to true to preserve current behavior. Set to false to skip updating the room name from Fusion</remarks>
    [JsonProperty("useFusionRoomName")]
    public bool UseFusionRoomName { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to use HTML format for help requests
    /// </summary>
    [JsonProperty("useHtmlFormatForHelpRequests")]
    public bool UseHtmlFormatForHelpRequests { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to use 24-hour time format
    /// </summary>
    [JsonProperty("use24HourTimeFormat")]
    public bool Use24HourTimeFormat { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to use a timeout for help requests
    /// </summary>
    [JsonProperty("useTimeoutForHelpRequests")]
    public bool UseTimeoutForHelpRequests { get; set; } = false;

    /// <summary>
    /// Gets or sets the timeout duration for help requests in milliseconds
    /// </summary>
    [JsonProperty("helpRequestTimeoutMs")]
    public int HelpRequestTimeoutMs { get; set; } = 30000;
}