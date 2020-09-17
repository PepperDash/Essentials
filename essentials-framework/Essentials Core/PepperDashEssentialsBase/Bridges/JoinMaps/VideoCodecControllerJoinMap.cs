using System;
using PepperDash.Essentials.Core;

namespace PepperDash_Essentials_Core.Bridges.JoinMaps
{
    public class VideoCodecControllerJoinMap:JoinMapBaseAdvanced
    {
        #region Status

        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData {JoinNumber = 1, JoinSpan = 1},
            new JoinMetadata
            {
                Description = "Device is Online",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        #endregion

        
        #region NearEndCameraControls

        [JoinName("NearCamUp")]
        public JoinDataComplete NearEndCameraUp = new JoinDataComplete(new JoinData {JoinNumber = 11, JoinSpan = 1},
            new JoinMetadata
            {
                Description = "Near Camera Up",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        [JoinName("NearCamDown")]
        public JoinDataComplete NearEndCameraDown = new JoinDataComplete(new JoinData { JoinNumber = 12, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Near Camera Down",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        [JoinName("NearCamLeft")]
        public JoinDataComplete NearEndCameraLeft = new JoinDataComplete(new JoinData { JoinNumber = 13, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Near Camera Left",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        [JoinName("NearCamRight")]
        public JoinDataComplete NearEndCameraRight = new JoinDataComplete(new JoinData { JoinNumber = 14, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Near Camera Right",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        [JoinName("NearCamZoomIn")]
        public JoinDataComplete NearEndCameraZoomIn = new JoinDataComplete(new JoinData { JoinNumber = 15, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Near Camera Zoom In",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        [JoinName("NearCamZoomOut")]
        public JoinDataComplete NearEndCameraZoomOut = new JoinDataComplete(new JoinData { JoinNumber = 16, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Near Camera Zoom Out",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        [JoinName("NearCamFocusNear")]
        public JoinDataComplete NearEndCameraFocusNear = new JoinDataComplete(new JoinData { JoinNumber = 17, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Near Camera Focus Near",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        [JoinName("NearCamFocusFar")]
        public JoinDataComplete NearEndCameraFocusFar = new JoinDataComplete(new JoinData { JoinNumber = 18, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Near Camera Focus Far",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("NearCamSelectSerial")]
        public JoinDataComplete NearEndCameraSelectSerial = new JoinDataComplete(
            new JoinData {JoinNumber = 21, JoinSpan = 1},
            new JoinMetadata
            {
                Description = "Camera Select & Feedback by name",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Serial
            });
        [JoinName("NearCamSelectAnalog")]
        public JoinDataComplete NearEndCameraSelectAnalog = new JoinDataComplete(
            new JoinData { JoinNumber = 21, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Camera Select & Feedback by index",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Analog
            });

        [JoinName("NearCamPresetSelect")]
        public JoinDataComplete NearEndCameraPresetSelect =
            new JoinDataComplete(new JoinData {JoinNumber = 31, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Recall Preset",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.Analog
                });

        [JoinName("NearCamPresetSave")]
        public JoinDataComplete NearEndCameraPresetSave =
            new JoinDataComplete(new JoinData {JoinNumber = 21, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Save Current Preset",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("NearCamPresetNames")]
        public JoinDataComplete NearEndCameraNames = new JoinDataComplete(new JoinData {JoinNumber = 11, JoinSpan = 1},
            new JoinMetadata
            {
                Description = "XSig - Camera Names",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });
        #endregion
        #region Far End Camera Controls
        [JoinName("FarCamUp")]
        public JoinDataComplete FarEndCameraUp = new JoinDataComplete(new JoinData { JoinNumber = 31, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Far Camera Up",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        [JoinName("FarCamDown")]
        public JoinDataComplete FarEndCameraDown = new JoinDataComplete(new JoinData { JoinNumber = 32, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Far Camera Down",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        [JoinName("FarCamLeft")]
        public JoinDataComplete FarEndCameraLeft = new JoinDataComplete(new JoinData { JoinNumber = 33, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Far Camera Left",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        [JoinName("FarCamRight")]
        public JoinDataComplete FarEndCameraRight = new JoinDataComplete(new JoinData { JoinNumber = 34, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Far Camera Right",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        [JoinName("FarCamZoomIn")]
        public JoinDataComplete FarEndCameraZoomIn = new JoinDataComplete(new JoinData { JoinNumber = 35, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Far Camera Zoom In",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        [JoinName("FarCamZoomOut")]
        public JoinDataComplete FarEndCameraZoomOut = new JoinDataComplete(new JoinData { JoinNumber = 36, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Far Camera Zoom Out",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        [JoinName("FarCamFocusNear")]
        public JoinDataComplete FarEndCameraFocusNear = new JoinDataComplete(new JoinData { JoinNumber = 37, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Far Camera Focus Near",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        [JoinName("FarCamFocusFar")]
        public JoinDataComplete FarEndCameraFocusFar = new JoinDataComplete(new JoinData { JoinNumber = 38, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Far Camera Focus Far",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("FarCamPresetSelect")]
        public JoinDataComplete FarEndCameraPresetSelect =
            new JoinDataComplete(new JoinData { JoinNumber = 41, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Recall Preset",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.Analog
                });
        #endregion
        #region Camera Tracking Controls

        [JoinName("CameraTrackingOn")]
        public JoinDataComplete CameraTrackingOn = new JoinDataComplete(new JoinData { JoinNumber = 51, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Camera Tracking On",
                JoinType = eJoinType.Digital,
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL});
        [JoinName("CameraTrackingOff")]
        public JoinDataComplete CameraTrackingOff = new JoinDataComplete(new JoinData {JoinNumber = 52, JoinSpan = 1},
            new JoinMetadata
            {
                Description = "Camera Tracking Off",
                JoinType = eJoinType.Digital,
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL
            });

        [JoinName("CameraTrackingToggle")]
        public JoinDataComplete CameraTrackingToggle = new JoinDataComplete(
            new JoinData {JoinNumber = 53, JoinSpan = 1},
            new JoinMetadata
            {
                Description = "Camera Tracking Toggle",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        #endregion

        public VideoCodecControllerJoinMap(uint joinStart) : base(joinStart, typeof(VideoCodecControllerJoinMap))
        {
        }

        public VideoCodecControllerJoinMap(uint joinStart, Type type) : base(joinStart, type)
        {
        }
    }
}