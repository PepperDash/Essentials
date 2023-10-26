extern alias Full;
using System;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    public class LayoutInfoChangedEventArgs : EventArgs
    {
        [JsonProperty("availableLayouts", NullValueHandling = NullValueHandling.Ignore)]
        public ZoomRoom.zConfiguration.eLayoutStyle AvailableLayouts { get; set; }
        [JsonProperty("currentSelectedLayout", NullValueHandling = NullValueHandling.Ignore)]
        public ZoomRoom.zConfiguration.eLayoutStyle CurrentSelectedLayout { get; set; }
        [JsonProperty("canSwapContentWithThumbnail", NullValueHandling = NullValueHandling.Ignore)]
        public bool CanSwapContentWithThumbnail { get; set; }
        [JsonProperty("contentSwappedWithThumbnail", NullValueHandling = NullValueHandling.Ignore)]
        public bool ContentSwappedWithThumbnail { get; set; }
        [JsonProperty("layoutViewIsOnFirstPage", NullValueHandling = NullValueHandling.Ignore)]
        public bool LayoutViewIsOnFirstPage { get; set; }
        [JsonProperty("layoutViewIsOnLastPage", NullValueHandling = NullValueHandling.Ignore)]
        public bool LayoutViewIsOnLastPage { get; set; }
    }
}