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
        /// The icon to display
        /// </summary>
		[JsonProperty("icon")]
		public string Icon { get; set; }

        /// <summary>
        /// Alternate icon to display
        /// </summary>
		[JsonProperty("altIcon")]
		public string AltIcon { get; set; }

        /// <summary>
        /// Indicates if the source should be included in the list
        /// </summary>
		[JsonProperty("includeInSourceList")]
		public bool IncludeInSourceList { get; set; }

        /// <summary>
        /// Determines the order the source appears in the list (ascending)
        /// </summary>
		[JsonProperty("order")]
		public int Order { get; set; }

        /// <summary>
        /// Key of the volume control device for the source
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
        /// The list of routes to run when source is selected
        /// </summary>
		[JsonProperty("routeList")]
		public List<SourceRouteListItem> RouteList { get; set; }

        /// <summary>
        /// Indicates if this source should be disabled for sharing via codec content
        /// </summary>
		[JsonProperty("disableCodecSharing")]
		public bool DisableCodecSharing { get; set; }

        /// <summary>
        /// Indicates if this source should be disabled for local routing
        /// </summary>
		[JsonProperty("disableRoutedSharing")]
		public bool DisableRoutedSharing { get; set; }

        /// <summary>
        /// The list of valid destination types for this source
        /// </summary>
        [JsonProperty("destinations")]
        public List<eSourceListItemDestinationTypes> Destinations { get; set; }

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