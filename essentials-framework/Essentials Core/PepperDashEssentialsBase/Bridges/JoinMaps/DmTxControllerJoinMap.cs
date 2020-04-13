using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Bridges
{
    public class DmTxControllerJoinMap : JoinMapBase
    {
        #region Digitals
        /// <summary>
        /// High when device is online (if not attached to a DMP3 or DM chassis with a CPU3 card
        /// </summary>
        public uint IsOnline { get; set; }
        /// <summary>
        /// High when video sync is detected
        /// </summary>
        public uint VideoSyncStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public uint FreeRunEnabled { get; set; }
        #endregion

        #region Analogs
        /// <summary>
        /// Sets and reports the video source
        /// </summary>
        public uint VideoInput { get; set; }
        /// <summary>
        /// Sets and reports the audio source
        /// </summary>
        public uint AudioInput { get; set; }
        /// <summary>
        /// Reports the highest supported HDCP state level for the corresponding input card
        /// </summary>
        public uint HdcpSupportCapability { get; set; }
        /// <summary>
        /// Sets and reports the current HDCP state for the corresponding input port
        /// </summary>
        public uint Port1HdcpState { get; set; }
        /// <summary>
        /// Sets and reports the current HDCP state for the corresponding input port
        /// </summary>
        public uint Port2HdcpState { get; set; }

        /// <summary>
        /// Sets and reports the current VGA Brightness level
        /// </summary>
        public uint VgaBrightness { get; set; }

        /// <summary>
        /// Sets and reports the current VGA Contrast level
        /// </summary>
        public uint VgaContrast { get; set; }
        #endregion

        #region Serials
        /// <summary>
        /// Reports the current input resolution
        /// </summary>
        public uint CurrentInputResolution { get; set; }
        #endregion


        public DmTxControllerJoinMap()
        {
            // Digital
            IsOnline = 1;
            VideoSyncStatus = 2;
            FreeRunEnabled = 3;
            // Serial
            CurrentInputResolution = 1;
            // Analog
            VideoInput = 1;
            AudioInput = 2;
            HdcpSupportCapability = 3;
            Port1HdcpState = 4;
            Port2HdcpState = 5;
            VgaBrightness = 6;
            VgaContrast = 7;
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;

            IsOnline = IsOnline + joinOffset;
            VideoSyncStatus = VideoSyncStatus + joinOffset;
            FreeRunEnabled = FreeRunEnabled + joinOffset;
            CurrentInputResolution = CurrentInputResolution + joinOffset;
            VideoInput = VideoInput + joinOffset;
            AudioInput = AudioInput + joinOffset;
            HdcpSupportCapability = HdcpSupportCapability + joinOffset;
            Port1HdcpState = Port1HdcpState + joinOffset;
            Port2HdcpState = Port2HdcpState + joinOffset;
            VgaBrightness = VgaBrightness + joinOffset;
            VgaContrast = VgaContrast + joinOffset;
        }
    }
}