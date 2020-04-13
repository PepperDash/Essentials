using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Bridges {
    public class DmBladeChassisControllerJoinMap : JoinMapBase {
        #region Digital/Analogs
        #endregion

        #region Digitals
        /// <summary>
        /// High when device is online
        /// </summary>
        public uint IsOnline { get; set; }
        /// <summary>
        /// Range reports video sync feedback for each input
        /// </summary>
        public uint VideoSyncStatus { get; set; }
        /// <summary>
        /// Range reports high if corresponding input's endpoint is online
        /// </summary>
        public uint InputEndpointOnline { get; set; }
        /// <summary>
        /// Range reports high if corresponding output's endpoint is online
        /// </summary>
        public uint OutputEndpointOnline { get; set; }
        /// <summary>
        /// Range reports high if corresponding input's transmitter supports bridging as a separate device for detailed AV switching, HDCP control, etc.
        /// </summary>
        public uint TxAdvancedIsPresent { get; set; } // indicates that there is an attached transmitter that should be bridged to be interacted with
        #endregion

        #region Analogs
        /// <summary>
        /// Range sets and reports the current video source for the corresponding output
        /// </summary>
        public uint OutputVideo { get; set; }
        /// <summary>
        /// Range sets and reports the current HDCP state for the corresponding input card
        /// </summary>
        public uint HdcpSupportState { get; set; }
        /// <summary>
        /// Range reports the highest supported HDCP state level for the corresponding input card
        /// </summary>
        public uint HdcpSupportCapability { get; set; }
        #endregion

        #region Serials
        /// <summary>
        /// Range sets and reports the name for the corresponding input card
        /// </summary>
        public uint InputNames { get; set; }
        /// <summary>
        /// Range sets and reports the name for the corresponding output card
        /// </summary>
        public uint OutputNames { get; set; }
        /// <summary>
        /// Range reports the name of the current video source for the corresponding output card
        /// </summary>
        public uint OutputCurrentVideoInputNames { get; set; }
        /// <summary>
        /// Range reports the current input resolution for each corresponding input card
        /// </summary>
        public uint InputCurrentResolution { get; set; }
        #endregion

        public DmBladeChassisControllerJoinMap() {
            //Digital/Analog

            //Digital 
            IsOnline = 11;
            VideoSyncStatus = 100; //101-299
            InputEndpointOnline = 500; //501-699
            OutputEndpointOnline = 700; //701-899
            TxAdvancedIsPresent = 1000; //1001-1199

            //Analog
            OutputVideo = 100; //101-299
            HdcpSupportState = 1000; //1001-1199
            HdcpSupportCapability = 1200; //1201-1399


            //Serial
            InputNames = 100; //101-299
            OutputNames = 300; //301-499
            OutputCurrentVideoInputNames = 2000; //2001-2199
            InputCurrentResolution = 2400; // 2401-2599
        }

        public override void OffsetJoinNumbers(uint joinStart) {
            var joinOffset = joinStart - 1;

            IsOnline = IsOnline + joinOffset;
            OutputVideo = OutputVideo + joinOffset;
            VideoSyncStatus = VideoSyncStatus + joinOffset;
            InputNames = InputNames + joinOffset;
            OutputNames = OutputNames + joinOffset;
            OutputCurrentVideoInputNames = OutputCurrentVideoInputNames + joinOffset;
            InputCurrentResolution = InputCurrentResolution + joinOffset;
            InputEndpointOnline = InputEndpointOnline + joinOffset;
            OutputEndpointOnline = OutputEndpointOnline + joinOffset;
            HdcpSupportState = HdcpSupportState + joinOffset;
            HdcpSupportCapability = HdcpSupportCapability + joinOffset;
        }
    }
}
