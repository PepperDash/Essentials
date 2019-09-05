using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Bridges
{
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