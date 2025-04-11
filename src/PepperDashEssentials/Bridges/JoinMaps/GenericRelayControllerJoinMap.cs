﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Bridges
{
    [Obsolete("Please use version PepperDash.Essentials.Core.Bridges")]
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