using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer
{
    public class MobileControlSIMPLRunDirectRouteActionJoinMap : JoinMapBaseAdvanced
    {
        [JoinName("AdvancedSharingModeFb")]
        public JoinDataComplete AdvancedSharingModeFb =
            new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Use Advanced Sharing Mode",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("AdvancedSharingModeOn")]
        public JoinDataComplete AdvancedSharingModeOn =
            new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Use Advanced Sharing Mode",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("AdvancedSharingModeOff")]
        public JoinDataComplete AdvancedSharingModeOff =
            new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Use Advanced Sharing Mode",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("AdvancedSharingModeToggle")]
        public JoinDataComplete AdvancedSharingModeToggle =
            new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Use Advanced Sharing Mode",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("SourceForDestinationJoinStart")]
        public JoinDataComplete SourceForDestinationJoinStart =
            new JoinDataComplete(new JoinData { JoinNumber = 51, JoinSpan = 10 },
            new JoinMetadata
            {
                Description = "Source to Route to Destination & FB",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Serial
            });

        [JoinName("SourceForDestinationAudio")]
        public JoinDataComplete SourceForDestinationAudio =
            new JoinDataComplete(new JoinData { JoinNumber = 61, JoinSpan = 1 },
                new JoinMetadata
                {
                    Description = "Source to Route to Destination & FB",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.Serial
                });

        public MobileControlSIMPLRunDirectRouteActionJoinMap(uint joinStart)
            : base(joinStart, typeof(MobileControlSIMPLRunDirectRouteActionJoinMap))
        {
        }
    }
}