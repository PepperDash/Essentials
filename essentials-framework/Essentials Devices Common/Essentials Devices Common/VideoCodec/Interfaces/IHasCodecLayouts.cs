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
        event EventHandler<LayoutInfoChangedEventArgs> AvailableLayoutsChanged;

        BoolFeedback LayoutViewIsOnFirstPageFeedback { get; }  // TODO: #697 [*] Consider modifying to report button visibility in func
        BoolFeedback LayoutViewIsOnLastPageFeedback { get; } // TODO: #697 [*] Consider modifying to report button visibility in func 
        BoolFeedback CanSwapContentWithThumbnailFeedback { get; }
        BoolFeedback ContentSwappedWithThumbnailFeedback { get; }
		StringFeedback LayoutSizeFeedback { get; } // TODO: #714 [ ] Feature Layout Size
		StringFeedback LayoutPositionFeedback { get; } // TODO: #714 [ ] Feature Layout Size

        ZoomRoom.zConfiguration.eLayoutStyle LastSelectedLayout { get; }
        ZoomRoom.zConfiguration.eLayoutStyle AvailableLayouts { get; }

        void GetAvailableLayouts(); // Mot sure this is necessary if we're already subscribed to zStatus Call Layout
        void SetLayout(ZoomRoom.zConfiguration.eLayoutStyle layoutStyle);
        void SwapContentWithThumbnail();

        void LayoutTurnNextPage();
        void LayoutTurnPreviousPage();

		void GetCurrentLayoutSize(); // TODO: #714 [ ] Feature Layout Size
		void SetLayoutSize(ZoomRoom.zConfiguration.eLayoutSize layoutSize); // TODO: #714 [ ] Feature Layout Size

		void GetCurrentLayoutPosition(); // TODO: #714 [ ] Feature Layout Size
		void SetLayoutPosition(ZoomRoom.zConfiguration.eLayoutPosition layoutPosition);  // TODO: #714 [ ] Feature Layout Size
    }

    public class LayoutInfoChangedEventArgs : EventArgs
    {
        public ZoomRoom.zConfiguration.eLayoutStyle AvailableLayouts { get; set; }
    }
}