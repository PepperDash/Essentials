namespace PepperDash.Essentials.Devices.Common.Codec
{
    public class CodecCallType
    {

        /// <summary>
        /// Takes the Cisco call type and converts to the matching enum
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
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