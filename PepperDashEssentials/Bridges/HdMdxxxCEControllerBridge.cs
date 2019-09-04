using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.DM;

namespace PepperDash.Essentials.Bridges
{
    public static class HdMdxxxCEControllerApiExtensions
    {
        public static void LinkToApi(this HdMdxxxCEController hdMdPair, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            var joinMap = JoinMapHelper.GetJoinMapForDevice(joinMapKey) as HdMdxxxCEControllerJoinMap;

            if (joinMap == null)
                joinMap = new HdMdxxxCEControllerJoinMap();

            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, hdMdPair, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            hdMdPair.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);
            hdMdPair.RemoteEndDetectedFeedback.LinkInputSig(trilist.BooleanInput[joinMap.RemoteEndDetected]);

            trilist.SetSigTrueAction(joinMap.AutoRouteOn, new Action(() => hdMdPair.AutoRouteOn()));
            hdMdPair.AutoRouteOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.AutoRouteOn]);
            trilist.SetSigTrueAction(joinMap.AutoRouteOff, new Action(() => hdMdPair.AutoRouteOff()));
            hdMdPair.AutoRouteOnFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.AutoRouteOff]);

            trilist.SetSigTrueAction(joinMap.PriorityRoutingOn, new Action(() => hdMdPair.PriorityRouteOn()));
            hdMdPair.PriorityRoutingOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PriorityRoutingOn]);
            trilist.SetSigTrueAction(joinMap.PriorityRoutingOff, new Action(() => hdMdPair.PriorityRouteOff()));
            hdMdPair.PriorityRoutingOnFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.PriorityRoutingOff]);

            trilist.SetSigTrueAction(joinMap.InputOnScreenDisplayEnabled, new Action(() => hdMdPair.OnScreenDisplayEnable()));
            hdMdPair.InputOnScreenDisplayEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.InputOnScreenDisplayEnabled]);
            trilist.SetSigTrueAction(joinMap.AutoRouteOff, new Action(() => hdMdPair.OnScreenDisplayDisable()));
            hdMdPair.InputOnScreenDisplayEnabledFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.InputOnScreenDisplayDisabled]);

            trilist.SetUShortSigAction(joinMap.VideoSource, new Action<ushort>((i) => hdMdPair.ExecuteSwitch(i, null, eRoutingSignalType.Video | eRoutingSignalType.Audio)));
            hdMdPair.VideoSourceFeedback.LinkInputSig(trilist.UShortInput[joinMap.VideoSource]);

            trilist.UShortInput[joinMap.SourceCount].UShortValue = (ushort)hdMdPair.InputPorts.Count;

            foreach (var input in hdMdPair.InputPorts)
            {
                var number = Convert.ToUInt16(input.Selector);
                hdMdPair.SyncDetectedFeedbacks[number].LinkInputSig(trilist.BooleanInput[joinMap.SyncDetected + number]);
                trilist.StringInput[joinMap.SourceNames + number].StringValue = input.Key;
            }

        }

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
}