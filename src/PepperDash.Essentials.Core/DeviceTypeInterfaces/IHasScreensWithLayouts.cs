using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// This defines a device that has screens with layouts
    /// Simply decorative
    /// </summary>
    public interface IHasScreensWithLayouts
    {
        /// <summary>
        /// A dictionary of screens, keyed by screen ID, that contains information about each screen and its layouts.
        /// </summary>
        Dictionary<uint, ScreenInfo> Screens { get; }
    }

    /// <summary>
    /// Represents information about a screen and its layouts.
    /// </summary>
    public class ScreenInfo
    {

        /// <summary>
        /// Indicates whether the screen is enabled or not.
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// The name of the screen.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The index of the screen.
        /// </summary>
        [JsonProperty("screenIndex")]
        public int ScreenIndex { get; set; }

        /// <summary>
        /// A dictionary of layout information for the screen, keyed by layout ID.
        /// </summary>
        [JsonProperty("layouts")]
        public Dictionary<uint, LayoutInfo> Layouts { get; set; }
    }

    /// <summary>
    /// Represents information about a layout on a screen.
    /// </summary>
    public class LayoutInfo
    {
        /// <summary>
        /// The display name for the layout
        /// </summary>
        [JsonProperty("layoutName")]
        public string LayoutName { get; set; }

        /// <summary>
        /// The index of the layout.
        /// </summary>
        [JsonProperty("layoutIndex")]
        public int LayoutIndex { get; set; }
    }
}
