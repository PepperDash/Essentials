using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Bridges
{
    [Obsolete("Please use version PepperDash.Essentials.Core.Bridges")]
    public class AirMediaControllerJoinMap : JoinMapBase
    {
        #region Digitals
        /// <summary>
        /// Indicates that the device is online when high
        /// </summary>
        public uint IsOnline { get; set; }
        /// <summary>
        /// Indicates that the device is in session when high
        /// </summary>
        public uint IsInSession { get; set; }
        /// <summary>
        /// Indicates sync detected on HDMI input when high
        /// </summary>
        public uint HdmiVideoSync { get; set; }
        /// <summary>
        /// Set High to enable automatic input routing and low to disable.  Feedback high when enabled
        /// </summary>
        public uint AutomaticInputRoutingEnabled { get; set; }
        #endregion

        #region Analogs
        /// <summary>
        /// Selects source and provides feedback
        /// </summary>
        public uint VideoOut { get; set; }
        /// <summary>
        /// Provided error feedback
        /// </summary>
        public uint ErrorFB { get; set; }
        /// <summary>
        /// Indicates the number of connected users as feedback
        /// </summary>
        public uint NumberOfUsersConnectedFB { get; set; }
        /// <summary>
        /// Sets the login code and provides the current code as feedback
        /// </summary>
        public uint LoginCode { get; set; }
        #endregion

        #region Serials
        /// <summary>
        /// Provides the name defined in config as feedback
        /// </summary>
        public uint Name { get; set; }
        /// <summary>
        /// Provides the connection address as feedback
        /// </summary>
        public uint ConnectionAddressFB { get; set; }
        /// <summary>
        /// Provides the hostname as feedback
        /// </summary>
        public uint HostnameFB { get; set; }
        /// <summary>
        /// Provides the serial number as feedback
        /// </summary>
        public uint SerialNumberFeedback { get; set; }
        #endregion

        public AirMediaControllerJoinMap()
        {
            // Digital
            IsOnline = 1;
            IsInSession = 2;
            HdmiVideoSync = 3;
            AutomaticInputRoutingEnabled = 4;

            // Analog
            VideoOut = 1;
            ErrorFB = 2;
            NumberOfUsersConnectedFB = 3;
            LoginCode = 4;

            // Serial
            Name = 1;
            ConnectionAddressFB = 2;
            HostnameFB = 3;
            SerialNumberFeedback = 4;
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;

            IsOnline = IsOnline + joinOffset;
            IsInSession = IsInSession + joinOffset;
            HdmiVideoSync = HdmiVideoSync + joinOffset;
            AutomaticInputRoutingEnabled = AutomaticInputRoutingEnabled + joinOffset;

            VideoOut = VideoOut + joinOffset;
            ErrorFB = ErrorFB + joinOffset;
            NumberOfUsersConnectedFB = NumberOfUsersConnectedFB + joinOffset;
            LoginCode = LoginCode + joinOffset;

            Name = Name + joinOffset;
            ConnectionAddressFB = ConnectionAddressFB + joinOffset;
            HostnameFB = HostnameFB + joinOffset;
            SerialNumberFeedback = SerialNumberFeedback + joinOffset;
        }
    }
}