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

        public CameraControllerJoinMap()
        {
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
            NumberOfPresets = 5;
            // Analog
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;

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