using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PepperDash.Essentials.Devices.Common.Codec;

/// <summary>
/// Represents an active call item for a codec, including details such as name, number, type, status, direction, and duration.
/// </summary>
public class CodecActiveCallItem
{
    /// <summary>
    /// The name associated with the call, if available.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }

    /// <summary>
    /// The phone number associated with the call, if available.
    /// </summary>
    [JsonProperty("number", NullValueHandling = NullValueHandling.Ignore)]
    public string Number { get; set; }

    /// <summary>
    /// The type of the call, such as audio or video. This is an optional property and may be null if the type is unknown or not applicable.
    /// </summary>
    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(StringEnumConverter))]
    public eCodecCallType Type { get; set; }

    /// <summary>
    /// The current status of the call, such as active, on hold, or disconnected. This is an optional property and may be null if the status is unknown or not applicable.
    /// </summary>
    [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(StringEnumConverter))]
    public eCodecCallStatus Status { get; set; }

    /// <summary>
    /// The direction of the call, such as incoming or outgoing. This is an optional property and may be null if the direction is unknown or not applicable.
    /// </summary>
    [JsonProperty("direction", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(StringEnumConverter))]
    public eCodecCallDirection Direction { get; set; }

    /// <summary>
    /// A unique identifier for the call, if available. This can be used to track the call across different events and status changes. This is an optional property and may be null if an identifier is not available or applicable.
    /// </summary>
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public string Id { get; set; }

    /// <summary>
    /// Indicates whether the call is currently on hold. This is an optional property and may be null if the hold status is unknown or not applicable.
    /// </summary>
    [JsonProperty("isOnHold", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsOnHold { get; set; }

    /// <summary>
    /// The duration of the call, if available. This can be used to track how long the call has been active. This is an optional property and may be null if the duration is not available or applicable.
    /// </summary>
    [JsonProperty("duration", NullValueHandling = NullValueHandling.Ignore)]
    public TimeSpan Duration { get; set; }

    //public object CallMetaData { get; set; }

    /// <summary>
    /// Returns true when this call is any status other than 
    /// Unknown, Disconnected, Disconnecting
    /// </summary>
    [JsonProperty("isActiveCall", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsActiveCall
    {
        get
        {
            return !(Status == eCodecCallStatus.Disconnected
                || Status == eCodecCallStatus.Disconnecting
                    || Status == eCodecCallStatus.Idle
                || Status == eCodecCallStatus.Unknown);
        }
    }
}

/// <summary>
/// Event args for when a call status changes, includes the call item with updated status and details.
/// </summary>
public class CodecCallStatusItemChangeEventArgs : EventArgs
{
    /// <summary>
    /// The call item that has changed status, including its updated status and details. This property is read-only and is set through the constructor when the event args are created.
    /// </summary>
    public CodecActiveCallItem CallItem { get; private set; }

    /// <summary>
    /// Constructor for CodecCallStatusItemChangeEventArgs
    /// </summary>
    /// <param name="item">The call item that has changed status.</param>
    public CodecCallStatusItemChangeEventArgs(CodecActiveCallItem item)
    {
        CallItem = item;
    }
}