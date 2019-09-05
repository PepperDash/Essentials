using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Bridges
{
    public class GenericRelayControllerJoinMap : JoinMapBase
    {
        #region Digitals
        /// <summary>
        /// Sets and reports the state of the relay (High = closed, Low = Open)
        /// </summary>
        public uint Relay { get; set; }
        #endregion

        public GenericRelayControllerJoinMap()
        {
            Relay = 1;
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;

            Relay = Relay + joinOffset;
        }
    }
}