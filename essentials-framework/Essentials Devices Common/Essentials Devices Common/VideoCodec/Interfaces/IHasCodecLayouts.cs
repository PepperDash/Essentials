using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    /// <summary>
    /// Defines the required elements for layout control
    /// </summary>
    public interface IHasCodecLayouts
    {
        StringFeedback LocalLayoutFeedback { get; }

        void LocalLayoutToggle();
		void LocalLayoutToggleSingleProminent();
		void MinMaxLayoutToggle();
    }

    /// <summary>
    /// Defines the requirements for Zoom Room layout control
    /// </summary>
    public interface IHasZoomRoomLayouts : IHasCodecLayouts
    {
        BoolFeedback LayoutViewIsOnFirstPage { get; }
        BoolFeedback LayoutViewIsOnLastPage { get; }
        BoolFeedback CanSwitchWallView { get; }
        BoolFeedback CanSwitchSpeakerView { get; }
        BoolFeedback CanSwitchShareOnAllScreens { get; }
        BoolFeedback CanSwapContentWithThumbnail { get; }

        List<ZoomRoom.eZoomRoomLayoutStyle> AvailableLayouts { get; }

        void GetLayouts(); // Mot sure this is necessary if we're already subscribed to zStatus Call Layout
        void SetLayout(ZoomRoom.eZoomRoomLayoutStyle layoutStyle);
        void SwapContentWithThumbnail();

        void LayoutTurnNextPage();
        void LayoutTurnPreviousPage();
    }
}