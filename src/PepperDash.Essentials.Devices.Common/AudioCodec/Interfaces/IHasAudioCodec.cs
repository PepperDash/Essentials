using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.AudioCodec
{
    /// <summary>
    /// For rooms that have audio codec
    /// </summary>
    public interface IHasAudioCodec : IHasInCallFeedback
    {
        /// <summary>
        /// Gets the audio codec device
        /// </summary>
        AudioCodecBase AudioCodec { get; }

        //List<PepperDash.Essentials.Devices.Common.Codec.CodecActiveCallItem> ActiveCalls { get; }
    }
}