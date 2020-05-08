using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Bridges
{
    public class DmTxControllerJoinMap : JoinMapBaseAdvanced
    {
<<<<<<< HEAD
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
        /// <summary>
        /// High when video sync is detected on input 1 of a multi-input tx
        /// </summary>
        public uint Input1VideoSyncStatus { get; set; }
        /// <summary>
        /// High when video sync is detected on input 2 of a multi-input tx
        /// </summary>
        public uint Input2VideoSyncStatus { get; set; }
        /// <summary>
        /// High when video sync is detected on input 3 of a multi-input tx
        /// </summary>
        public uint Input3VideoSyncStatus { get; set; }
        #endregion
=======
        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "DM TX Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("VideoSyncStatus")]
        public JoinDataComplete VideoSyncStatus = new JoinDataComplete(new JoinData() { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata() { Label = "DM TX Video Sync", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("FreeRunEnabled")]
        public JoinDataComplete FreeRunEnabled = new JoinDataComplete(new JoinData() { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata() { Label = "DM TX Enable Free Run Set / Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

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
>>>>>>> development


<<<<<<< HEAD
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
            Input1VideoSyncStatus = 4;
            Input2VideoSyncStatus = 5;
            Input3VideoSyncStatus = 6;
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
            Input1VideoSyncStatus = Input1VideoSyncStatus + joinOffset;
            Input2VideoSyncStatus = Input2VideoSyncStatus + joinOffset;
            Input3VideoSyncStatus = Input3VideoSyncStatus + joinOffset;
            CurrentInputResolution = CurrentInputResolution + joinOffset;
            VideoInput = VideoInput + joinOffset;
            AudioInput = AudioInput + joinOffset;
            HdcpSupportCapability = HdcpSupportCapability + joinOffset;
            Port1HdcpState = Port1HdcpState + joinOffset;
            Port2HdcpState = Port2HdcpState + joinOffset;
            VgaBrightness = VgaBrightness + joinOffset;
            VgaContrast = VgaContrast + joinOffset;
        }
=======
>>>>>>> development
    }
}