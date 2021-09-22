using System;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges.JoinMaps;


namespace PepperDash.Essentials.Devices.Common.VideoCodec.Cisco
{
    public class CiscoCodecJoinMap : VideoCodecControllerJoinMap
    {
        #region Digital

        [JoinName("ActivateDoNotDisturbMode")]
        public JoinDataComplete ActivateDoNotDisturbMode = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 221,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Activates Do Not Disturb Mode.  FB High if active.",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("DeactivateDoNotDisturbMode")]
        public JoinDataComplete DeactivateDoNotDisturbMode = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 222,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Deactivates Do Not Disturb Mode.  FB High if deactivated.",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("ToggleDoNotDisturbMode")]
        public JoinDataComplete ToggleDoNotDisturbMode = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 223,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Toggles Do Not Disturb Mode.",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("ActivateStandby")]
        public JoinDataComplete ActivateStandby = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 226,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Activates Standby Mode.  FB High if active.",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("DeactivateStandby")]
        public JoinDataComplete DeactivateStandby = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 227,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Deactivates Standby Mode.  FB High if deactivated.",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("ActivateHalfWakeMode")]
        public JoinDataComplete ActivateHalfWakeMode = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 228,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Activates Half Wake Mode.  FB High if active.",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital
            });

        #endregion


        #region Analog


        #endregion


        #region Serials


        #endregion

        public CiscoCodecJoinMap(uint joinStart)
            : base(joinStart, typeof(CiscoCodecJoinMap))
        {
        }

        public CiscoCodecJoinMap(uint joinStart, Type type)
            : base(joinStart, type)
        {
        }
    }
}