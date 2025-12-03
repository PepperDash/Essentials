namespace PepperDash.Essentials.Devices.Common.Codec
{
    /// <summary>
    /// Enumeration of eCodecCallStatus values
    /// </summary>
    public enum eCodecCallStatus
    {
        /// <summary>
        /// Unknown call status
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Call is connected
        /// </summary>
        Connected,

        /// <summary>
        /// Call is connecting
        /// </summary>
        Connecting,

        /// <summary>
        /// Call is dialing
        /// </summary>
        Dialing,

        /// <summary>
        /// Call is disconnected
        /// </summary>
        Disconnected,

        /// <summary>
        /// Call is disconnecting
        /// </summary>
        Disconnecting,

        /// <summary>
        /// Early media is being sent/received
        /// </summary>
        EarlyMedia,

        /// <summary>
        /// Call is idle
        /// </summary>
        Idle,

        /// <summary>
        /// Call is on hold
        /// </summary>
        OnHold,

        /// <summary>
        /// Call is ringing
        /// </summary>
        Ringing,

        /// <summary>
        /// Call is preserved
        /// </summary>
        Preserved,

        /// <summary>
        /// Call is remote preserved
        /// </summary>
        RemotePreserved,
    }


    /// <summary>
    /// Represents a CodecCallStatus
    /// </summary>
    public class CodecCallStatus
    {

        /// <summary>
        /// Takes the Cisco call type and converts to the matching enum
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// <summary>
        /// ConvertToStatusEnum method
        /// </summary>
        public static eCodecCallStatus ConvertToStatusEnum(string s)
        {
            switch (s)
            {
                case "Connected":
                    {
                        return eCodecCallStatus.Connected;
                    }
                case "Connecting":
                    {
                        return eCodecCallStatus.Connecting;
                    }
                case "Dialling":
                    {
                        return eCodecCallStatus.Dialing;
                    }
                case "Disconnected":
                    {
                        return eCodecCallStatus.Disconnected;
                    }
                case "Disconnecting":
                    {
                        return eCodecCallStatus.Disconnecting;
                    }
                case "EarlyMedia":
                    {
                        return eCodecCallStatus.EarlyMedia;
                    }
                case "Idle":
                    {
                        return eCodecCallStatus.Idle;
                    }
                case "OnHold":
                    {
                        return eCodecCallStatus.OnHold;
                    }
                case "Ringing":
                    {
                        return eCodecCallStatus.Ringing;
                    }
                case "Preserved":
                    {
                        return eCodecCallStatus.Preserved;
                    }
                case "RemotePreserved":
                    {
                        return eCodecCallStatus.RemotePreserved;
                    }
                default:
                    return eCodecCallStatus.Unknown;
            }

        }

    }
}