using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Devices.Common.Codec

{
    /// <summary>
    /// Enumeration of eCodecCallType values
    /// </summary>
    public enum eCodecCallType
    {
        Unknown = 0, 
        Audio, 
        Video, 
        AudioCanEscalate, 
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