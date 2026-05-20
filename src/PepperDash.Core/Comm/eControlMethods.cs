using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Core
{
    /// <summary>
    /// Crestron Control Methods for a comm object
    /// </summary>
    public enum eControlMethod
    {
        /// <summary>
        /// 
        /// </summary>
        None = 0,
        /// <summary>
        /// RS232/422/485
        /// </summary>
        Com = 1,
        /// <summary>
        /// Crestron IpId (most Crestron ethernet devices)
        /// </summary>
        IpId = 2,
        /// <summary>
        /// Crestron IpIdTcp (HD-MD series, etc.)
        /// </summary>
        IpidTcp = 3,
        /// <summary>
        /// Crestron IR control
        /// </summary>
        IR = 4,
        /// <summary>
        /// SSH client
        /// </summary>
        Ssh = 5,
        /// <summary>
        /// TCP/IP client
        /// </summary>
        Tcpip = 6,
        /// <summary>
        /// Telnet
        /// </summary>
        Telnet = 7,
        /// <summary>
        /// Crestnet device
        /// </summary>
        Cresnet = 8,
        /// <summary>
        /// CEC Control, via a DM HDMI port
        /// </summary>
        Cec = 9,
        /// <summary>
        /// UDP Server
        /// </summary>
        Udp = 10,

        /// <summary>
        /// HTTP client
        /// </summary>
        Http = 11,
        /// <summary>
        /// HTTPS client
        /// </summary>
        Https = 12,
        /// <summary>
        /// Websocket client
        /// </summary>
        Ws = 13,
        /// <summary>
        /// Secure Websocket client 
        /// </summary>
        Wss = 14,
        /// <summary>
        /// Secure TCP/IP
        /// </summary>
        SecureTcpIp = 15,
        /// <summary>
        /// Used when comms needs to be handled in SIMPL and bridged opposite the normal direction
        /// </summary>
        ComBridge = 16,
        /// <summary>
        /// InfinetEX control
        /// </summary>
        InfinetEx = 17,
        /// <summary>
        /// UDP client
        /// </summary>
        UdpClient = 18,
    }
}