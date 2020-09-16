using System;
using PepperDash.Essentials.Core;

namespace PepperDash_Essentials_Core.Bridges.JoinMaps
{
    public class VideoCodecControllerJoinMap:JoinMapBaseAdvanced
    {
        #region Status

        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData {JoinNumber = 1, JoinSpan = 1},
            new JoinMetadata
            {
                Description = "Device is Online",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        #endregion

        
        #region NearEndCameraControls

        public JoinDataComplete NearEndCameraUp = new JoinDataComplete(new JoinData {JoinNumber = 11, JoinSpan = 1},
            new JoinMetadata
            {
                Description = "Near Camera Up",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        public JoinDataComplete NearEndCameraDown = new JoinDataComplete(new JoinData { JoinNumber = 12, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Near Camera Down",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        public JoinDataComplete NearEndCameraLeft = new JoinDataComplete(new JoinData { JoinNumber = 13, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Near Camera Left",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        public JoinDataComplete NearEndCameraRight = new JoinDataComplete(new JoinData { JoinNumber = 14, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Near Camera Right",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        public JoinDataComplete NearEndCameraZoomIn = new JoinDataComplete(new JoinData { JoinNumber = 15, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Near Camera Zoom In",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        public JoinDataComplete NearEndCameraZoomOut = new JoinDataComplete(new JoinData { JoinNumber = 16, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Near Camera Zoom Out",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        public JoinDataComplete NearEndCameraFocusNear = new JoinDataComplete(new JoinData { JoinNumber = 17, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Near Camera Focus Near",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        public JoinDataComplete NearEndCameraFocusFar = new JoinDataComplete(new JoinData { JoinNumber = 18, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Near Camera Right",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        public JoinDataComplete NearEndCameraSelectSerial = new JoinDataComplete(
            new JoinData {JoinNumber = 11, JoinSpan = 1},
            new JoinMetadata
            {
                Description = "Camera Select & Feedback by name",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Serial
            });

        public JoinDataComplete NearEndCameraSelectAnalog = new JoinDataComplete(
            new JoinData { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Camera Select & Feedback by index",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Serial
            });



        #endregion
        #region Camera Tracking Controls

        public JoinDataComplete CameraTrackingOn = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Camera Tracking On",
                JoinType = eJoinType.Digital,
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL});

        public JoinDataComplete CameraTrackingOff = new JoinDataComplete(new JoinData {JoinNumber = 12, JoinSpan = 1},
            new JoinMetadata
            {
                Description = "Camera Tracking Off",
                JoinType = eJoinType.Digital,
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL
            });

        public JoinDataComplete CameraTrackingToggle = new JoinDataComplete(
            new JoinData {JoinNumber = 13, JoinSpan = 1},
            new JoinMetadata
            {
                Description = "Camera Tracking Toggle",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        #endregion

        public VideoCodecControllerJoinMap(uint joinStart) : base(joinStart)
        {
        }

        public VideoCodecControllerJoinMap(uint joinStart, Type type) : base(joinStart, type)
        {
        }
    }
}