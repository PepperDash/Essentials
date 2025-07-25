using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    /// <summary>
    /// Enumeration of eCodecCallStatus values
    /// </summary>
    public enum eCodecCallStatus
    {
        Unknown = 0,
        Connected, 
        Connecting, 
        Dialing, 
        Disconnected,
        Disconnecting, 
        EarlyMedia, 
        Idle,
        OnHold, 
        Ringing, 
        Preserved, 
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