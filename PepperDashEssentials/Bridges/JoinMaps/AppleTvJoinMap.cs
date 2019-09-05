using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Bridges
{
    public class AppleTvJoinMap : JoinMapBase
    {
        #region Digitals
        /// <summary>
        /// Sends up arrow command while high
        /// </summary>
        public uint UpArrow { get; set; }
        /// <summary>
        /// Sends down arrow command while high
        /// </summary>
        public uint DnArrow { get; set; }
        /// <summary>
        /// Sends left arrow command while high
        /// </summary>
        public uint LeftArrow { get; set; }
        /// <summary>
        /// Sends right arrow command while high
        /// </summary>
        public uint RightArrow { get; set; }
        /// <summary>
        /// Sends menu command
        /// </summary>
        public uint Menu { get; set; }
        /// <summary>
        /// Sends select command
        /// </summary>
        public uint Select { get; set; }
        /// <summary>
        /// Sends play/pause command
        /// </summary>
        public uint PlayPause { get; set; }
        #endregion

        public AppleTvJoinMap()
        {
            UpArrow = 1;
            DnArrow = 2;
            LeftArrow = 3;
            RightArrow = 4;
            Menu = 5;
            Select = 6;
            PlayPause = 7;
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;

            UpArrow = UpArrow + joinOffset;
            DnArrow = DnArrow + joinOffset;
            LeftArrow = LeftArrow + joinOffset;
            RightArrow = RightArrow + joinOffset;
            Menu = Menu + joinOffset;
            Select = Select + joinOffset;
            PlayPause = PlayPause + joinOffset;
        }
    }
}