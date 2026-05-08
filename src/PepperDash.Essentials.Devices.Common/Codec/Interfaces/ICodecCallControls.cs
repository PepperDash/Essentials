using System.Collections.Generic;

namespace PepperDash.Essentials.Devices.Common.Codec;

/// <summary>
/// Defines call control functionality for a codec, extending the base dialer interface
/// with active call list access and meeting dialing.
/// </summary>
public interface ICodecCallControls : IHasDialer
{
    /// <summary>
    /// Gets the list of currently active, dialing, or incoming calls
    /// </summary>
    List<CodecActiveCallItem> ActiveCalls { get; }

    /// <summary>
    /// Dials the specified meeting
    /// </summary>
    /// <param name="meeting">The meeting to dial</param>
    void Dial(Meeting meeting);
}
