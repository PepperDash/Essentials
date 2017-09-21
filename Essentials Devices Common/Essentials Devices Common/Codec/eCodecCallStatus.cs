using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Devices.Common.Codec

{
    public enum eCodecCallStatus
    {
        Unknown = 0,
        Idle,
        Dialing,
        Ringing,
        Connecting,
        Connected,
        Disconnecting,
        Incoming,
        OnHold,
        EarlyMedia,
        Preserved,
        RemotePreserved,
        Disconnected
    }

    public class CodecCallStatus
    {

        /// <summary>
        /// Takes the Cisco status and converts to the matching enum
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static eCodecCallStatus ConvertToStatusEnum(string s)
        {
            switch (s)
            {
                case "Idle":
                    {
                        return eCodecCallStatus.Idle;
                    }
                case "Dialling":
                    {
                        return eCodecCallStatus.Dialing;
                    }
                case "Ringing":
                    {
                        return eCodecCallStatus.Ringing;
                    }
                case "Connecting":
                    {
                        return eCodecCallStatus.Connecting;
                    }
                case "Connected":
                    {
                        return eCodecCallStatus.Connected;
                    }
                case "Disconnecting":
                    {
                        return eCodecCallStatus.Disconnecting;
                    }
                case "Incoming":
                    {
                        return eCodecCallStatus.Incoming;
                    }
                case "OnHold":
                    {
                        return eCodecCallStatus.OnHold;
                    }
                case "EarlyMedia":
                    {
                        return eCodecCallStatus.EarlyMedia;
                    }
                case "Preserved":
                    {
                        return eCodecCallStatus.Preserved;
                    }
                case "RemotePreserved":
                    {
                        return eCodecCallStatus.RemotePreserved;
                    }
                case "Disconnected":
                    {
                        return eCodecCallStatus.Disconnected;
                    }
                default:
                    return eCodecCallStatus.Unknown;
            }
        }
    }
}