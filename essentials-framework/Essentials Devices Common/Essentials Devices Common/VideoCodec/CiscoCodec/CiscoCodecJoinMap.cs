using System;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges.JoinMaps;


namespace PepperDash.Essentials.Devices.Common.VideoCodec.Cisco
{
    public class CiscoCodecJoinMap : VideoCodecControllerJoinMap
    {
        #region Digital

        [JoinName("PresentationLocalOnly")]
        public JoinDataComplete PresentationLocalOnly = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 205,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Presentation Local Only Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("PresentationLocalRemote")]
        public JoinDataComplete PresentationLocalRemote = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 206,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Presentation Local and Remote Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("ActivateDoNotDisturbMode")]
        public JoinDataComplete ActivateDoNotDisturbMode = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 241,
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
                JoinNumber = 242,
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
                JoinNumber = 243,
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
                JoinNumber = 246,
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
                JoinNumber = 247,
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
                JoinNumber = 248,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Activates Half Wake Mode.  FB High if active.",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("EnteringStandbyMode")]
        public JoinDataComplete EnteringStandbyMode = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 249,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "High to indicate that the codec is entering standby mode",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        #endregion


        #region Analog

        [JoinName("RingtoneVolume")]
        public JoinDataComplete RingtoneVolume = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 21,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Ringtone volume set/FB.  Valid values are 0 - 100 in increments of 5 (5, 10, 15, 20, etc.)",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Analog
            });

        [JoinName("PresentationSource")]
        public JoinDataComplete PresentationSource = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 201,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Presentation set/FB.  Valid values are 0 - 6 depending on the codec model.",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Analog
            });


        #endregion


        #region Serials

        [JoinName("CommandToDevice")]
        public JoinDataComplete CommandToDevice = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 5,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Sends a serial command to the device.  Do not include the delimiter, it will be added automatically.",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Serial
            });



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