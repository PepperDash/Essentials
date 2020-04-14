using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Bridges
{
    public class DmpsAudioOutputControllerJoinMap : JoinMapBase
    {
        #region Digital/Analog
        /// <summary>
        /// Range of joins for Master Volume 
        /// Analog join 1 is volume level and feedback
        /// Digital join 1 is Mute on and feedback
        /// Digital join 2 is Mute off and feedback
        /// Digital join 3 is volume up
        /// Digital join 4 is volume down
        /// </summary>
        public uint MasterVolume { get; set; }
        /// <summary>
        /// Range of joins for Source Volume 
        /// Analog join 11 is volume level and feedback
        /// Digital join 11 is Mute on and feedback
        /// Digital join 12 is Mute off and feedback
        /// Digital join 13 is volume up
        /// Digital join 14 is volume down
        /// </summary>
        public uint SourceVolume { get; set; }
        /// <summary>
        /// Range of joins for Codec1 Volume (if applicable)
        /// Analog join 21 is volume level and feedback
        /// Digital join 21 is Mute on and feedback
        /// Digital join 22 is Mute off and feedback
        /// Digital join 23 is volume up
        /// Digital join 24 is volume down
        /// </summary>
        public uint Codec1Volume { get; set; }
        /// <summary>
        /// Range of joins for Codec2 Volume (if applicable)
        /// Analog join 31 is volume level and feedback
        /// Digital join 31 is Mute on and feedback
        /// Digital join 32 is Mute off and feedback
        /// Digital join 33 is volume up
        /// Digital join 34 is volume down
        /// </summary>
        public uint Codec2Volume { get; set; }
        #endregion

        public DmpsAudioOutputControllerJoinMap()
        {
            MasterVolume = 1; // 1-10
            SourceVolume = 11; // 11-20
            Codec1Volume = 21; // 21-30
            Codec2Volume = 31; // 31-40
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart;

            MasterVolume = MasterVolume + joinOffset;
            SourceVolume = SourceVolume + joinOffset;
            Codec1Volume = Codec1Volume + joinOffset;
            Codec2Volume = Codec2Volume + joinOffset;
        }
    }
}