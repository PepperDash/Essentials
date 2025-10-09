namespace PepperDash.Essentials.Devices.Common.Codec
{
    /// <summary>
    /// Enumeration of eMeetingPrivacy values
    /// </summary>
    public enum eMeetingPrivacy
    {
        /// <summary>
        /// Unknown meeting privacy level
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Public meeting
        /// </summary>
        Public,

        /// <summary>
        /// Private meeting
        /// </summary>
        Private
    }

    /// <summary>
    /// Represents a CodecCallPrivacy
    /// </summary>
    public class CodecCallPrivacy
    {
        /// <summary>
        /// Takes the Cisco privacy type and converts to the matching enum
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// <summary>
        /// ConvertToDirectionEnum method
        /// </summary>
        public static eMeetingPrivacy ConvertToDirectionEnum(string s)
        {
            switch (s.ToLower())
            {
                case "public":
                    {
                        return eMeetingPrivacy.Public;
                    }
                case "private":
                    {
                        return eMeetingPrivacy.Private;
                    }
                default:
                    return eMeetingPrivacy.Unknown;
            }

        }

    }
}