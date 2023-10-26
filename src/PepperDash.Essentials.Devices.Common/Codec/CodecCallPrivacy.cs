namespace PepperDash.Essentials.Devices.Common.Codec
{
    public class CodecCallPrivacy
    {
        /// <summary>
        /// Takes the Cisco privacy type and converts to the matching enum
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
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