using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Bridges
{
    [Obsolete("Please use version PepperDash.Essentials.Core.Bridges")]
    public class DmRmcControllerJoinMap : JoinMapBase
    {
        #region Digitals
        /// <summary>
        /// High when device is online (if not attached to a DMP3 or DM chassis with a CPU3 card
        /// </summary>
        public uint IsOnline { get; set; }
        #endregion

        #region Serials
        /// <summary>
        /// Reports the current output resolution
        /// </summary>
        public uint CurrentOutputResolution { get; set; }
        /// <summary>
        /// Reports the EDID manufacturer value
        /// </summary>
        public uint EdidManufacturer { get; set; }
        /// <summary>
        /// Reports the EDID Name value
        /// </summary>
        public uint EdidName { get; set; }
        /// <summary>
        /// Reports the EDID preffered timing value
        /// </summary>
        public uint EdidPrefferedTiming { get; set; }
        /// <summary>
        /// Reports the EDID serial number value
        /// </summary>
        public uint EdidSerialNumber { get; set; }
        #endregion

        #region Analogs
        public uint AudioVideoSource { get; set; }
        #endregion

        public DmRmcControllerJoinMap()
        {
            // Digital
            IsOnline = 1;

            // Serial
            CurrentOutputResolution = 1;
            EdidManufacturer = 2;
            EdidName = 3;
            EdidPrefferedTiming = 4;
            EdidSerialNumber = 5;

            //Analog
            AudioVideoSource = 1;
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;

            IsOnline = IsOnline + joinOffset;
            CurrentOutputResolution = CurrentOutputResolution + joinOffset;
            EdidManufacturer = EdidManufacturer + joinOffset;
            EdidName = EdidName + joinOffset;
            EdidPrefferedTiming = EdidPrefferedTiming + joinOffset;
            EdidSerialNumber = EdidSerialNumber + joinOffset;
            AudioVideoSource = AudioVideoSource + joinOffset;
        }
    }
}