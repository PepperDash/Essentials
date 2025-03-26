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
        Com,
        /// <summary>
        /// Crestron IpId (most Crestron ethernet devices)
        /// </summary>
        IpId,
        /// <summary>
        /// Crestron IpIdTcp (HD-MD series, etc.)
        /// </summary>
        IpidTcp,
        /// <summary>
        /// Crestron IR control
        /// </summary>
        IR,
        /// <summary>
        /// SSH client
        /// </summary>
        Ssh,
        /// <summary>
        /// TCP/IP client
        /// </summary>
        Tcpip,
        /// <summary>
        /// Telnet
        /// </summary>
        Telnet,
        /// <summary>
        /// Crestnet device
        /// </summary>
        Cresnet,
        /// <summary>
        /// CEC Control, via a DM HDMI port
        /// </summary>
        Cec,
        /// <summary>
        /// UDP Server
        /// </summary>
        Udp,
        /// <summary>
        /// HTTP client
        /// </summary>
        Http,
        /// <summary>
        /// HTTPS client
        /// </summary>
        Https,
        /// <summary>
        /// Websocket client
        /// </summary>
        Ws,
        /// <summary>
        /// Secure Websocket client 
        /// </summary>
        Wss,
        /// <summary>
        /// Secure TCP/IP
        /// </summary>
        SecureTcpIp
    }
}