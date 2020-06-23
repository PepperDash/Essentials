using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Bridges
{
    public class HdMdNxM4kEControllerJoinMap : JoinMapBaseAdvanced
    {
        [JoinName("Name")]
        public JoinDataComplete Name = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "Device Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("EnableAutoRoute")]
        public JoinDataComplete EnableAutoRoute = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "Enable Automatic Routing on 4x1 Switchers", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("InputName")]
        public JoinDataComplete InputName = new JoinDataComplete(new JoinData() { JoinNumber = 2, JoinSpan = 8 },
            new JoinMetadata() { Label = "Device Input Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("InputSync")]
        public JoinDataComplete InputSync = new JoinDataComplete(new JoinData() { JoinNumber = 2, JoinSpan = 8 },
            new JoinMetadata() { Label = "Device Input Sync", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("OutputName")]
        public JoinDataComplete OutputName = new JoinDataComplete(new JoinData() { JoinNumber = 11, JoinSpan = 2 },
            new JoinMetadata() { Label = "Device Output Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("OutputRoute")]
        public JoinDataComplete OutputRoute = new JoinDataComplete(new JoinData() { JoinNumber = 11, JoinSpan = 2 },
            new JoinMetadata() { Label = "Device Output Route Set/Get", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("OutputRoutedName")]
        public JoinDataComplete OutputRoutedName = new JoinDataComplete(new JoinData() { JoinNumber = 16, JoinSpan = 2 },
            new JoinMetadata() { Label = "Device Output Route Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("EnableInputHdcp")]
        public JoinDataComplete EnableInputHdcp = new JoinDataComplete(new JoinData() { JoinNumber = 11, JoinSpan = 8 },
            new JoinMetadata() { Label = "Device Enable Input Hdcp", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DisableInputHdcp")]
        public JoinDataComplete DisableInputHdcp = new JoinDataComplete(new JoinData() { JoinNumber = 21, JoinSpan = 8 },
            new JoinMetadata() { Label = "Device Disnable Input Hdcp", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData() { JoinNumber = 30, JoinSpan = 1 },
            new JoinMetadata() { Label = "Device Onlne", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        public HdMdNxM4kEControllerJoinMap(uint joinStart)
            : base(joinStart, typeof(HdMdNxM4kEControllerJoinMap))
        {
        }
    }
}