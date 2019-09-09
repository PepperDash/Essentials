using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Bridges
{
    public class DigitalLoggerJoinMap : JoinMapBase
    {
        public uint IsOnline { get; set; }
        public uint CircuitNames { get; set; }
        public uint CircuitState { get; set; }
        public uint CircuitCycle { get; set; }
        public uint CircuitIsCritical { get; set; }
        public uint CircuitOnCmd { get; set; }
        public uint CircuitOffCmd { get; set; }

        public DigitalLoggerJoinMap()
        {
            // Digital
            IsOnline = 9;
            CircuitState = 0;
            CircuitCycle = 0;
            CircuitIsCritical = 10;
            CircuitOnCmd = 10;
            CircuitOffCmd = 20;
            // Serial
            CircuitNames = 0;
            // Analog
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;

            IsOnline = IsOnline + joinOffset;
            CircuitNames = CircuitNames + joinOffset;
            CircuitState = CircuitState + joinOffset;
            CircuitCycle = CircuitCycle + joinOffset;
            CircuitIsCritical = CircuitIsCritical + joinOffset;
            CircuitOnCmd = CircuitOnCmd + joinOffset;
            CircuitOffCmd = CircuitOffCmd + joinOffset;
        }
    }
}