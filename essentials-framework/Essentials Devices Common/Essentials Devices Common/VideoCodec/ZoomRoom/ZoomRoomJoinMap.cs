using System;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges.JoinMaps;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.ZoomRoom
{
    public class ZoomRoomJoinMap : VideoCodecControllerJoinMap
    {
        // TODO: #697 Set join numbers

        [JoinName("LayoutIsOnFirstPage")]
        public JoinDataComplete LayoutIsOnFirstPage =
            new JoinDataComplete(new JoinData { JoinNumber = 999, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Indicates if layout is on first page",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("LayoutIsOnLastPage")]
        public JoinDataComplete LayoutIsOnLastPage =
            new JoinDataComplete(new JoinData { JoinNumber = 999, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Indicates if layout is on first page",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("LayoutTurnToNextPage")]
        public JoinDataComplete LayoutTurnToNextPage =
            new JoinDataComplete(new JoinData { JoinNumber = 999, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Turns layout view to next page",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("LayoutTurnToPreviousPage")]
        public JoinDataComplete LayoutTurnToPreviousPage =
            new JoinDataComplete(new JoinData { JoinNumber = 999, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Turns layout view to previous page",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("CanSwapContentWithThumbnail")]
        public JoinDataComplete CanSwapContentWithThumbnail =
            new JoinDataComplete(new JoinData { JoinNumber = 999, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "FB Indicates if content can be swapped with thumbnail",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("SwapContentWithThumbnail")]
        public JoinDataComplete SwapContentWithThumbnail =
            new JoinDataComplete(new JoinData { JoinNumber = 999, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Pulse to swap content with thumbnail.  FB reports current state",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("GetSetCurrentLayout")]
        public JoinDataComplete GetSetCurrentLayout =
            new JoinDataComplete(new JoinData { JoinNumber = 999, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Sets and reports the current layout.  Use the LayoutXXXXIsAvailable signals to determine valid layouts",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Serial
            });

        [JoinName("GetAvailableLayouts")]
        public JoinDataComplete GetAvailableLayouts =
            new JoinDataComplete(new JoinData { JoinNumber = 999, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Gets the available layouts.  Will update the LayoutXXXXXIsAvailbale signals.",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Serial
            });

        [JoinName("LayoutGalleryIsAvailable")]
        public JoinDataComplete LayoutGalleryIsAvailable =
            new JoinDataComplete(new JoinData { JoinNumber = 999, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "FB Indicates if layout 'Gallery' is available",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("LayoutSpeakerIsAvailable")]
        public JoinDataComplete LayoutSpeakerIsAvailable =
            new JoinDataComplete(new JoinData { JoinNumber = 999, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "FB Indicates if layout 'Speaker' is available",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("LayoutStripIsAvailable")]
        public JoinDataComplete LayoutStripIsAvailable =
            new JoinDataComplete(new JoinData { JoinNumber = 999, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "FB Indicates if layout 'Strip' is available",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("LayoutShareAllIsAvailable")]
        public JoinDataComplete LayoutShareAllIsAvailable =
            new JoinDataComplete(new JoinData { JoinNumber = 999, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "FB Indicates if layout 'ShareAll' is available",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        public ZoomRoomJoinMap(uint joinStart)
            : base(joinStart, typeof(ZoomRoomJoinMap))
        {
        }

        public ZoomRoomJoinMap(uint joinStart, Type type)
            : base(joinStart, type)
        {
        }
    }
}