using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core;


namespace PepperDash.Essentials.Bridges
{
    public class GenericLightingJoinMap : JoinMapBase
    {
        public uint IsOnline { get; set; }
        public uint SelectScene { get; set; }
        public uint LightingSceneOffset { get; set; }
        public uint ButtonVisibilityOffset { get; set; }
        public uint IntegrationIdSet { get; set; }

        public GenericLightingJoinMap()
        {
            // Digital
            IsOnline = 1;
            SelectScene = 1;
            IntegrationIdSet = 1;
            LightingSceneOffset = 10;
            ButtonVisibilityOffset = 40;
            // Analog
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;

            IsOnline = IsOnline + joinOffset;
            SelectScene = SelectScene + joinOffset;
            LightingSceneOffset = LightingSceneOffset + joinOffset;
            ButtonVisibilityOffset = ButtonVisibilityOffset + joinOffset;
        }
    }
}