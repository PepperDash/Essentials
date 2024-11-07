using System;
using PepperDash.Essentials.Core.JoinMaps;

namespace PepperDash.Essentials.Core.Bridges.JoinMaps
{
    /// <summary>
    /// Join map for CameraBase devices
    /// </summary>
    public class CameraControllerJoinMap : JoinMapBaseAdvanced
    {
        [JoinName("TiltUp")]
        public JoinDataComplete TiltUp = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 }, new JoinMetadata { Description = "Tilt Up", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        [JoinName("TiltDown")]
        public JoinDataComplete TiltDown = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 }, new JoinMetadata { Description = "Tilt Down", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        [JoinName("PanLeft")]
        public JoinDataComplete PanLeft = new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 }, new JoinMetadata { Description = "Pan Left", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        [JoinName("PanRight")]
        public JoinDataComplete PanRight = new JoinDataComplete(new JoinData { JoinNumber = 4, JoinSpan = 1 }, new JoinMetadata { Description = "Pan Right", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        [JoinName("ZoomIn")]
        public JoinDataComplete ZoomIn = new JoinDataComplete(new JoinData { JoinNumber = 5, JoinSpan = 1 }, new JoinMetadata { Description = "Zoom In", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        [JoinName("ZoomOut")]
        public JoinDataComplete ZoomOut = new JoinDataComplete(new JoinData { JoinNumber = 6, JoinSpan = 1 }, new JoinMetadata { Description = "Zoom Out", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData { JoinNumber = 9, JoinSpan = 1 }, new JoinMetadata { Description = "Is Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });
        [JoinName("PowerOn")]
        public JoinDataComplete PowerOn = new JoinDataComplete(new JoinData { JoinNumber = 7, JoinSpan = 1 }, new JoinMetadata { Description = "Power On", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        [JoinName("PowerOff")]
        public JoinDataComplete PowerOff = new JoinDataComplete(new JoinData { JoinNumber = 8, JoinSpan = 1 }, new JoinMetadata { Description = "Power Off", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("NumberOfPresets")]
        public JoinDataComplete NumberOfPresets = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 1 }, new JoinMetadata { Description = "Tells Essentials the number of defined presets", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Analog });
        [JoinName("PresetRecallStart")]
        public JoinDataComplete PresetRecallStart = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 20 }, new JoinMetadata { Description = "Preset Recall Start", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        [JoinName("PresetLabelStart")]
        public JoinDataComplete PresetLabelStart = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 20 }, new JoinMetadata { Description = "Preset Label Start", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Serial });
        [JoinName("PresetSaveStart")]
        public JoinDataComplete PresetSaveStart = new JoinDataComplete(new JoinData { JoinNumber = 31, JoinSpan = 20 }, new JoinMetadata { Description = "Preset Save Start", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("CameraModeAuto")]
        public JoinDataComplete CameraModeAuto = new JoinDataComplete(new JoinData { JoinNumber = 51, JoinSpan = 1 }, new JoinMetadata { Description = "Camera Mode Auto", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        [JoinName("CameraModeManual")]
        public JoinDataComplete CameraModeManual = new JoinDataComplete(new JoinData { JoinNumber = 52, JoinSpan = 1 }, new JoinMetadata { Description = "Camera Mode Manual", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        [JoinName("CameraModeOff")]
        public JoinDataComplete CameraModeOff = new JoinDataComplete(new JoinData { JoinNumber = 53, JoinSpan = 1 }, new JoinMetadata { Description = "Camera Mode Off", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("SupportsCameraModeAuto")]
        public JoinDataComplete SupportsCameraModeAuto = new JoinDataComplete(new JoinData { JoinNumber = 55, JoinSpan = 1 }, new JoinMetadata { Description = "Supports Camera Mode Auto", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        [JoinName("SupportsCameraModeOff")]
        public JoinDataComplete SupportsCameraModeOff = new JoinDataComplete(new JoinData { JoinNumber = 56, JoinSpan = 1 }, new JoinMetadata { Description = "Supports Camera Mode Off", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        [JoinName("SupportsPresets")]
        public JoinDataComplete SupportsPresets = new JoinDataComplete(new JoinData { JoinNumber = 57, JoinSpan = 1 }, new JoinMetadata { Description = "Supports Presets", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public CameraControllerJoinMap(uint joinStart)
            : this(joinStart, typeof(CameraControllerJoinMap))
        {
        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        protected CameraControllerJoinMap(uint joinStart, Type type) : base(joinStart, type){}
    }
}