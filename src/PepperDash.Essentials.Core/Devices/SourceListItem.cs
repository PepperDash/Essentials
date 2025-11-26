using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines the type of source list item, which can be a route, off, or other.
    /// This is used to categorize the source list items in a room.
    /// The type is serialized to JSON and can be used to determine how the item should be displayed or handled in the UI.
    /// </summary>
    public enum eSourceListItemType
    {
        /// <summary>
        /// Represents a typical route.
        /// </summary>
        Route,
        /// <summary>
        /// Represents an off route.
        /// </summary>
        Off,
        /// <summary>
        /// Represents some other type of route
        /// </summary>
        Other,
    }

    /// <summary>
    /// Represents a SourceListItem
    /// </summary>
    public class SourceListItem
    {
        /// <summary>
        /// The key of the source item, which is used to identify it in the DeviceManager
        /// </summary>
        [JsonProperty("sourceKey")]
        public string SourceKey { get; set; }

        /// <summary>
        /// Returns the source Device for this, if it exists in DeviceManager
        /// </summary>
        [JsonIgnore]
        public Device SourceDevice
        {
            get
            {
                if (_SourceDevice == null)
                    _SourceDevice = DeviceManager.GetDeviceForKey(SourceKey) as Device;
                return _SourceDevice;
            }
        }

        private Device _SourceDevice;

        /// <summary>
        /// Gets either the source's Name or this AlternateName property, if 
        /// defined.  If source doesn't exist, returns "Missing source"
        /// </summary>
        [JsonProperty("preferredName")]
        public string PreferredName
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                {
                    if (SourceDevice == null)
                        return "---";
                    return SourceDevice.Name;
                }
                return Name;
            }
        }

        /// <summary>
        /// A name that will override the source's name on the UI
        /// </summary>
        [JsonProperty("name")]

        public string Name { get; set; }

        /// <summary>
        /// Specifies and icon for the source list item
        /// </summary>
		[JsonProperty("icon")]
        public string Icon { get; set; }

        /// <summary>
        /// Alternate icon
        /// </summary>
		[JsonProperty("altIcon")]
        public string AltIcon { get; set; }

        /// <summary>
        /// Indicates if the item should be included in the source list
        /// </summary>
		[JsonProperty("includeInSourceList")]
        public bool IncludeInSourceList { get; set; }

        /// <summary>
        /// Used to specify the order of the items in the source list when displayed
        /// </summary>
		[JsonProperty("order")]
        public int Order { get; set; }

        /// <summary>
        /// The key of the device for volume control
        /// </summary>
		[JsonProperty("volumeControlKey")]
        public string VolumeControlKey { get; set; }

        /// <summary>
        /// The type of source list item
        /// </summary>
		[JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public eSourceListItemType Type { get; set; }

        /// <summary>
        /// The list of routes to execute for this source list item
        /// </summary>
		[JsonProperty("routeList")]
        public List<SourceRouteListItem> RouteList { get; set; }

        /// <summary>
        /// Indicates if this source should be disabled for sharing to the far end call participants via codec content
        /// </summary>
		[JsonProperty("disableCodecSharing")]
        public bool DisableCodecSharing { get; set; }

        /// <summary>
        /// Indicates if this source should be disabled for routing to a shared output
        /// </summary>
		[JsonProperty("disableRoutedSharing")]
        public bool DisableRoutedSharing { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("destinations")]
        public List<eSourceListItemDestinationTypes> Destinations { get; set; }
        /// <summary>
        /// A means to reference a source list for this source item, in the event that this source has an input that can have sources routed to it
        /// </summary>
        [JsonProperty("sourceListKey")]
        public string SourceListKey { get; set; }

        /// <summary>
        /// Indicates if the device associated with this source is controllable
        /// </summary>
        [JsonProperty("isControllable")]
        public bool IsControllable { get; set; }

        /// <summary>
        /// Indicates that the device associated with this source has audio available
        /// </summary>
        [JsonProperty("isAudioSource")]
        public bool IsAudioSource { get; set; }

        /// <summary>
        /// Hide source on UI when Avanced Sharing is enabled
        /// </summary>
        [JsonProperty("disableAdvancedRouting")]
        public bool DisableAdvancedRouting { get; set; }

        /// <summary>
        /// Hide source on UI when Simpl Sharing is enabled
        /// </summary>
        [JsonProperty("disableSimpleRouting")]
        public bool DisableSimpleRouting { get; set; }

        /// <summary>
        /// The key of the device that provides video sync for this source item
        /// </summary>
        [JsonProperty("syncProviderDeviceKey")]
        public string SyncProviderDeviceKey { get; set; }

        /// <summary>
        /// Indicates if the source supports USB connections
        /// </summary>
        [JsonProperty("supportsUsb")]
        public bool SupportsUsb { get; set; }

        /// <summary>
        /// The key of the source port associated with this source item
        /// This is used to identify the specific port on the source device that this item refers to for advanced routing
        /// </summary>
        [JsonProperty("sourcePortKey")]
        public string SourcePortKey { get; set; }


        /// <summary>
        /// Default constructor for SourceListItem, initializes the Icon to "Blank"
        /// </summary>
        public SourceListItem()
        {
            Icon = "Blank";
        }

        /// <summary>
        /// Returns a string representation of the SourceListItem, including the SourceKey and Name
        /// </summary>
        /// <returns> A string representation of the SourceListItem</returns>
        public override string ToString()
        {
            return $"{SourceKey}:{Name}";
        }
    }

    /// <summary>
    /// Represents a route in a source list item, which defines the source and destination keys and the type of signal being routed
    /// </summary>
    public class SourceRouteListItem
    {
        /// <summary>
        /// The key of the source device to route from
        /// </summary>
        [JsonProperty("sourceKey")]
        public string SourceKey { get; set; }

        /// <summary>
        /// The key of the source port to route from
        /// </summary>
        [JsonProperty("sourcePortKey")]
        public string SourcePortKey { get; set; }

        /// <summary>
        /// The key of the destination device to route to
        /// </summary>
        [JsonProperty("destinationKey")]
        public string DestinationKey { get; set; }

        /// <summary>
        /// The key of the destination port to route to
        /// </summary>
        [JsonProperty("destinationPortKey")]
        public string DestinationPortKey { get; set; }

        /// <summary>
        /// The type of signal being routed, such as audio or video
        /// </summary>
        [JsonProperty("type")]
        public eRoutingSignalType Type { get; set; }

        /// <summary>
        /// Key for a destination list item. If BOTH SourceListItemKey AND DestinationListItemKey are defined,
        /// then the direct route method should be used.
        /// </summary>
        [JsonProperty("destinationListItemKey", NullValueHandling = NullValueHandling.Ignore)]
        public string DestinationListItemKey { get; set; }

        /// <summary>
        /// Key for a source list item. If BOTH SourceListItemKey AND DestinationListItemKey are defined,
        /// then the direct route method should be used.
        /// </summary>
        [JsonProperty("sourceListItemKey", NullValueHandling = NullValueHandling.Ignore)]
        public string SourceListItemKey { get; set; }
    }

    /// <summary>
    /// Defines the valid destination types for SourceListItems in a room
    /// </summary>
    [Obsolete]
    public enum eSourceListItemDestinationTypes
    {
        /// <summary>
        /// Default display, used for the main video output in a room
        /// </summary>
        defaultDisplay,
        /// <summary>
        /// Left display
        /// </summary>
        leftDisplay,
        /// <summary>
        /// Right display
        /// </summary>
        rightDisplay,
        /// <summary>
        /// Center display
        /// </summary>
        centerDisplay,
        /// <summary>
        /// Program audio, used for the main audio output in a room
        /// </summary>
        programAudio,
        /// <summary>
        /// Codec content, used for sharing content to the far end in a video call
        /// </summary>
        codecContent,
        /// <summary>
        /// Front left display, used for rooms with multiple displays
        /// </summary>
        frontLeftDisplay,
        /// <summary>
        /// Front right display, used for rooms with multiple displays
        /// </summary>
        frontRightDisplay,
        /// <summary>
        /// Rear left display, used for rooms with multiple displays
        /// </summary>
        rearLeftDisplay,
        /// <summary>
        /// Rear right display, used for rooms with multiple displays
        /// </summary>
        rearRightDisplay,
        /// <summary>
        /// Auxiliary display 1, used for additional displays in a room
        /// </summary>
        auxDisplay1,
        /// <summary>
        /// Auxiliary display 2, used for additional displays in a room
        /// </summary>
        auxDisplay2,
        /// <summary>
        /// Auxiliary display 3, used for additional displays in a room
        /// </summary>
        auxDisplay3,
        /// <summary>
        /// Auxiliary display 4, used for additional displays in a room
        /// </summary>
        auxDisplay4,
        /// <summary>
        /// Auxiliary display 5, used for additional displays in a room
        /// </summary>
        auxDisplay5,
        /// <summary>
        /// Auxiliary display 6, used for additional displays in a room
        /// </summary>
        auxDisplay6,
        /// <summary>
        /// Auxiliary display 7, used for additional displays in a room
        /// </summary>
        auxDisplay7,
        /// <summary>
        /// Auxiliary display 8, used for additional displays in a room
        /// </summary>
        auxDisplay8,
        /// <summary>
        /// Auxiliary display 9, used for additional displays in a room
        /// </summary>
        auxDisplay9,
        /// <summary>
        /// Auxiliary display 10, used for additional displays in a room
        /// </summary>
        auxDisplay10,
    }
}