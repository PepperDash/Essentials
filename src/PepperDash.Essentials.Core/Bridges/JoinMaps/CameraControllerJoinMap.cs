using System;

namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Represents a CameraControllerJoinMap
    /// </summary>
    public class CameraControllerJoinMap : JoinMapBaseAdvanced
    {
        /// <summary>
        /// Tilt Up
        /// </summary>
        [JoinName("TiltUp")]
        public JoinDataComplete TiltUp = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 }, new JoinMetadata { Description = "Tilt Up", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Tilt Down
        /// </summary>
        [JoinName("TiltDown")]
        public JoinDataComplete TiltDown = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 }, new JoinMetadata { Description = "Tilt Down", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Pan Left
        /// </summary>
        [JoinName("PanLeft")]
        public JoinDataComplete PanLeft = new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 }, new JoinMetadata { Description = "Pan Left", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Pan Right
        /// </summary>
        [JoinName("PanRight")]
        public JoinDataComplete PanRight = new JoinDataComplete(new JoinData { JoinNumber = 4, JoinSpan = 1 }, new JoinMetadata { Description = "Pan Right", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Zoom In
        /// </summary>
        [JoinName("ZoomIn")]
        public JoinDataComplete ZoomIn = new JoinDataComplete(new JoinData { JoinNumber = 5, JoinSpan = 1 }, new JoinMetadata { Description = "Zoom In", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Zoom Out
        /// </summary>
        [JoinName("ZoomOut")]
        public JoinDataComplete ZoomOut = new JoinDataComplete(new JoinData { JoinNumber = 6, JoinSpan = 1 }, new JoinMetadata { Description = "Zoom Out", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Is Online
        /// </summary>
        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData { JoinNumber = 9, JoinSpan = 1 }, new JoinMetadata { Description = "Is Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Power On
        /// </summary>
        [JoinName("PowerOn")]
        public JoinDataComplete PowerOn = new JoinDataComplete(new JoinData { JoinNumber = 7, JoinSpan = 1 }, new JoinMetadata { Description = "Power On", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Power Off
        /// </summary>
        [JoinName("PowerOff")]
        public JoinDataComplete PowerOff = new JoinDataComplete(new JoinData { JoinNumber = 8, JoinSpan = 1 }, new JoinMetadata { Description = "Power Off", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Number Of Presets
        /// </summary>
        [JoinName("NumberOfPresets")]
        public JoinDataComplete NumberOfPresets = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 1 }, new JoinMetadata { Description = "Tells Essentials the number of defined presets", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Analog });
        
        /// <summary>
        /// Preset Recall Start
        /// </summary>
        [JoinName("PresetRecallStart")]
        public JoinDataComplete PresetRecallStart = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 20 }, new JoinMetadata { Description = "Preset Recall Start", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Preset Label Start
        /// </summary>
        [JoinName("PresetLabelStart")]
        public JoinDataComplete PresetLabelStart = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 20 }, new JoinMetadata { Description = "Preset Label Start", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Serial });
        
        /// <summary>
        /// Preset Save Start
        /// </summary>
        [JoinName("PresetSaveStart")]
        public JoinDataComplete PresetSaveStart = new JoinDataComplete(new JoinData { JoinNumber = 31, JoinSpan = 20 }, new JoinMetadata { Description = "Preset Save Start", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Camera Mode Auto
        /// </summary>
        [JoinName("CameraModeAuto")]
        public JoinDataComplete CameraModeAuto = new JoinDataComplete(new JoinData { JoinNumber = 51, JoinSpan = 1 }, new JoinMetadata { Description = "Camera Mode Auto", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Camera Mode Manual
        /// </summary>
        [JoinName("CameraModeManual")]
        public JoinDataComplete CameraModeManual = new JoinDataComplete(new JoinData { JoinNumber = 52, JoinSpan = 1 }, new JoinMetadata { Description = "Camera Mode Manual", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Camera Mode Off
        /// </summary>
        [JoinName("CameraModeOff")]
        public JoinDataComplete CameraModeOff = new JoinDataComplete(new JoinData { JoinNumber = 53, JoinSpan = 1 }, new JoinMetadata { Description = "Camera Mode Off", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Supports Camera Mode Manual
        /// </summary>
        [JoinName("SupportsCameraModeAuto")]
        public JoinDataComplete SupportsCameraModeAuto = new JoinDataComplete(new JoinData { JoinNumber = 55, JoinSpan = 1 }, new JoinMetadata { Description = "Supports Camera Mode Auto", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Supports Camera Mode Off
        /// </summary>
        [JoinName("SupportsCameraModeOff")]
        public JoinDataComplete SupportsCameraModeOff = new JoinDataComplete(new JoinData { JoinNumber = 56, JoinSpan = 1 }, new JoinMetadata { Description = "Supports Camera Mode Off", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });
        
        /// <summary>
        /// Supports Presets
        /// </summary>
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