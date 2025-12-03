namespace PepperDash.Essentials.Devices.Common.Codec

{
    /// <summary>
    /// Enumeration of eCodecCallType values
    /// </summary>
    public enum eCodecCallType
    {
        /// <summary>
        /// Unknown call type
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Audio-only call type
        /// </summary>
        Audio,

        /// <summary>
        /// Video call type
        /// </summary>
        Video,

        /// <summary>
        /// Audio call that can be escalated to video
        /// </summary>
        AudioCanEscalate,

        /// <summary>
        /// Forward all call type
        /// </summary>
        ForwardAllCall
    }

    /// <summary>
    /// Represents a CodecCallType
    /// </summary>
    public class CodecCallType
    {

        /// <summary>
        /// Takes the Cisco call type and converts to the matching enum
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// <summary>
        /// ConvertToTypeEnum method
        /// </summary>
        public static eCodecCallType ConvertToTypeEnum(string s)
        {
            switch (s)
            {
                case "Audio":
                    {
                        return eCodecCallType.Audio;
                    }
                case "Video":
                    {
                        return eCodecCallType.Video;
                    }
                case "AudioCanEscalate":
                    {
                        return eCodecCallType.AudioCanEscalate;
                    }
                case "ForwardAllCall":
                    {
                        return eCodecCallType.ForwardAllCall;
                    }
                default:
                    return eCodecCallType.Unknown;
            }

        }

    }
}