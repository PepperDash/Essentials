using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.AudioCodec;

/// <summary>
/// Interface for devices that have an audio codec.
/// </summary>
public interface IHasAudioCodec:IHasInCallFeedback
{
    /// <summary>
    /// Gets the audio codec device
    /// </summary>
    AudioCodecBase AudioCodec { get; }
}