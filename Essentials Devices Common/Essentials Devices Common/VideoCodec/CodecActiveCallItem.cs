using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Devices.Common.VideoCodec

{
    public class CodecActiveCallItem
    {
        public string Name { get; set; }

        public string Number { get; set; }

        public eCodecCallType Type { get; set; }

        public eCodecCallStatus Status { get; set; }

        public eCodecCallDirection Direction { get; set; }

        public string Id { get; set; }

        public object CallMetaData { get; set; }
    }

    public enum eCodecCallType
    {
        Unknown = 0, Audio, Video, AudioCanEscalate, ForwardAllCall
    }

    public enum eCodecCallStatus
    {
        Unknown = 0, Idle, Dialing, Ringing, Connecting, Connected, Disconnecting, Incoming, OnHold, EarlyMedia, Preserved, RemotePreserved, Disconnected
    }

    public enum eCodecCallDirection
    {
        Unknown = 0, Incoming, Outgoing
    }
}