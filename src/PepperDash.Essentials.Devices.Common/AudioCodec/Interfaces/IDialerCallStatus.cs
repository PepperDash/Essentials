using System.Collections.Generic;
using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.Devices.Common.AudioCodec;

/// <summary>
/// Defines the contract for a device that has dialer call status information. This is used to provide a common interface for the AudioCodecBaseMessenger to get call status information without requiring the full AudioCodecBase class
/// </summary>
public interface IDialerCallStatus : IHasDialer
{
    /// <summary>
    /// 
    /// </summary>
    AudioCodecInfo CodecInfo { get; }

    /// <summary>
    /// Gets or sets the list of active calls for the device.
    /// </summary>
    List<CodecActiveCallItem> ActiveCalls { get; set; }
}
