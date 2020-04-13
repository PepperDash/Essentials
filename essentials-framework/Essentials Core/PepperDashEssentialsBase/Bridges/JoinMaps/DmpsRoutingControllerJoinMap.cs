using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Bridges
{
    public class DmpsRoutingControllerJoinMap : JoinMapBase
    {
        #region Digitals
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
        #endregion


        #region Analogs
        /// <summary>
        /// Range sets and reports the current video source for the corresponding output
        /// </summary>
        public uint OutputVideo { get; set; }
        /// <summary>
        /// Range sets and reports the current audio source for the corresponding output
        /// </summary>
        public uint OutputAudio { get; set; }
        /// <summary>
        /// Range sets and reports the current Usb source for the corresponding output
        /// </summary>
        //public uint OutputUsb { get; set; }
        ///// <summary>
        ///// Range sets and reports the current Usb source for the corresponding input
        ///// </summary>
        //public uint InputUsb { get; set; }
        ///// <summary>
        ///// Range sets and reports the current HDCP state for the corresponding input card
        ///// </summary>
        //public uint HdcpSupportState { get; set; }
        ///// <summary>
        ///// Range reports the highest supported HDCP state level for the corresponding input card
        ///// </summary>
        //public uint HdcpSupportCapability { get; set; }
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
        /// Range reports the name of the current audio source for the corresponding output card
        /// </summary>
        public uint OutputCurrentAudioInputNames { get; set; }
        /// <summary>
        /// Range reports the current input resolution for each corresponding input card
        /// </summary>
        public uint InputCurrentResolution { get; set; }
        #endregion


        public DmpsRoutingControllerJoinMap()
        {
            //Digital 
            VideoSyncStatus = 100; //101-299
            InputEndpointOnline = 500; //501-699
            OutputEndpointOnline = 700; //701-899

            //Analog
            OutputVideo = 100; //101-299
            OutputAudio = 300; //301-499
            //OutputUsb = 500; //501-699
            //InputUsb = 700; //701-899
            VideoSyncStatus = 100; //101-299
            //HdcpSupportState = 1000; //1001-1199
            //HdcpSupportCapability = 1200; //1201-1399


            //Serial
            InputNames = 100; //101-299
            OutputNames = 300; //301-499
            OutputCurrentVideoInputNames = 2000; //2001-2199
            OutputCurrentAudioInputNames = 2200; //2201-2399
            InputCurrentResolution = 2400; // 2401-2599
            InputEndpointOnline = 500; //501-699
            OutputEndpointOnline = 700; //701-899

        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;

            OutputVideo = OutputVideo + joinOffset;
            OutputAudio = OutputAudio + joinOffset;
            //OutputUsb = OutputUsb + joinOffset;
            //InputUsb = InputUsb + joinOffset;
            VideoSyncStatus = VideoSyncStatus + joinOffset;
            InputNames = InputNames + joinOffset;
            OutputNames = OutputNames + joinOffset;
            OutputCurrentVideoInputNames = OutputCurrentVideoInputNames + joinOffset;
            OutputCurrentAudioInputNames = OutputCurrentAudioInputNames + joinOffset;
            InputCurrentResolution = InputCurrentResolution + joinOffset;
            InputEndpointOnline = InputEndpointOnline + joinOffset;
            OutputEndpointOnline = OutputEndpointOnline + joinOffset;
            //HdcpSupportState = HdcpSupportState + joinOffset;
            //HdcpSupportCapability = HdcpSupportCapability + joinOffset;
        }
    }
}