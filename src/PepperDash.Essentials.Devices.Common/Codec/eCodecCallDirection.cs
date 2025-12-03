namespace PepperDash.Essentials.Devices.Common.Codec

{
    /// <summary>
    /// Enumeration of eCodecCallDirection values
    /// </summary>
    public enum eCodecCallDirection
    {
        /// <summary>
        /// Unknown call direction
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Incoming call direction
        /// </summary>
        Incoming,

        /// <summary>
        /// Outgoing call direction
        /// </summary>
        Outgoing
    }

    /// <summary>
    /// Represents a CodecCallDirection
    /// </summary>
    public class CodecCallDirection
    {
        /// <summary>
        /// Takes the Cisco call type and converts to the matching enum
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// <summary>
        /// ConvertToDirectionEnum method
        /// </summary>
        public static eCodecCallDirection ConvertToDirectionEnum(string s)
        {
            switch (s.ToLower())
            {
                case "incoming":
                    {
                        return eCodecCallDirection.Incoming;
                    }
                case "outgoing":
                    {
                        return eCodecCallDirection.Outgoing;
                    }
                default:
                    return eCodecCallDirection.Unknown;
            }

        }

    }
}