namespace PepperDash.Essentials.Devices.Common.AudioCodec
{
    /// <summary>
    /// Implements a common set of data about a codec
    /// </summary>
    public interface IAudioCodecInfo
    {
        /// <summary>
        /// Gets the codec information
        /// </summary>
        AudioCodecInfo CodecInfo { get; }
    }

    /// <summary>
    /// Stores general information about a codec
    /// </summary>
    public abstract class AudioCodecInfo
    {
        /// <summary>
        /// Gets or sets the phone number
        /// </summary>
        public abstract string PhoneNumber { get; set; }
    }
}