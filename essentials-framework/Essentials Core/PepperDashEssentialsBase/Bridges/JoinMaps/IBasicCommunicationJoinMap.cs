using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Bridges
{
    public class IBasicCommunicationJoinMap : JoinMapBase
    {
        #region Digitals
        /// <summary>
        /// Set High to connect, Low to disconnect
        /// </summary>
        public uint Connect { get; set; }
        /// <summary>
        /// Reports Connected State (High = Connected)
        /// </summary>
        public uint Connected { get; set; }
        #endregion

        #region Analogs
        /// <summary>
        /// Reports the connections status value
        /// </summary>
        public uint Status { get; set; }
        #endregion

        #region Serials
        /// <summary>
        /// Data back from port
        /// </summary>
        public uint TextReceived { get; set; }
        /// <summary>
        /// Sends data to the port
        /// </summary>
        public uint SendText { get; set; }
        /// <summary>
        /// Takes a JSON serialized string that sets a COM port's parameters
        /// </summary>
        public uint SetPortConfig { get; set; }
        #endregion

        public IBasicCommunicationJoinMap()
        {
            TextReceived = 1;
            SendText = 1;
            SetPortConfig = 2;
            Connect = 1;
            Connected = 1;
            Status = 1;
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;

            TextReceived = TextReceived + joinOffset;
            SendText = SendText + joinOffset;
            SetPortConfig = SetPortConfig + joinOffset;
            Connect = Connect + joinOffset;
            Connected = Connected + joinOffset;
            Status = Status + joinOffset;
        }
    }
}