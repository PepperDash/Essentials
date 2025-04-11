﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Bridges
{
    [Obsolete("Please use version PepperDash.Essentials.Core.Bridges")]
    public class IDigitalInputJoinMap : JoinMapBase
    {
        #region Digitals
        /// <summary>
        /// Reports the state of the digital input
        /// </summary>
        public uint InputState { get; set; }
        #endregion

        public IDigitalInputJoinMap()
        {
            InputState = 1;
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;

            InputState = InputState + joinOffset;
        }
    }
}