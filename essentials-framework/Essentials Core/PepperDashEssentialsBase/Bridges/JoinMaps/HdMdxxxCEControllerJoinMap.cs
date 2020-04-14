using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Bridges
{
    public class HdMdxxxCEControllerJoinMap : JoinMapBase
    {
        #region Digitals
        /// <summary>
        /// High when the pair is online
        /// </summary>
        public uint IsOnline { get; set; }

        /// <summary>
        /// High when the remote end device is online
        /// </summary>
        public uint RemoteEndDetected { get; set; }

        /// <summary>
        /// Sets Auto Route On and provides feedback
        /// </summary>
        public uint AutoRouteOn { get; set; }

        /// <summary>
        /// Sets Auto Route Off and provides feedback
        /// </summary>
        public uint AutoRouteOff { get; set; }

        /// <summary>
        /// Sets Priority Routing On and provides feedback
        /// </summary>
        public uint PriorityRoutingOn { get; set; }

        /// <summary>
        /// Sets Priority Routing Off and provides feedback
        /// </summary>
        public uint PriorityRoutingOff { get; set; }

        /// <summary>
        /// Enables OSD and provides feedback
        /// </summary>
        public uint InputOnScreenDisplayEnabled { get; set; }

        /// <summary>
        /// Disables OSD and provides feedback
        /// </summary>
        public uint InputOnScreenDisplayDisabled { get; set; }

        /// <summary>
        /// Provides Video Sync Detected feedback for each input
        /// </summary>
        public uint SyncDetected { get; set; }
        #endregion

        #region Analogs
        /// <summary>
        /// Sets the video source for the receiver's HDMI out and provides feedback
        /// </summary>
        public uint VideoSource { get; set; }

        /// <summary>
        /// Indicates the number of sources supported by the Tx/Rx pair
        /// </summary>
        public uint SourceCount { get; set; }
        #endregion

        #region Serials
        /// <summary>
        /// Indicates the name of each input port
        /// </summary>
        public uint SourceNames { get; set; }
        #endregion

        public HdMdxxxCEControllerJoinMap()
        {
            //Digital
            IsOnline = 1;
            RemoteEndDetected = 2;
            AutoRouteOn = 3;
            AutoRouteOff = 4;
            PriorityRoutingOn = 5;
            PriorityRoutingOff = 6;
            InputOnScreenDisplayEnabled = 7;
            InputOnScreenDisplayDisabled = 8;
            SyncDetected = 10; // 11-15

            //Analog
            VideoSource = 1;
            SourceCount = 2;

            //Serials
            SourceNames = 10; // 11-15
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;

            IsOnline = IsOnline + joinOffset;
            RemoteEndDetected = RemoteEndDetected + joinOffset;
            AutoRouteOn = AutoRouteOn + joinOffset;
            AutoRouteOff = AutoRouteOff + joinOffset;
            PriorityRoutingOn = PriorityRoutingOn + joinOffset;
            PriorityRoutingOff = PriorityRoutingOff + joinOffset;
            InputOnScreenDisplayEnabled = InputOnScreenDisplayEnabled + joinOffset;
            InputOnScreenDisplayDisabled = InputOnScreenDisplayDisabled + joinOffset;
            SyncDetected = SyncDetected + joinOffset;

            VideoSource = VideoSource + joinOffset;
            SourceCount = SourceCount + joinOffset;

            SourceNames = SourceNames + joinOffset;
        }
    }
}