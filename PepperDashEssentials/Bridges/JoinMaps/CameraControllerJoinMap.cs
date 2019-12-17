using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Bridges
{
    public class CameraControllerJoinMap : JoinMapBase
    {
        public uint IsOnline { get; set; }
        public uint PowerOff { get; set; }
        public uint PowerOn { get; set; }
        public uint Up { get; set; }
        public uint Down { get; set; }
        public uint Left { get; set; }
        public uint Right { get; set; }
        public uint ZoomIn { get; set; }
        public uint ZoomOut { get; set; }
        public uint PresetRecallOffset { get; set; }
        public uint PresetSaveOffset { get; set; }
        public uint NumberOfPresets { get; set; }

        public enum eCameraControllerJoinMapKey
        {
            IsOnline,
            PowerOn,
            PowerOff,
            TiltUp,
            TiltDown,
            PanLeft,
            PanRight,
            ZoomIn,
            ZoomOut,
            PresetRecallStart,
            PresetSaveStart,
            PresetLabelStart,
            NumberOfPresets
        }

        public CameraControllerJoinMap()
        {
            Joins = new Dictionary<string, JoinMetadata>();

            Joins.Add(eCameraControllerJoinMapKey.IsOnline.ToString(), new JoinMetadata() 
                { JoinNumber = 9, Label = "Is Online", JoinCapabilities = eJoinCapabilities.Read, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(eCameraControllerJoinMapKey.PowerOn.ToString(), new JoinMetadata() 
                { JoinNumber = 7, Label = "Power On", JoinCapabilities = eJoinCapabilities.Read | eJoinCapabilities.Write, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(eCameraControllerJoinMapKey.PowerOff.ToString(), new JoinMetadata() 
                { JoinNumber = 8, Label = "Power Off", JoinCapabilities = eJoinCapabilities.Read | eJoinCapabilities.Write, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(eCameraControllerJoinMapKey.TiltUp.ToString(), new JoinMetadata() 
                { JoinNumber = 1, Label = "Tilt Up", JoinCapabilities = eJoinCapabilities.Write, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(eCameraControllerJoinMapKey.TiltDown.ToString(), new JoinMetadata() 
                { JoinNumber = 2, Label = "TiltDown", JoinCapabilities = eJoinCapabilities.Write, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(eCameraControllerJoinMapKey.PanLeft.ToString(), new JoinMetadata() 
                { JoinNumber = 3, Label = "Pan Left", JoinCapabilities = eJoinCapabilities.Write, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(eCameraControllerJoinMapKey.PanRight.ToString(), new JoinMetadata() 
                { JoinNumber = 4, Label = "Pan Right", JoinCapabilities = eJoinCapabilities.Write, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(eCameraControllerJoinMapKey.ZoomIn.ToString(), new JoinMetadata() 
                { JoinNumber = 5, Label = "Zoom In", JoinCapabilities = eJoinCapabilities.Write, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(eCameraControllerJoinMapKey.ZoomOut.ToString(), new JoinMetadata() 
                { JoinNumber = 6, Label = "Zoom Out", JoinCapabilities = eJoinCapabilities.Write, JoinSpan = 1, JoinType = eJoinType.Digital });

            Joins.Add(eCameraControllerJoinMapKey.PresetRecallStart.ToString(), new JoinMetadata() 
                { JoinNumber = 11, Label = "Preset Recall Start", JoinCapabilities = eJoinCapabilities.Write, JoinSpan = 20, JoinType = eJoinType.Digital });
            Joins.Add(eCameraControllerJoinMapKey.PresetLabelStart.ToString(), new JoinMetadata() 
                { JoinNumber = 11, Label = "Preset Label Start", JoinCapabilities = eJoinCapabilities.Write, JoinSpan = 20, JoinType = eJoinType.Serial });

            Joins.Add(eCameraControllerJoinMapKey.PresetSaveStart.ToString(), new JoinMetadata() 
                { JoinNumber = 31, Label = "Preset Save Start", JoinCapabilities = eJoinCapabilities.Write, JoinSpan = 20, JoinType = eJoinType.Digital });

            Joins.Add(eCameraControllerJoinMapKey.NumberOfPresets.ToString(), new JoinMetadata() 
                { JoinNumber = 5, Label = "Number of Presets", JoinCapabilities = eJoinCapabilities.Write, JoinSpan = 1, JoinType = eJoinType.Analog });

            // Digital
            IsOnline = 9;
            PowerOff = 8;
            PowerOn = 7;
            Up = 1;
            Down = 2;
            Left = 3;
            Right = 4;
            ZoomIn = 5;
            ZoomOut = 6;
            PresetRecallOffset = 10;
            PresetSaveOffset = 30;

            // Analog
            NumberOfPresets = 5;
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;

            
            foreach (var join in Joins)
            {
                join.Value.JoinNumber = join.Value.JoinNumber + joinOffset;
            }



            IsOnline = IsOnline + joinOffset;
            PowerOff = PowerOff + joinOffset;
            PowerOn = PowerOn + joinOffset;
            Up = Up + joinOffset;
            Down = Down + joinOffset;
            Left = Left + joinOffset;
            Right = Right + joinOffset;
            ZoomIn = ZoomIn + joinOffset;
            ZoomOut = ZoomOut + joinOffset;
            PresetRecallOffset = PresetRecallOffset + joinOffset;
            PresetSaveOffset = PresetSaveOffset + joinOffset;
        }
    }
}