using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Bridges
{
    public class DmTxControllerJoinMap : JoinMapBaseAdvanced
    {
        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "DM TX Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("VideoSyncStatus")]
        public JoinDataComplete VideoSyncStatus = new JoinDataComplete(new JoinData() { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata() { Label = "DM TX Video Sync", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("FreeRunEnabled")]
        public JoinDataComplete FreeRunEnabled = new JoinDataComplete(new JoinData() { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata() { Label = "DM TX Enable Free Run Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Input1VideoSyncStatus")]
        public JoinDataComplete Input1VideoSyncStatus = new JoinDataComplete(new JoinData() { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata() { Label = "Input 1 Video Sync Status", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Input2VideoSyncStatus")]
        public JoinDataComplete Input2VideoSyncStatus = new JoinDataComplete(new JoinData() { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata() { Label = "Input 2 Video Sync Status", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Input3VideoSyncStatus")]
        public JoinDataComplete Input3VideoSyncStatus = new JoinDataComplete(new JoinData() { JoinNumber = 6, JoinSpan = 1 },
            new JoinMetadata() { Label = "Input 3 Video Sync Status", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("CurrentInputResolution")]
        public JoinDataComplete CurrentInputResolution = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "DM TX Current Input Resolution", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("VideoInput")]
        public JoinDataComplete VideoInput = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "DM TX Video Input Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("AudioInput")]
        public JoinDataComplete AudioInput = new JoinDataComplete(new JoinData() { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata() { Label = "DM TX Audio Input Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("HdcpSupportCapability")]
        public JoinDataComplete HdcpSupportCapability = new JoinDataComplete(new JoinData() { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata() { Label = "DM TX HDCP Support Capability", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("Port1HdcpState")]
        public JoinDataComplete Port1HdcpState = new JoinDataComplete(new JoinData() { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata() { Label = "DM TX Port 1 HDCP State Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("Port2HdcpState")]
        public JoinDataComplete Port2HdcpState = new JoinDataComplete(new JoinData() { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata() { Label = "DM TX Port 2 HDCP State Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("VgaBrightness")]
        public JoinDataComplete VgaBrightness = new JoinDataComplete(new JoinData() { JoinNumber = 6, JoinSpan = 1 },
            new JoinMetadata() { Label = "DM TX VGA Brightness", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("VgaContrast")]
        public JoinDataComplete VgaContrast = new JoinDataComplete(new JoinData() { JoinNumber = 7, JoinSpan = 1 },
            new JoinMetadata() { Label = "DM TX Online", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });


        public DmTxControllerJoinMap(uint joinStart)
            : base(joinStart, typeof(DmTxControllerJoinMap))
        {
        }
    }
}