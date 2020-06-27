namespace PepperDash.Essentials.Core.Devices.AudioCodec
{
    /// <summary>
    /// Implements a common set of data about a codec
    /// </summary>
    public interface IAudioCodecInfo
    {
        AudioCodecInfo CodecInfo { get; }
    }

    /// <summary>
    /// Stores general information about a codec
    /// </summary>
    public abstract class AudioCodecInfo
    {
        public abstract string PhoneNumber { get; set; }
    }
}