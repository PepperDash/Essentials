using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Bridges
{
    public class DmChassisControllerJoinMap : JoinMapBaseAdvanced
    {
        [JoinName("EnableAudioBreakaway")]
        public JoinDataComplete EnableAudioBreakaway = new JoinDataComplete(
            new JoinData {JoinNumber = 4, JoinSpan = 1},
            new JoinMetadata
            {
                Label = "DM Chassis enable audio breakaway routing",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("EnableUsbBreakaway")]
        public JoinDataComplete EnableUsbBreakaway = new JoinDataComplete(
            new JoinData { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata
            {
                Label = "DM Chassis enable USB breakaway routing",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("SystemId")]
        public JoinDataComplete SystemId = new JoinDataComplete(new JoinData() { JoinNumber = 10, JoinSpan = 1 },
            new JoinMetadata() { Label = "DM Chassis SystemId Get/Set/Trigger/", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.DigitalAnalog });

        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData() { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata() { Label = "DM Chassis Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("VideoSyncStatus")]
        public JoinDataComplete VideoSyncStatus = new JoinDataComplete(new JoinData() { JoinNumber = 101, JoinSpan = 32 },
            new JoinMetadata() { Label = "DM Input Video Sync", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("InputEndpointOnline")]
        public JoinDataComplete InputEndpointOnline = new JoinDataComplete(new JoinData() { JoinNumber = 501, JoinSpan = 32 },
            new JoinMetadata() { Label = "DM Chassis Input Endpoint Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("OutputEndpointOnline")]
        public JoinDataComplete OutputEndpointOnline = new JoinDataComplete(new JoinData() { JoinNumber = 701, JoinSpan = 32 },
            new JoinMetadata() { Label = "DM Chassis Output Endpoint Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("TxAdvancedIsPresent")]
        public JoinDataComplete TxAdvancedIsPresent = new JoinDataComplete(new JoinData() { JoinNumber = 1001, JoinSpan = 32 },
            new JoinMetadata() { Label = "DM Chassis Tx Advanced Is Present", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("OutputDisabledByHdcp")]
        public JoinDataComplete OutputDisabledByHdcp = new JoinDataComplete(new JoinData() { JoinNumber = 1201, JoinSpan = 32 },
            new JoinMetadata() { Label = "DM Chassis Output Disabled by HDCP", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("OutputVideo")]
        public JoinDataComplete OutputVideo = new JoinDataComplete(new JoinData() { JoinNumber = 101, JoinSpan = 32 },
            new JoinMetadata() { Label = "DM Chassis Output Video Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("OutputAudio")]
        public JoinDataComplete OutputAudio = new JoinDataComplete(new JoinData() { JoinNumber = 301, JoinSpan = 32 },
            new JoinMetadata() { Label = "DM Chassis Output Audio Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("OutputUsb")]
        public JoinDataComplete OutputUsb = new JoinDataComplete(new JoinData() { JoinNumber = 501, JoinSpan = 32 },
            new JoinMetadata() { Label = "DM Chassis Output USB Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("InputUsb")]
        public JoinDataComplete InputUsb = new JoinDataComplete(new JoinData() { JoinNumber = 701, JoinSpan = 32 },
            new JoinMetadata() { Label = "DM Chassis Input Usb Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("HdcpSupportState")]
        public JoinDataComplete HdcpSupportState = new JoinDataComplete(new JoinData() { JoinNumber = 1001, JoinSpan = 32 },
            new JoinMetadata() { Label = "DM Chassis Input HDCP Support State", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

        [JoinName("HdcpSupportCapability")]
        public JoinDataComplete HdcpSupportCapability = new JoinDataComplete(new JoinData() { JoinNumber = 1201, JoinSpan = 32 },
            new JoinMetadata() { Label = "DM Chassis Input HDCP Support Capability", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("InputNames")]
        public JoinDataComplete InputNames = new JoinDataComplete(new JoinData() { JoinNumber = 101, JoinSpan = 32 },
            new JoinMetadata() { Label = "DM Chassis Input Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("OutputNames")]
        public JoinDataComplete OutputNames = new JoinDataComplete(new JoinData() { JoinNumber = 301, JoinSpan = 32 },
            new JoinMetadata() { Label = "DM Chassis Output Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("OutputCurrentVideoInputNames")]
        public JoinDataComplete OutputCurrentVideoInputNames = new JoinDataComplete(new JoinData() { JoinNumber = 2001, JoinSpan = 32 },
            new JoinMetadata() { Label = "DM Chassis Video Output Currently Routed Video Input Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("OutputCurrentAudioInputNames")]
        public JoinDataComplete OutputCurrentAudioInputNames = new JoinDataComplete(new JoinData() { JoinNumber = 2201, JoinSpan = 32 },
            new JoinMetadata() { Label = "DM Chassis Audio Output Currently Routed Video Input Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("InputCurrentResolution")]
        public JoinDataComplete InputCurrentResolution = new JoinDataComplete(new JoinData() { JoinNumber = 2401, JoinSpan = 32 },
            new JoinMetadata() { Label = "DM Chassis Input Current Resolution", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        public DmChassisControllerJoinMap(uint joinStart)
            : base(joinStart, typeof(DmChassisControllerJoinMap))
        {
        }
    }
}
