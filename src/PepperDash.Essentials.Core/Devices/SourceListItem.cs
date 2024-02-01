

using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// 
	/// </summary>
	public enum eSourceListItemType
	{
		Route, Off, Other, SomethingAwesomerThanThese
	}

	/// <summary>
	/// Represents an item in a source list - can be deserialized into.
	/// </summary>
	public class SourceListItem
	{
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
		Device _SourceDevice;

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

		public SourceListItem()
		{
			Icon = "Blank";
		}

        
	}

	public class SourceRouteListItem
	{
		[JsonProperty("sourceKey")]
		public string SourceKey { get; set; }

		[JsonProperty("destinationKey")]
		public string DestinationKey { get; set; }

		[JsonProperty("type")]
		public eRoutingSignalType Type { get; set; }
	}

    /// <summary>
    /// Defines the valid destination types for SourceListItems in a room
    /// </summary>
    public enum eSourceListItemDestinationTypes
    {
        defaultDisplay,
        leftDisplay,
        rightDisplay,
        programAudio,
        codecContent
    }
}