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

		[JsonProperty("icon")]
		public string Icon { get; set; }

		[JsonProperty("altIcon")]
		public string AltIcon { get; set; }

		[JsonProperty("includeInSourceList")]
		public bool IncludeInSourceList { get; set; }

		[JsonProperty("order")]
		public int Order { get; set; }

		[JsonProperty("volumeControlKey")]
		public string VolumeControlKey { get; set; }

		[JsonProperty("type")]
		[JsonConverter(typeof(StringEnumConverter))]
		public eSourceListItemType Type { get; set; }

		[JsonProperty("routeList")]
		public List<SourceRouteListItem> RouteList { get; set; }

		[JsonProperty("disableCodecSharing")]
		public bool DisableCodecSharing { get; set; }

		[JsonProperty("disableRoutedSharing")]
		public bool DisableRoutedSharing { get; set; }

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
}